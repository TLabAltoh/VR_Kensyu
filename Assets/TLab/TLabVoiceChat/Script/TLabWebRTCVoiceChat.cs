using System.Collections;
using System.IO;
using System.IO.Compression;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(TLabWebRTCDataChannel))]
public class TLabWebRTCVoiceChat : MonoBehaviour
{
    [Header("RTCDataChannel")]
    [SerializeField] private TLabWebRTCDataChannel m_dataChannel;

    [Header("Audio Info")]

    [Tooltip("Playback the sound recorded from the microphone yourself or")]
    [SerializeField] private bool m_loopBackSelf = false;

    [Tooltip("Delivering input from the microphone or")]
    [SerializeField] public bool m_isStreaming = false;

    public static TLabWebRTCVoiceChat Instance;

    //
    // Own sound
    //

    private AudioSource m_microphoneSource;
    private AudioClip m_microphoneClip;
    private string m_microphoneName;

    private int m_writeHead;
    private int m_readHead;

    private POTBuf[] potBuffers = new POTBuf[POTBuf.POT_max + 1];

    public delegate void MicCallbackDelegate(float[] buff);
    public MicCallbackDelegate floatsInDelegate;

    private byte[] m_voiceBuffer            = new byte[PACKET_BUFFER_SIZE];
    private int m_vbWriteHead               = 0;
    private const int PACKET_BUFFER_SIZE    = VOICE_BUFFER_SIZE << SIZE_OF_FLOAT_LOG2;
    private const int VOICE_BUFFER_SIZE     = 1024;
    private const int SIZE_OF_FLOAT_LOG2    = 2;
    private const int FREQUENCY             = 44100;
    private const double TIME_LENGTH        = (double)VOICE_BUFFER_SIZE / (double)FREQUENCY;

    //
    // Other's sound
    //

    private Hashtable m_voicePlayers = new Hashtable();

    //
    private const string m_thisName = "[tlabvoicechat] ";

    /*
     * Obtain microphone input in real time
     * https://forum.unity.com/threads/real-time-audio-from-microphone.145686/
     */

    private class POTBuf
    {
        // 2^6 = 64
        // 2^7 = 128
        // 2^8 = 256
        // 2^9 = 512
        // 2^10 = 1024

        public const int POT_min = 6;
        public const int POT_max = 10;

        const int redundancy = 8;
        int index = 0;

        float[][] internalBuffers = new float[redundancy][];

        public float[] buf
        {
            get
            {
                return internalBuffers[index];
            }
        }

        public void Cycle()
        {
            index = (index + 1) % redundancy;
        }

        public POTBuf(int POT)
        {
            for (int r = 0; r < redundancy; r++)
                internalBuffers[r] = new float[1 << POT];
        }
    }

#if UNITY_EDITOR
    public void SetServerAddr(string addr)
    {
        
    }
#endif

    public void RegistClient(string name, TLabVoiceChatPlayer player)
    {
        m_voicePlayers[name] = player;
    }

    public void ReleaseClient(string name)
    {
        m_voicePlayers.Remove(name);
    }

    private void SetupBuffers()
    {
        for (int k = POTBuf.POT_min; k <= POTBuf.POT_max; k++)
            potBuffers[k] = new POTBuf(k);
    }

    private unsafe void LongCopy(byte* src, byte* dst, int count)
    {
        // https://github.com/neuecc/MessagePack-CSharp/issues/117

        while (count >= 8)
        {
            *(ulong*)dst = *(ulong*)src;
            dst += 8;
            src += 8;
            count -= 8;
        }
        if (count >= 4)
        {
            *(uint*)dst = *(uint*)src;
            dst += 4;
            src += 4;
            count -= 4;
        }
        if (count >= 2)
        {
            *(ushort*)dst = *(ushort*)src;
            dst += 2;
            src += 2;
            count -= 2;
        }
        if (count >= 1)
        {
            *dst = *src;
        }
    }

    public void SendVoice(byte[] voice)
    {
        m_dataChannel.SendRTCMsg(voice);
    }

    public void OnVoice(string srcName, byte[] voiceBytes)
    {
        TLabVoiceChatPlayer player = m_voicePlayers[srcName] as TLabVoiceChatPlayer;

        if (player == null)
            return;

        byte[] voiceBuffer  = Decompress(voiceBytes);
        float[] voice       = new float[VOICE_BUFFER_SIZE];

        unsafe
        {
            fixed (byte* src = voiceBuffer)
            fixed (float* dst = voice)
            {
                byte* srcTmp = src;
                byte* dstTmp = (byte*)dst;

                LongCopy(srcTmp, dstTmp, PACKET_BUFFER_SIZE);
            }
        }

        player.PlayVoice(voice);
    }

    public static byte[] Compress(byte[] data)
    {
        MemoryStream output = new MemoryStream();
        using (DeflateStream dstream = new DeflateStream(output, System.IO.Compression.CompressionLevel.Optimal))
        {
            dstream.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }

    public static byte[] Decompress(byte[] data)
    {
        MemoryStream input = new MemoryStream(data);
        MemoryStream output = new MemoryStream();
        using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
        {
            dstream.CopyTo(output);
        }
        return output.ToArray();
    }

    private void Update()
    {
        m_writeHead = Microphone.GetPosition(m_microphoneName);

        if (m_readHead == m_writeHead || potBuffers == null || !m_isStreaming)
            return;

        // Say audio.clip.samples (S)  = 100
        // if w=1, r=0, we want 1 sample.  ( S + 1 - 0 ) % S = 1 YES
        // if w=0, r=99, we want 1 sample.  ( S + 0 - 99 ) % S = 1 YES
        int nFloatsToGet = (m_microphoneClip.samples + m_writeHead - m_readHead) % m_microphoneClip.samples;

        for (int k = POTBuf.POT_max; k >= POTBuf.POT_min; k--)
        {
            POTBuf B = potBuffers[k];

            // 1 << k;
            int n = B.buf.Length;

            while (nFloatsToGet >= n)
            {
                m_microphoneClip.GetData(B.buf, m_readHead);
                m_readHead = (m_readHead + n) % m_microphoneClip.samples;

                if (floatsInDelegate != null)
                    floatsInDelegate(B.buf);

                B.Cycle();
                nFloatsToGet -= n;
            }
        }
    }

    private void Awake()
    {
        Instance = this;

        var configuration = AudioSettings.GetConfiguration();
        configuration.dspBufferSize = VOICE_BUFFER_SIZE;
        AudioSettings.Reset(configuration);
        Debug.Log(m_thisName + configuration.dspBufferSize);
    }

    private void Start()
    {
        m_microphoneSource = GetComponent<AudioSource>();

        string deviceList = "Currently connected device:\n";
        foreach (string deveice in Microphone.devices)
            deviceList += "\t" + deveice + "\n";
        Debug.Log(m_thisName + deviceList);

        m_microphoneName = Microphone.devices[0];

        m_microphoneClip = Microphone.Start(m_microphoneName, true, 1, FREQUENCY);

        if (m_microphoneClip == null)
            Debug.Log(m_thisName + "Failed to recording, using " + m_microphoneName);
        else
            Debug.Log(m_thisName + "Start recording, using " + m_microphoneName + ", samples: " + m_microphoneClip.samples + ", channels: " + m_microphoneClip.channels);

        if (m_loopBackSelf)
        {
            while (!(Microphone.GetPosition(m_microphoneName) > 0)) { }
            m_microphoneSource.clip = m_microphoneClip;
            m_microphoneSource.loop = true;
            m_microphoneSource.Play();

            Debug.Log(m_thisName + "Sart Loop Back");
        }

        SetupBuffers();

        //
        // Callback function to process microphone input acquired in real time
        // Send to server when send buffer exceeds 4800 bytes
        //

        floatsInDelegate += (float[] buffer) =>
        {
            // Note that pointers are handled on a byte scale.
            int buffSizeInByte = buffer.Length << SIZE_OF_FLOAT_LOG2;
            int sum = m_vbWriteHead + buffSizeInByte;

            if (sum > PACKET_BUFFER_SIZE)
            {
                unsafe
                {
                    // Pointers defined with fixed must not be incremented.

                    fixed (byte* root = m_voiceBuffer)
                    fixed (float* src = buffer)
                    {
                        int size = PACKET_BUFFER_SIZE - m_vbWriteHead;

                        byte* srcTmp = (byte*)src;
                        byte* dstTmp = root + m_vbWriteHead;

                        LongCopy(srcTmp, dstTmp, size);
                        srcTmp += size;

                        // Use Base64 encoding for lossless conversion
                        SendVoice(Compress(m_voiceBuffer));

                        m_vbWriteHead = 0;

                        int remain = buffSizeInByte - size;

                        dstTmp = root;

                        LongCopy(srcTmp, dstTmp, remain);

                        m_vbWriteHead = remain;
                    }
                }
            }
            else if (sum == PACKET_BUFFER_SIZE)
            {
                unsafe
                {
                    fixed (byte* root = m_voiceBuffer)
                    fixed (float* src = buffer)
                    {
                        byte* srcTmp = (byte*)src;
                        byte* dstTmp = root + m_vbWriteHead;

                        LongCopy(srcTmp, dstTmp, buffSizeInByte);

                        SendVoice(Compress(m_voiceBuffer));

                        m_vbWriteHead = 0;
                    }
                }
            }
            else
            {
                unsafe
                {
                    fixed (byte* root = m_voiceBuffer)
                    fixed (float* src = buffer)
                    {
                        byte* srcTmp = (byte*)src;
                        byte* dstTmp = root + m_vbWriteHead;

                        LongCopy(srcTmp, dstTmp, buffSizeInByte);

                        m_vbWriteHead = (m_vbWriteHead + buffSizeInByte) % PACKET_BUFFER_SIZE;
                    }
                }
            }
        };
    }
}

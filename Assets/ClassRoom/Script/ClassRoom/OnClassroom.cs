using UnityEngine;
using UnityEngine.SceneManagement;

public class OnClassroom : MonoBehaviour
{
    [SerializeField] private Transform m_centerEyeAnchor;
    [SerializeField] private Transform m_targetPanel;
    [SerializeField] private Transform m_webViewPanel;

    [SerializeField] private TLabSyncClient m_syncClient;
    [SerializeField] private TLabWebRTCVoiceChat m_voiceChat;

    void ChangeScene()
    {
        m_syncClient.Close();
        m_voiceChat.Close();

        SceneManager.LoadScene("Entry", LoadSceneMode.Single);
    }

    public void ExitClassroom()
    {
        Invoke("ChangeScene", 1.5f);
    }

    public void ShowReference()
    {
        SwitchPanel(m_webViewPanel);
    }

    private void SwitchPanel(Transform target, bool active)
    {
        target.gameObject.SetActive(active);

        if (active == true)
        {
            target.position = m_centerEyeAnchor.position + m_centerEyeAnchor.forward * 1.0f;
            target.right = (m_centerEyeAnchor.position - target.position).normalized;
            target.forward = Vector3.up;
        }
    }

    private bool SwitchPanel(Transform target)
    {
        bool active = target.gameObject.activeSelf;
        SwitchPanel(target, !active);

        return active;
    }

    private void Start()
    {
        m_targetPanel.gameObject.SetActive(false);
    }

    private void Update()
    {
        if(OVRInput.GetDown(OVRInput.Button.Start) == true)
            if (SwitchPanel(m_targetPanel) == false)
                SwitchPanel(m_webViewPanel, false);
    }
}
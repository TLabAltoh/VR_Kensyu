using System;
using System.Collections;
using UnityEngine;

public class TextController : MonoBehaviour
{
    public class TextControllerTransform
    {
        public TextControllerTransform(Vector3 localPosition, Vector3 localScale, Quaternion localRotation)
        {
            this.localPosotion = localPosition;
            this.localScale    = localScale;
            this.localRotation = localRotation;
        }

        public Vector3 localPosotion;
        public Vector3 localScale;
        public Quaternion localRotation;
    }

    [SerializeField] private Transform m_target;

    [SerializeField] private float m_forward;
    [SerializeField] private float m_vertical;
    [SerializeField] private float m_horizontal;

    private TextControllerTransform m_initialTransform;

    private void LerpScale(Transform target, TextControllerTransform start, TextControllerTransform end, float lerpValue)
    {
        target.localScale = Vector3.Lerp(start.localScale, end.localScale, lerpValue);
    }

    private IEnumerator FadeInTask()
    {
        // ���݂�Transform
        TextControllerTransform currentTransform = new TextControllerTransform(
            this.transform.localPosition,
            this.transform.localScale,
            this.transform.localRotation);

        const float duration = 0.5f;
        float current = 0.0f;
        while(current < duration)
        {
            current += Time.deltaTime;
            LerpScale(this.transform, currentTransform, m_initialTransform, current / duration);
            yield return null;
        }
    }

    private IEnumerator FadeOutTask()
    {
        // ���݂�Transform
        TextControllerTransform currentTransform = new TextControllerTransform(
            this.transform.localPosition,
            this.transform.localScale,
            this.transform.localRotation);

        // Scale��(0, 0, 0)��Transform(�^�[�Q�b�g)
        TextControllerTransform targetTransform = new TextControllerTransform(
            this.transform.localPosition,
            Vector3.zero,
            this.transform.localRotation);

        const float duration = 0.5f;
        float current = 0.0f;
        while(current < duration)
        {
            current += Time.deltaTime;
            LerpScale(this.transform, currentTransform, targetTransform, current / duration);
            yield return null;
        }
    }

    public void FadeIn()
    {
        StartCoroutine("FadeInTask");
    }

    public void FadeOut()
    {
        StartCoroutine("FadeOutTask");
    }

    void Start()
    {
        string name     = this.gameObject.name;
        int anchorIndex = Int32.Parse(name[name.Length - 1].ToString());
        if (anchorIndex != TLabSyncClient.Instalce.SeatIndex) Destroy(this.gameObject);

        this.transform.parent = null;

        m_initialTransform = new TextControllerTransform(
            this.transform.localPosition,
            this.transform.localScale,
            this.transform.localRotation);
    }

    void Update()
    {
        if (m_target == null) return;

        Transform mainCamera = Camera.main.transform;
        Vector3 diff    = mainCamera.position - m_target.position;
        Vector3 offset  = diff.normalized * m_forward + Vector3.up * m_vertical + Vector3.Cross(diff.normalized,Vector3.up) * m_horizontal;

        this.transform.position = m_target.position + offset;
        this.transform.LookAt(mainCamera, Vector3.up);
    }
}
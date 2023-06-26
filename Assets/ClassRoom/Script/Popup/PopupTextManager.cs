using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PopupTextManager : MonoBehaviour
{
    [System.Serializable]
    public class PointerPopupPair
    {
        public TextController controller;
        public GameObject target;
    }

    public PointerPopupPair[] PointerPairs
    {
        get
        {
            return m_pointerPairs;
        }
    }

    [SerializeField] protected TextController[] m_controllers;
    [SerializeField] protected PointerPopupPair[] m_pointerPairs;

    public TextController GetTextController(int index)
    {
        if (index < m_pointerPairs.Length)
            return m_pointerPairs[index].controller;
        else
            return null;
    }

    private void OnDestroy()
    {
        // PopupTextManager��j������Ƃ��CPopupTextManager���ێ����Ă���TextController(�Ƃ��������GameObject)���ꏏ�ɔj������D
        // TextController��Start()��transform.parent = null(�y�A�����g������)���Ă���̂ŁCPopupManager������GameObject��j��
        // ���邾���ł͉����N���Ȃ�(TextController������GameObject�̓V�[���Ɏc�葱����) ----> �ȉ��̍s�ňꏏ�ɔj������΂����D

        if(m_controllers != null)
            foreach(TextController controller in m_controllers) Destroy(controller.gameObject);

        if (m_pointerPairs != null)
            foreach (PointerPopupPair pointerPair in m_pointerPairs) Destroy(pointerPair.controller.gameObject);
    }
}

#region CustomEditor
#if UNITY_EDITOR
[CustomEditor(typeof(PopupTextManager))]
public class PopupTextManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        PopupTextManager manager = target as PopupTextManager;

        if (GUILayout.Button("Overwrite Outline for Popup Selectable"))
        {
            for(int index = 0; index < manager.PointerPairs.Length; index++)
            {
                GameObject target = manager.PointerPairs[index].target;

                TLabOutlineSelectable outlineSelectable = target.GetComponent<TLabOutlineSelectable>();
                PopupSelectable popupSelectable         = target.GetComponent<PopupSelectable>();
                if (popupSelectable == null) popupSelectable = target.AddComponent<PopupSelectable>();

                if (outlineSelectable != null)
                {
                    popupSelectable.OutlineMat      = outlineSelectable.OutlineMat;
                    popupSelectable.PopupManager    = manager;
                    popupSelectable.Index           = index++;

                    DestroyImmediate(outlineSelectable);
                }

                if (popupSelectable != null) EditorUtility.SetDirty(popupSelectable);
            }

            EditorUtility.SetDirty(manager);
        }

        if (GUILayout.Button("Revert to OutlineSelectable"))
        {
            for (int index = 0; index < manager.PointerPairs.Length; index++)
            {
                GameObject target = manager.PointerPairs[index].target;

                TLabOutlineSelectable outlineSelectable = target.GetComponent<TLabOutlineSelectable>();
                PopupSelectable popupSelectable         = target.GetComponent<PopupSelectable>();
                if (outlineSelectable == null || outlineSelectable == popupSelectable)
                    outlineSelectable = popupSelectable.gameObject.AddComponent<TLabOutlineSelectable>();

                if (popupSelectable != null)
                {
                    outlineSelectable.OutlineMat = popupSelectable.OutlineMat;
                    DestroyImmediate(popupSelectable);
                }

                if (outlineSelectable != null) EditorUtility.SetDirty(outlineSelectable);
            }

            EditorUtility.SetDirty(manager);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
#endregion CustomEditor
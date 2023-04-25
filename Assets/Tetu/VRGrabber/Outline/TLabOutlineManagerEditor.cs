using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(TLabOutlineManager))]
public class TLabOutlineManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TLabOutlineManager manager = target as TLabOutlineManager;

        if (GUILayout.Button("Process"))
        {
            manager.BakeVertexColor();
        }
    }
}
#endif
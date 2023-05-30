using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(TLabShelfSyncManager))]
[CanEditMultipleObjects]
public class TLabShelfSyncManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        TLabShelfSyncManager manager = target as TLabShelfSyncManager;

        foreach(TLabShelfObjInfo shelfInfo in manager.m_shelfObjInfos)
        {
            TLabSyncGrabbable grabbable = shelfInfo.obj.GetComponent<TLabSyncGrabbable>();
            if (grabbable == null)
                grabbable = shelfInfo.obj.AddComponent<TLabSyncGrabbable>();

            // Do not allow useGravity on shelf objects

            grabbable.m_enableSync = true;
            grabbable.m_autoSync = false;
            grabbable.m_locked = false;

            grabbable.UseRigidbody(true, false);

            TLabSyncRotatable rotatable = grabbable.gameObject.GetComponent<TLabSyncRotatable>();
            if (rotatable == null)
                grabbable.gameObject.AddComponent<TLabSyncRotatable>();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
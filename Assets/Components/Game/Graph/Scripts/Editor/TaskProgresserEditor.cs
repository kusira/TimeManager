using UnityEditor;
using UnityEngine;

namespace Components.Game.Graph.Scripts.Editor
{
    [CustomEditor(typeof(TaskProgresser))]
    public class TaskProgresserEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // プロパティを描画 (workerObjectsを除く)
            DrawPropertiesExcluding(serializedObject, "m_Script", "workerObjects");

            serializedObject.ApplyModifiedProperties();

            TaskProgresser progresser = (TaskProgresser)target;

            if (GUILayout.Button("Initialize"))
            {
                progresser.Initialize();
            }
        }
    }
}

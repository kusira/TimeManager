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

            // workerObjectsのカスタム描画
            SerializedProperty workerObjects = serializedObject.FindProperty("workerObjects");
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Worker References (Assign in Inspector)", EditorStyles.boldLabel);
            
            EditorGUI.indentLevel++;
            int newSize = EditorGUILayout.IntField("Size", workerObjects.arraySize);
            if (newSize != workerObjects.arraySize)
            {
                workerObjects.arraySize = newSize;
            }

            for (int i = 0; i < workerObjects.arraySize; i++)
            {
                string label = "Worker " + ((char)('A' + i)).ToString();
                SerializedProperty element = workerObjects.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(element, new GUIContent(label));
            }
            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();

            TaskProgresser progresser = (TaskProgresser)target;

            if (GUILayout.Button("Initialize"))
            {
                progresser.Initialize();
            }
        }
    }
}

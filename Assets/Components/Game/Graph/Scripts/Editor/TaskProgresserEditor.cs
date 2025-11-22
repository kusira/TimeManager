using UnityEditor;
using UnityEngine;

namespace Components.Game.Graph.Scripts.Editor
{
    [CustomEditor(typeof(TaskProgresser))]
    public class TaskProgresserEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            TaskProgresser progresser = (TaskProgresser)target;

            if (GUILayout.Button("Initialize"))
            {
                progresser.Initialize();
            }
        }
    }
}


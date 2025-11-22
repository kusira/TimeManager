using UnityEditor;
using UnityEngine;

namespace Components.Game.Items.Scripts.Editor
{
    [CustomEditor(typeof(ItemAssigner))]
    public class ItemAssignerEditor : UnityEditor.Editor
    {
        private string testItemId = "";

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ItemAssigner assigner = (ItemAssigner)target;

            GUILayout.Space(10);
            GUILayout.Label("Debug: Assign Item", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Item ID:", GUILayout.Width(60));
            testItemId = GUILayout.TextField(testItemId);
            
            if (GUILayout.Button("Assign", GUILayout.Width(60)))
            {
                assigner.AssignItem(testItemId);
            }
            GUILayout.EndHorizontal();
        }
    }
}


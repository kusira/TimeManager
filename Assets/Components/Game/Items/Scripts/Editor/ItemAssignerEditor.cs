using UnityEditor;
using UnityEngine;

namespace Components.Game.Items.Scripts.Editor
{
    [CustomEditor(typeof(ItemAssigner))]
    public class ItemAssignerEditor : UnityEditor.Editor
    {
        private string testItemId = "";
        private int testCount = 1;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ItemAssigner assigner = (ItemAssigner)target;

            GUILayout.Space(10);
            GUILayout.Label("Debug: Assign Item", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Item ID:", GUILayout.Width(60));
            testItemId = GUILayout.TextField(testItemId);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Count:", GUILayout.Width(60));
            testCount = EditorGUILayout.IntField(testCount);
            GUILayout.EndHorizontal();
            
            if (GUILayout.Button("Assign"))
            {
                assigner.AssignItem(testItemId, testCount);
            }
        }
    }
}

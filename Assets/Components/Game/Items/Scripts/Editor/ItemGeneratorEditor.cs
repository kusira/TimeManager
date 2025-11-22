using UnityEditor;
using UnityEngine;

namespace Components.Game.Items.Scripts.Editor
{
    [CustomEditor(typeof(ItemGenerator))]
    public class ItemGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ItemGenerator generator = (ItemGenerator)target;

            GUILayout.Space(10);
            if (GUILayout.Button("Generate Items"))
            {
                generator.GenerateItems();
            }
        }
    }
}


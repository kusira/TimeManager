using UnityEditor;
using UnityEngine;

namespace Components.Game.Graph.Scripts.Editor
{
    [CustomEditor(typeof(GraphGenerator))]
    public class GraphGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GraphGenerator arrangementer = (GraphGenerator)target;

            if (GUILayout.Button("Generate Graph"))
            {
                arrangementer.GenerateGraph();
            }
        }
    }
}


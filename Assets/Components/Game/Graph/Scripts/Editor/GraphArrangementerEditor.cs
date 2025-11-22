using UnityEditor;
using UnityEngine;

namespace Components.Game.Graph.Scripts.Editor
{
    [CustomEditor(typeof(GraphArrangementer))]
    public class GraphArrangementerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GraphArrangementer arrangementer = (GraphArrangementer)target;

            if (GUILayout.Button("Generate Graph"))
            {
                arrangementer.GenerateGraph();
            }
        }
    }
}


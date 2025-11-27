using UnityEditor;
using UnityEngine;
using Components.Game;

namespace Components.Common.Scripts.Editor
{
    [CustomEditor(typeof(StageManager))]
    public class StageManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("セーブデータ管理", EditorStyles.boldLabel);

            StageManager stageManager = (StageManager)target;

            EditorGUILayout.BeginHorizontal();
            
            // 現在の最大到達ステージを表示
            int maxStage = StageManager.GetMaxReachedStage();
            EditorGUILayout.LabelField($"最大到達ステージ: {maxStage}", GUILayout.Width(200));

            // セーブデータ削除ボタン
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("セーブデータ削除", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog(
                    "セーブデータ削除",
                    "セーブデータを削除しますか？\nこの操作は取り消せません。",
                    "削除",
                    "キャンセル"))
                {
                    stageManager.DeleteSaveData();
                }
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }
    }
}


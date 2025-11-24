using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Components.Game; // StageManagerのため

namespace Components.Game.Canvas.Scripts
{
    public class ReplayManager : MonoBehaviour
    {
        [Tooltip("リプレイボタンをアサインしてください")]
        [SerializeField] private Button replayButton;

        [Tooltip("FadeManagerをアサインしてください (nullの場合は自動で探します)")]
        [SerializeField] private FadeManager fadeManager;

        [Tooltip("クリックしてからフェードアウト開始までの遅延時間 (秒)")]
        [SerializeField] private float clickDelay = 0f;

        private void Start()
        {
            // FadeManagerがアサインされていない場合、シーン内から探す
            if (fadeManager == null)
            {
                fadeManager = FindFirstObjectByType<FadeManager>();
            }

            if (replayButton != null)
            {
                replayButton.onClick.AddListener(OnReplayClicked);
            }
        }

        private void OnReplayClicked()
        {
            // リプレイ時は現在のステージインデックスを維持する
            if (StageManager.Instance != null)
            {
                StageManager.SetNextStage(StageManager.Instance.CurrentStageIndex);
            }

            string currentSceneName = SceneManager.GetActiveScene().name;

            if (fadeManager != null)
            {
                fadeManager.FadeOutAndLoadScene(currentSceneName, clickDelay);
            }
            else
            {
                // フェードマネージャーが見つからない場合は即座にリロード
                SceneManager.LoadScene(currentSceneName);
            }
        }
    }
}


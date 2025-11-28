using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Components.Game.Canvas.Scripts
{
    public class HomeButtonManager : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("遷移先のタイトルシーン名")]
        [SerializeField] private string titleSceneName = "TitleScene";
        
        [Tooltip("クリックしてからフェードアウト開始までの遅延時間 (秒)")]
        [SerializeField] private float clickDelay = 0f;

        [Header("Components")]
        [Tooltip("ホームに戻るボタンをアサインしてください")]
        [SerializeField] private Button homeButton;

        [Tooltip("FadeManagerをアサインしてください (nullの場合は自動で探します)")]
        [SerializeField] private FadeManager fadeManager;

        [Header("Audio")]
        [Tooltip("AudioSource (nullの場合は自動で探すかAddします)")]
        [SerializeField] private AudioSource audioSource;

        private void Start()
        {
            // FadeManagerがアサインされていない場合、シーン内から探す
            if (fadeManager == null)
            {
                fadeManager = FindFirstObjectByType<FadeManager>();
            }

            if (homeButton != null)
            {
                homeButton.onClick.AddListener(OnHomeClicked);
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        private void OnHomeClicked()
        {
            // SE再生
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.PlayOneShot(audioSource.clip);
            }

            // タイトルに戻る際はTimeScaleを戻す（ポーズ中などの場合を考慮）
            Time.timeScale = 1f;

            if (fadeManager != null)
            {
                fadeManager.FadeOutAndLoadScene(titleSceneName, clickDelay);
            }
            else
            {
                // フェードマネージャーが見つからない場合は即座に遷移
                SceneManager.LoadScene(titleSceneName);
            }
        }
    }
}

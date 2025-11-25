using UnityEngine;
using UnityEngine.SceneManagement;

namespace Components.Game
{
    public class GameBGMManager : MonoBehaviour
    {
        public static GameBGMManager Instance { get; private set; }

        [Tooltip("このBGMを維持するシーン名のリスト")]
        [SerializeField] private string[] keepBgmScenes = new string[] { "GameScene" };

        private AudioSource audioSource;

        private void Awake()
        {
            // シングルトン設定 (シーン遷移しても破棄されない)
            if (Instance != null && Instance != this)
            {
                // すでにインスタンスが存在する場合、新しい方は破棄する
                // ただし、BGMを途切れさせないために、既存のインスタンスを使う
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = GetComponent<AudioSource>();
            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.Play();
            }

            // シーンロードイベントを監視して、タイトルに戻ったときなどに破棄するかどうか制御する
            // (もしゲームシーン以外でこのBGMを止めたければここで制御)
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // 現在のシーンが維持対象リストに含まれているかチェック
            bool isKeepScene = false;
            foreach (var sceneName in keepBgmScenes)
            {
                if (scene.name == sceneName)
                {
                    isKeepScene = true;
                    break;
                }
            }

            // 対象外のシーンなら破棄
            if (!isKeepScene)
            {
                Destroy(gameObject);
            }
        }
    }
}


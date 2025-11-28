using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class XButtonManager : MonoBehaviour
{
    [SerializeField, TextArea]
    private string tweetText = "『仕事が納期に間に合わない！』をクリアしました！\n\n\nhttps://unityroom.com/games/work_never_ends\n\n#UnityRoom";

    void Start()
    {
        var button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnTweetButtonClicked);
        }
    }

    private void OnTweetButtonClicked()
    {
        // URLエンコードを行う
        string escapedText = UnityWebRequest.EscapeURL(tweetText);
        
        // Twitter(X)の投稿画面URLを作成
        // intent/tweet または share どちらでも可ですが、intent/tweetが一般的
        string url = "https://twitter.com/intent/tweet?text=" + escapedText;

        // ブラウザを開く
        Application.OpenURL(url);
    }
}


using UnityEngine;

public class OpenURL : MonoBehaviour
{
    [SerializeField] private string url = "https://example.com";

    // ボタンのOnClick()にこの関数を登録する
    public void Open()
    {
        Application.OpenURL(url);
    }
}

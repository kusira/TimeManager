using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

[Serializable]
public class ButtonDataItem
{
    public string scene;
    public string comment;
    public string workTime;   
    public string difficulty; 
}

[Serializable]
public class ButtonDataList
{
    public ButtonDataItem[] list;
}

public class StageSelect : MonoBehaviour
{
    public static StageSelect Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private UnityEngine.UI.Image commentImage;
    [SerializeField] private TextMeshProUGUI commentText;
    [SerializeField] private TextMeshProUGUI workTimeText;
    [SerializeField] private TextMeshProUGUI difficultyText;

    [Header("JSON (TextAsset)")]
    [SerializeField] private TextAsset jsonFile;

    private ButtonDataList data;

    void Awake()
    {
        Instance = this; // シングルトン
    }

    void Start()
    {
        // 初期状態
        commentImage.gameObject.SetActive(false);
        commentText.text = "";
        workTimeText.text = "";
        difficultyText.text = "";

        // JSONロード
        string wrapped = "{\"list\":" + jsonFile.text + "}";
        data = JsonUtility.FromJson<ButtonDataList>(wrapped);
    }

    // ← ボタンがカーソルに触れたら呼ばれる
    public void ShowComment(int index)
    {
        if (index >= data.list.Length) return;

        commentText.text    = data.list[index].comment;
        workTimeText.text   = data.list[index].workTime;
        difficultyText.text = data.list[index].difficulty;

        commentImage.gameObject.SetActive(true);
    }

    // ← カーソルが離れたら呼ばれる
    public void HideComment(int index)
    {
        commentImage.gameObject.SetActive(false);
    }

    // ← ボタンが押されたら呼ばれる
    public void LoadScene(int index)
    {
        if (index >= data.list.Length) return;
        Debug.Log(data.list[index].scene);
        if (data.list[index].scene != "non")
        {
            SceneManager.LoadScene(data.list[index].scene);
        }
    }
}
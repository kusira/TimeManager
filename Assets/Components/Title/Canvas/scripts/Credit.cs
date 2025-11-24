using UnityEngine;

public class Credit : MonoBehaviour
{
    [SerializeField] private GameObject[] objects;     // 切り替える対象のオブジェクト
    [SerializeField] private int startIndex = 0;       // 最初に表示するインデックス

    private int currentIndex = 0;

    void Start()
    {
        // すべて非アクティブ化
        foreach (var obj in objects)
            obj.SetActive(false);

        // startIndex を基準に初期表示
        if (objects.Length > 0 && startIndex >= 0 && startIndex < objects.Length)
        {
            objects[startIndex].SetActive(true);
            currentIndex = startIndex;
        }
        else
        {
            Debug.LogWarning("startIndex が不正です");
        }
    }

    public void SwitchNext()
    {
        if (objects.Length == 0) return;

        objects[currentIndex].SetActive(false);

        currentIndex = (currentIndex + 1) % objects.Length;

        objects[currentIndex].SetActive(true);
    }
}

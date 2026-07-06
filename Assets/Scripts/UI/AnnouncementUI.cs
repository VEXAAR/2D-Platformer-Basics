using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnnouncementUI : MonoBehaviour
{
    private TextMeshProUGUI tmp;

    void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
    }

    public void SetText(string text, float duration = 2.5f)
    {
        tmp.text = text;

        Invoke("RemoveText", duration);
    }

    private void RemoveText()
    {
        tmp.text = "";
    }
}

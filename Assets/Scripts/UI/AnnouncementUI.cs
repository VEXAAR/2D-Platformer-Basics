using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnnouncementUI : MonoBehaviour
{
    private TextMeshProUGUI tmp; // Referens till TextMeshPro komponenten.

    void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
    }

    // Sätter texten till "text" och visar den i "duration" sekunder.
    public void SetText(string text, float duration = 2.5f)
    {
        tmp.text = text;

        Invoke("RemoveText", duration); // Efter "duration" sekunder så kallar den RemoveText()
    }

    // Tar bort texten som visas på skärmen.
    private void RemoveText()
    {
        tmp.text = "";
    }
}

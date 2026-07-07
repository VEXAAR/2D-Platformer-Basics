using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PowerupsUI : MonoBehaviour
{
    // Gameobjects av ikonerna för att duplicera.
    [SerializeField] private GameObject jumpIcon;
    [SerializeField] private GameObject dashIcon;
    // RectTransforms av objekten som ska vara förälder till ikonerna.
    [SerializeField] private RectTransform jumpParent;
    [SerializeField] private RectTransform dashParent;
    // Positionen som nya ikoner ska utgå ifrån.
    [SerializeField] private Vector3 jumpIconPos;
    [SerializeField] private Vector3 dashIconPos;

    private PlayerController player;

    void Start()
    {
        player = FindAnyObjectByType<PlayerController>(); // Hittar objekt i scenträdet som har komponenten PlayerController.
        jumpIcon.SetActive(false);
        dashIcon.SetActive(false);
    }

    // Rensar och skapar om powerup UI ikonerna
    public void SetPowerupIcons()
    {
        foreach (Transform c in jumpParent.GetComponentsInChildren<Transform>()) // För varje barn (c) till "jumpParent" så kör den igenom denna loop.
        {
            if (c == jumpParent.transform) continue; // Om c (barnet) är föräldern (för GetComponentsInChildren() tar även med föräldern av någon anledning)
            Destroy(c.gameObject); // Tar bort barnet ur scenen.
        }
        foreach (Transform c in dashParent.GetComponentsInChildren<Transform>())
        {
            if (c == dashParent.transform) continue;
            Destroy(c.gameObject);
        }

        for (int i = 0; i < player.extraJumps; i++) // Loopar igenom en gång för hur många extra jumps spelaren har.
        {
            GameObject jump = Instantiate(jumpIcon, jumpParent); // Instantierar en kopia av "jumpIcon" objektet.
            jump.SetActive(true); // Aktiverar kopian (eftersom det orginella objektet är avaktiverat)
            jump.transform.position = jumpIconPos + new Vector3(35 * i, 0, 0); // Sätter kopians position till startpositionen + en offset beroende på hur många som finns redan.
        }
        for (int i = 0; i < player.maxDash; i++)
        {
            GameObject dash = Instantiate(dashIcon, dashParent);
            dash.SetActive(true);
            dash.transform.position = dashIconPos + new Vector3(35 * i, 0, 0);
        }
    }

    // När spelaren landar på marken blir alla powerups vita.
    public void ResetPowerups()
    {
        if (jumpParent.childCount > 0)
        {
            foreach (Image c in jumpParent.GetComponentsInChildren<Image>())
            {
                c.color = Color.white;
            }
        }
        if (dashParent.childCount > 0)
        {
            foreach (Image c in dashParent.GetComponentsInChildren<Image>())
            {
                c.color = Color.white;
            }
        }
    }

    // När spelaren använder ett dubbelhopp blir en av ikonerna mörk.
    public void UseJump()
    {
        foreach (Image c in jumpParent.GetComponentsInChildren<Image>())
        {
            if (c.color == Color.white) // Om ikonens färg är vit
            {
                c.color = Color.darkGray;
                return; // Sluta loopa igenom
            }
        }
    }

    // När spelaren använder en dash blir en av ikonerna mörk.
    public void UseDash()
    {
        foreach (Image c in dashParent.GetComponentsInChildren<Image>())
        {
            if (c.color == Color.white)
            {
                c.color = Color.darkGray;
                return;
            }
        }
    }
}

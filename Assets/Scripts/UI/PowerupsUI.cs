using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PowerupsUI : MonoBehaviour
{
    [SerializeField] private GameObject jumpIcon;
    [SerializeField] private GameObject dashIcon;
    [SerializeField] private RectTransform jumpParent;
    [SerializeField] private RectTransform dashParent;
    [SerializeField] private Vector3 jumpIconPos;
    [SerializeField] private Vector3 dashIconPos;

    private PlayerController player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        jumpIcon.SetActive(false);
        dashIcon.SetActive(false);
    }

    public void SetPowerupIcons()
    {
        foreach (Transform c in jumpParent.GetComponentsInChildren<Transform>())
        {
            if (c == jumpParent.transform) continue;
            Destroy(c.gameObject);
        }
        foreach (Transform c in dashParent.GetComponentsInChildren<Transform>())
        {
            if (c == dashParent.transform) continue;
            Destroy(c.gameObject);
        }

        for (int i = 0; i < player.extraJumps; i++)
        {
            GameObject jump = Instantiate(jumpIcon, jumpParent);
            jump.SetActive(true);
            jump.transform.position = jumpIconPos + new Vector3(35 * i, 0, 0);
        }
        for (int i = 0; i < player.maxDash; i++)
        {
            GameObject dash = Instantiate(dashIcon, dashParent);
            dash.SetActive(true);
            dash.transform.position = dashIconPos + new Vector3(35 * i, 0, 0);
            Debug.Log(dash);
        }
    }

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

    public void UseJump()
    {
        foreach (Image c in jumpParent.GetComponentsInChildren<Image>())
        {
            if (c.color == Color.white)
            {
                c.color = Color.darkGray;
                return;
            }
        }
    }

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

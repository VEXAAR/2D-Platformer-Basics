using Unity.VisualScripting;
using UnityEngine;

public class PowerupPickup : MonoBehaviour
{
    enum Powerup
    {
        DOUBLEJUMP,
        DASH,
        WALLJUMP,
    }

    [SerializeField] private Powerup powerupType;
    [SerializeField] private string announceText;

    private AnnouncementUI announce;

    void Start()
    {
        announce = FindAnyObjectByType<AnnouncementUI>();
    }


    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("Player")) // Om objektet som kom in i triggered ligger på lagret vid namn "Player"
        {
            PlayerController player = collider.GetComponent<PlayerController>();

            GivePowerup(player);

            Destroy(gameObject);
        }
    }

    void GivePowerup(PlayerController player)
    {
        switch (powerupType)
        {
            case Powerup.DASH:
                player.maxDash += 1;
                break;
            case Powerup.DOUBLEJUMP:
                player.extraJumps += 1;
                break;
            case Powerup.WALLJUMP:
                player.wallJumpAbility = true;
                break;
        }

        announce.SetText(announceText);
        FindAnyObjectByType<PowerupsUI>().SetPowerupIcons();
    }
}

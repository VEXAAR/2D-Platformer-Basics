using Unity.VisualScripting;
using UnityEngine;

public class PowerupPickup : MonoBehaviour
{
    // En enum (enumerator) är tekniskt sett bara en int, men man kan visa informationen med namn istället för siffror.
    enum Powerup
    {
        DOUBLEJUMP,
        DASH,
        WALLJUMP,
    }

    [SerializeField] private Powerup powerupType; // Vilken typ av powerup spelaren får när de plockar upp denna pickup, visat i formen av enumeratorn ovanför.
    [SerializeField] private string announceText; // Texten som ska visas när spelaren plockar upp denna pickup.

    private AnnouncementUI announce; // En referens till AnnouncementUI objektet.
    private PowerupsUI powerupUI; // En referens till PowerupsUI objektet

    void Start()
    {
        announce = FindAnyObjectByType<AnnouncementUI>(); // Hittar AnnouncementUI objektet i scenträdet.
        powerupUI = FindAnyObjectByType<PowerupsUI>();
    }

    // När en spelare kliver in i detta objekts collider area.
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("Player")) // Om objektet som kom in i triggered ligger på lagret vid namn "Player"
        {
            PlayerController player = collider.GetComponent<PlayerController>(); // Får en referens till spelarobjektets PlayerController komponent (scriptet)

            GivePowerup(player); // Kallar GivePowerup metoden med argumentet "player"

            Destroy(gameObject); // Raderar detta objekt ur trädet.
        }
    }

    // Ger spelaren en powerup beroende på vad för powerupType som blev satt.
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

        announce.SetText(announceText); // Visar en text på skärmen med AnnouncementUI.SetText() metoden.
        powerupUI.SetPowerupIcons(); // Matchar powerup UIn med mängden extra hopp och dashes som spelaren har.
    }
}

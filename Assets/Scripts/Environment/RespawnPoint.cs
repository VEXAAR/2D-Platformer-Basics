using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    [Tooltip("I vilken ordning alla checkpoints kommer i")]
    public int ordering; // Om spelaren aktiverar 0, sedan 1, sedan går till 0 igen så behålls deras respawn vid den med högre tal.
    // (Om alla är samma siffra så blir den spelaren var vid senast den man respawnar vid)

    private bool active; // Om denna RespawnPoint är aktiverad. (Om aktiveringsanimationen har spelats)

    // Blir kallad av Unity när ett annat objekt kommer in i detta objekts trigger collider.
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Player")) return; // Om objektet som kom in i trigger zonen inte är på lagret vid namn "Player"

        if (!active) // Om animationen inte har spelats.
        {
            Activate();
        }

        Health health = collision.GetComponentInChildren<Health>(); // Får komponenten "Health" hos ett barn till spelarobjektet.

        if (!health.respawnPoint || health.respawnPoint.ordering <= ordering) // Om spelaren inte har en RespawnPoint, eller om denna RespawnPoint har högre eller lika
        {                                                                     // ordering än den som spelaren har satt.
            health.respawnPoint = this; // Sätter spelarens respawnPoint till this (denna klass)
        }
        
    }

    // Spelar en aktiveringsanimation.
    void Activate()
    {
        active = true;
        return;
    }
}
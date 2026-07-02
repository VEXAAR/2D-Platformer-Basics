using UnityEngine;

public class Health : MonoBehaviour
{
    [Tooltip("Max hälsa för den som äger detta objekt")]
    [SerializeField] private int maxHealth = 10;

    private int health; // Spelarens nuvarande hälsa.
    public RespawnPoint respawnPoint; // Objektet spelaren spawnar vid om de dör.

    // Blir kallad i början av spelet. Sätter spelarens hälsa till max.
    void Start()
    {
        health = maxHealth;
    }

    // Blir kallad av andra objekt som kan göra skada.
    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0) // Om hälsan blir 0 eller mindre.
        {
            Die();
        }
    }

    // Blir kallad när health blir 0 (eller mindre)
    void Die()
    {
        if (GetComponent<PlayerController>())
        {
            GetComponent<PlayerAnimator>().Animate("die");
            GetComponent<PlayerController>().Die();
        }
    }

    // Om spelaren dör så respawnar dem, antingen vid den satta respawnPoint:en eller vid (0, 0, 0) Blir kallad av PlayerController när respawn animationen är klar.
    public void Respawn()
    {
        if (respawnPoint)
        {
            GetComponent<Rigidbody2D>().position = respawnPoint.transform.position;
        }

        else
        {
            GetComponent<Rigidbody2D>().position = Vector3.zero;
        }
    }
}

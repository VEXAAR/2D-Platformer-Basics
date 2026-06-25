using Unity.VisualScripting;
using UnityEditor.EditorTools;
using UnityEngine;

public class DamageZone : MonoBehaviour
{
    [Tooltip("How much damage the player receives from entering the area")]
    [SerializeField] private int damage = 10;

    // Blir kallad av Unity när ett annat objekt kommer in i detta objekts trigger collider.
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player")) // Om objektet som kom in i triggered ligger på lagret vid namn "Player"
        {
            GameObject player = collision.gameObject;
            player.GetComponentInChildren<Health>().TakeDamage(damage); // Får komponenten "Health" hos ett barn till spelarobjektet, och kallar TakeDamage hos den.
        }
    }
}

using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float move_speed = 1f;

    // När allt har uppdaterats (vanliga Update() funktionen har körts på alla spelobjekt) så kallas LateUpdate.
    void LateUpdate()
    {
        Vector2 pos = Vector2.Lerp(transform.position, player.position, move_speed * Time.deltaTime); // Interpererar en ny position mellan sin nuvarande och spelaren.
        transform.position = new Vector3(pos.x, pos.y, -10f); // Sätter sin position till den nya (viktigt att kamerans z-position alltid är -10)
    }
}

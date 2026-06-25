using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float move_speed = 1f;

    void LateUpdate()
    {
        Vector2 pos = Vector2.Lerp(transform.position, player.position, move_speed * Time.deltaTime);
        transform.position = new Vector3(pos.x, pos.y, -10f);
    }
}

using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Tooltip("Hur mycket backgrunden bör \"parallaxa\" horizontellt, mellan 0 - 1, där 1 betyder att den följer kameran, och 0 där den står stilla i världen")]
    [SerializeField] private float horizontalAmount = 0.5f;
    [Tooltip("Hur mycket backgrunden bör \"parallaxa\" vertikalt, mellan 0 - 1, där 1 betyder att den följer kameran, och 0 där den står stilla i världen")]
    [SerializeField] private float verticalAmount = 0.5f;

    private Vector3 startPos; // Bakgrundens position vid start
    private Camera cam; // Spelarkameran.

    void Start()
    {
        startPos = transform.position;

        cam = Camera.main; // Hittar kameran som används.
    }

    void LateUpdate()
    {
        float distX = cam.transform.position.x * horizontalAmount;
        float distY = cam.transform.position.y * verticalAmount;

        transform.position = new Vector3(startPos.x + distX, startPos.y + distY, startPos.z);
    }
}
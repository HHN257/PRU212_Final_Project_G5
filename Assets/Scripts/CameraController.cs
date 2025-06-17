using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float minY = 0f;
    public float maxY = 5f;

    public Transform target;
    public float followSpeed = 5f;

    public float zoomSpeed = 2f;
    public float minZoom = 3f;
    public float maxZoom = 6f;

    public Vector2 offset = new Vector2(0f, 2f); // Optional offset from the player

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;

        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            float clampedY = Mathf.Clamp(target.position.y + offset.y, minY, maxY);
            Vector3 desiredPosition = new Vector3(
                target.position.x + offset.x,
                clampedY,
                -10f
            );
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        }

        // Zoom control
        if (Input.GetKey(KeyCode.Z))
        {
            cam.orthographicSize -= zoomSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.X))
        {
            cam.orthographicSize += zoomSpeed * Time.deltaTime;
        }

        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
    }
}

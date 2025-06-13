using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;            // Player to follow
    public float followSpeed = 5f;      // Follow smoothness
    public float zoomSpeed = 2f;        // Zoom smoothness
    public float minZoom = 3f;          // Closest zoom
    public float maxZoom = 6f;          // Farthest zoom
    public float fixedYOffset = 4f;     // Fixed vertical offset

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
        // Smooth follow on X only
        if (target != null)
        {
            Vector3 targetPos = new Vector3(target.position.x, fixedYOffset, -10f);
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
        }

        // Zoom in/out with Z and X
        if (Input.GetKey(KeyCode.Z))
        {
            cam.orthographicSize -= zoomSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.X))
        {
            cam.orthographicSize += zoomSpeed * Time.deltaTime;
        }

        // Clamp zoom
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
    }
}

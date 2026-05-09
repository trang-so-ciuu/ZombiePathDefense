using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement")]
    public float dragSpeed = 0.5f;

    [Header("Zoom")]
    public float zoomSpeed = 200f;
    public float minY = 10f;
    public float maxY = 150f;

    [Header("Map Limit")]
    public float mapWidth = 40f;
    public float mapLength = 40f;

    private Vector3 dragOrigin;
    private Vector3 dragStartCamPos;
    [SerializeField] private Camera cam;

    void Update()
    {
        MoveByMouseDrag();
        Zoom();
        ClampPosition();
    }

    // ===== KÉO CHUỘT TRÁI =====
    void MoveByMouseDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
            dragStartCamPos = transform.position;
            return;
        }

        if (!Input.GetMouseButton(0)) return;

        Vector3 difference = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
        Vector3 move = new Vector3(-difference.x * dragSpeed * transform.position.y,
                                    0,
                                   -difference.y * dragSpeed * transform.position.y);
        transform.position = dragStartCamPos + move;
    }

    // ===== ZOOM LĂN CHUỘT =====
    void Zoom()
    {
        float zoom = cam.orthographicSize;
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0f) return;

        zoom -= scroll * zoomSpeed * Time.deltaTime;
        zoom = Mathf.Clamp(zoom, minY, maxY);
        cam.orthographicSize = zoom;
    }

    // ===== GIỚI HẠN MAP =====
    void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -mapWidth / 2, mapWidth / 2);
        pos.z = Mathf.Clamp(pos.z, -mapLength / 2, mapLength / 2);
        transform.position = pos;
    }
}
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Control")]
    public bool clickToMoveCamera = false;
    public bool canZoom = true;
    public float sensitivity = 5f;
    public Vector2 cameraLimit = new Vector2(-45, 40);

    [Header("Follow Settings")]
    public float followSmoothTimeXZ = 0.1f;
    public float followSmoothTimeY = 0.2f;

    [Header("Collision Settings")]
    public float collisionRadius = 0.3f;
    public float minDistance = 0.5f;
    public LayerMask collisionMask;

    private float mouseX;
    private float mouseY;
    private float offsetDistanceY;

    private Transform player;
    private Vector3 velocity = Vector3.zero;

    private Vector3 desiredCameraOffset; // постоянный offset от игрока

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;

        // Изначальный offset от игрока
        desiredCameraOffset = transform.position - player.position;
        offsetDistanceY = desiredCameraOffset.y;

        if (!clickToMoveCamera)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        // Zoom мышью
        if (canZoom && Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            Camera.main.fieldOfView -= Input.GetAxis("Mouse ScrollWheel") * sensitivity * 2f;
            Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, 20f, 90f);
        }

        // Проверка клика для движения камеры
        if (clickToMoveCamera && Input.GetAxisRaw("Fire2") == 0)
            return;

        // Поворот камеры мышью
        mouseX += Input.GetAxis("Mouse X") * sensitivity;
        mouseY += Input.GetAxis("Mouse Y") * sensitivity;
        mouseY = Mathf.Clamp(mouseY, cameraLimit.x, cameraLimit.y);

        transform.rotation = Quaternion.Euler(-mouseY, mouseX, 0);
    }

    void LateUpdate()
    {
        // Целевая позиция камеры без коллизий
        Vector3 targetPosition = player.position + desiredCameraOffset;

        // --- Smooth Follow ---
        float smoothX = Mathf.SmoothDamp(transform.position.x, targetPosition.x, ref velocity.x, followSmoothTimeXZ);
        float smoothZ = Mathf.SmoothDamp(transform.position.z, targetPosition.z, ref velocity.z, followSmoothTimeXZ);
        float smoothY = Mathf.SmoothDamp(transform.position.y, targetPosition.y, ref velocity.y, followSmoothTimeY);

        Vector3 smoothPosition = new Vector3(smoothX, smoothY, smoothZ);

        // --- Camera Collision ---
        Vector3 direction = smoothPosition - player.position;
        float distance = direction.magnitude;
        direction.Normalize();

        RaycastHit hit;
        if (Physics.SphereCast(player.position, collisionRadius, direction, out hit, distance, collisionMask))
        {
            float adjustedDistance = Mathf.Clamp(hit.distance - collisionRadius, minDistance, distance);
            smoothPosition = player.position + direction * adjustedDistance;
        }

        transform.position = smoothPosition;
    }
}
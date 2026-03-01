using UnityEngine;

public class SC_CameraCollision : MonoBehaviour
{
    [SerializeField] private Transform referenceTransform;
    [SerializeField] private float collisionOffset = 0.3f;
    [SerializeField] private float cameraSpeed = 15f;

    private Vector3 defaultPos;
    private Vector3 directionNormalized;
    private Transform parentTransform;
    private float defaultDistance;

    void Start()
    {
        defaultPos = transform.localPosition;
        directionNormalized = defaultPos.normalized;
        parentTransform = transform.parent;
        defaultDistance = Vector3.Distance(defaultPos, Vector3.zero);
    }

    void LateUpdate()
    {
        Vector3 currentPos = defaultPos;
        RaycastHit hit;

        Vector3 dirTmp = parentTransform.TransformPoint(defaultPos) - referenceTransform.position;

        if (Physics.SphereCast(referenceTransform.position, collisionOffset, dirTmp, out hit, defaultDistance))
        {
            currentPos = directionNormalized * (hit.distance - collisionOffset);
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, currentPos, Time.deltaTime * cameraSpeed);
    }
}
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 6f;
    public Vector3 offset = new Vector3(0f, 1.5f, -10f); // Luôn để Z = -10

    [Header("Nhìn trước bục kế tiếp")]
    [Range(0f, 1f)]
    [Tooltip("0 = chỉ theo player, 1 = kéo hẳn về giữa player và bục tiếp theo")]
    public float lookAheadWeight = 0.35f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 focusPoint = target.position;

        // Dùng PeekNextPlatform (không có side-effect) thay vì
        // GetNextTargetPlatform (có thể spawn bục mỗi lần gọi)
        Transform nextPlatform = PlatformSpawner.Instance != null
            ? PlatformSpawner.Instance.PeekNextPlatform()
            : null;

        if (nextPlatform != null)
        {
            focusPoint = Vector3.Lerp(target.position, nextPlatform.position, lookAheadWeight);
        }

        Vector3 desiredPosition = new Vector3(focusPoint.x, focusPoint.y, 0f) + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraResolutionScaler : MonoBehaviour
{
    [Header("Kích thước chuẩn thiết kế")]
    public float referenceAspectWidth = 1080f;
    public float referenceAspectHeight = 1920f;
    public float defaultOrthographicSize = 8f;

    [Header("Biên an toàn (world units) cộng thêm vào bề ngang")]
    public float extraMarginWidth = 0.5f;

    private Camera cam;
    private int lastScreenWidth;
    private int lastScreenHeight;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    IEnumerator Start()
    {
        // Đợi 1 frame để Screen.width/height ổn định trên thiết bị thật
        // (Awake chạy quá sớm trên nhiều máy, đặc biệt máy có safe-area/notch)
        yield return null;
        UpdateCameraSize();
    }

    void LateUpdate()
    {
        // Chỉ tính lại khi thật sự đổi resolution (xoay máy, resize cửa sổ...),
        // tránh tính mỗi frame gây tốn kém vô ích.
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            UpdateCameraSize();
        }
    }

    void UpdateCameraSize()
    {
        if (cam == null) cam = GetComponent<Camera>();

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        float targetAspect = referenceAspectWidth / referenceAspectHeight;
        float targetHalfWidth = defaultOrthographicSize * targetAspect + extraMarginWidth;

        float currentAspect = (float)Screen.width / Screen.height;
        float sizeToFitWidth = targetHalfWidth / currentAspect;

        // Không bao giờ zoom nhỏ hơn size mặc định (đảm bảo luôn thấy đủ
        // chiều dọc tham chiếu, kể cả trên tablet có tỉ lệ rộng hơn 1080:1920)
        cam.orthographicSize = Mathf.Max(sizeToFitWidth, defaultOrthographicSize);
    }
}
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI scoreText;

    private int currentScore = 0;

    [Header("Gán GameObject Popup vào đây")]
    public GameObject popupLevelUp;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateScoreDisplay();
    }

    // Cộng điểm khi nhảy trúng bục
    public void AddScore(int amount = 1)
    {
        currentScore += amount;
        UpdateScoreDisplay();
    }

    // Cập nhật text điểm số ở giữa màn hình
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
    }

    // Sự kiện khi bấm vào nút Face ID góc dưới trái
    public void OnFaceIDClicked()
    {
        Debug.Log("Face ID Button Clicked!");
        // Bạn có thể thêm code mở popup thông tin/skin nhân vật ở đây
    }

    // Hàm mở popup
    public void OpenPopup()
    {
        if (popupLevelUp != null)
        {
            popupLevelUp.SetActive(true);
            // Nếu muốn nhân vật tạm dừng nhảy trong lúc xem popup:
            // Time.timeScale = 0f;
        }
    }

    // Hàm đóng popup
    public void ClosePopup()
    {
        if (popupLevelUp != null)
        {
            popupLevelUp.SetActive(false);
            // Time.timeScale = 1f; // Trả lại thời gian bình thường nếu có tạm dừng
        }
    }

    // Hàm chuyển đổi bật/tắt khi bấm nút FaceID
    public void TogglePopup()
    {
        if (popupLevelUp != null)
        {
            bool isCurrentActive = popupLevelUp.activeSelf;
            popupLevelUp.SetActive(!isCurrentActive);
        }
    }
}
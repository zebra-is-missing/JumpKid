using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public PopUpManager popUpManager;

    // Biến static lưu lại trạng thái xuyên suốt quá trình load lại scene
    public static bool autoStartOnReload = false;

    [Header("Panels")]
    public GameObject mainMenuPanel; // Kéo MenuPanel vào đây
    public GameObject gameOverPanel; // Kéo GameOverPanel vào đây

    [Header("UI References")]
    public TextMeshProUGUI scoreText;

    [Header("Gán UI Image hiển thị hình")]
    [SerializeField] private Image faceIdImage;
    [SerializeField] private Image currentCharacterImage;
    public Image popupCharacterImage;
    public GameObject popupLevelUp;

    [Header("Cấu hình Level")]
    public int stepsPerLevel = 1;
    public List<LevelSkin> levels = new List<LevelSkin>();

    private int currentScore = 0;
    private int currentLevelIndex = -1;
    private int totalSpawnedPlatforms = 0;

    public bool isPlaying = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Ẩn panel Game Over lúc khởi động
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Kiểm tra xem có phải vừa bấm YES từ Game Over không
        if (autoStartOnReload)
        {
            autoStartOnReload = false; // Reset cờ
            StartGame(); // Bỏ qua menu, vào game ngay
        }
        else
        {
            // Vào game lần đầu bình thường -> Hiện menu chính
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
            }
        }

        UpdateScoreDisplay();
        CheckAndUpdateLevel();
    }

    public void StartGame()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        isPlaying = true;
    }

    // ==========================================
    // XỬ LÝ NÚT YES VÀ NO
    // ==========================================

    // Gán hàm này vào nút YES (ReplayButton) -> Chơi lại ngay lập tức
    public void OnReplayYesClicked()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();

        autoStartOnReload = true; // Bật cờ để scene mới biết cần vào game luôn
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    // Gán hàm này vào nút NO (NoReplayButton) -> Nhảy về Menu chính
    public void OnReplayNoClicked()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();

        autoStartOnReload = false; // Đảm bảo cờ tắt để khi load xong sẽ dừng ở Main Menu
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void ShowGameOver()
    {
        isPlaying = false;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayGameOver();
        }


        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void AddScore(int amount = 1)
    {
        currentScore += amount;
        UpdateScoreDisplay();
        CheckAndUpdateLevel();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
    }

    private void CheckAndUpdateLevel()
    {
        if (levels == null || levels.Count == 0) return;

        int targetIndex = currentScore / stepsPerLevel;
        targetIndex = Mathf.Clamp(targetIndex, 0, levels.Count - 1);
        int popupIndex = currentScore % stepsPerLevel;
        // Gọi popup
        if(popupIndex == 0 && currentLevelIndex >= 0)
        {
            popUpManager.ShowLevelUpPopUp(levels[currentLevelIndex], levels[currentLevelIndex + 1]);
        }

        if (targetIndex != currentLevelIndex)
        {
            currentLevelIndex = targetIndex;
            ApplyLevelSkin(levels[currentLevelIndex]);
        }
    }

    private void AnimateFaceID()
    {
        if(faceIdImage == null) return;
        faceIdImage.transform.DOScale(1.2f, 0.2f).SetLoops(2, LoopType.Yoyo);
    }

    private void ApplyLevelSkin(LevelSkin skin)
    {
        if (faceIdImage != null /*&& skin.faceIdSprite != null*/)
        {
            faceIdImage.sprite = skin.faceIdSprite;
            AnimateFaceID();
        }

        if(currentCharacterImage != null)
        {
            currentCharacterImage.sprite = skin.popupSprite;
        }
        PlayerController.Instance.UpdateSkin(skin.characterIdle, skin.characterJump, skin.characterLand);
        /*
        if (popupCharacterImage != null && skin.popupSprite != null)
        {
            popupCharacterImage.sprite = skin.popupSprite;
        }

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.UpdateSkin(skin.characterIdle, skin.characterJump, skin.characterLand);
        }
        */
    }

    public Sprite GetCurrentPlatformSprite()
    {
        if (levels == null || levels.Count == 0) return null;

        int platformLevelIndex = totalSpawnedPlatforms / stepsPerLevel;
        platformLevelIndex = Mathf.Clamp(platformLevelIndex, 0, levels.Count - 1);

        LevelSkin skin = levels[platformLevelIndex];
        if (skin.platformSprites == null || skin.platformSprites.Length == 0) return null;

        int spriteIndexInLevel = (totalSpawnedPlatforms % stepsPerLevel) % skin.platformSprites.Length;
        totalSpawnedPlatforms++;

        return skin.platformSprites[spriteIndexInLevel];
    }

    public void OnFaceIDClicked()
    {
        TogglePopup();
        //popUpManager.ToggleLevelPopup();
    }

    public void OpenPopup()
    {
        if (popupLevelUp != null) popupLevelUp.SetActive(true);
    }

    public void ClosePopup()
    {
        if (popupLevelUp != null) popupLevelUp.SetActive(false);
    }

    public void TogglePopup()
    {
        if (popupLevelUp != null)
        {
            popupLevelUp.SetActive(!popupLevelUp.activeSelf);
        }
    }
}
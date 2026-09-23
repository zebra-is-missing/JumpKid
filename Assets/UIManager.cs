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

    public static bool autoStartOnReload = false;

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject gameOverPanel;

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
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (popupLevelUp != null)
        {
            popupLevelUp.SetActive(false);
        }

        if (autoStartOnReload)
        {
            autoStartOnReload = false;
            StartGame();
        }
        else
        {
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

    public void OnReplayYesClicked()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();

        autoStartOnReload = true;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void OnReplayNoClicked()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();

        autoStartOnReload = false;
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

        // Tính toán level tương ứng với điểm hiện tại
        int targetIndex = currentScore / stepsPerLevel;
        targetIndex = Mathf.Clamp(targetIndex, 0, levels.Count - 1);

        // 1. Khi vừa vào game (Start), khởi tạo skin ban đầu mà không mở popup
        if (currentLevelIndex == -1)
        {
            currentLevelIndex = targetIndex;
            ApplyLevelSkin(levels[currentLevelIndex]);
            return;
        }

        // 2. Chỉ kích hoạt Popup và áp dụng Skin mới khi ĐẠT ĐỦ ĐIỂM LÊN LEVEL MỚI
        if (targetIndex > currentLevelIndex)
        {
            int oldIndex = currentLevelIndex;
            currentLevelIndex = targetIndex;

            // Gọi hiển thị popup thăng cấp chuyển từ skin cũ sang skin mới
            if (popUpManager != null)
            {
                popUpManager.ShowLevelUpPopUp(levels[oldIndex], levels[currentLevelIndex]);
            }

            ApplyLevelSkin(levels[currentLevelIndex]);
        }
    }

    private void AnimateFaceID()
    {
        if (faceIdImage == null) return;
        faceIdImage.transform.DOScale(1.2f, 0.2f).SetLoops(2, LoopType.Yoyo);
    }

    private void ApplyLevelSkin(LevelSkin skin)
    {
        if (faceIdImage != null)
        {
            faceIdImage.sprite = skin.faceIdSprite;
            AnimateFaceID();
        }

        if (currentCharacterImage != null)
        {
            currentCharacterImage.sprite = skin.popupSprite;
        }

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.UpdateSkin(skin.characterIdle, skin.characterJump, skin.characterLand);
        }
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
            bool nextState = !popupLevelUp.activeSelf;
            popupLevelUp.SetActive(nextState);

            CanvasGroup cg = popupLevelUp.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.blocksRaycasts = nextState;
                cg.interactable = nextState;
            }
        }
    }
}
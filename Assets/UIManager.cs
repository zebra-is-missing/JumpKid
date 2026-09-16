using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI scoreText;

    [Header("Gán UI Image hiển thị hình")]
    public Image faceIdImage;
    public Image popupCharacterImage;
    public GameObject popupLevelUp;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateScoreDisplay();
    }

    public void AddScore(int amount = 1)
    {
        currentScore += amount;
        UpdateScoreDisplay();
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

        if (targetIndex != currentLevelIndex)
        {
            currentLevelIndex = targetIndex;
            ApplyLevelSkin(levels[currentLevelIndex]);
    }

    private void ApplyLevelSkin(LevelSkin skin)
    {
        if (faceIdImage != null && skin.faceIdSprite != null)
        {
            faceIdImage.sprite = skin.faceIdSprite;
        }

        if (popupCharacterImage != null && skin.popupSprite != null)
    {
            popupCharacterImage.sprite = skin.popupSprite;
        }

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.UpdateSkin(skin.characterIdle, skin.characterJump);
        }
        }
    }

    public void OnFaceIDClicked()
    {
        TogglePopup();
    }

    public void OpenPopup()
        {
        if (popupLevelUp != null) popupLevelUp.SetActive(true);
        }
    }

    public void TogglePopup()
    {
        if (popupLevelUp != null)
        {
            popupLevelUp.SetActive(!popupLevelUp.activeSelf);
        }
    }
}
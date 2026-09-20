using UnityEngine;
using System.Collections;
using DG.Tweening;

public class PopUpManager : MonoBehaviour
{
    private GameInputManager gameInputManager;
    private UIManager uiManager;

    [Header("Animation")]
    [SerializeField] private float time = 0.5f;
    [SerializeField] private float offsetY = -200f;

    private CanvasGroup popupCanvasGroup;
    private RectTransform popupRect;
    private Vector2 originalPos;
    /*
    private void AnimationFadeOn()
    {
        popupRect.anchoredPosition = originalPos + new Vector2(0, offsetY);
        popupCanvasGroup.alpha = 0;
    }
    */

    public void Start()
    {
        uiManager = UIManager.Instance;
        gameInputManager = GameInputManager.Instance;

        popupRect = uiManager.popupLevelUp.GetComponent<RectTransform>();
        popupCanvasGroup = uiManager.popupLevelUp.GetComponent<CanvasGroup>();
        if(popupCanvasGroup == null)
        {
            popupCanvasGroup = uiManager.popupLevelUp.AddComponent<CanvasGroup>();
        }
        originalPos = popupRect.anchoredPosition;
    }

    public void ShowLevelUpPopUp(LevelSkin oldSkin, LevelSkin newSkin)
    {
        StartCoroutine(PlayLevelUpSequence(oldSkin, newSkin));
    }

    private IEnumerator PlayLevelUpSequence(LevelSkin oldSkin, LevelSkin newSkin)
    {
        if(uiManager == null)
        {
            yield break;
        }

        if(uiManager.popupLevelUp == null || oldSkin == null || newSkin == null) yield break;

        // Đóng băng game
        gameInputManager.LockInput();
        Time.timeScale = 0f;

        // Hiển thị popup với hình cũ
        uiManager.popupCharacterImage.sprite = oldSkin.popupSprite;
        uiManager.popupLevelUp.SetActive(true);

        // Chờ tầm 0.5 giây
        yield return new WaitForSecondsRealtime(0.5f);

        // Chuyển skin
        Debug.Log("ApplyLevelSkin set NEW");
        uiManager.popupCharacterImage.sprite = newSkin.popupSprite;

        // Thêm hiệu ứng, chưa thêm đâu, cẩn thận
        yield return new WaitForSecondsRealtime(0.5f);

        // Chờ người chơi bấm đóng popup
        yield return new WaitForSecondsRealtime(2f);
        uiManager.popupLevelUp.SetActive(false);

        Time.timeScale = 1f;
        gameInputManager.UnlockInput();
    }
}

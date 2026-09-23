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
    
    private void AnimationFadeOn()
    {
        // Reset trạng thái ban đầu
        popupRect.anchoredPosition = originalPos + new Vector2(0, offsetY);
        popupCanvasGroup.alpha = 0f;

        Sequence seq = DOTween.Sequence();

        seq.Join(popupRect.DOAnchorPos(originalPos, time).SetEase(Ease.OutBack));
        seq.Join(popupCanvasGroup.DOFade(1f, time));
        seq.SetUpdate(true); // vẫn chạy khi Time.timeScale = 0
    }
    
    private IEnumerator AnimationFadeOff()
    {
        Sequence seq = DOTween.Sequence();

        seq.Join(popupRect.DOAnchorPos(originalPos + new Vector2(0, offsetY), time));
        seq.Join(popupCanvasGroup.DOFade(0f, time));
        seq.SetUpdate(true);
        yield return seq.WaitForCompletion();
    }

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

        AnimationFadeOn();

        // Chờ tầm 0.5 giây
        yield return new WaitForSecondsRealtime(0.5f);

        // Chuyển skin
        uiManager.popupCharacterImage.sprite = newSkin.popupSprite;

        // Thêm hiệu ứng nảy khi level up
        uiManager.popupCharacterImage.transform.localScale = Vector3.one;
        uiManager.popupCharacterImage.transform.DOPunchScale(Vector3.one * 0.1f, 0.5f, 8, 0.8f).SetUpdate(true);
        // DOPucnhScale(độ mạnh, thời gian, rung, đàn hồi)
        yield return new WaitForSecondsRealtime(0.5f);

        // Chờ người chơi bấm đóng popup
        yield return new WaitForSecondsRealtime(2f);
        yield return AnimationFadeOff();
        uiManager.popupLevelUp.SetActive(false);

        popupCanvasGroup.alpha = 1f;
        popupRect.anchoredPosition = originalPos;

        Time.timeScale = 1f;
        gameInputManager.UnlockInput();
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using TMPro;

public class PopUpManager : MonoBehaviour
{
    private GameInputManager gameInputManager;
    private UIManager uiManager;

    [Header("Popup Skin Animation")]
    [SerializeField] private float time = 0.5f;
    [SerializeField] private float offsetY = -200f;
    [SerializeField] private GameObject inputBlocker;

    [Header("Story Animation")]
    [SerializeField] private float storyFadeTime = 0.4f;
    [SerializeField] private float textSpeed = 0.035f;

    private CanvasGroup popupCanvasGroup;
    private RectTransform popupRect;
    private Vector2 originalPos;
    private CanvasGroup storyCanvasGroup;

    // Biến tương tác Visual Novel
    private bool isStoryClicked = false;
    private bool isTyping = false;

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

        // Setup Popup Skin
        if (uiManager.popupLevelUp != null)
        {
            popupRect = uiManager.popupLevelUp.GetComponent<RectTransform>();
            popupCanvasGroup = uiManager.popupLevelUp.GetComponent<CanvasGroup>();
            if (popupCanvasGroup == null)
            {
                popupCanvasGroup = uiManager.popupLevelUp.AddComponent<CanvasGroup>();
            }
            originalPos = popupRect.anchoredPosition;
        }

        // Setup Story Panel
        if (uiManager.storyPanel != null)
        {
            storyCanvasGroup = uiManager.storyPanel.GetComponent<CanvasGroup>();
            if (storyCanvasGroup == null)
            {
                storyCanvasGroup = uiManager.storyPanel.AddComponent<CanvasGroup>();
            }
            uiManager.storyPanel.SetActive(false);

            if (uiManager.storyClickArea != null)
            {
                uiManager.storyClickArea.onClick.RemoveAllListeners();
                uiManager.storyClickArea.onClick.AddListener(OnStoryScreenClicked);
            }
        }

        if (inputBlocker != null) inputBlocker.SetActive(false);
    }
    private void OnStoryScreenClicked()
    {
        isStoryClicked = true;
    }

    public void ShowLevelUpPopUp(LevelSkin oldSkin, LevelSkin newSkin)
    {
        StartCoroutine(PlayLevelUpSequence(oldSkin, newSkin));
    }

    private IEnumerator PlayLevelUpSequence(LevelSkin oldSkin, LevelSkin newSkin)
    {
        if (uiManager == null || uiManager.popupLevelUp == null || oldSkin == null || newSkin == null)
            yield break;

        // Khóa Input & Tạm dừng game
        if (inputBlocker != null) inputBlocker.SetActive(true);
        if (gameInputManager != null) gameInputManager.LockInput();
        Time.timeScale = 0f;

        // POPUP FADE IN 
        if (uiManager.popupCharacterImage != null)
        {
            uiManager.popupCharacterImage.sprite = oldSkin.popupSprite;
        }

        uiManager.popupLevelUp.SetActive(true);
        AnimationFadeOn();

        // Chờ Popup trượt/hiện lên hoàn tất
        yield return new WaitForSecondsRealtime(time);

        //HIỆN CỐT TRUYỆN ĐÈ LÊN POPUP
        StoryData story = newSkin.storyData;
        if (story != null && story.scenes != null && story.scenes.Count > 0 && uiManager.storyPanel != null)
        {
            uiManager.storyPanel.SetActive(true);
            storyCanvasGroup.alpha = 0f;

            // Đảm bảo ảnh phủ fade luôn tàng hình lúc ban đầu
            if (uiManager.storyBgFadeImage != null)
            {
                Color c = uiManager.storyBgFadeImage.color;
                c.a = 0f;
                uiManager.storyBgFadeImage.color = c;
            }

            // Gán ảnh cho Scene đầu tiên
            if (uiManager.storyBgImage != null && story.scenes[0].sceneImage != null)
            {
                uiManager.storyBgImage.sprite = story.scenes[0].sceneImage;
            }

            // FADE IN BAN ĐẦU (Chỉ chạy 1 lần cho Scene đầu)
            yield return storyCanvasGroup.DOFade(1f, storyFadeTime).SetUpdate(true).WaitForCompletion();

            // Duyệt qua từng Scene
            for (int s = 0; s < story.scenes.Count; s++)
            {
                StorySceneNode sceneNode = story.scenes[s];

                // TỪ SCENE THỨ 2 TRỞ ĐI: Fade In đè ảnh mới lên trên ảnh cũ
                if (s > 0)
                {
                    if (uiManager.storyBgFadeImage != null && sceneNode.sceneImage != null)
                    {
                        uiManager.storyBgFadeImage.sprite = sceneNode.sceneImage;

                        // Fade in ảnh mới đè lên ảnh cũ
                        yield return uiManager.storyBgFadeImage.DOFade(1f, storyFadeTime)
                            .SetUpdate(true)
                            .WaitForCompletion();

                        // Chuyển ảnh mới xuống làm ảnh nền chính và giấu ảnh fade đi
                        uiManager.storyBgImage.sprite = sceneNode.sceneImage;
                        Color c = uiManager.storyBgFadeImage.color;
                        c.a = 0f;
                        uiManager.storyBgFadeImage.color = c;
                    }
                    else if (uiManager.storyBgImage != null && sceneNode.sceneImage != null)
                    {
                        // Phương án phụ nếu bạn chưa gán storyBgFadeImage
                        uiManager.storyBgImage.sprite = sceneNode.sceneImage;
                    }
                }

                // Chạy từng câu thoại trong Scene hiện tại
                for (int d = 0; d < sceneNode.dialogues.Count; d++)
                {
                    string targetSentence = sceneNode.dialogues[d];
                    yield return StartCoroutine(PlayDialogueRoutine(targetSentence));

                    yield return new WaitForSecondsRealtime(0.15f);

                    // Chờ người chơi click
                    isStoryClicked = false;
                    yield return new WaitUntil(() => isStoryClicked);
                    isStoryClicked = false;
                }

                // Dọn text sau khi đọc xong mỗi scene
                uiManager.storyText.text = string.Empty;
                uiManager.storyText.ForceMeshUpdate();
            }

            // HẾT SCENE CUỐI CÙNG: FADE OUT TOÀN BỘ CỐT TRUYỆN
            yield return storyCanvasGroup.DOFade(0f, storyFadeTime).SetUpdate(true).WaitForCompletion();
            uiManager.storyPanel.SetActive(false);

            // Xóa sprite khỏi bộ nhớ tạm
            uiManager.storyBgImage.sprite = null;
            if (uiManager.storyBgFadeImage != null) uiManager.storyBgFadeImage.sprite = null;
        }

        // THỰC HIỆN ANIMATION SKIN MỚI TRÊN POPUP
        yield return new WaitForSecondsRealtime(0.2f);

        // Đổi hình sang Skin mới + Nảy hiệu ứng Punch
        if (uiManager.popupCharacterImage != null)
        {
            uiManager.popupCharacterImage.sprite = newSkin.popupSprite;
            uiManager.popupCharacterImage.transform.localScale = Vector3.one;
            uiManager.popupCharacterImage.transform
                .DOPunchScale(Vector3.one * 0.2f, 0.5f, 8, 0.8f)
                .SetUpdate(true);
        }

        // Dừng lại 1.5s - 2s
        yield return new WaitForSecondsRealtime(1.8f);

        // POPUP FADE OUT VÀ MỞ KHÓA GAME
        yield return AnimationFadeOff();
        uiManager.popupLevelUp.SetActive(false);

        popupCanvasGroup.alpha = 1f;
        popupRect.anchoredPosition = originalPos;

        if (uiManager != null)
        {
            uiManager.UpdateFaceID(newSkin);
        }

        Time.timeScale = 1f;
        yield return null;

        if (inputBlocker != null) inputBlocker.SetActive(false);
        if (gameInputManager != null) gameInputManager.UnlockInput();
    }

    private IEnumerator PlayDialogueRoutine(string sentence)
    {
        isTyping = true;
        isStoryClicked = false;
        uiManager.storyText.text = "";

        for (int i = 0; i < sentence.Length; i++)
        {
            // Nếu người chơi click khi đang gõ chữ -> Hiện toàn bộ câu lập tức
            if (isStoryClicked)
            {
                uiManager.storyText.text = sentence;
                isStoryClicked = false;
                break;
            }

            uiManager.storyText.text += sentence[i];
            yield return new WaitForSecondsRealtime(textSpeed);
        }

        isTyping = false;
    }
}

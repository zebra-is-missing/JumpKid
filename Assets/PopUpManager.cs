using UnityEngine;
using System.Collections;

public class PopUpManager : MonoBehaviour
{
    private UIManager uiManager;
    public void Start()
    {
        uiManager = UIManager.Instance;
    }

    public void ShowLevelUpPopUp(LevelSkin oldSkin, LevelSkin newSkin)
    {
        StartCoroutine(PlayLevelUpSequence(oldSkin, newSkin));
    }

    private IEnumerator PlayLevelUpSequence(LevelSkin oldSkin, LevelSkin newSkin)
    {
        if(uiManager == null)
        {
            Debug.LogError("UIManager is NULL");
            yield break;
        }

        if(uiManager.popupLevelUp == null || oldSkin == null || newSkin == null) yield break;

        // Hiển thị popup với hình cũ
        uiManager.popupCharacterImage.sprite = oldSkin.popupSprite;
        uiManager.popupLevelUp.SetActive(true);

        // Chờ tầm 0.5 giây
        yield return new WaitForSeconds(0.5f);

        // Chuyển skin
        uiManager.popupCharacterImage.sprite = newSkin.popupSprite;

        // Thêm hiệu ứng, chưa thêm đâu, cẩn thận
        yield return new WaitForSeconds(0.5f);

        // Chờ người chơi bấm đóng popup
        yield return new WaitForSeconds(2f);
        uiManager.popupLevelUp.SetActive(false);
    }
}

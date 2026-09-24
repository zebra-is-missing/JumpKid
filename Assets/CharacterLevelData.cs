using UnityEngine;

[System.Serializable]
public class LevelSkin
{
    [Tooltip("Level tương ứng")]
    public int level = 1;

    [Header("Sprite Nhân vật")]
    public Sprite characterIdle;    // Char đứng
    public Sprite characterJump;    // Char nhảy
    public Sprite characterLand;    // Char đáp

    [Header("Sprite UI")]
    public Sprite faceIdSprite;       // Ảnh Face ID góc trái
    public Sprite popupSprite;        // Ảnh toàn thân hiển thị trong Popup
    [Header("Story Dialog")]
    public Sprite storyIllustration; // Ảnh minh họa cốt truyện (nếu có)
    [TextArea(3, 5)]
    public string storyText;         // Đoạn text cốt truyện tương ứng mốc này
    [Header("Sprite Platform của Level này")]
    [Tooltip("Các hình ảnh bục xuất hiện trong level này")]
    public Sprite[] platformSprites;
}
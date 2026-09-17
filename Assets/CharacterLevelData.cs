using UnityEngine;

[System.Serializable]
public class LevelSkin
{
    [Tooltip("Level tương ứng")]
    public int level = 1;

    [Header("Sprite Nhân vật")]
    public Sprite characterIdle;
    public Sprite characterJump;
    public Sprite characterLand;

    [Header("Sprite UI")]
    public Sprite faceIdSprite;       // Ảnh Face ID góc trái
    public Sprite popupSprite;        // Ảnh toàn thân hiển thị trong Popup

    [Header("Sprite Platform của Level này")]
    [Tooltip("Các hình ảnh bục xuất hiện trong level này")]
    public Sprite[] platformSprites;
}
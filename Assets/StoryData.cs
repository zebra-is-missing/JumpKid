using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StorySceneNode
{
    [Header("Ảnh nền cho Scene này")]
    public Sprite sceneImage;

    [Header("Danh sách các câu thoại trong Scene này")]
    [TextArea(2, 4)]
    public List<string> dialogues = new List<string>();
}

[CreateAssetMenu(fileName = "StoryData_Level_", menuName = "Game Data/Story Data")]
public class StoryData : ScriptableObject
{
    public List<StorySceneNode> scenes = new List<StorySceneNode>();
}
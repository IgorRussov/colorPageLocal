using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelFrameUI : MonoBehaviour
{
    public Image levelPreviewImage;
    private int levelIndex;

    public void InitializeLevelFrame(LevelData levelData, int index)
    {
        Texture2D tex = levelData.TextureForLevelsList;
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
        levelPreviewImage.sprite = sprite;
        levelIndex = index;
    }

    public void OnPointerClick(PointerEventData eventData) 
    {
        LoadBoundLevel();
    }

    public void LoadBoundLevel()
    {
        GameObject.FindObjectOfType<LevelManager>().LoadLevel(levelIndex);

    }

    
}

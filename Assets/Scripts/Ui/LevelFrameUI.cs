using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelFrameUI : MonoBehaviour
{
    public Image levelPreviewImage;
    public GameObject lockedUi;
    //public GameObject completedUi;
    public GameObject newLevelUi;
    private int levelIndex;

    public void InitializeLevelFrame(LevelData levelData, int index, bool firstUncompleted)
    {
        Texture2D tex = levelData.TextureForLevelsList;
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
        levelPreviewImage.sprite = sprite;
        levelIndex = index;
        if (levelData.CompletedLevel)
        {
            SetCompletedUi();
        }
        else if (firstUncompleted)
            SetNewLevelUi();
        else
            SetLockedUi();

    }

    private void SetCompletedUi()
    {
        //levelPreviewImage.rectTransform.localScale = Vector3.one * 2;
        //completedUi.SetActive(true);
    }

    private void SetNewLevelUi()
    {
        //newLevelUi.SetActive(true);
    }

    private void SetLockedUi()
    {
        lockedUi.SetActive(true);
        levelIndex = -1;
    }

    public void OnPointerClick(PointerEventData eventData) 
    {
        if (levelIndex != -1)
            LoadBoundLevel();
    }

    public void LoadBoundLevel()
    {
        if (levelIndex != -1)
            GameObject.FindObjectOfType<LevelManager>().LoadLevel(levelIndex);

    }

    
}

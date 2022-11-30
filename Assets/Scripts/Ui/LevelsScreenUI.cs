using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelsScreenUI : MonoBehaviour
{
    public GameObject contentAllContainer;
    public GameObject horizontalPanelPrefab;
    public GameObject levelFramePrefab;

    public int framesInRow;

    private List<LevelFrameUI> levelFrameUI;

    public void CreateLevelObjects()
    {
        List<LevelData> levelData = LevelManager.levelDataList;
        levelFrameUI = new List<LevelFrameUI>();
        int counter = 0;
        RectTransform currentHorizontalPanel = null;
        foreach (LevelData ld in levelData)
        {
            if (counter % framesInRow == 0)
            {
                GameObject newHorizontalPanel = GameObject.Instantiate(horizontalPanelPrefab, contentAllContainer.transform);
                currentHorizontalPanel = newHorizontalPanel.GetComponent<RectTransform>();
            }
            GameObject newFrame = GameObject.Instantiate(levelFramePrefab, currentHorizontalPanel.transform);
            LevelFrameUI newLevelFrameUi = newFrame.GetComponent<LevelFrameUI>();
            newLevelFrameUi.InitializeLevelFrame(ld, counter);
            levelFrameUI.Add(newLevelFrameUi);
            counter++;
        }
    }

    
}

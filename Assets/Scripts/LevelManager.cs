using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    public LevelData[] levelData;
    private static int levelIndex = 0;
    public static List<LevelData> levelDataList;

    public static Texture2D CurrentLevelPreviewTexture
    {
        get
        {
            return levelDataList[levelIndex].PreviewTexture;
        }
    }

    private void Awake()
    {
        if (levelDataList == null)
        {
            levelDataList = levelData.ToList();
        }
       
    }

    public void LoadTextures()
    {
        foreach (LevelData ld in levelDataList)
            ld.LoadPreviewTexture();
    }

    void Start()
    {
        if (GameControl.Instance != null)
        {
            GameControl.Instance.StartLevel(levelDataList[levelIndex]);
            UiControl.Instance.SetCurrentLevelText("LEVEL " + (levelIndex + 1));
        }
       
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    public void NextLevel()
    {
        levelIndex = (levelIndex + 1) % levelData.Length;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

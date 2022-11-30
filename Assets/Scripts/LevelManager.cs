using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    public LevelData[] levelData;
    public static int levelIndex = 0;
    public static List<LevelData> levelDataList;

    public static Texture2D CurrentLevelPreviewTexture
    {
        get
        {
            return levelDataList[levelIndex].PreviewTexture;
        }
    }

    public static string CurrentLevelName
    {
        get
        {
            return levelDataList[levelIndex].svgFileName;
        }
    }

    public static Texture2D GetPreviewTextureByLevel(int levelIndex)
    {
        return levelDataList[levelIndex].PreviewTexture;
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
            ld.LoadTextures();
        LevelData firstUncompletedLevel = levelDataList.Where(ld => !ld.CompletedLevel).FirstOrDefault();
        if (firstUncompletedLevel != null)
            levelIndex = levelDataList.IndexOf(firstUncompletedLevel);
        else
            levelIndex = 0;
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

    public void LoadLevel(int levelIndex)
    {
        LevelManager.levelIndex = levelIndex;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public static void SetLevelCompletedImage(int levelIndex, Texture2D image)
    {
        levelDataList[levelIndex].CompletedTexture = image;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public LevelData[] levelData;
    private static int levelIndex = 0;

    private void Awake()
    {

    }

    void Start()
    {
        GameControl.Instance.StartLevel(levelData[levelIndex]);
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

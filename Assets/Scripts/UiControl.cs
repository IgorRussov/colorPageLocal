using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UiControl : MonoBehaviour
{
    public static UiControl Instance;

    public GameControl gameControl;

    public GameObject colorButtonsPanel;
    public Button[] colorButtons;
    private Color[] buttonColors;
    private GameStateSelectColor selectColorState;
    public GameObject undoButton;
    public GameObject nextFillButton;
    public GameObject winScreen;

    public GameObject flashImage;
    public Cinemachine.CinemachineVirtualCamera snapshotVirtualCamera;

    public TMPro.TMP_Text currentLevelText;

    private void Awake()
    {
        Instance = this;
        gameControl = GameObject.FindObjectOfType<GameControl>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Called when the player need to select a color
    /// </summary>
    /// <param name="colors"></param>
    /// <param name="selectColorState"></param>
    public void EnableColorSelection(Color[] colors, GameStateSelectColor selectColorState)
    {
        this.selectColorState = selectColorState;
        buttonColors = (Color[])colors.Clone();
        ColorUtils.Shuffle(buttonColors);
        colorButtonsPanel.SetActive(true);
        for (int i = 0; i < buttonColors.Length; i++)
            colorButtons[i].image.color = buttonColors[i];
        nextFillButton.SetActive(false);
    }

    public void ColorButtonPressed(int buttonIndex)
    {
        colorButtonsPanel.SetActive(false);
        selectColorState.ColorSelected(buttonColors[buttonIndex]);
    }

    private GameStateDrawingFill drawingFillState;

    public void StartFill(GameStateDrawingFill drawingFillState)
    {
        this.drawingFillState = drawingFillState;
        colorButtonsPanel.SetActive(false);

    }

    public void ShowNextFillButton()
    {
        nextFillButton.SetActive(true);
    }

    public void EndFillButtonPressed()
    {
        nextFillButton.SetActive(false);
        drawingFillState.FinishFill();
    }

    public void HideColorButtons()
    {
        colorButtonsPanel.SetActive(false);
    }

    public void ShowVictoryScreen()
    {
        //GameObject.FindObjectOfType<CameraControl>().CreateSnapshot(winSnapshotTexture);
        flashImage.SetActive(true);
        flashImage.GetComponent<Animator>().SetTrigger("Flash");
        LeanTween.delayedCall(1.0f / 60 * 3, () =>
            {
                snapshotVirtualCamera.Priority = 11;
                winScreen.SetActive(true);
                undoButton.SetActive(false);
            });
    }

    public void NextLevel()
    {
        GameObject.FindObjectOfType<LevelManager>().NextLevel();
    }

    public void RestartLevel()
    {
        GameObject.FindObjectOfType<LevelManager>().RestartLevel();
    }

    public void SetCurrentLevelText(string text)
    {
        currentLevelText.text = text;
    }

    public void UndoButtonPressed()
    {
        gameControl.Undo();
    }

}

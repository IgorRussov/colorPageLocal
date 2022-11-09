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
    public GameObject nextFillButton;
    public GameObject startOverButton;
    public GameObject finishedDrawingText;

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
        buttonColors = colors;
        colorButtonsPanel.SetActive(true);
        for (int i = 0; i < buttonColors.Length; i++)
            colorButtons[i].image.color = colors[i];
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

    public void ShowFinishedText()
    {
        finishedDrawingText.SetActive(true);
    }

    public void HideFinishedText()
    {
        finishedDrawingText.SetActive(false);
    }

    public void UndoButtonPressed()
    {
        gameControl.Undo();
    }

}

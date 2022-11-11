using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugUi : MonoBehaviour
{
    public GameObject debugMenu;
    public TMPro.TMP_InputField strokeSpeedInput;
    public TMPro.TMP_InputField fillSpeedInput;
    public GameControl gameControl;

    public void ToggleDebugMenu()
    {
        debugMenu.SetActive(!debugMenu.activeSelf);
        if (debugMenu.activeSelf)
            GameStateManager.DisableInput();
        else
            GameStateManager.EnableInput();
    }

    public void OnEnable()
    {
        //strokeSpeedInput.text = gameControl.drawSpeed.ToString("0");
        //fillSpeedInput.text = gameControl.fillMoveSpeed.ToString("0.000");
    }


    public void StrokeSpeedInputTextChanged(string text)
    {
        /*
        float prevValue = gameControl.drawSpeed;
        float newValue;
        if (float.TryParse(strokeSpeedInput.text, out newValue))
        {
            gameControl.drawSpeed = newValue;
        }
        else
        {
            strokeSpeedInput.text = gameControl.drawSpeed.ToString("0");
        }
        */
    }

    public void FillSpeedInputTextChanged(string text)
    {
        /*
        float prevValue = gameControl.fillMoveSpeed;
        float newValue;
        if (float.TryParse(fillSpeedInput.text, out newValue))
        {
            gameControl.fillMoveSpeed = newValue;
        }
        else
        {
            fillSpeedInput.text = gameControl.fillMoveSpeed.ToString("0.000");
        }
        */
    }

    public void ChangeStrokeSpeed(float ammount)
    {
        /*
        float prevValue = gameControl.drawSpeed;
        prevValue += ammount;
        strokeSpeedInput.text = prevValue.ToString();
        StrokeSpeedInputTextChanged(strokeSpeedInput.text);
        */
    }

    public void ChangeFillSpeed(float ammount)
    {
        /*
        float prevValue = gameControl.fillMoveSpeed;
        prevValue += ammount;
        fillSpeedInput.text = prevValue.ToString();
        FillSpeedInputTextChanged(fillSpeedInput.text);
        */
    }

    public void PencilToOrigin()
    {
        Pencil.instance.ForcedMove(Vector2.zero, false);
    }

    public void ResetButtonPressed()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}

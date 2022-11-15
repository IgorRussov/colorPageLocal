using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// State for when the player is drawing fill of a shape
/// </summary>
public class GameStateDrawingFill : GameBaseState
{
    private Vector2 lastPos;
    private float minPointDiff = 0.05f;
    private Vector2 drawTextureSize;

    private GameStateManager gameStateManager;

    public void FinishFill()
    {
        gameStateManager.gameControl.FinishFillStage();
        if (!gameStateManager.gameControl.gameStageInfo.FinishedDrawing)
            gameStateManager.SwitchState(gameStateManager.selectColorState);
        else
            gameStateManager.SwitchState(gameStateManager.finishedState);
    }

    public override void EnterState(GameStateManager game)
    {

        this.gameStateManager = game;
        //game.gameControl.drawingZone.SetMaskSprite(game.gameControl.gameStageInfo.FillStageIndex);
        UiControl.Instance.StartFill(this);
        lastPos = Vector2.zero;

        Pencil.instance.SetPencilMode(PencilMode.DrawFill);
        textureFillers = new List<TextureFiller>();

        drawTextureSize = game.gameControl.drawingZone.FillTextureSize;
    }


    public override void InputDelta(GameStateManager game, Vector2 delta)
    {
        Pencil.instance.GetDelta(delta);
    }

    public override void InputPressed(GameStateManager game)
    {
        Pencil.instance.RecieveInitialPosition(GameStateManager.touchPosition);
        //Pencil.instance.lifted = false;
    }

    public override void InputReleased(GameStateManager game)
    {
        //Pencil.instance.lifted = true;
    }

    public override void UpdateState(GameStateManager game)
    {
        Vector2 pos = Pencil.instance.gameObject.transform.position;

        if ((lastPos - pos).magnitude > minPointDiff)
        {
            Vector2 fillerPos = pos * PositionConverter.SvgPixelsPerUnit;
            fillerPos += drawTextureSize * 0.5f;
            //Debug.Log("Adding filler at pos + " + fillerPos);
            lastPos = pos;

            game.gameControl.drawingZone.AddColorPainter(fillerPos, 
                game.gameControl.gameStageInfo.FillStageIndex);
        }

        game.gameControl.drawingZone.UpdateDrawFill();
        //CalculateTextureFill();
    }

    List<TextureFiller> textureFillers;

    private void CalculateTextureFill()
    {
        for(int i = 0; i < textureFillers.Count; i++)
        {
            if (textureFillers[i].UpdateTexture())
            {
                textureFillers.RemoveAt(i);
                i--;
            }
        }
        //drawTexture.Apply();
    }

    public override void UndoRequested(GameStateManager game, GameStageInfo info)
    {
        game.SwitchState(game.selectColorState);
    }
}

public struct ColorPainter
{
    public static readonly int StructSize = 3 * 4;

    public float startTime;
    public Vector2 texturePos;
}

public class TextureFiller
{
    Texture2D textureToFill;
    Color32 color;
    int radius;
    float fillSpeed;
    float radiusNow;
    Vector2Int texturePos;

    public TextureFiller(Texture2D textureToFill, Color color, int radius, float fillSpeed, Vector2Int texturePos)
    {
        this.textureToFill = textureToFill;
        this.color = color;
        this.radius = radius;
        this.fillSpeed = fillSpeed;
        this.texturePos = texturePos;

        radiusNow = radius * 2 / 3;
    }

    public bool UpdateTexture()
    {
        int w = textureToFill.width;
        int h = textureToFill.height;
        radiusNow += fillSpeed * Time.fixedDeltaTime;
        int r = Mathf.RoundToInt(Mathf.Min(radiusNow, radius));
        int minX = Mathf.Max(0, texturePos.x - r);
        int maxX = Mathf.Min(w, texturePos.x + r);
        int minY = Mathf.Max(0, texturePos.y - r);
        int maxY = Mathf.Min(h, texturePos.y + r);

        for(int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
            {
                int dx = texturePos.x - x;
                int dy = texturePos.y - y;
                if (dx * dx + dy * dy <= radius * radius)
                    textureToFill.SetPixel(x, y, color);
            }
        return radiusNow >= radius;
    }


}

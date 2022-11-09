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
    private float fillSpeed = 50;
    private Vector2 pos;

    private GameStateManager gameStateManager;
    private Texture2D drawTexture;

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
        pos = Vector2.zero;
        pos = Camera.main.transform.position;

        Pencil.instance.SetPencilMode(PencilMode.DrawFill);
        textureFillers = new List<TextureFiller>();

        drawTexture = game.gameControl.drawingZone.drawFillTexture;
    }

    private Vector2 previousDelta = Vector2.zero;

    public override void InputDelta(GameStateManager game, Vector2 delta)
    {
        Vector2 acceleration = delta - previousDelta;
        previousDelta = delta;

        //Pencil.instance.acceleration = acceleration * game.gameControl.fillMoveSpeed * Time.deltaTime;

        Pencil.instance.GetDelta(delta * game.gameControl.fillMoveSpeed * Time.deltaTime);
    }

    public override void InputPressed(GameStateManager game)
    {
        Pencil.instance.lifted = false;
    }

    public override void InputReleased(GameStateManager game)
    {
        Pencil.instance.lifted = true;
        previousDelta = Vector2.zero;
    }

    public override void UpdateState(GameStateManager game)
    {
        Vector2 pos = Pencil.instance.gameObject.transform.position;

        if ((lastPos - pos).magnitude > minPointDiff)
        {
            Vector2Int fillerPos = new Vector2Int(Mathf.RoundToInt(pos.x * 100), Mathf.RoundToInt(pos.y * 100));
            fillerPos = new Vector2Int(fillerPos.x + drawTexture.width / 2, fillerPos.y + drawTexture.height / 2);
            //Debug.Log("Adding filler at pos + " + fillerPos);
            lastPos = pos;

            TextureFiller filler = new TextureFiller(
                drawTexture,
                Pencil.instance.GetColorByStage(game.gameControl.gameStageInfo.FillStageIndex),
                Mathf.RoundToInt(game.gameControl.drawingZone.fillStrokeWidth / 2),
                fillSpeed,
                fillerPos
                );
            textureFillers.Add(filler);
        }

        CalculateTextureFill();
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
        drawTexture.Apply();
    }

    public override void UndoRequested(GameStateManager game, GameStageInfo info)
    {
        game.SwitchState(game.selectColorState);
    }
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

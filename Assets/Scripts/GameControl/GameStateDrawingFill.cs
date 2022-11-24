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
    private int pixelsPerQuadrant = 25;
    private Vector2 drawTextureSize;

    private GameStateManager gameStateManager;
    private FillQuadrant[,] quadrants;
    private int quadrantsToContinue;
    private int visitedQuadrants;

    private void SetupQuadrants(Texture2D maskTexture, float percentToContinue)
    {
        int unvisitedNumber;
        quadrants = TextureCounter.GetFillQuadrants(maskTexture, pixelsPerQuadrant, out unvisitedNumber);
        visitedQuadrants = 0;
        quadrantsToContinue = Mathf.RoundToInt(unvisitedNumber * percentToContinue);
        if (unvisitedNumber < 500 / (pixelsPerQuadrant))
            quadrantsToContinue /= 4;
    }

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
        Pencil.instance.MoveToPosForNextFillShape();
        //game.gameControl.drawingZone.SetMaskSprite(game.gameControl.gameStageInfo.FillStageIndex);
        UiControl.Instance.StartFill(this);
        lastPos = Vector2.zero;

        Pencil.instance.SetPencilMode(PencilMode.DrawFill);
        textureFillers = new List<TextureFiller>();
        game.gameControl.drawingZone.filledPercent = 0;
        drawTextureSize = game.gameControl.drawingZone.FillTextureSize;
        Texture2D maskTexture = game.gameControl.drawingZone.maskTexture;
        SetupQuadrants(maskTexture, game.gameControl.requiredFillToContinue);
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

    private void UpdateQuadrants(Vector2 pos)
    {
        int posi = Mathf.FloorToInt(pos.x / pixelsPerQuadrant);
        int posj = Mathf.FloorToInt(pos.y / pixelsPerQuadrant);

        int radius = 4;
        for (int i = -radius; i < radius; i++)
            for(int j = -radius; j < radius; j++)
            {
                int qi = posi + i;
                int qj = posj + j;
                if (qi >= 0 && qi < quadrants.GetLength(0) && qj >= 0 && qj < quadrants.GetLength(1))
                {
                    if (quadrants[qi, qj] == FillQuadrant.Unvisited)
                    {
                        quadrants[qi, qj] = FillQuadrant.Visited;

                        visitedQuadrants++;
                        //Debug.Log(visitedQuadrants + " " + quadrantsToContinue);
                    }
                    if (visitedQuadrants >= quadrantsToContinue)
                        UiControl.Instance.ShowNextFillButton();
                }
            }
        
    }

    public override void UpdateState(GameStateManager game)
    {
        Vector2 pos = Pencil.instance.gameObject.transform.position;
        Vector2 fillerPos = pos * PositionConverter.SvgPixelsPerUnit * PositionConverter.TextureScale;
        fillerPos += drawTextureSize * 0.5f;
        UpdateQuadrants(fillerPos / PositionConverter.TextureScale);


        if (!Pencil.instance.mustForcedMove)
            if ((lastPos - pos).magnitude > minPointDiff)
            {
              
                lastPos = pos;

                game.gameControl.drawingZone.AddColorPainter(fillerPos, 
                    game.gameControl.gameStageInfo.FillStageIndex);


            }

        game.gameControl.drawingZone.UpdateDrawFill();
        /*
        if (game.gameControl.drawingZone.filledPercent > game.gameControl.requiredFillToContinue)
        {

            UiControl.Instance.ShowNextFillButton();
        }
          */ 

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

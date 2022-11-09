using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Structure which contains info about current game stage and 
/// allows for easy access to computed parameters
/// </summary>
public struct GameStageInfo
{
    /// <summary>
    /// Current stage of the game, including stroke and fill stage
    /// </summary>
    public int drawStage;
    /// <summary>
    /// Total number of stroke shapes in the drawing
    /// </summary>
    public int strokeShapesCount;
    /// <summary>
    /// Total number of fill shapes in the drawing
    /// </summary>
    public int fillShapesCount;

    /// <summary>
    /// Length in svg units of the drawn part of the stroke shape
    /// </summary>
    public float drawnAmmount;
    /// <summary>
    /// Total (with aditional ending line) length of the stroke shape that is being drawn now
    /// </summary>
    public float strokeShapeLength;

    /// <summary>
    /// If the current stage of the game is drawing stroke shapes
    /// </summary>
    public bool InStrokeStage
    {
        get
        {
            return drawStage < strokeShapesCount;
        }
    }

    /// <summary>
    /// If the current stage of the game is drawing fill shapes
    /// </summary>
    public bool InFillStage
    {
        get { return !InStrokeStage; }
    }

    /// <summary>
    /// If the player has drawn more than stroke shape with continuation
    /// </summary>
    public bool MustEndDraw
    {
        get
        {
            return drawnAmmount >= strokeShapeLength;
        }
    }

    /// <summary>
    /// If all of the required shapes have been drawn (shows level end)
    /// </summary>
    public bool FinishedDrawing
    {
        get
        {
            return drawStage >= strokeShapesCount + fillShapesCount;
        }
    }

    /// <summary>
    /// Index of the fill shape that is drawn now 
    /// </summary>
    public int FillStageIndex
    {
        get
        {
            if (FinishedDrawing)
                return fillShapesCount - 1;
            return drawStage - strokeShapesCount;
        }
    }

    /// <summary>
    /// How much of the current stroke shape is drawn (from 0 to 1)
    /// </summary>
    public float DrawnPart
    {
        get
        {
            return drawnAmmount / strokeShapeLength;
        }
    }
}
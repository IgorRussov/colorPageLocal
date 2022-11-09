using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class which controlls the pencil - graphic representation of drawing
/// </summary>
public class Pencil : MonoBehaviour
{
    public static Pencil instance;

    public float forcedMoveSpeed;
    public float freeMoveSpeed;
    public Vector2 liftedOffset;
    [HideInInspector]
    public bool lifted;
    [HideInInspector]
    public bool mustDraw;
    [HideInInspector]
    public bool mustMoveForced;
    public SpriteRenderer tipSpriteRenderer;

    public void UpdateColorRepres(int fillStage)
    {
        if (fillStage != -1)
            tipSpriteRenderer.color = usedColors[fillStage];
        else
            tipSpriteRenderer.color = Color.black;
    }

    public Color GetColorByStage(int stage)
    {
        return usedColors[stage];
    }

    public void SetColorByStage(Color color, int stage)
    {
        if (stage < usedColors.Count)
            usedColors[stage] = color;
        else
            usedColors.Add(color);
        UpdateColorRepres(stage);
    }

    private List<Color> usedColors = new List<Color>();
  

    private Vector3 TrueTarget
    {
        get
        {
            return target + (lifted ? liftedOffset : Vector2.zero);
        }
    }

    public Vector2 target;

    public void BindGameControlEvents(GameControl gameControl)
    {
        gameControl.StrokeDrawStarted += PressPencil;
        gameControl.StrokeDrawStopped += LiftPencil;
    }

    public void ForcedMove(Vector2 target, bool moveInWorldSpace)
    {
        Vector2 currentDiff;
        if (moveInWorldSpace)
            currentDiff = target - new Vector2(transform.position.x, transform.position.y);
        else
            currentDiff = target - new Vector2(transform.localPosition.x, transform.localPosition.y);
        this.target = new Vector2(transform.localPosition.x, transform.localPosition.y) + currentDiff;
    }

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateForcedMove();
    }

    void UpdateForcedMove()
    {
        Vector3 pos = transform.localPosition;
        Vector2 toTarget = TrueTarget - pos;
        float dist = toTarget.magnitude;
        if (dist > float.Epsilon)
        {
            float moveDist = Mathf.Min(dist, forcedMoveSpeed * Time.deltaTime);
            transform.Translate(toTarget.normalized * moveDist);
        }
    }

    public void PressPencil()
    {
        lifted = false;
    }

    public void LiftPencil()
    {
        lifted = false;
    }
}

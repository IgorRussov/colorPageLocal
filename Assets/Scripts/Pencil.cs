using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PencilMode
{
    Inactive,
    DrawStroke,
    DrawFill
}

/// <summary>
/// Class which controlls the pencil - graphic representation of drawing
/// </summary>
public class Pencil : MonoBehaviour
{
    public static Vector2 PosForNextFillShape;

    public static Pencil instance;

    [HideInInspector]
    private PencilMode pencilMode;

    public float forcedMoveSpeed;
    public float freeMoveSpeed;
    public Vector2 liftedOffset;
    [HideInInspector]
    public bool lifted;
    private float liftedAmmount;
    public float liftSpeed;
    public SpriteRenderer tipSpriteRenderer;
    public GameObject pencilRepresObject;

    private Rigidbody rigidbody;
    private Collider collider;

    [HideInInspector]
    public Vector2 acceleration;

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
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponentInChildren<Collider>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (pencilMode == PencilMode.DrawStroke)
            UpdateForcedMove();
        else if (pencilMode == PencilMode.DrawFill)
            UpdateDrawFill();
        UpdateRepresPosition();
    }

    void UpdateDrawFill()
    {
        //if (acceleration != Vector2.zero)
        //{
            //rigidbody.AddForce(acceleration);
            //acceleration = Vector2.zero;
            rigidbody.velocity = delta;
            delta = Vector2.zero;
        //}


    }

    void UpdateRepresPosition()
    {
        if (lifted && liftedAmmount != 1)
        {
            liftedAmmount = Mathf.Min(1, liftedAmmount + liftSpeed * Time.deltaTime);
            pencilRepresObject.transform.localPosition = liftedOffset * liftedAmmount;
        }
        else if (!lifted && liftedAmmount != 0)
        {
            liftedAmmount = Mathf.Max(0, liftedAmmount - liftSpeed * Time.deltaTime);
            pencilRepresObject.transform.localPosition = liftedOffset * liftedAmmount;
        }

    }

    void UpdateForcedMove()
    {
        Vector2 pos = transform.localPosition;
        Vector2 toTarget = target - pos;
        float dist = toTarget.magnitude;
        if (dist > float.Epsilon)
        {
            float moveDist = Mathf.Min(dist, forcedMoveSpeed * Time.deltaTime);
            rigidbody.MovePosition((Vector2)transform.position + (toTarget.normalized * moveDist));
        }
    }

    public void PressPencil()
    {
        lifted = false;
    }
    
    public void LiftPencil()
    {
        //Debug.Log("LIFT");
        lifted = true;
    }

    private Vector2 delta;
    public void GetDelta(Vector2 delta)
    {
        //Debug.Log("GET delta" + delta);
        this.delta = delta;
    }

    public void MoveToPosForNextFillShape()
    {
        rigidbody.MovePosition(PosForNextFillShape + (Vector2)transform.parent.position);
    }

    public void SetPencilMode(PencilMode newMode)
    {
        pencilMode = newMode;
        switch(newMode)
        {
            case (PencilMode.Inactive):
                collider.enabled = false;
                break;
            case (PencilMode.DrawFill):
                
                collider.enabled = true;
                break;
            case (PencilMode.DrawStroke):
                collider.enabled = false;
                break;
        }
    }
}

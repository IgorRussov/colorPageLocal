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
    [Header("Hierarchy objects")]
    public SpriteRenderer tipSpriteRenderer;
    public GameObject pencilRepresObject;
    [Header("Movement properties")]
    public float forcedMoveSpeed;
    public float drawFillMaxSpeed;
    public float drawFillSmoothTime;
    [Header("Graphical repres properties")]
    public float liftSpeed;
    public Vector2 liftedOffset;

    [HideInInspector]
    public bool lifted;

    private float liftedAmmount;
    private PencilMode pencilMode;
    private Rigidbody rigidbody;
    private Collider collider;

    public bool PressedCompletely
    {
        get
        {
            return !lifted && liftedAmmount == 0;
        }
    }

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
        else if (pencilMode == PencilMode.DrawFill && wantMove)
            UpdateDrawFill();
        UpdateRepresPosition();
    }

    private Vector2 pointerPosition;
    private Vector2 pointerOffset;
    private bool wantMove;

    public void RecieveInitialPosition(Vector2 pointerPosition)
    {
        wantMove = true;
        this.pointerPosition = pointerPosition;
        pointerOffset = PositionConverter.LocalSpaceToScreenSpace(transform.localPosition) - this.pointerPosition;
    }    

    public void GetDelta(Vector2 delta)
    {
        wantMove = true;
        pointerPosition += delta;
    }

    private Vector2 currentVelocity;

    private void OnDrawGizmos()
    {
        /*
        Gizmos.DrawCube(newPos + (Vector2)transform.position, Vector3.one * 0.5f);
        Gizmos.DrawCube(transform.position, Vector3.one * 0.3f);

        Vector2 screenSpacePosition = pointerPosition + pointerOffset;
        Vector2 localSpacePosition = PositionConverter.ScreenSpaceToLocalSpace(screenSpacePosition);
        Gizmos.DrawCube(localSpacePosition, Vector3.one);
        */
    }

    private Vector2 newPos;

    void UpdateDrawFill()
    {
        //Debug.Log(pointerPosition);
        Vector2 screenSpacePosition = pointerPosition + pointerOffset;
        Vector2 localSpacePosition = PositionConverter.ScreenSpaceToLocalSpace(screenSpacePosition);

        Vector2 localPos = (Vector2)transform.localPosition;

        //Vector2 v = (localSpacePosition - localPos).normalized * drawFillMaxSpeed * Time.fixedDeltaTime;
        //rigidbody.velocity = v;
        newPos = Vector2.SmoothDamp(localPos, localSpacePosition, ref currentVelocity, drawFillSmoothTime, drawFillMaxSpeed);
        rigidbody.velocity = currentVelocity;
        //Vector2 newPosWorld = transform.TransformPoint(newPos);
        //rigidbody.position = newPosWorld;
        //rigidbody.MovePosition(newPos);
        //rigidbody.position = newPos;
        //Vector3 newPosition = rigidbody.position + transform.TransformDirection(newPos);
        //rigidbody.MovePosition(newPosition);
    }

    void UpdateRepresPosition()
    {
        //Debug.Log("Lifted ? " + lifted +  
        //    ", Lifted ammount: " + liftedAmmount + ", position = "
        //    + pencilRepresObject.transform.localPosition);
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
        lifted = true;
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
                rigidbody.velocity = Vector2.zero;
                break;
            case (PencilMode.DrawFill):
                wantMove = false;
                collider.enabled = true;
                break;
            case (PencilMode.DrawStroke):
                collider.enabled = false;
                break;
        }
    }
}

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
    [Header("Forced move properties")]
    public float forcedMoveMaxSpeed;
    public float forcedMoveSmoothTime;
    [Header("Draw fill move properties")]
    public float drawFillMaxSpeed;
    public float drawFillSmoothTime;
    [Header("Graphical repres properties")]
    public float liftSpeed;
    public Vector2 liftedOffset;
    public float liftedScaleChange;
    public Vector2 offscreenOffset;
    [Header("Animation properties")]
    public float rotationAmplitude;
    public float strokeRotationSpeed;
    public float fillRotationSpeed;

    [HideInInspector]
    public bool lifted;

    private float liftedAmmount;
    private PencilMode pencilMode;
    private Rigidbody rigidbody;
    private Collider collider;
    [HideInInspector]
    public bool mustForcedMove = false;

    public bool PressedCompletely
    {
        get
        {
            return !lifted && liftedAmmount == 0;
        }
    }

    public Vector2 ScaledOffscreenOffset
    {
        get
        {
            float scale = Camera.main.orthographicSize / 5;
            return offscreenOffset * scale;
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

    public void InstantMove(Vector2 target, bool moveInWorldSpace)
    {
        //Debug.Log("Instant move to " + target);
        mustForcedMove = false;
        Vector2 currentDiff;
        if (moveInWorldSpace)
            currentDiff = target - new Vector2(transform.position.x, transform.position.y);
        else
            currentDiff = target - new Vector2(transform.localPosition.x, transform.localPosition.y);
        Vector2 newPos = new Vector2(transform.localPosition.x, transform.localPosition.y) + currentDiff;
        rigidbody.MovePosition((Vector3)newPos + transform.parent.position);

    }

    public void ForcedMove(Vector2 target, bool moveInWorldSpace)
    {
        // Debug.Log("Forced move to " + target);
        mustForcedMove = true;

        collider.enabled = false;
        currentVelocity = Vector2.zero;
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
        if (mustForcedMove)
            UpdateForcedMove();
        else if (pencilMode == PencilMode.DrawFill && wantMove)
            UpdateDrawFill();
        UpdateRepresPosition();
    }


    private void Update()
    {
        
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

   
    [HideInInspector]
    public Vector2 currentVelocity;

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

    void UpdateDrawFill()
    {
        //Debug.Log(pointerPosition);
        Vector2 screenSpacePosition = pointerPosition + pointerOffset;
        Vector2 localSpacePosition = PositionConverter.ScreenSpaceToLocalSpace(screenSpacePosition);

        Vector2 localPos = (Vector2)transform.localPosition;

        Vector2.SmoothDamp(localPos, localSpacePosition, ref currentVelocity, drawFillSmoothTime, drawFillMaxSpeed);
        rigidbody.velocity = currentVelocity;

        float phase = Mathf.Lerp(-1, 1, currentVelocity.x / drawFillMaxSpeed);
        rigidbody.rotation = Quaternion.Euler(0, 0, rotationAmplitude * phase);
    }

    void UpdateRepresPosition()
    {
        if (lifted && liftedAmmount != 1) //Must lift more
        {
            liftedAmmount = Mathf.Min(1, liftedAmmount + liftSpeed * Time.deltaTime);
            pencilRepresObject.transform.localPosition = liftedOffset * liftedAmmount;
            pencilRepresObject.transform.localScale = Vector3.one * (liftedAmmount * liftedScaleChange + 1);
        }
        else if (!lifted && liftedAmmount != 0) //Must lift less
        {
            liftedAmmount = Mathf.Max(0, liftedAmmount - liftSpeed * Time.deltaTime);
            pencilRepresObject.transform.localPosition = liftedOffset * liftedAmmount;
            pencilRepresObject.transform.localScale = Vector3.one * (liftedAmmount * liftedScaleChange + 1);
        }

    }

    private float r = 0;

    void UpdateForcedMove()
    {
        float smoothTime = pencilMode == PencilMode.DrawFill ? 0.01f : forcedMoveSmoothTime;
        Vector2.SmoothDamp(transform.localPosition, target, ref currentVelocity, smoothTime, forcedMoveMaxSpeed);
      
        rigidbody.velocity = currentVelocity;
        if (currentVelocity.magnitude < 0.01f)
        {
            mustForcedMove = false;
            collider.enabled = true;
        }
            
        /*
        float dist = toTarget.magnitude;
        if (dist > float.Epsilon)
        {
            float speed = forcedMoveSpeed;
            if (!lifted)
                speed *= 10;
            float moveDist = Mathf.Min(dist, speed * Time.deltaTime);
            rigidbody.MovePosition((Vector2)transform.position + (toTarget.normalized * moveDist));
            float dR = moveDist * strokeRotationSpeed;
            if (!lifted)
                r += dR;
            rigidbody.rotation = Quaternion.Euler(0, 0, rotationAmplitude * Mathf.Sin(r));
        }
        
         */
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
        ForcedMove(PosForNextFillShape, false);
        //rigidbody.MovePosition(PosForNextFillShape + (Vector2)transform.parent.position);
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
                //collider.enabled = true;
                break;
            case (PencilMode.DrawStroke):
                collider.enabled = false;
                break;
        }
    }

    public void MoveOffscren()
    {
        ForcedMove((Vector2)Camera.main.transform.position + ScaledOffscreenOffset, true);
    }
}

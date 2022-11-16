using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.VectorGraphics;
using System.Linq;

struct TargetMember
{
    public Transform transform;
    public float addedTime;
    public int stageAddedIndex;
}

public class CameraControl : MonoBehaviour
{
    public float drawingPadding;
    public CinemachineTargetGroup cinemachineTargetGroup;
    public CinemachineVirtualCamera cinemachineVirtualCamera;
    public Transform targetParentObject;
    [Header("View transition settings")]
    public float fullWeightTime;
    public float startWeight;
    public float cameraDamping;

    public static CameraControl Instance;

    private List<TargetMember> targetMembers = new List<TargetMember>();
    private List<TargetMember> targetsToRemove = new List<TargetMember>();

    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        LeanTween.delayedCall(0.1f, SetCameraDamping);
    }

    private void SetCameraDamping()
    {
        CinemachineFramingTransposer cft = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        cft.m_XDamping = cameraDamping;
        cft.m_YDamping = cameraDamping;
        cft.m_ZDamping = cameraDamping;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAddedTargets();
        UpdateTargetsToRemove();
    }

    void UpdateAddedTargets()
    {
        foreach(TargetMember tm in targetMembers)
        {
            if (tm.addedTime + fullWeightTime >= Time.time)
            {
                int index = cinemachineTargetGroup.FindMember(tm.transform);
                float newWeight = Mathf.Lerp(startWeight, 1, (Time.time - tm.addedTime) / fullWeightTime);
                cinemachineTargetGroup.m_Targets[index].weight = newWeight;
            }
        }
    }

    void UpdateTargetsToRemove()
    {
        for(int i = 0; i < targetsToRemove.Count; i++)
        {
            TargetMember tm = targetsToRemove[i];
            int index = cinemachineTargetGroup.FindMember(tm.transform);
            float newWeight = Mathf.Lerp(1, 0, (Time.time - tm.addedTime) / fullWeightTime);
            if (newWeight < 0.05f)
            {
                cinemachineTargetGroup.RemoveMember(tm.transform);
                targetsToRemove.Remove(tm);
                i--;
            }
            else
                cinemachineTargetGroup.m_Targets[index].weight = newWeight;
        }
    }

    public void RemoveTargetsOfStage(int stageIndex)
    {
        List<TargetMember> toRemove = targetMembers.Where(tm => tm.stageAddedIndex == stageIndex).ToList();
        targetMembers.RemoveAll(tm => tm.stageAddedIndex == stageIndex);
        toRemove.ForEach(tm =>
        {
            tm.addedTime = Time.time;
            targetsToRemove.Add(tm);
        });

    }

    public void AddShapeToView(Shape shape, int stageIndex)
    {
        Rect shapeRect = ShapeUtils.GetShapeRect(shape);
        float minX = shapeRect.xMin;
        float maxX = shapeRect.xMax;
        float minY = -shapeRect.yMin;
        float maxY = -shapeRect.yMax;
        AddCameraTarget(new Vector2(minX, minY) / PositionConverter.SvgPixelsPerUnit, stageIndex);
        AddCameraTarget(new Vector2(maxX, maxY) / PositionConverter.SvgPixelsPerUnit, stageIndex);
    }

    public void AddCameraTarget(Vector3 localPos, int stageIndex)
    {
        GameObject target = new GameObject("Camera target");
        target.transform.SetParent(targetParentObject);
        target.transform.localPosition = localPos;
        cinemachineTargetGroup.AddMember(target.transform, startWeight, drawingPadding);
        TargetMember tm = new TargetMember();
        tm.transform = target.transform;
        tm.addedTime = Time.time;
        tm.stageAddedIndex = stageIndex;
        targetMembers.Add(tm);
    }

}

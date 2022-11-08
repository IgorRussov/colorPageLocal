using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class LevelData : ScriptableObject
{
    [Header("Must be in StreamingAssets/VectorFiles, no .svg")]
    public string svgFileName;
    public int[] strokeShapesOrder;
    public int[] fillShapesOrder;
    public Color[][] fillShapesColors;
}

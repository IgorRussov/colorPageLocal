#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelDataWorker))]
public class LevelDataWorkerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelDataWorker myScript = (LevelDataWorker)target;
        if (GUILayout.Button("Read level data (LAUNCH GAME)"))
        {
            myScript.wantRead = true;
        }
        if (GUILayout.Button("Save level data (LAUNCH GAME)"))
        {
            myScript.wantSave = true;
        }
    }
}
#endif
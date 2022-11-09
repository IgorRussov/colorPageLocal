#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomEditor(typeof(LevelDataWorker))]
public class LevelDataWorkerEditor : Editor
{/*
    public override VisualElement CreateInspectorGUI()
    {
        var container = new VisualElement();



        var iterator = serializedObject.GetIterator();
        if (iterator.NextVisible(true))
        {
            do
            {
                var propertyField = new PropertyField(iterator.Copy()) { name = "PropertyField:" + iterator.propertyPath };

                if (iterator.propertyPath == "m_Script" && serializedObject.targetObject != null)
                    propertyField.SetEnabled(value: false);

                container.Add(propertyField);
            }
            while (iterator.NextVisible(false));
        }

        return container;
    }
    */
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelDataWorker myScript = (LevelDataWorker)target;
        if (GUILayout.Button("Highlight stroke shape"))
        {
            myScript.wantHighlightStroke = true;
        }
        if (GUILayout.Button("Highlight fill shape"))
        {
            myScript.wantHighlightFill = true;
        }

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
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
        LevelDataWorker myScript = (LevelDataWorker)target;

        if (GUILayout.Button("Read level data from svg file"))
        {
            myScript.wantRead = true;
        }
        if (GUILayout.Button("Edit existing level data"))
        {
            myScript.wantReadData = true;
        }


        DrawDefaultInspector();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Highlight stroke shape"))
        {
            myScript.wantHighlightStroke = true;
        }
        if (GUILayout.Button("Highlight fill shape"))
        {
            myScript.wantHighlightFill = true;
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Save level data"))
        {
            myScript.wantSave = true;
        }



    }
    
}
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(MapPreview))]
public class MapPreviewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapPreview mapgen = (MapPreview) target;

        if(DrawDefaultInspector()){
            if(mapgen.autoUpdate){
                mapgen.DrawMapInEditor();
            }
        };

        if(GUILayout.Button("Generate")){
            mapgen.DrawMapInEditor();
        }
    }
}

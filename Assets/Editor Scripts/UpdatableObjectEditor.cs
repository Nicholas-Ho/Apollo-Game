#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(UpdatableObject), true)]
public class UpdatableObjectEditor : Editor
{
    public override void OnInspectorGUI(){
        base.OnInspectorGUI();

        UpdatableObject data = (UpdatableObject) target;

        if(GUILayout.Button("Update")){
            data.NotifyOfUpdatedValues();
        }
    }
}
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PopulateSettings : UpdatableObject
{
    public float radius;
    public int numSamplesBeforeRejection;
    public List<GameObject> simpleObjectList = new List<GameObject>();
    public List<PopulateObjectsInfo> objectInfoList = new List<PopulateObjectsInfo>();

    [HideInInspector]
    public float[] radiiArray;
    [HideInInspector]
    public float[] ratioArray;

    //Pre-processor Directive
    #if UNITY_EDITOR
    protected override void OnValidate(){
        int numObjects = objectInfoList.Count;
        radiiArray = new float[numObjects];
        ratioArray = new float[numObjects];

        for(int i = 0; i < numObjects; i++){
            radiiArray[i] = objectInfoList[i].radius;
            ratioArray[i] = objectInfoList[i].probability;
        }

        base.OnValidate();
    }
    #endif
}

[System.Serializable]
public struct PopulateObjectsInfo{
    public GameObject gameObject;
    public float radius;
    public float probability;
}
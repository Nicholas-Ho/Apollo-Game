using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SamplingTester : MonoBehaviour
{
    public enum RadiiMode{SingleRadius, VariableRadii};
    public RadiiMode radiiMode;
    public float radius = 1f;
    public List<PointInfo> pointInfo = new List<PointInfo>();
    public Vector2 sampleRegionSize = Vector2.one;
    public int numSamplesBeforeRejection = 30;
    public float gizmoRadius = 0.5f;

    List<Vector2> points;
    List<Vector3> pointsV3;

    void OnValidate(){
        if(radiiMode == RadiiMode.SingleRadius){
            points = PoissonDiscSampling.GeneratePoints(radius, sampleRegionSize, numSamplesBeforeRejection);
        } else {
            float[] radii = new float[pointInfo.Count];
            float[] ratios = new float[pointInfo.Count];
            for(int i = 0; i < pointInfo.Count; i++){
                radii[i] = pointInfo[i].radius;
                ratios[i] = pointInfo[i].probability;
            }
            pointsV3 = PoissonDiscSampling.GeneratePointsVariableRadii(radii, ratios, sampleRegionSize, numSamplesBeforeRejection, "radii");
        }
        
    }

    void OnDrawGizmos(){
        Gizmos.DrawWireCube(sampleRegionSize / 2, sampleRegionSize);

        if(radiiMode == RadiiMode.SingleRadius){
            if(points != null){
                foreach(Vector2 point in points){
                    Gizmos.DrawSphere(point, gizmoRadius);
                }
            }
        } else {
            if(pointsV3 != null){
                foreach(Vector3 point in pointsV3){
                    Gizmos.DrawSphere(new Vector3(point.x, point.y, 0), point.z / 2);
                }
            }
        }
    }
}

[System.Serializable]
public struct PointInfo{
    public float radius;
    public float probability;
}

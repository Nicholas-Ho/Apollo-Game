using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class MeshSettings : UpdatableObject
{
    public const int numSupportedLOD = 5;
    public const int numSupportedChunkSizes = 9;
    public const int numSupportedFlatShadingChunkSizes = 3;
    public static readonly int[] supportedChunkSizes = {48, 72, 96, 120, 144, 168, 192, 216, 240};

    [Range(0, numSupportedChunkSizes-1)]
    public int chunkSizeIndex;
    [Range(0, numSupportedFlatShadingChunkSizes-1)]
    public int flatShadingChunkSizeIndex;

    public float meshScale = 2.5f;
    
    public bool useFlatShading;

    // Number of vertices in a mesh rendered at LOD = 0. Includes 2 extra vertices used to calculate normals
    public int numVertsPerLine{
        get{
            return supportedChunkSizes[(useFlatShading)?flatShadingChunkSizeIndex:chunkSizeIndex] + 5;
        }
    }

    public float meshWorldSize{
        get{
            return(numVertsPerLine - 3) * meshScale;
        }
    }
}

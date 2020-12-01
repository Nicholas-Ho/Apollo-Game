using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartStateTerrainGenerator : MonoBehaviour
{
    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureSettings;
    public int startStateSeed = 0;

    public int colliderLODIndex;
    public Material mapMaterial;
    public LODInfo[] detailLevels;
    public Transform viewer;
    float meshWorldSize;
    int chunksViewableInDist;
    List<TerrainChunk> terrainChunks = new List<TerrainChunk>();

    void Start()
    {
        textureSettings.ApplyToMaterial(mapMaterial);
        textureSettings.UpdateMeshHeights(mapMaterial, heightMapSettings.minHeight * meshSettings.meshScale, heightMapSettings.maxHeight * meshSettings.meshScale);

        float maxViewingDist = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        meshWorldSize = meshSettings.meshWorldSize;
        chunksViewableInDist = Mathf.RoundToInt(maxViewingDist / meshWorldSize);

        heightMapSettings.noiseSettings.seed = startStateSeed;

        int currentChunkCoordX = Mathf.RoundToInt(viewer.position.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewer.position.z / meshWorldSize);

        for(int Yoffset = -chunksViewableInDist; Yoffset <= chunksViewableInDist; Yoffset++){
            for(int Xoffset = -chunksViewableInDist; Xoffset <= chunksViewableInDist; Xoffset++){
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + Xoffset, currentChunkCoordY + Yoffset);

                TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, mapMaterial, viewer, false);
                terrainChunks.Add(newChunk);
                newChunk.Load();
            }
        }
    }
}
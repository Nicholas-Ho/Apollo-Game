using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    const float viewerMoveThreshForChunkUpdate = 25f;
    const float sqrViewerMoveThreshForChunkUpdate = viewerMoveThreshForChunkUpdate * viewerMoveThreshForChunkUpdate;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureSettings;

    public bool endlessMode;
    public int colliderLODIndex;
    public Material mapMaterial;
    public LODInfo[] detailLevels;
    public Transform viewer;
    Vector2 viewerPos;
    Vector2 viewerPosOld;
    float meshWorldSize;
    int chunksViewableInDist;
    Dictionary<Vector2, TerrainChunk> terrainChunkDict = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunkPool = new List<TerrainChunk>();
    static List<TerrainChunk> VisibleTerrainChunks = new List<TerrainChunk>();

    public bool randomizeSeedOnPlay;

    void Start()
    {
        VisibleTerrainChunks.Clear();

        textureSettings.ApplyToMaterial(mapMaterial);
        textureSettings.UpdateMeshHeights(mapMaterial, heightMapSettings.minHeight * meshSettings.meshScale, heightMapSettings.maxHeight * meshSettings.meshScale);

        float maxViewingDist = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        meshWorldSize = meshSettings.meshWorldSize;
        chunksViewableInDist = Mathf.RoundToInt(maxViewingDist / meshWorldSize);

        if(randomizeSeedOnPlay){
            heightMapSettings.noiseSettings.seed = (int)System.DateTime.Now.Ticks;
        }

        if(!endlessMode){
            UpdateVisibleChunks();
        } else {
            UpdateVisibleChunksEndless();
        }
    }

    void Update(){
        viewerPos = new Vector2(viewer.position.x, viewer.position.z);

        if(viewerPos != viewerPosOld){
            foreach(TerrainChunk chunk in VisibleTerrainChunks){
                chunk.UpdateCollisionMesh();
            }
        }

        if((viewerPos - viewerPosOld).sqrMagnitude > sqrViewerMoveThreshForChunkUpdate){
            viewerPosOld = viewerPos;
            
            if(!endlessMode){
                UpdateVisibleChunks();
            } else {
                UpdateVisibleChunksEndless();
            }
        }
    }

    void UpdateVisibleChunks(){
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
        for(int i = VisibleTerrainChunks.Count - 1; i >= 0; i--){
            alreadyUpdatedChunkCoords.Add(VisibleTerrainChunks[i].coord);
            VisibleTerrainChunks[i].UpdateTerrainChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(viewerPos.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPos.y / meshWorldSize);

        for(int Yoffset = -chunksViewableInDist; Yoffset <= chunksViewableInDist; Yoffset++){
            for(int Xoffset = -chunksViewableInDist; Xoffset <= chunksViewableInDist; Xoffset++){
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + Xoffset, currentChunkCoordY + Yoffset);

                if(!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord)){
                    if(terrainChunkDict.ContainsKey(viewedChunkCoord)){
                        terrainChunkDict[viewedChunkCoord].UpdateTerrainChunk();
                    } else {
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, mapMaterial, viewer, false);
                        terrainChunkDict.Add(viewedChunkCoord, newChunk);
                        newChunk.onVisibilityChange += OnChunkVisibilityChange;
                        newChunk.Load();
                    }
                }
            }
        }
    }

    void UpdateVisibleChunksEndless(){
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
        int currentChunkCoordX = Mathf.RoundToInt(viewerPos.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPos.y / meshWorldSize);

        for(int i = VisibleTerrainChunks.Count - 1; i >= 0; i--){
            alreadyUpdatedChunkCoords.Add(VisibleTerrainChunks[i].coord);
            VisibleTerrainChunks[i].UpdateTerrainChunk(currentChunkCoordY); // UpdateTerrainChunk is overloaded
        }

        // Make sure that "forward" is the positive z direction!
        for(int Yoffset = -1; Yoffset <= chunksViewableInDist; Yoffset++){
            for(int Xoffset = -chunksViewableInDist/2; Xoffset <= chunksViewableInDist/2; Xoffset++){
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + Xoffset, currentChunkCoordY + Yoffset);

                if(!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord)){
                    if(terrainChunkDict.ContainsKey(viewedChunkCoord)){
                        terrainChunkDict[viewedChunkCoord].UpdateTerrainChunk(currentChunkCoordY);
                    } else if(terrainChunkPool.Count > 0){
                        TerrainChunk chunk = terrainChunkPool[terrainChunkPool.Count - 1];
                        terrainChunkDict.Remove(chunk.coord);
                        terrainChunkPool.Remove(chunk);

                        chunk.ReassignChunk(viewedChunkCoord);
                        chunk.Load();
                        terrainChunkDict.Add(viewedChunkCoord, chunk);
                    } else {
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, mapMaterial, viewer, true);
                        terrainChunkDict.Add(viewedChunkCoord, newChunk);
                        newChunk.onVisibilityChange += OnChunkVisibilityChange;
                        newChunk.onVisibilityChangeEndless += OnChunkVisibilityChangeEndless;
                        newChunk.Load();
                    }
                }
            }
        }
    }

    void OnChunkVisibilityChange(TerrainChunk chunk, bool isVisible){
        if(isVisible){
            VisibleTerrainChunks.Add(chunk);
        } else {
            VisibleTerrainChunks.Remove(chunk);
        }
    }

    void OnChunkVisibilityChangeEndless(TerrainChunk chunk, bool isVisible, bool isBehind){
        if(isVisible){
            VisibleTerrainChunks.Add(chunk);
        } else {
            VisibleTerrainChunks.Remove(chunk);
            if(isBehind){
                terrainChunkPool.Add(chunk);
            }
        }
    }
}

[System.Serializable]
public struct LODInfo{
    [Range(0, MeshSettings.numSupportedLOD-1)]
    public int lod;
    public float visibleDstThreshold;

    public float sqrVisibleDstThreshold{
        get{
            return visibleDstThreshold * visibleDstThreshold;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{
    public event System.Action<TerrainChunk, bool> onVisibilityChange;
    public event System.Action<TerrainChunk, bool, bool> onVisibilityChangeEndless;
    const float colliderGenerationViewingDist = 5f;

    public Vector2 coord;
    GameObject meshObject;
    Vector2 sampleCentre;
    Bounds bounds;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    LODInfo[] detailLevels;
    LODMesh[] lodMeshes;
    int colliderLODIndex;
    float maxViewingDist;

    HeightMap heightMap;
    bool heightMapReceived;
    int previousLODIndex = -1;

    bool hasSetCollider = false;

    bool endless = false;
    bool wasReassigned = false;

    bool populated = false;
    List<GameObject> objectsInScene = new List<GameObject>();

    HeightMapSettings heightMapSettings;
    MeshSettings meshSettings;
    PopulateSettings populateSettings;

    Transform viewer;
    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, PopulateSettings populateSettings, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Material material, Transform viewer, bool endless)
    {
        this.endless = endless;

        this.viewer = viewer;

        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.populateSettings = populateSettings;

        this.coord = coord;

        this.detailLevels = detailLevels;
        this.colliderLODIndex = colliderLODIndex;

        sampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
        Vector2 position = coord * meshSettings.meshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);

        meshObject = new GameObject("Terrain Chunk");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();

        meshRenderer.material = material;

        meshObject.transform.position = new Vector3(position.x, 0, position.y);
        meshObject.transform.parent = parent;
        SetVisible(false);

        lodMeshes = new LODMesh[detailLevels.Length];
        for(int i = 0; i < detailLevels.Length; i++){
            lodMeshes[i] = new LODMesh(detailLevels[i].lod);
            lodMeshes[i].updateCallback += UpdateTerrainChunk;
            if(i == colliderLODIndex){
                lodMeshes[i].updateCallback += UpdateCollisionMesh;
            }
        }

        this.maxViewingDist = detailLevels[detailLevels.Length-1].visibleDstThreshold;
    }

    public void ReassignChunk(Vector2 coord){
        this.coord = coord;

        sampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
        Vector2 position = coord * meshSettings.meshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);
        meshObject.transform.position = new Vector3(position.x, 0, position.y);

        meshFilter.mesh = null;

        Array.Clear(lodMeshes, 0, lodMeshes.Length);
        for(int i = 0; i < detailLevels.Length; i++){
            lodMeshes[i] = new LODMesh(detailLevels[i].lod);
            lodMeshes[i].updateCallback += UpdateTerrainChunk;
            if(i == colliderLODIndex){
                lodMeshes[i].updateCallback += UpdateCollisionMesh;
            }
        }

        this.heightMapReceived = false;
        this.hasSetCollider = false;

        wasReassigned = true;
    }

    public void Load(){
        ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, sampleCentre), OnHeightMapReceived);
    }

    void OnHeightMapReceived(object HeightMapObject){
        this.heightMap = (HeightMap)HeightMapObject;
        heightMapReceived = true;

        UpdateTerrainChunk();
    }

    Vector2 viewerPos{
        get{
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }

    public void UpdateTerrainChunk()
    {
        bool wasVisible = isVisible();

        if(heightMapReceived){
            float viewerDistFromEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPos));
            bool visible = viewerDistFromEdge < maxViewingDist;

            if(visible){
                int lodIndex = 0;

                for(int i = 0; i < detailLevels.Length - 1; i++){
                    if(viewerDistFromEdge > detailLevels[i].visibleDstThreshold){
                        lodIndex++;
                    } else {
                        break;
                    }
                }

                if(lodIndex != previousLODIndex || wasReassigned){ // wasReassigned is only used during Endless
                    LODMesh lodMesh = lodMeshes[lodIndex];
                    if(lodMesh.hasMesh){
                        previousLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;
                        wasReassigned = false;

                        if(lodIndex == 0 && !populated){
                            Populate();
                            populated = true;
                        }
                    } else if(!lodMesh.hasRequestedMesh){
                        lodMesh.RequestMesh(heightMap, meshSettings);
                    }
                }
            }

            if(wasVisible != visible){
                SetVisible(visible);

                if(onVisibilityChange != null){
                    onVisibilityChange(this, visible);
                }
            }
        }
    }

    public void UpdateTerrainChunkEndless(int currentChunkCoordY)
    {
        bool wasVisible = isVisible();

        if(heightMapReceived){
            float viewerDistFromEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPos));
            bool visible;

            bool isBehind = coord.y < currentChunkCoordY - 1;
            if(isBehind){
                visible = false;
            } else {
                visible = viewerDistFromEdge < maxViewingDist;
            }

            if(visible){
                int lodIndex = 0;

                for(int i = 0; i < detailLevels.Length - 1; i++){
                    if(viewerDistFromEdge > detailLevels[i].visibleDstThreshold){
                        lodIndex++;
                    } else {
                        break;
                    }
                }

                if(lodIndex != previousLODIndex || wasReassigned){
                    LODMesh lodMesh = lodMeshes[lodIndex];
                    if(lodMesh.hasMesh){
                        previousLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;
                        wasReassigned = false;
                    } else if(!lodMesh.hasRequestedMesh){
                        lodMesh.RequestMesh(heightMap, meshSettings);
                    }
                }
            }

            if(wasVisible != visible){
                SetVisible(visible);

                if(onVisibilityChangeEndless != null){
                    onVisibilityChangeEndless(this, visible, isBehind);
                }
            }
        }
    }

    public void UpdateCollisionMesh(){
        if(!hasSetCollider){
            float sqrDistFromViewerToEdge = bounds.SqrDistance(viewerPos);

            if(sqrDistFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDstThreshold){
                if(!lodMeshes[colliderLODIndex].hasRequestedMesh){
                    lodMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings);
                }
            }

            if(sqrDistFromViewerToEdge < colliderGenerationViewingDist * colliderGenerationViewingDist){
                if(lodMeshes[colliderLODIndex].hasMesh){
                    meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                    hasSetCollider = true;
                }
            }
        }
    }

    public void SetVisible(bool visible){
        meshObject.SetActive(visible);
    }

    public bool isVisible(){
        return meshObject.activeSelf;
    }

    void Populate(){
        /*Vector2 regionSize = new Vector2(meshSettings.meshWorldSize, meshSettings.meshWorldSize);
        List<Vector2> objectPositions = PoissonDiscSampling.GeneratePoints(populateSettings.radius, regionSize, populateSettings.numSamplesBeforeRejection);

        if(objectPositions != null){
            foreach(Vector2 objectPosition in objectPositions){
                float xPos = coord.x * meshSettings.meshWorldSize + (objectPosition.x - (meshSettings.meshWorldSize / 2));
                float zPos = coord.y * meshSettings.meshWorldSize + (objectPosition.y - (meshSettings.meshWorldSize / 2));
                Vector3 objectPos3D = new Vector3(0, 0, 0);
            }
        }*/
    }
}

class LODMesh{
    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    int lod;
    public event System.Action updateCallback;

    public LODMesh(int lod){
        this.lod = lod;
    }

    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings){
        hasRequestedMesh = true;
        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, lod, meshSettings), OnMeshDataReceived);
    }

    public void OnMeshDataReceived(object meshDataObject){
        mesh = ((MeshData)meshDataObject).createMesh();
        hasMesh = true;

        updateCallback();
    }
}

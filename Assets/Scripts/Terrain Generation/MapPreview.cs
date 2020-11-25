using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPreview : MonoBehaviour
{
    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureData;
    public PopulateSettings populateSettings;

    public Material terrainMaterial;
    Object[] objectMaterials;


    public enum DrawMode{noiseMap, Mesh, falloffMap, Scene};
    public DrawMode drawMode;
    public enum PopulateMode{Simple, VariableRadii};
    public PopulateMode populateMode;

    public bool autoUpdate;


    [Range(0, MeshSettings.numSupportedLOD)]
    public int editorLevelOfDetail;

    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;

    List<GameObject> sceneObjects = new List<GameObject>();

    public void DrawMapInEditor(){
        int sceneObjectNum = sceneObjects.Count;
        for(int i = 0; i < sceneObjectNum; i++){
            GameObject sceneObject = sceneObjects[sceneObjectNum - 1 - i];
            sceneObjects.Remove(sceneObject);
            DestroyImmediate(sceneObject);
        }

        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, Vector2.zero);

        textureData.ApplyToMaterial(terrainMaterial);
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight * meshSettings.meshScale, heightMapSettings.maxHeight * meshSettings.meshScale);

        if(drawMode == DrawMode.noiseMap){
            DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
        } else if(drawMode == DrawMode.Mesh){
            DrawMesh(GenerateMesh(heightMap.values, editorLevelOfDetail, meshSettings));
        } else if(drawMode == DrawMode.falloffMap){
            DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(FalloffMapGenerator.GenerateFalloffMap(meshSettings.numVertsPerLine), 0, 1)));
        } else if(drawMode == DrawMode.Scene){
            DrawScene(GenerateMesh(heightMap.values, editorLevelOfDetail, meshSettings));
        }
    }

    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height) / 10f;

        textureRenderer.gameObject.SetActive(true);
        meshFilter.gameObject.SetActive(false);
    }

    MeshData GenerateMesh(float[,] heightMap, int levelOfDetail, MeshSettings meshSettings){
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(heightMap, levelOfDetail, meshSettings);

        return meshData;
    }

    public void DrawMesh(MeshData meshData)
    {
        meshFilter.sharedMesh = meshData.createMesh();
        meshCollider.sharedMesh = meshFilter.sharedMesh;

        textureRenderer.gameObject.SetActive(false);
        meshFilter.gameObject.SetActive(true);
    }

    public void DrawScene(MeshData meshData){
        DrawMesh(meshData);
        Populate();
    }

    void Populate(){
        if(populateMode == PopulateMode.Simple){
            float maxHeight = heightMapSettings.maxHeight;
            Transform transform = GetComponent<Transform>();

            Vector2 regionSize = new Vector2(meshSettings.meshWorldSize, meshSettings.meshWorldSize);
            List<Vector2> objectPositions = PoissonDiscSampling.GeneratePoints(populateSettings.radius, regionSize, populateSettings.numSamplesBeforeRejection);

            RaycastHit hit;
            if(objectPositions != null){
                foreach(Vector2 objectPosition in objectPositions){
                    float xPos = 0 * meshSettings.meshWorldSize + (objectPosition.x - (meshSettings.meshWorldSize / 2));
                    float zPos = 0 * meshSettings.meshWorldSize + (objectPosition.y - (meshSettings.meshWorldSize / 2));
                    Ray ray = new Ray(new Vector3(xPos, maxHeight, zPos), Vector3.down);
                    if(meshCollider.Raycast(ray, out hit, maxHeight)){
                        Vector3 objectPos3D = new Vector3(xPos, maxHeight - hit.distance, zPos);
                        Debug.Log(objectPos3D);
                        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal.normalized);
                        sceneObjects.Add(Instantiate(populateSettings.simpleObjectList[Random.Range(0, populateSettings.simpleObjectList.Count)], objectPos3D, rotation));
                        sceneObjects[sceneObjects.Count - 1].transform.rotation = Quaternion.AngleAxis(Random.Range(0, 360), transform.up);

                        sceneObjects[sceneObjects.Count - 1].transform.parent = transform;
                    }
                }
            }
        } else {
            float maxHeight = heightMapSettings.maxHeight;
            Transform transform = GetComponent<Transform>();

            Vector2 regionSize = new Vector2(meshSettings.meshWorldSize, meshSettings.meshWorldSize);
            List<Vector3> objectPositions = PoissonDiscSampling.GeneratePointsVariableRadii(populateSettings.radiiArray, populateSettings.ratioArray, regionSize, populateSettings.numSamplesBeforeRejection, "object_index");

            RaycastHit hit;
            if(objectPositions != null){
                foreach(Vector3 objectPosition in objectPositions){
                    int objectIndex = (int)objectPosition.z;

                    float xPos = 0 * meshSettings.meshWorldSize + (objectPosition.x - (meshSettings.meshWorldSize / 2));
                    float zPos = 0 * meshSettings.meshWorldSize + (objectPosition.y - (meshSettings.meshWorldSize / 2));
                    Ray ray = new Ray(new Vector3(xPos, maxHeight, zPos), Vector3.down);
                    if(meshCollider.Raycast(ray, out hit, maxHeight)){
                        Vector3 objectPos3D = new Vector3(xPos, maxHeight - hit.distance, zPos);
                        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal.normalized);
                        sceneObjects.Add(Instantiate(populateSettings.objectInfoList[objectIndex].gameObject, objectPos3D, rotation));
                        sceneObjects[sceneObjects.Count - 1].transform.rotation = Quaternion.AngleAxis(Random.Range(0, 360), transform.up);

                        sceneObjects[sceneObjects.Count - 1].transform.parent = transform;
                    }
                }
            }
        }
    }

    void OnValuesUpdated(){
        if(!Application.isPlaying){
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated(){
        textureData.ApplyToMaterial(terrainMaterial);
        objectMaterials = Resources.LoadAll("Object Materials/Unity 5");

        for(int i = 0; i < objectMaterials.Length; i++){
            textureData.ApplyToObjectMaterial((Material)objectMaterials[i]);
        }
    }

    void OnValidate(){
        if(heightMapSettings != null){
            heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            heightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }

        if(meshSettings != null){
            meshSettings.OnValuesUpdated -= OnValuesUpdated;
            meshSettings.OnValuesUpdated += OnValuesUpdated;
        }

        if(textureData != null){
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }

        if(populateSettings != null){
            populateSettings.OnValuesUpdated -= OnValuesUpdated;
            populateSettings.OnValuesUpdated += OnValuesUpdated;
            populateSettings.OnValuesUpdated -= OnTextureValuesUpdated;
            populateSettings.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }
}
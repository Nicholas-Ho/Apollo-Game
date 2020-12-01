using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopulationObjectManager : MonoBehaviour
{
    public PopulateSettings populateSettings;
    [SerializeField]
    protected TerrainGenerator terrainGenerator;
    public float endlessDistBehindViewer = 20;
    protected Transform viewer;
    protected float maxViewDist;
    bool endless;
    List<PopulationObjectData> gameObjectManager = new List<PopulationObjectData>();
    List<PopulationObjectData> activeGameObjects = new List<PopulationObjectData>();
    protected Dictionary<int, List<PopulationObjectData>> gameObjectPool = new Dictionary<int, List<PopulationObjectData>>(); // A list for each object type (object index as key) holding gameObjectManager index

    // Start is called before the first frame update
    void Start()
    {
        viewer = terrainGenerator.viewer;
        maxViewDist = terrainGenerator.detailLevels[terrainGenerator.colliderLODIndex].visibleDstThreshold + Mathf.Sqrt(2) * terrainGenerator.meshSettings.meshWorldSize;
        endless = terrainGenerator.endlessMode;

        for(int i = 0; i < populateSettings.objectInfoList.Count; i++){
            gameObjectPool[i] = new List<PopulationObjectData>();
        }
    }

    void Update(){
        UpdateAllGameObjects();
    }

    public void Populate(Vector2 coord, float maxHeight, float meshWorldSize, MeshCollider meshCollider){

        Vector2 regionSize = new Vector2(meshWorldSize, meshWorldSize);
        List<Vector3> objectPositions = PoissonDiscSampling.GeneratePointsVariableRadii(populateSettings.radiiArray, populateSettings.ratioArray, regionSize, populateSettings.numSamplesBeforeRejection, "object_index");

        RaycastHit hit;
        if(objectPositions != null && objectPositions.Count > 0){
            foreach(Vector3 objectPosition in objectPositions){
                int objectIndex = (int)objectPosition.z;

                float xPos = coord.x * meshWorldSize + (objectPosition.x - (meshWorldSize / 2));
                float zPos = coord.y * meshWorldSize + (objectPosition.y - (meshWorldSize / 2));
                Ray ray = new Ray(new Vector3(xPos, maxHeight, zPos), Vector3.down);
                if(meshCollider.Raycast(ray, out hit, maxHeight)){
                    Vector3 objectPos3D = new Vector3(xPos, maxHeight - hit.distance, zPos);
                    if(gameObjectPool[objectIndex].Count > 0){
                        PopulationObjectData reassignedObjectData = gameObjectPool[objectIndex][gameObjectPool[objectIndex].Count - 1];
                        GameObject reassignedObject = reassignedObjectData.gameObject;

                        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal.normalized);
                        reassignedObject.transform.position = objectPos3D;
                        reassignedObject.transform.rotation = rotation;
                        reassignedObject.transform.rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), reassignedObject.transform.up);

                        reassignedObject.SetActive(true);
                        activeGameObjects.Add(reassignedObjectData);
                        gameObjectPool[objectIndex].Remove(reassignedObjectData);
                    } else {
                        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal.normalized);
                        GameObject newObject = GameObject.Instantiate(populateSettings.objectInfoList[objectIndex].gameObject, objectPos3D, rotation);
                        newObject.transform.rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), newObject.transform.up);

                        newObject.transform.parent = transform;
                        PopulationObjectData info = new PopulationObjectData();
                        info.gameObject = newObject;
                        info.objectIndex = objectIndex;
                        activeGameObjects.Add(info);
                        gameObjectManager.Add(info);
                    }
                }
            }
        }
    }

    void UpdateAllGameObjects(){
        for(int i = activeGameObjects.Count - 1; i >= 0; i--){
            /*float distX = activeGameObjects[i].transform.position.x - viewer.position.x;
            float distY = activeGameObjects[i].transform.position.y - viewer.position.y;
            float distZ = activeGameObjects[i].transform.position.z - viewer.position.z;*/
            Vector3 gameObjectPos = activeGameObjects[i].gameObject.transform.position;
            float dist = Vector3.Distance(gameObjectPos, viewer.position);
            if(dist > maxViewDist || (endless && gameObjectPos.z < viewer.position.z - endlessDistBehindViewer)){
                activeGameObjects[i].gameObject.SetActive(false);
                gameObjectPool[activeGameObjects[i].objectIndex].Add(activeGameObjects[i]);

                activeGameObjects.RemoveAt(i);
            }
        }
    }
}

public struct PopulationObjectData{
    public GameObject gameObject;
    public int objectIndex;
}
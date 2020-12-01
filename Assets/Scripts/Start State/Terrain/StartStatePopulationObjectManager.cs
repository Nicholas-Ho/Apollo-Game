using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartStatePopulationObjectManager : PopulationObjectManager
{
    public StartStateTerrainGenerator startStateTerrainGenerator;

    void Start()
    {
        viewer = startStateTerrainGenerator.viewer;
        maxViewDist = startStateTerrainGenerator.detailLevels[startStateTerrainGenerator.colliderLODIndex].visibleDstThreshold + Mathf.Sqrt(2) * startStateTerrainGenerator.meshSettings.meshWorldSize;

        for(int i = 0; i < populateSettings.objectInfoList.Count; i++){
            gameObjectPool[i] = new List<PopulationObjectData>();
        }
    }

    void Update()
    {
        
    }
}

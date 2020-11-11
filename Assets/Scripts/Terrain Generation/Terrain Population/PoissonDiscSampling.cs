using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PoissonDiscSampling
{
    public static List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection){
        float cellSize = radius / Mathf.Sqrt(2);

        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();

        spawnPoints.Add(sampleRegionSize / 2);

        while(spawnPoints.Count > 0){
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Vector2 spawnCentre = spawnPoints[spawnIndex];
            bool pointAccepted = false;

            for(int i = 0; i < numSamplesBeforeRejection; i++){
                float angle = Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 newPoint = spawnCentre + dir * Random.Range(radius, 2 * radius);

                if(isValid(newPoint, radius, sampleRegionSize, cellSize, points, grid)){
                    points.Add(newPoint);
                    spawnPoints.Add(newPoint);
                    grid[(int)(newPoint.x / cellSize), (int)(newPoint.y / cellSize)] = points.Count;
                    pointAccepted = true;

                    break;
                }
            }

            if(!pointAccepted){
                spawnPoints.RemoveAt(spawnIndex);
            }
        }

        return points;
    }

    static bool isValid(Vector2 newPoint, float radius, Vector2 sampleRegionSize, float cellSize, List<Vector2> points, int[,] grid){
        if(newPoint.x >= 0 && newPoint.x < sampleRegionSize.x && newPoint.y >= 0 && newPoint.y < sampleRegionSize.y){
            int cellX = (int)(newPoint.x / cellSize);
            int cellY = (int)(newPoint.y / cellSize);

            int searchStartX = Mathf.Max(0, cellX - 2);
            int searchStartY = Mathf.Max(0, cellY - 2);
            int searchEndX = Mathf.Min(grid.GetLength(0) - 1, cellX + 2);
            int searchEndY = Mathf.Min(grid.GetLength(1) - 1, cellY + 2);

            for(int i = searchStartX; i <= searchEndX; i++){
                for(int j = searchStartY; j <= searchEndY; j++){
                    int pointIndex = grid[i, j] - 1;
                    if(pointIndex != -1){
                        float sqrDist = (newPoint - points[pointIndex]).sqrMagnitude;

                        if(sqrDist < radius * radius){
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        return false;
    }

    public static List<Vector2> GeneratePointsVariableRadii(float[] radius, float[] ratios, Vector2 sampleRegionSize, int numSamplesBeforeRejection){
        float minRadius = float.MaxValue;
        float ratioSum = 0;
        for(int i = 0; i < radius.Length; i++){
            if(radius[i] < minRadius){
                minRadius = radius[i];
            }

            ratioSum += ratios[i];
        }

        float[] probabilities = new float[ratios.Length];
        probabilities[0] = ratios[0] / ratioSum;
        for(int i = 1; i < probabilities.Length; i++){
            probabilities[i] = (ratios[i] / ratioSum) + probabilities[i - 1];
        }

        float cellSize = minRadius / Mathf.Sqrt(2);

        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
        List<Vector3> points = new List<Vector3>(); // points.x and points.y are coordinates, points.z is the radius of the point
        List<Vector2> pointCoords = new List<Vector2>();
        List<Vector3> spawnPoints = new List<Vector3>();
        float maxRadius = 0;

        spawnPoints.Add(new Vector3(sampleRegionSize.x / 2, sampleRegionSize.y / 2, radius[Random.Range(0, radius.Length)]));

        while(spawnPoints.Count > 0){
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Vector3 spawnCentre = spawnPoints[spawnIndex];
            bool pointAccepted = false;

            for(int i = 0; i < numSamplesBeforeRejection; i++){
                float angle = Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 newPointCoords = new Vector2(spawnCentre.x, spawnCentre.y) + dir * Random.Range(spawnCentre.z, 2 * spawnCentre.z);
                float newRadius = minRadius;
                float radiusDeterminant = Random.Range(0f, 1f);
                for(int j = 0; j < probabilities.Length; j++){
                    if(radiusDeterminant < probabilities[j]){
                        newRadius = radius[j];
                        if(newRadius > maxRadius){
                            maxRadius = newRadius;
                        }
                        break;
                    }
                }
                Vector3 newPoint = new Vector3(newPointCoords.x, newPointCoords.y, newRadius);

                if(isValidVariableRadii(newPoint, maxRadius, sampleRegionSize, cellSize, points, grid)){
                    points.Add(newPoint);
                    pointCoords.Add(newPointCoords);
                    spawnPoints.Add(newPoint);
                    grid[(int)(newPoint.x / cellSize), (int)(newPoint.y / cellSize)] = points.Count;
                    pointAccepted = true;

                    break;
                }
            }

            if(!pointAccepted){
                spawnPoints.RemoveAt(spawnIndex);
            }
        }

        return pointCoords;
    }

    // Overload for returning Vector3 (points with radii). Takes in extra string returnMode.
    public static List<Vector3> GeneratePointsVariableRadii(float[] radius, float[] ratios, Vector2 sampleRegionSize, int numSamplesBeforeRejection, string returnMode){
        float minRadius = float.MaxValue;
        float ratioSum = 0;
        for(int i = 0; i < radius.Length; i++){
            if(radius[i] < minRadius){
                minRadius = radius[i];
            }

            ratioSum += ratios[i];
        }

        float[] probabilities = new float[ratios.Length];
        probabilities[0] = ratios[0] / ratioSum;
        for(int i = 1; i < probabilities.Length; i++){
            probabilities[i] = (ratios[i] / ratioSum) + probabilities[i - 1];
        }

        float cellSize = minRadius / Mathf.Sqrt(2);

        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
        List<Vector3> points = new List<Vector3>(); // points.x and points.y are coordinates, points.z is the radius of the point
        List<Vector3> pointCoords = new List<Vector3>();
        List<Vector3> spawnPoints = new List<Vector3>();
        float maxRadius = 0;

        spawnPoints.Add(new Vector3(sampleRegionSize.x / 2, sampleRegionSize.y / 2, radius[Random.Range(0, radius.Length)]));

        while(spawnPoints.Count > 0){
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Vector3 spawnCentre = spawnPoints[spawnIndex];
            bool pointAccepted = false;

            for(int i = 0; i < numSamplesBeforeRejection; i++){
                float angle = Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 newPointCoords = new Vector2(spawnCentre.x, spawnCentre.y) + dir * Random.Range(spawnCentre.z, 2 * spawnCentre.z);
                float newRadius = minRadius;
                float radiusDeterminant = Random.Range(0f, 1f);
                float radiusIndex = 0;
                for(int j = 0; j < probabilities.Length; j++){
                    if(radiusDeterminant < probabilities[j]){
                        newRadius = radius[j];
                        radiusIndex = j;
                        if(newRadius > maxRadius){
                            maxRadius = newRadius;
                        }
                        break;
                    }
                }
                Vector3 newPoint = new Vector3(newPointCoords.x, newPointCoords.y, newRadius);

                if(isValidVariableRadii(newPoint, maxRadius, sampleRegionSize, cellSize, points, grid)){
                    points.Add(newPoint);
                    pointCoords.Add(new Vector3(newPointCoords.x, newPointCoords.y, (returnMode == "object_index") ? radiusIndex : 0));
                    spawnPoints.Add(newPoint);
                    grid[(int)(newPoint.x / cellSize), (int)(newPoint.y / cellSize)] = points.Count;
                    pointAccepted = true;

                    break;
                }
            }

            if(!pointAccepted){
                spawnPoints.RemoveAt(spawnIndex);
            }
        }

        if(returnMode == "radii"){
            return points;
        } else { // Returns list of (x, y, object index) if returnMode is "object_index", returns list of (x, y, 0) otherwise.
            return pointCoords;
        }
    }

    static bool isValidVariableRadii(Vector3 newPoint, float maxRadius, Vector2 sampleRegionSize, float cellSize, List<Vector3> points, int[,] grid){
        if(newPoint.x >= 0 && newPoint.x < sampleRegionSize.x && newPoint.y >= 0 && newPoint.y < sampleRegionSize.y){
            int cellX = (int)(newPoint.x / cellSize);
            int cellY = (int)(newPoint.y / cellSize);

            float radius = newPoint.z;
            int searchSquares = Mathf.CeilToInt(maxRadius / cellSize);

            int searchStartX = Mathf.Max(0, cellX - searchSquares);
            int searchStartY = Mathf.Max(0, cellY - searchSquares);
            int searchEndX = Mathf.Min(grid.GetLength(0) - 1, cellX + searchSquares);
            int searchEndY = Mathf.Min(grid.GetLength(1) - 1, cellY + searchSquares);

            for(int i = searchStartX; i <= searchEndX; i++){
                for(int j = searchStartY; j <= searchEndY; j++){
                    int pointIndex = grid[i, j] - 1;
                    if(pointIndex != -1){
                        float sqrDist = (newPoint - points[pointIndex]).sqrMagnitude;
                        float effectiveRadius = Mathf.Max(radius, points[pointIndex].z);

                        if(sqrDist < effectiveRadius * effectiveRadius){
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        return false;
    }
}

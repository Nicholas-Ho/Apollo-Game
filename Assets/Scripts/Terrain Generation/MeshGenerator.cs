using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, int levelOfDetail, MeshSettings meshSettings)
    {
        int skipIncrement = (levelOfDetail == 0)?1:levelOfDetail * 2;

        int numVertsPerLine = meshSettings.numVertsPerLine;

        Vector2 topLeft = new Vector2(-1, 1) * meshSettings.meshWorldSize / 2f;

        MeshData meshData = new MeshData(numVertsPerLine, skipIncrement, meshSettings.useFlatShading);
        int[,] vertexIndicesMap = new int[numVertsPerLine, numVertsPerLine];
        int meshIndex = 0;
        int outOfMeshVertIndex = -1;

        for(int y = 0; y < numVertsPerLine; y ++){
            for(int x = 0; x < numVertsPerLine; x ++){
                bool isOutOfMesh = x == 0 || x == numVertsPerLine - 1 || y == 0 || y == numVertsPerLine - 1;
                bool isSkippedVert = x > 2 && x < numVertsPerLine - 3 && y > 2 && y < numVertsPerLine - 3 && ((x - 2) % skipIncrement != 0 || (y - 2) % skipIncrement != 0);

                if(isOutOfMesh){
                    vertexIndicesMap[x, y] = outOfMeshVertIndex;
                    outOfMeshVertIndex--;
                } else if(!isSkippedVert) {
                    vertexIndicesMap[x, y] = meshIndex;
                    meshIndex++;
                }
            }
        }
        
        for(int y = 0; y < numVertsPerLine; y ++){
            for(int x = 0; x < numVertsPerLine; x ++){
                bool isSkippedVert = x > 2 && x < numVertsPerLine - 3 && y > 2 && y < numVertsPerLine - 3 && ((x - 2) % skipIncrement != 0 || (y - 2) % skipIncrement != 0);

                if(!isSkippedVert){
                    bool isOutOfMesh = x == 0 || x == numVertsPerLine - 1 || y == 0 || y == numVertsPerLine - 1;
                    bool isEdgeMeshVert = (y == 1 || y == numVertsPerLine - 2 || x == 1 || x == numVertsPerLine - 2) && !isOutOfMesh;
                    bool isMainMeshVert = ((x - 2) % skipIncrement == 0 && (y - 2) % skipIncrement == 0) && !isOutOfMesh && !isEdgeMeshVert;
                    bool isEdgeConnVert = (y == 2 || y == numVertsPerLine -3 || x == 2 || x == numVertsPerLine - 3) && !isOutOfMesh && !isEdgeMeshVert && !isMainMeshVert;

                    int vertexIndex = vertexIndicesMap[x, y];

                    Vector2 percent = new Vector2(x-1, y-1) / (numVertsPerLine - 3);
                    float height = heightMap[x, y];
                    Vector2 vertexPosition2D = topLeft + new Vector2(percent.x, -percent.y) * meshSettings.meshWorldSize;

                    if(isEdgeConnVert){
                        bool isVertical = x == 2 || x == numVertsPerLine - 3;
                        int distFromMainVertA = ((isVertical) ? y - 2 : x - 2) % skipIncrement;
                        int distFromMainVertB = skipIncrement - distFromMainVertA;
                        float distPercentFromAToB = distFromMainVertA / (float)skipIncrement;

                        float heightMainVertA = heightMap[(isVertical) ? x : x - distFromMainVertA, (isVertical) ? y - distFromMainVertA : y];
                        float heightMainVertB = heightMap[(isVertical) ? x : x + distFromMainVertB, (isVertical) ? y + distFromMainVertB : y];

                        height = (1 - distPercentFromAToB) * heightMainVertA + distPercentFromAToB * heightMainVertB;
                    }

                    meshData.addVertex(new Vector3(vertexPosition2D.x, height * meshSettings.meshScale, vertexPosition2D.y), percent, vertexIndex);

                    bool createTriangle = x < numVertsPerLine - 1 && y < numVertsPerLine - 1 && (!isEdgeConnVert || (x != 2 && y != 2));

                    if(createTriangle){
                        int currentIncrement = (isMainMeshVert && x != numVertsPerLine - 3 && y != numVertsPerLine - 3)?skipIncrement:1;

                        int a = vertexIndicesMap[x, y];
                        int b = vertexIndicesMap[x + currentIncrement, y];
                        int c = vertexIndicesMap[x, y + currentIncrement];
                        int d = vertexIndicesMap[x + currentIncrement, y + currentIncrement];

                        meshData.addTriangles(a, d, c);
                        meshData.addTriangles(d, a, b);
                    }
                }
            }
        }
        meshData.ProcessMesh();

        return meshData;
    }
}

public class MeshData
{
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;

    Vector3[] outOfMeshVertices;
    int[] outOfMeshTriangles;

    int triangleIndex;
    int outOfMeshTriangleIndex;

    Vector3[] bakedNormals;

    bool useFlatShading;


    public MeshData(int numVertsPerLine, int skipIncrement, bool useFlatShading){
        this.useFlatShading = useFlatShading;

        int numEdgeVerts = ((numVertsPerLine - 2) * 4 - 4);
        int numConnVerts = (skipIncrement - 1) * (numVertsPerLine - 5) / skipIncrement * 4;
        int numMainVertsPerLine = (numVertsPerLine - 5) / skipIncrement * 4 + 1;
        int numMainVerts = numMainVertsPerLine * numMainVertsPerLine;

        vertices = new Vector3[numEdgeVerts + numConnVerts + numMainVerts];
        uvs = new Vector2[vertices.Length];
        
        int numMeshEdgeTriangles = 8 * (numVertsPerLine - 4);
        int numMainTriangles = (numMainVertsPerLine - 1) * (numMainVertsPerLine - 1);
        triangles = new int[(numMeshEdgeTriangles + numMainTriangles) * 3];

        outOfMeshVertices = new Vector3[numVertsPerLine * 4 - 4];
        outOfMeshTriangles = new int[24 * (numVertsPerLine - 2)];
    }

    public void addVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex){
        if(vertexIndex < 0){
            outOfMeshVertices[-vertexIndex - 1] = vertexPosition;
        } else {
            vertices[vertexIndex] = vertexPosition;
            uvs[vertexIndex] = uv;
        }
    }

    public void addTriangles(int a, int b, int c){
        if(a < 0 || b < 0 || c < 0){
            outOfMeshTriangles[outOfMeshTriangleIndex] = a;
            outOfMeshTriangles[outOfMeshTriangleIndex + 1] = b;
            outOfMeshTriangles[outOfMeshTriangleIndex + 2] = c;

            outOfMeshTriangleIndex += 3;
        } else {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;

            triangleIndex += 3;
        }
    }

    Vector3[] CalculateNormals(){
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        //Rendered Triangles
        int triangleCount = triangles.Length / 3;

        for(int i = 0; i < triangleCount; i++){
            int normalTriangleIndex = i * 3;
            int triangleVertexA = triangles[normalTriangleIndex];
            int triangleVertexB = triangles[normalTriangleIndex + 1];
            int triangleVertexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(triangleVertexA, triangleVertexB, triangleVertexC);
            vertexNormals[triangleVertexA] += triangleNormal;
            vertexNormals[triangleVertexB] += triangleNormal;
            vertexNormals[triangleVertexC] += triangleNormal;
        }

        //Border Triangles
        int borderTriangleCount = outOfMeshTriangles.Length / 3;

        for(int i = 0; i < borderTriangleCount; i++){
            int normalTriangleIndex = i * 3;
            int triangleVertexA = outOfMeshTriangles[normalTriangleIndex];
            int triangleVertexB = outOfMeshTriangles[normalTriangleIndex + 1];
            int triangleVertexC = outOfMeshTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(triangleVertexA, triangleVertexB, triangleVertexC);
            if(triangleVertexA >= 0){
                vertexNormals[triangleVertexA] += triangleNormal;
            }
            if(triangleVertexB >= 0){
                vertexNormals[triangleVertexB] += triangleNormal;
            }
            if(triangleVertexC >= 0){
                vertexNormals[triangleVertexC] += triangleNormal;
            }
        }

        for(int i = 0; i < vertexNormals.Length; i++){
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    void BakeNormals(){
        bakedNormals = CalculateNormals();
    }

    Vector3 SurfaceNormalFromIndices(int a, int b, int c){
        Vector3 vertexA = (a < 0) ? outOfMeshVertices[-a - 1] : vertices[a];
        Vector3 vertexB = (b < 0) ? outOfMeshVertices[-b - 1] : vertices[b];
        Vector3 vertexC = (c < 0) ? outOfMeshVertices[-c - 1] : vertices[c];

        Vector3 ab = vertexB - vertexA;
        Vector3 ac = vertexC - vertexA;

        return Vector3.Cross(ab, ac).normalized;
    }

    void FlatShading(){
        Vector3[] flatShadedVertices = new Vector3[triangles.Length];
        Vector2[] flatShadedUvs = new Vector2[triangles.Length];

        for(int i = 0; i < triangles.Length; i++){
            flatShadedVertices[i] = vertices[triangles[i]];
            flatShadedUvs[i] = uvs[triangles[i]];
            triangles[i] = i;
        }

        vertices = flatShadedVertices;
        uvs = flatShadedUvs;
    }

    public void ProcessMesh(){
        if(useFlatShading){
            FlatShading();
        } else {
            BakeNormals();
        }
    }

    public Mesh createMesh(){
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        if(useFlatShading){
            mesh.RecalculateNormals();
        } else {
            mesh.normals = bakedNormals;
        }
        
        return mesh;
    }
}
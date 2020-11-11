using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalloffMapGenerator
{
    public static float[,] GenerateFalloffMap(int size)
    {
        float[,] map = new float[size, size];

        for(int i = 0; i < size; i++){
            for(int j = 0; j < size; j++){
                float x = (i/(float)size) * 2 - 1;
                float y = (j/(float)size) * 2 - 1;

                float maxValue = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));

                map[i, j] = FalloffFunction(maxValue);
            }
        }

        return map;
    }

    static float FalloffFunction(float x){
        float a = 3f;
        float b = 2.2f;

        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow((b - b * x), a));
    }
}

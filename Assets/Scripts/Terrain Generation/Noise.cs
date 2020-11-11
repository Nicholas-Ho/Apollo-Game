using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public enum NormalizeMode {Local, Global};
    public enum NoiseFunction {Perlin, Billow, Ridged};
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseSettings settings, Vector2 sampleCentre)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        float maxPossibleHeight = 0f;
        float amplitude = 1f;

        System.Random prng = new System.Random(settings.seed);
        Vector2[] octaveOffset = new Vector2[settings.octaves];
        for(int i = 0; i < settings.octaves; i++){
            float offsetX = prng.Next(-100000, 100000) + settings.offset.x + sampleCentre.x;
            float offsetY = prng.Next(-100000, 100000) - settings.offset.y - sampleCentre.y;
            octaveOffset[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= settings.persistence;
        }

        if(settings.scale <= 0){
            settings.scale = 0.0001f;
        }

        float maxLocalHeight = float.MinValue;
        float minLocalHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for(int y = 0; y < mapHeight; y++){
            for(int x = 0; x < mapWidth; x++){
                float noiseHeight = GetNoiseValue(x, y, halfWidth, halfHeight, octaveOffset, settings);

                if(noiseHeight > maxLocalHeight){
                    maxLocalHeight = noiseHeight;
                }
                if(noiseHeight < minLocalHeight){
                    minLocalHeight = noiseHeight;
                }

                noiseMap[x,y] = noiseHeight;

                if(settings.normalizeMode == NormalizeMode.Global){
                    noiseMap[x,y] = NormalizeHeight(noiseMap[x,y], maxPossibleHeight, settings.noiseFunction);
                }
            }
        }

        if(settings.normalizeMode == NormalizeMode.Local){
            for(int y = 0; y < mapHeight; y++){
                for(int x = 0; x < mapWidth; x++){
                    noiseMap[x,y] = Mathf.InverseLerp(minLocalHeight, maxLocalHeight, noiseMap[x,y]);
                }
            }
        }
        
        return noiseMap;
    }

    static float GetNoiseValue(int x, int y, float halfWidth, float halfHeight, Vector2[] octaveOffset, NoiseSettings settings){
        float noiseHeight = 0;
        float amplitude = 1f;
        float frequency = 1f;
        
        if(settings.noiseFunction == NoiseFunction.Perlin){
            // Noise Scale = 50
            //Persistence = 0.4/0.25, Lacunarity = 2.1/3
            
            for(int i = 0; i < settings.octaves; i++){
                float sampleX = (x - halfWidth + octaveOffset[i].x) / settings.scale * frequency;
                float sampleY = (y - halfHeight + octaveOffset[i].y) / settings.scale * frequency;

                float perlinNoise = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                noiseHeight += perlinNoise * amplitude;

                amplitude *= settings.persistence;
                frequency *= settings.lacunarity;
            }
        } else if(settings.noiseFunction == NoiseFunction.Billow){
            // Noise Scale = 75

            for(int i = 0; i < settings.octaves; i++){
                float sampleX = (x - halfWidth + octaveOffset[i].x) / settings.scale * frequency;
                float sampleY = (y - halfHeight + octaveOffset[i].y) / settings.scale * frequency;

                float perlinNoise = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                noiseHeight += perlinNoise * amplitude;

                amplitude *= settings.persistence;
                frequency *= settings.lacunarity;
            }

            noiseHeight = Mathf.Abs(noiseHeight);

        } else if(settings.noiseFunction == NoiseFunction.Ridged){
            // Noise Scale = 150

            for(int i = 0; i < settings.octaves; i++){
                float sampleX = (x - halfWidth + octaveOffset[i].x) / settings.scale * frequency;
                float sampleY = (y - halfHeight + octaveOffset[i].y) / settings.scale * frequency;

                float perlinNoise = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                noiseHeight += perlinNoise * amplitude;

                amplitude *= settings.persistence;
                frequency *= settings.lacunarity;
            }

            noiseHeight = 1 - Mathf.Abs(noiseHeight);
        }
        return noiseHeight;
    }

    static float NormalizeHeight(float value, float maxPossibleHeight, NoiseFunction noiseFunction){
        float normalizedHeight = 1f;

        if(noiseFunction == NoiseFunction.Perlin){
            normalizedHeight = (value + 1) / (2f * maxPossibleHeight / 1.75f); //1.75 is an arbitrary estimation
        } else if(noiseFunction == NoiseFunction.Billow){
            normalizedHeight = value / (maxPossibleHeight / 1.25f); //1.75 is an arbitrary estimation
        } if(noiseFunction == NoiseFunction.Ridged){
            normalizedHeight = value / (maxPossibleHeight / 1.5f); //1.75 is an arbitrary estimation
        }

        return Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
    }
}

[System.Serializable]
public class NoiseSettings{
    public int seed;
    public Vector2 offset;
    public float scale = 50f;
    public int octaves = 3;
    [Range(0,1)]
    public float persistence = 0.4f;
    public float lacunarity = 2f;
    public Noise.NoiseFunction noiseFunction;
    public Noise.NormalizeMode normalizeMode;

    public void ValidateValues(){
        scale = Mathf.Max(scale, 0.001f);
        octaves = Mathf.Max(octaves, 1);
        lacunarity = Mathf.Max(lacunarity, 1);
        persistence = Mathf.Clamp01(persistence);
    }
}

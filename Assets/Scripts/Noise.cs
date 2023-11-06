using UnityEngine;

namespace TerrainProceduralGenerator
{
    public static class Noise
    {
        public enum NormalizeMode
        {
            Local,
            Global
        }

        public static float[,] GenerationNoiseMap(int mapWidth, int mapHeight, NoiseSettings settings,
            Vector2 sampleCenter)
        {
            System.Random prng = new System.Random(settings.seed);
            Vector2[] octaveOffsets = new Vector2[settings.octaves];
            float maxPossibleHeight = 0;
            float amplitude = 1;
            float frequency = 1;

            for (int i = 0; i < settings.octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + settings.offset.x + sampleCenter.x;
                float offsetY = prng.Next(-100000, 100000) - settings.offset.y - sampleCenter.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);

                maxPossibleHeight += amplitude;
                amplitude *= settings.persistance;
            }

            float[,] noiseMap = new float[mapWidth, mapHeight];
            float[,] fallOf = FallOfGenerator.GenerateFalloffMap(mapWidth);

            float maxLocalNoiseHeight = float.MinValue;
            float minLocalNoiseHeight = float.MaxValue;

            float halfWidth = mapWidth / 2;
            float halfHeight = mapHeight / 2;

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {

                    amplitude = 1;
                    frequency = 1;
                    float noiseHeight = 0;
                    for (int i = 0; i < settings.octaves; i++)
                    {
                        float sampleX = (x - halfWidth + octaveOffsets[i].x) / settings.scale * frequency;
                        float sampleY = (y - halfHeight + octaveOffsets[i].y) / settings.scale * frequency;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        
                        noiseHeight += perlinValue * amplitude - fallOf[x, y]/* * mult*/;
                        
                        amplitude *= settings.persistance;
                        frequency *= settings.lacunarity;
                    }

                    if (noiseHeight > maxLocalNoiseHeight)
                    {
                        maxLocalNoiseHeight = noiseHeight;
                    }

                    if (noiseHeight < minLocalNoiseHeight)
                    {
                        minLocalNoiseHeight = noiseHeight;
                    }

                    noiseMap[x, y] = noiseHeight;
                    if (settings.normalizeMode == NormalizeMode.Global)
                    {
                        float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight);
                        noiseMap[x, y] =
                            Mathf.Clamp(normalizedHeight, 0,
                                int.MaxValue); // Ограничения мак и мин (изменить мин для низин)
                    }
                }
            }

            if (settings.normalizeMode == NormalizeMode.Local)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    for (int x = 0; x < mapWidth; x++)
                    {
                        noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                    }
                }
            }

            return noiseMap;
        }
    }

    [System.Serializable]
    public class NoiseSettings
    {
        public Noise.NormalizeMode normalizeMode;

        public float scale = 10000;
        public int octaves = 7;
        [Range(0, 1)] public float persistance = .5f;
        public float lacunarity = 3;
        public int seed = 0;
        public Vector2 offset;

        public NoiseSettings(float scale, int octaves, float persistance, float lacunarity, int seed, Vector2 offset,
            Noise.NormalizeMode normalizeMode)
        {
            this.scale = scale;
            this.octaves = octaves;
            this.persistance = persistance;
            this.lacunarity = lacunarity;
            this.seed = seed;
            this.offset = offset;
            this.normalizeMode = normalizeMode;
        }

        public void ValidateValues()
        {
            scale = Mathf.Max(scale, 0.01f);
            octaves = Mathf.Max(octaves, 1);
            lacunarity = Mathf.Max(lacunarity, 1);
            persistance = Mathf.Clamp01(persistance);
        }
    }
}
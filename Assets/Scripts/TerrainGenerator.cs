using System.Collections.Generic;
using TerrainProceduralGenerator;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] private float multiplier;
    [SerializeField] private Material terrainMaterial;
    [SerializeField] private Terrain _terrain;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] [Range(0, 1)] private float variantNoise;
    [SerializeField] private bool startedTerrainGenerator;
    [SerializeField] private float minHeight;
    [SerializeField] private float maxHeight;
    [SerializeField] private GameObject treePrefab;
    [SerializeField] private GameObject bushesPrefab;
    [SerializeField] private int treeCount;
    [SerializeField] private int bushesCount;
    [SerializeField] private GameObject playerGo;

    private List<Terrain> terrains = new List<Terrain>();
    private List<Chunk> _chunks = new List<Chunk>();
    private Chunk root;

    [Header("Noise param")] [SerializeField]
    private float scale = 10000;

    [SerializeField] private int octaves = 7;
    [SerializeField] [Range(0, 1)] private float persistance = .5f;
    [SerializeField] private float lacunarity = 3;
    [SerializeField] private int seed = 0;
    [SerializeField] private Vector2 offset;
    [SerializeField] private Noise.NormalizeMode normalizeMode;
    [SerializeField] private TreeAndBushes _treeAndBushes;

    void Start()
    {
        var res = _terrain.terrainData.heightmapResolution;

        _terrain.terrainData.SetHeights(0, 0, PerlinNoiseGeneration(res, variantNoise));

        terrains.Add(_terrain);

        _terrain.materialTemplate = terrainMaterial;
        Chunk chunk = _terrain.GetComponent<Chunk>();
        _chunks.Add(chunk);

        root = chunk;

        if (startedTerrainGenerator)
        {
            TerrainChunksAdding(8, _terrain, res, variantNoise);
        }
    }

    // Update is called once per frame
    void Update()
    {
        root.AddNeighbor(_chunks);

        for (int i = 0; i < _chunks.Count; i++)
        {
            float distanceBetweenRoot = Vector3.Distance(playerGo.transform.position, root.terrain.GetPosition());
            float distanceBetweenChunk =
                Vector3.Distance(playerGo.transform.position, _chunks[i].terrain.GetPosition());
            if (distanceBetweenRoot > distanceBetweenChunk)
            {
                root = _chunks[i];
                TerrainChunksAdding(8, root.terrain, root.terrain.terrainData.heightmapResolution, variantNoise);
            }

            if (distanceBetweenChunk > root.terrain.terrainData.size.x * 2)
            {
                if (!_chunks[i].Equals(null))
                {
                    _chunks[i].gameObject.SetActive(false);
                }
            }
        }
    }

    private float[,] PerlinNoiseGeneration(int res, float variant)
    {
        var mesh = new float[res, res];

        if (variant > 0.5f)
        {
            for (int x = 0; x < res; x++)
            {
                for (int y = 0; y < res; y++)
                {
                    mesh[x, y] = Mathf.PerlinNoise(x * multiplier, y * multiplier) * multiplier;
                }
            }
        }
        else
        {
            mesh = Noise.GenerationNoiseMap(width, height,
                new NoiseSettings(scale, octaves, persistance, lacunarity, seed, offset, normalizeMode), Vector2.zero);
        }

        return mesh;
    }

    private void TerrainChunksAdding(int number, Terrain rootTerrain, int res, float variant)
    {
        var mesh = new float[res, res];
        int mult = 1;
        for (int i = 0; i < number; i++)
        {
            if (rootTerrain.GetPosition() == Vector3.zero)
            {
                CreatingTerrain(rootTerrain, mesh, res, variant, res, i, mult);
                mult *= -1;
            }
            else
            {
                CreatingNextTerrains(rootTerrain, mesh, res, variant, res, i, mult);
                mult *= -1;
            }
        }
    }

    private void CreatingTerrain(Terrain rootTerrain, float[,] mesh, int res, float variant, int heightmapRes, int i,
        int mult)
    {
        TerrainData terraindata = new TerrainData();
        terraindata.heightmapResolution = heightmapRes;
        terraindata.size = new Vector3(rootTerrain.terrainData.size.x, Random.Range(minHeight, maxHeight),
            rootTerrain.terrainData.size.x);
        GameObject terrain = Terrain.CreateTerrainGameObject(terraindata);
        terrain.name = "Chunk";
        terrain.tag = "Terrain";
        terrain.transform.parent = gameObject.transform;
        Terrain ter = terrain.GetComponent<Terrain>();
        Chunk chunk = terrain.gameObject.AddComponent<Chunk>();
        chunk.SetRootTerrain(ter);
        _chunks.Add(chunk);
        ter.materialTemplate = terrainMaterial;
        mesh = PerlinNoiseGeneration(res, variant);
        ter.terrainData.SetHeights(0, 0, mesh);

        Vector3 pos = StartPlacementChunks(i, rootTerrain, mult);
        terrain.transform.position = pos;
        terrains.Add(ter);
        _treeAndBushes.VegetationGenerator(mesh, ter, treePrefab, treeCount, bushesPrefab, bushesCount);
    }

    private Vector3 StartPlacementChunks(int i, Terrain rootTerrain, int mult)
    {
        Vector3 pos = new Vector3();

        if (i < 2)
        {
            pos += new Vector3(rootTerrain.terrainData.size.x, 0, 0) * mult;
        }
        else if (i < 4)
        {
            pos += new Vector3(0, 0, rootTerrain.terrainData.size.x) * mult;
        }
        else if (i < 6)
        {
            float x = rootTerrain.terrainData.size.x * mult;
            float z = rootTerrain.terrainData.size.x * mult;
            pos += new Vector3(x, 0, z);
        }
        else
        {
            float z = rootTerrain.terrainData.size.x * mult;
            mult *= -1;
            float x = rootTerrain.terrainData.size.x * mult;
            pos += new Vector3(x, 0, z);
        }

        return pos;
    }

    private void CreatingNextTerrains(Terrain rootTerrain, float[,] mesh, int res, float variant, int heightmapRes,
        int count, int mult)
    {
        Chunk chunkRoot = rootTerrain.GetComponent<Chunk>();
        HashSet<Terrain> neighbors = chunkRoot.GetNeighborList();


        Vector3 pos = PlacementNextChunks(count, rootTerrain, mult);


        TerrainData terraindata = new TerrainData();
        terraindata.heightmapResolution = heightmapRes;
        terraindata.size = new Vector3(rootTerrain.terrainData.size.x, Random.Range(minHeight, maxHeight),
            rootTerrain.terrainData.size.x);
        GameObject terrain = Terrain.CreateTerrainGameObject(terraindata);
        terrain.name = "Chunk";
        terrain.tag = "Terrain";
        terrain.transform.parent = gameObject.transform;
        Terrain ter = terrain.GetComponent<Terrain>();
        Chunk chunk = terrain.gameObject.AddComponent<Chunk>();
        chunk.SetRootTerrain(ter);
        _chunks.Add(chunk);
        ter.materialTemplate = terrainMaterial;
        mesh = PerlinNoiseGeneration(res, variant);
        ter.terrainData.SetHeights(0, 0, mesh);

        terrain.transform.position = pos;
        terrains.Add(ter);
        _treeAndBushes.VegetationGenerator(mesh, ter, treePrefab, treeCount, bushesPrefab, bushesCount);
    }

    private Vector3 PlacementNextChunks(int i, Terrain rootTerrain, int mult)
    {
        Vector3 pos = rootTerrain.GetPosition();

        if (i < 2)
        {
            pos += new Vector3(rootTerrain.terrainData.size.x, 0, 0) * mult;
        }
        else if (i < 4)
        {
            pos += new Vector3(0, 0, rootTerrain.terrainData.size.x) * mult;
        }
        else if (i < 6)
        {
            float x = rootTerrain.terrainData.size.x * mult;
            float z = rootTerrain.terrainData.size.x * mult;
            pos += new Vector3(x, 0, z);
        }
        else
        {
            float z = rootTerrain.terrainData.size.x * mult;
            mult *= -1;
            float x = rootTerrain.terrainData.size.x * mult;
            pos += new Vector3(x, 0, z);
        }

        return pos;
    }
}
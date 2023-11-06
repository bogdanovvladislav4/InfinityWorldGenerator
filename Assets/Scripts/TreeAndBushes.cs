using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TerrainProceduralGenerator
{
    public class TreeAndBushes : MonoBehaviour
    {
        public void VegetationGenerator(float[,] coord, Terrain terrain, GameObject treePrefab, int treeCount,
            GameObject bushesPrefab, int bushesCount)
        {
            List<GameObject> trees = new List<GameObject>();
            List<GameObject> bushes = new List<GameObject>();
            List<Vector3> coordsBushes = new List<Vector3>();
            int res = (int)terrain.terrainData.size.x;

            for (int i = 0; i < treeCount; i++)
            {
                List<float> coordsX= new List<float>();
                List<float> coordsY= new List<float>();
                int x = Random.Range(0, res);
                int y = Random.Range(0, res);
                if (coord[x, y] > 0.5f && trees.Count < treeCount && !coordsX.Contains(x) && !coordsY.Contains(y))
                {
                    Vector3 pos = new Vector3(x + terrain.transform.position.x, terrain.terrainData.GetHeight(x, y),
                        y + terrain.transform.position.z);
                    GameObject tree = Instantiate(treePrefab, pos, quaternion.identity);
                    tree.transform.parent = terrain.transform;
                    trees.Add(tree);
                    i++;
                    coordsX.Add(x);
                    coordsY.Add(y);
                }
            }

            for (int i = 0; i < treeCount; i++)
            {
                List<float> coordsX= new List<float>();
                List<float> coordsY= new List<float>();
                int x = Random.Range(0, res);
                int y = Random.Range(0, res);
                if (coord[x, y] > 0.2f && bushes.Count < bushesCount && !coordsX.Contains(x) && !coordsY.Contains(y))
                {
                    Vector3 pos = new Vector3(x + terrain.transform.position.x, terrain.terrainData.GetHeight(x, y),
                        y + terrain.transform.position.z);
                    GameObject bushe = Instantiate(bushesPrefab, pos, quaternion.identity);
                    bushe.transform.parent = terrain.transform;
                    bushes.Add(bushe);
                    i++;
                    coordsX.Add(x);
                    coordsY.Add(y);
                }
            }
        }
    }
}
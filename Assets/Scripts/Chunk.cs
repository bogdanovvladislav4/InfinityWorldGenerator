using System;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainProceduralGenerator
{
    public class Chunk : MonoBehaviour
    {
        private  HashSet<Terrain> _neighborChunks;
        public Terrain terrain;

        internal GameObject topNeighbor;
        internal GameObject bottomNeighbor;
        internal GameObject leftNeighbor;
        internal GameObject rightNeighbor;

        private void Start()
        {
            _neighborChunks = new HashSet<Terrain>();
        }

        public Chunk(Terrain terrain)
        {
            this.terrain = terrain;
        }

        public void SetRootTerrain(Terrain ter)
        {
            terrain = ter;
        }

        public HashSet<Terrain> GetNeighborList()
        {
            return _neighborChunks;
        }
        
        

        public void AddNeighbor(List<Chunk> chunks)
        {
            Debug.Log(_neighborChunks.Count);

            foreach (var chunk in chunks)
            {
                Debug.Log(Vector3.Distance(chunk.terrain.GetPosition(), terrain.GetPosition()));
                if (!chunk.Equals(null))
                {
                    if (Vector3.Distance(chunk.terrain.GetPosition(), terrain.GetPosition()) < 512 && !chunk.terrain.Equals(terrain))
                    {
                        _neighborChunks.Add(terrain);
                    }
                }
            }
        }
    }
}
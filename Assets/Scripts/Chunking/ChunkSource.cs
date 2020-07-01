using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Chunking
{
    public class ChunkSource:MonoBehaviour
    {
        public int detailDistance;
        public int commonDistance;

        private int currentCX;
        private int currentCY;

        private bool initialized = false;

        public void Update()
        { 
            int cx = Chunk.FullDivision(transform.position.x, Chunk.chunkwidth);
            int cy = Chunk.FullDivision(transform.position.z, Chunk.chunkwidth);
            if (!initialized)
            {
                initialized = true;

                var switchonChunksDetail = ChunkManager.GetChunksInRange(detailDistance, transform.position);
                var switchonChunksCommons = ChunkManager.GetChunksInRange(commonDistance, transform.position);
                foreach (Chunk c in ChunkManager.chunks)
                {
                    if (!switchonChunksDetail.Contains(c))
                    { 
                        c.TurnOffDetails();
                    }
                    if (!switchonChunksCommons.Contains(c))
                    {
                        c.TurnOffCommons();
                    }
                }
            }
            if (cx != currentCX || cy != currentCY)
            {
                currentCX = cx;
                currentCY = cy;
                

                var switchoffChunks = ChunkManager.GetChunksInRange(detailDistance + 1,transform.position);
                var switchonChunks = ChunkManager.GetChunksInRange(detailDistance, transform.position);

                foreach (Chunk c in switchonChunks)
                {
                    switchoffChunks.Remove(c);
                    if (!c.detailIsOn)
                    {
                        c.TurnOnDetails();
                    }
                }
                foreach (Chunk c in switchoffChunks)
                {
                    if (c.detailIsOn)
                    {
                        c.TurnOffDetails();
                    }
                }

                var switchoffChunksCommons = ChunkManager.GetChunksInRange(commonDistance + 1, transform.position);
                var switchonChunksCommons = ChunkManager.GetChunksInRange(commonDistance, transform.position);

                foreach (Chunk c in switchonChunksCommons)
                {
                    switchoffChunksCommons.Remove(c);
                    if (!c.commonIsOn)
                    {
                        c.TurnOnCommons();
                    }
                }
                foreach (Chunk c in switchoffChunksCommons)
                {
                    if (c.commonIsOn)
                    {
                        c.TurnOffCommons();
                    }
                }

            }

        }
    }
}

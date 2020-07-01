using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Chunking
{
    public static class ChunkManager
    {
        public static List<Chunk> chunks = new List<Chunk>();



        public static List<Chunk> GetChunksInRange(int range, Vector3 position)
        {
            List<Chunk> rList = new List<Chunk>();
            int cx = Chunk.FullDivision(position.x, Chunk.chunkwidth);
            int cy = Chunk.FullDivision(position.z, Chunk.chunkwidth);

            foreach (Chunk c in chunks)
            {
                if (Mathf.Abs(cx - c.x) <= range && Mathf.Abs(cy - c.y) <= range)
                {
                    rList.Add(c);
                }
            }

            return rList;
        }


        public static void AddRenderer(MeshRenderer renderer, Vector3 immutablePosition, bool isDetail = true)
        {
            int cx = Chunk.FullDivision(immutablePosition.x, Chunk.chunkwidth);
            int cy = Chunk.FullDivision(immutablePosition.z, Chunk.chunkwidth);

            bool foundChunk = false;
            foreach (Chunk c in chunks)
            {
                if (cx == c.x && cy == c.y)
                {
                    foundChunk = true;
                    if (isDetail)
                    {
                        c.detailRenderers.Add(renderer);
                    }
                    else {
                        c.commonRenderers.Add(renderer);
                    }
                }
            }
            if (!foundChunk)
            {
                Chunk nChunk = new Chunk(cx,cy);
                chunks.Add(nChunk);
                if (isDetail)
                {
                    nChunk.detailRenderers.Add(renderer);
                }
                else
                {
                    nChunk.commonRenderers.Add(renderer);
                }
            }

        }
        

    }
}

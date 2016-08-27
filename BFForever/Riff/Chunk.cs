﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFForever.Riff
{
    public abstract class Chunk
    {
        public Chunk(FString idx)
        {
            IndexKey = idx;
        }

        public static Chunk FromStream(AwesomeReader ar)
        {
            int chunkType = ar.ReadInt32(); // INDX or STbl or ZOBJ
            int chunkSize = ar.ReadInt32();
            
            long idx = ar.ReadInt64(); // Index key

            Chunk chunk;
            switch (chunkType)
            {
                case Constant.INDX:
                    chunk = null;
                    break;
                case Constant.STbl:
                    chunk = new StringTable(idx);
                    chunk.ImportData(ar);

                    break;
                case Constant.ZOBJ:
                    FString directory = ar.ReadInt64();
                    FString type = ar.ReadInt64();
                    ar.BaseStream.Position += 8; // Skips zeros
                    
                    switch(type.Key)
                    {
                        case Constant.RIFF_Index2:
                            chunk = new Index2(idx);
                            break;
                        case Constant.RIFF_PackageDef:
                            chunk = new PackageDef(idx);
                            break;
                        case Constant.RIFF_Catalog2:
                            chunk = new Catalog2(idx);
                            break;
                        default:
                            return null;
                    }
                    ((ZObject)chunk).Directory = directory;
                    chunk.ImportData(ar);

                    break;
                default:
                    return null;
            }

            return chunk;
        }

        public abstract void ImportData(AwesomeReader ar);

        public FString IndexKey { get; set; }
    }
}

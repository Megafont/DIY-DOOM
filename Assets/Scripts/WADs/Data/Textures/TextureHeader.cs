using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace DIY_DOOM.WADs.Data.Textures
{
    public class TextureHeader
    {
        public uint TexturesCount;
        public uint TexturesOffset;
        
        private List<uint> _TextureDataOffsets;



        public TextureHeader()
        {
            _TextureDataOffsets = new List<uint>();
        }

        public void AddTextureDataOffset(uint offset)
        {
            _TextureDataOffsets.Add(offset);
        }

        public uint GetTextureDataOffset(int index)
        {
            return _TextureDataOffsets[index];
        }   

    }
}
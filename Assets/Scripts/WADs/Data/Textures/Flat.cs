using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DIY_DOOM.Utils.Textures;


namespace DIY_DOOM.WADs.Data.Textures
{
    /// <summary>
    /// This class holds the raw data for a flat (a floor/ceiling texture).
    /// These lumps in DOOM.WAD are always 4096 bytes, containing a 64x64 pixel texture.
    /// Each byte is an index into the color palette. The pixels are in row major order,
    /// meaning the first 64 bytes are the first row of pixels in the image and so on.
    /// </summary>
    public class Flat
    {
        private string _Name;
        private byte[] _FlatData;


        public Flat(string name, byte[] flatData)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The passed in name canned not be null, an empty string, or a white-space only string!");
            }

            if (flatData == null)
            {
                throw new ArgumentNullException("The passed in flat data array cannot be null!");
            }
            else if (flatData.Length != 4096)
            {
                throw new ArgumentException("The passed in flat data array should be exactly 4096 bytes long!");
            }


            _Name = name;

            _FlatData = flatData;
        }

        public Texture2D RenderToTexture2D(Palette palette)
        {
            Texture2D texture = TextureUtils.CreateBlankDoomTexture(_Name, 64, 64);

            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    int byteIndex = y * 64 + x;
                    texture.SetPixel(x, 
                                     63 - y, 
                                     palette[_FlatData[byteIndex]]);

                } // end for X
            } // end for Y

            texture.Apply();

            return texture;
        }

        public byte GetFlatDataByte(int index)
        {
            return _FlatData[index];
        }



        public string Name { get { return _Name; } }

        public int FlatDataByteCount { get { return _FlatData.Length; } }
    }
}

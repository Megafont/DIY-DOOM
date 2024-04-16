using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DIY_DOOM.Utils.Textures;
using System.Security.Cryptography;
using Unity.VisualScripting.Antlr3.Runtime.Tree;


namespace DIY_DOOM.WADs.Data.Textures
{
    public class Patch
    {
        private string _Name;
        private List<PatchColumn> _PatchData;

        private uint _Height;
        private uint _Width;
        private int _X_Offset;
        private int _Y_Offset;

        private List<int> _ColumnStartIndex;



        public Patch(string name, PatchHeader patchHeader)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The passed in name canned not be null, an empty string, or a white-space only string!");
            }


            _Name = name;

            Initialize(patchHeader);

            _PatchData = new List<PatchColumn>();

            _ColumnStartIndex = new List<int>();
        }

        public void Initialize(PatchHeader patchHeader)
        {
            _Height = patchHeader.Height;
            _Width = patchHeader.Width;
            _X_Offset = patchHeader.X_Offset;
            _Y_Offset = patchHeader.Y_Offset;
        }

        public void AddPatchColumn(PatchColumn patchColumn)
        {
            _PatchData.Add(patchColumn);
        }

        public void AppendColumnStartIndex()
        {
            _ColumnStartIndex.Add(_PatchData.Count);
        }

        public Texture2D RenderToTexture2D(Palette palette)
        {
            Texture2D texture = TextureUtils.CreateBlankDoomTexture(_Name, (int) _Width, (int) _Height);


            int x = 0;
            foreach (PatchColumn patchColumn in _PatchData)
            {
                if (patchColumn.TopDelta == 0xFF)
                {
                    x++;
                    continue;
                }

                for (int y = 0; y < patchColumn.Length; y++)
                {
                    texture.SetPixel((int) x, 
                                     (int) _Height - (y + patchColumn.TopDelta) - 1, 
                                     palette[patchColumn[y]]);
                }

            }


            texture.Apply();

            return texture;
        }

        public void ComposeColumn(Texture2D texture, ref int patchColumnIndex, int columnOffsetIndex, int yOffset, Palette palette)
        {
            int textureHeight = texture.height;
            
            while (_PatchData[patchColumnIndex].TopDelta != 0xFF)
            {
                int colLength = _PatchData[patchColumnIndex].Length;

                int yPos = yOffset + _PatchData[patchColumnIndex].TopDelta;
                int maxRun = colLength;


                if (yPos < 0)
                {
                    maxRun -= yPos;
                    //yPos = 0;   // THIS LINE CAUSES THE TEXTURE TO NOT RENDER QUITE RIGHT IN SOME CASES. I left it here for now just in case.
                }

                  
                if (yPos + maxRun >= _PatchData[patchColumnIndex].Length)
                {
                    maxRun = textureHeight - yPos;
                }

                maxRun = Mathf.Abs(maxRun);
                if (maxRun >= colLength)
                    maxRun = colLength;

                //Debug.Log($"    COLUMN[{patchColumnIndex}]    yPos: {yPos}    maxRun: {maxRun}    texWidth: {texture.width}    texHeight: {textureHeight}    patchWidth: {_Width}    patchHeight: {_Height}    colLength: {colLength}    yOffset: {yOffset}    topDelta: {_PatchData[patchColumnIndex].TopDelta}");
                for (int y = 0; y < maxRun; y++)
                {
                    int pixY = y + yPos;
                    if (pixY < 0) pixY += textureHeight;

                    
                    texture.SetPixel(columnOffsetIndex,
                                     textureHeight - pixY - 1,
                                     palette[_PatchData[patchColumnIndex].GetColumnData(y)]);
                } // end for y


                patchColumnIndex++;

            } // end while

        }

        public PatchColumn GetColumnData(int index)
        {
            return _PatchData[index];
        }

        public int GetColumnDataIndex(int index)
        {
            return _ColumnStartIndex[index];
        }



        public string Name { get { return _Name; } }
        public uint Height { get { return _Height; } }
        public uint Width { get { return _Width; } }
        public int X_Offset { get { return _X_Offset; } }
        public int Y_Offset { get { return _Y_Offset; } }
    }
}

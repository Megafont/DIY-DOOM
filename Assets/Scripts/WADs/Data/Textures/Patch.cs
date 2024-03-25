using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DIY_DOOM.Utils.Textures;


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
            Texture2D texture = new Texture2D((int)_Width, (int)_Height, TextureFormat.RGBA32, true);
            texture.name = _Name;
            texture.alphaIsTransparency = true;
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.SetAllPixelsToColor(Color.clear);


            int x = 0;
            foreach (PatchColumn patchColumn in _PatchData)
            {
                if (patchColumn.TopDelta == 0xFF)
                {
                    x++;
                    continue;
                }

                Debug.Log(x);

                for (int y = 0; y < patchColumn.Length; y++)
                {
                    texture.SetPixel((int) _Width - 1 - x, y + patchColumn.TopDelta, palette[patchColumn[y]]);
                }

            }


            texture.Apply();

            return texture;
        }

        public void ComposeColumn(Texture2D texture, int height, ref int patchColumnIndex, int columnOffsetIndex, int yOrigin, Palette palette)
        {
            while (_PatchData[patchColumnIndex].TopDelta != 0xFF)
            {
                int yPos = yOrigin + _PatchData[patchColumnIndex].TopDelta;
                int maxRun = _PatchData[patchColumnIndex].Length;


                if (yPos < 0)
                {                    
                    maxRun -= yPos;
                    //yPos = 0;
                }

                
                if (yPos + maxRun > height &&  yPos >= 0)
                {
                    maxRun = height - yPos;
                }
                
                               
                for (int y = 0; y < maxRun; y++)
                {
                    int colDataIndex = Mathf.Abs(y % _PatchData[patchColumnIndex].Length);
                    
                    texture.SetPixel(columnOffsetIndex,
                                     y + yPos,
                                     palette[_PatchData[patchColumnIndex].GetColumnData(colDataIndex)]);

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

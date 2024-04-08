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
                int yPos = (yOffset + _PatchData[patchColumnIndex].TopDelta);
                int maxRun = _PatchData[patchColumnIndex].Length;

                
                if (yPos < 0)
                {                                        
                    maxRun -= yPos;
                    //yPos = 0;
                }
                else 
                {
                    yPos *= -1;
                    maxRun += yPos;
                }

                
                if (_Height > textureHeight)
                {

                }

                if (maxRun - yPos > textureHeight)
                {
                    //maxRun -= textureHeight;
                }
                
                //maxRun = Mathf.Abs(maxRun);
                int diff = textureHeight - (int) _Height;
                //if (diff < 0)
                //    diff += textureHeight;

                yPos = diff + yPos;

                Debug.Log($"    COLUMN[{patchColumnIndex}]    yPos: {yPos}    maxRun: {maxRun}    texWidth: {texture.width}    texHeight: {textureHeight}    patchWidth: {_Width}    patchHeight: {_Height}    yOffset: {yOffset}    topDelta: {_PatchData[patchColumnIndex].TopDelta}    colLength: {_PatchData[patchColumnIndex].Length}    texHeight-PatchHeight: {textureHeight - _Height}");
                for (int y = 0; y < maxRun; y++)
                {
                    int colDataIndex = Mathf.Abs(maxRun - y - 1) % _PatchData[patchColumnIndex].Length;

                    texture.SetPixel(columnOffsetIndex,
                                     y,
                                     palette[_PatchData[patchColumnIndex].GetColumnData(colDataIndex)]);
                } // end for y


                patchColumnIndex++;

            } // end while

        }

        public void ComposeColumn2(Texture2D texture, ref int patchColumnIndex, int columnOffsetIndex, int yOffset, Palette palette)
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
                    yPos = 0;
                }

                  
                if (yPos + maxRun > textureHeight)
                {
                    //yPos = 0;
                    maxRun = textureHeight - yPos;
                }

                maxRun = Mathf.Abs(maxRun);
                //if (maxRun <= 0) maxRun = colLength;
                //int diff = textureHeight - (int)_Height;
                //if (diff < 0)
                //    diff += textureHeight;

                //yPos = diff + yPos;

                Debug.Log($"    COLUMN[{patchColumnIndex}]    yPos: {yPos}    maxRun: {maxRun}    texWidth: {texture.width}    texHeight: {textureHeight}    patchWidth: {_Width}    patchHeight: {_Height}    colLength: {colLength}    yOffset: {yOffset}    topDelta: {_PatchData[patchColumnIndex].TopDelta}    texHeight-PatchHeight: {textureHeight - _Height}");
                for (int y = 0; y < maxRun; y++)
                {
                    //int colDataIndex = (y - yOffset) % colLength;

                    //int pixY = yOffset >= 0 ? y : yOffset;
                    int pixY = y + yOffset;
                    if (pixY < 0) pixY += textureHeight;
                    Debug.Log("pixY: " + pixY);
                    //Debug.Log("A: " + pixY);
                    //if (pixY < 0) pixY += _PatchData[patchColumnIndex].Length;
                    //Debug.Log("B: " + pixY);
                    
                    texture.SetPixel(columnOffsetIndex,
                                     textureHeight - pixY - 1,
                                     palette[_PatchData[patchColumnIndex].GetColumnData(y)]);
                } // end for y


                patchColumnIndex++;

            } // end while

        }

        /*
        public void ComposeColumn3(Texture2D texture, int xOffset, int yOffset, Palette palette)
        {
            int textureHeight = texture.height;

            //Texture2D patch
            while (_PatchData[patchColumnIndex].TopDelta != 0xFF)
            {
                int colLength = _PatchData[patchColumnIndex].Length;

                int yPos = Mathf.Abs(yOffset) + _PatchData[patchColumnIndex].TopDelta;
                int maxRun = colLength;


                if (yPos < 0)
                {
                    maxRun += yPos;
                    yPos = 0;
                }


                if (yPos + maxRun > textureHeight)
                {
                    maxRun = textureHeight - yPos;
                }

                //maxRun = Mathf.Abs(maxRun);
                //int diff = textureHeight - (int)_Height;
                //if (diff < 0)
                //    diff += textureHeight;

                //yPos = diff + yPos;

                Debug.Log($"    COLUMN[{patchColumnIndex}]    yPos: {yPos}    maxRun: {maxRun}    texWidth: {texture.width}    texHeight: {textureHeight}    patchWidth: {_Width}    patchHeight: {_Height}    colLength: {colLength}    yOffset: {yOffset}    topDelta: {_PatchData[patchColumnIndex].TopDelta}    texHeight-PatchHeight: {textureHeight - _Height}");
                for (int y = 0; y < maxRun; y++)
                {
                    int colDataIndex = Mathf.Abs(y + yOffset) % colLength;

                    int pixY = yOffset >= 0 ? y : _PatchData[patchColumnIndex].Length - y - 1;
                    //Debug.Log("A: " + pixY);
                    //if (pixY < 0) pixY += _PatchData[patchColumnIndex].Length;
                    //Debug.Log("B: " + pixY);
                    texture.SetPixel(columnOffsetIndex,
                                     textureHeight - y,
                                     palette[_PatchData[patchColumnIndex].GetColumnData(pixY)]);
                } // end for y


                patchColumnIndex++;

            } // end while

        }
        */

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

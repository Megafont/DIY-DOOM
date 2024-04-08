using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DIY_DOOM.Utils.Textures;


namespace DIY_DOOM.WADs.Data.Textures
{
    public class TextureData
    {
        public string TextureName;

        public uint Flags;
        public uint Width;
        public uint Height;
        public uint ColumnDirectory;
        public uint PatchCount;


        private int _OverlapSize;

        private List<TexturePatch> _TexturePatches;

        private List<int> _ColumnPatchCount;
        private List<int> _ColumnIndex;
        private List<int> _ColumnPatch;



        public TextureData()
        {
            _TexturePatches = new List<TexturePatch>();

            _ColumnPatchCount = new List<int>();
            _ColumnIndex = new List<int>();
            _ColumnPatch = new List<int>();
        }

        public void AddTexturePatch(TexturePatch texturePatch)
        {
            texturePatch.PatchName = AssetManager.Instance.GetPatchName((int) texturePatch.PatchNameIndex);

            _TexturePatches.Add(texturePatch);

            Initialize();
        }

        private void ClearInitData()
        {
            _ColumnPatchCount.Clear();
            _ColumnIndex.Clear();
            _ColumnPatch.Clear();


            for (int i = 0; i < Width; i++)
            {
                _ColumnPatchCount.Add(0);
                _ColumnIndex.Add(0);
                _ColumnPatch.Add(0);
            }
        }

        private void Initialize()
        {
            AssetManager assetManager = AssetManager.Instance;

            ClearInitData();


            for (int i = 0; i < _TexturePatches.Count; i++)
            {
                // Get the texture patch info.
                Patch patch = assetManager.GetRawPatchData(_TexturePatches[i].PatchName);

                int xStart = _TexturePatches[i].X_Offset;
                int maxWidth = xStart + (int)patch.Width;

                int xPos = xStart;

                if (xStart < 0)
                {
                    xPos = 0;
                }

                if (maxWidth > Width)
                {
                    maxWidth = (int)Width;
                }

                
                while(xPos < maxWidth)
                {
                    _ColumnPatchCount[xPos]++;
                    _ColumnPatch[xPos] = i;
                    _ColumnIndex[xPos] = patch.GetColumnDataIndex(xPos - xStart);
                    xPos++;

                } // end while

            } // end for i


            DoInitCleanupAndUpdate();
        }

        private void DoInitCleanupAndUpdate()
        {
            // Cleanup and updating
            for (int i = 0; i < Width; i++)
            {
                if (_ColumnPatchCount[i] > 1)
                {
                    _ColumnPatch[i] = -1;
                    _ColumnIndex[i] = _OverlapSize;
                    _OverlapSize += (int)Height;
                }
            }
        }

        public Texture2D RenderToTexture2D(Palette palette)
        {
            AssetManager assetManager = AssetManager.Instance;

            Texture2D texture = TextureUtils.CreateBlankDoomTexture(TextureName, (int) Width, (int) Height);

            bool YES = TextureName == "BROWN144" ? true : false;

            for (int i = 0; i < _TexturePatches.Count; i++)
            {
                // Get the texture patch info.
                Patch patch = assetManager.GetRawPatchData(_TexturePatches[i].PatchName);
                
                int xStart = _TexturePatches[i].X_Offset;
                int maxWidth = xStart + (int) patch.Width;

                int xPos = xStart;

                if (xStart < 0)
                {
                    xPos = 0;
                }

                if (maxWidth > Width)
                {
                    maxWidth = (int) Width;
                }

                //if (YES)
                    Debug.Log($"[{i}]    PatchOffsets: {xPos},{_TexturePatches[i].Y_Offset}    TexSize: {Width}x{Height}");

                while (xPos < maxWidth)
                {
                    int patchColumnIndex = patch.GetColumnDataIndex(xPos - xStart);

                    /*
                    patch.ComposeColumn2(texture,
                                        (int)xPos,
                                        _TexturePatches[i].Y_Offset,
                                        palette);
                    */
                    patch.ComposeColumn2(texture, 
                                        ref patchColumnIndex, 
                                        (int) xPos,
                                        _TexturePatches[i].Y_Offset, 
                                        palette);

                    xPos++;

                } // end while

            } // end for i


            texture.Apply();

            return texture;
        }

    }
}

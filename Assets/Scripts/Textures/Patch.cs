using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DIY_DOOM.Maps;
using System.Runtime.CompilerServices;
using UnityEngine.Experimental.Rendering;


public class Patch
{
    private string _Name;
    private List<WAD_PatchColumn> _PatchData;

    private uint _Height;
    private uint _Width;
    private int _X_Offset;
    private int _Y_Offset;
    


    public Patch(string name, WAD_PatchHeader patchHeader)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("The passed in name canned not be null, an empty string, or a white-space only string!");
        }


        _Name = name;

        Initialize(patchHeader);

        _PatchData = new List<WAD_PatchColumn>();
    }

    public void Initialize(WAD_PatchHeader patchHeader)
    {
        _Height = patchHeader.Height;
        _Width = patchHeader.Width;
        _X_Offset = patchHeader.X_Offset;
        _Y_Offset = patchHeader.Y_Offset;
    }

    public void AddPatchColumn(WAD_PatchColumn patchColumn)
    {
        _PatchData.Add(patchColumn);
    }

    public Texture2D RenderToTexture2D(PaletteDef palette)
    {
        Texture2D texture = new Texture2D((int) _Width, (int) _Height, TextureFormat.RGBA32, true);
        texture.name = _Name;        
        texture.alphaIsTransparency = true;
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.SetAllPixelsToColor(Color.clear);


        int x = 0;
        foreach (WAD_PatchColumn patch in _PatchData) 
        {
            if (patch.TopDelta == 0xFF)
            {
                x++;
                continue;
            }


            for (int y = 0; y < patch.Length; y++)
            {
                texture.SetPixel(x, y + patch.TopDelta, palette[patch[y]]);
            }
            texture.Apply();
        }


        return texture;
    }



    public string Name { get { return _Name; } }
    public uint Height { get { return _Height; } }
    public uint Width { get { return _Width; } }
    public int X_Offset { get { return _X_Offset; } }
    public int Y_Offset { get { return _Y_Offset; } }
}

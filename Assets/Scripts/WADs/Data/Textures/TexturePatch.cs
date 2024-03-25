using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace DIY_DOOM.WADs.Data.Textures
{
    public struct TexturePatch
    {
        public int X_Offset;
        public int Y_Offset;

        public uint PatchNameIndex;       
        public uint StepDir;
        public uint ColorMap;

        public string PatchName;
    }
}
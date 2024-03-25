using System;
using UnityEngine;


namespace DIY_DOOM.WADs.Data.Maps
{
    [Flags]
    public enum LineDefFlags
    {
        BlocksPlayersAndMonsters = 0,
        BlocksMonsters = 1,
        TwoSided = 2,
        UpperTextureIsUnpegged = 4,
        LowerTextureIsUnpegged = 8,
        Secret = 16,
        BlocksSound = 32,
        NeverShowsOnAutoMap = 64,
        AlwaysShowsOnAutoMap = 128
    }

    public struct LineDef
    {
        public uint StartVertexID;
        public uint EndVertexID;
        public LineDefFlags Flags;
        public uint LineType;
        public uint SectorTag;

        public int LeftSideDef;
        public int RightSideDef;


        public void DEBUG_Print()
        {
            Debug.Log("LINEDEF");
            Debug.Log(new string('-', 256));
            Debug.Log($"Start Vertex: {StartVertexID}");
            Debug.Log($"End Vertex: {EndVertexID}");
            Debug.Log($"Flags: {Flags}");
            Debug.Log($"Line Type: {LineType}");
            Debug.Log($"Sector Tag: {SectorTag}");
            Debug.Log($"Right Side Def: {RightSideDef}");
            Debug.Log($"Left Side Def: {LeftSideDef}");
            Debug.Log(new string('-', 256));
        }
    }


}
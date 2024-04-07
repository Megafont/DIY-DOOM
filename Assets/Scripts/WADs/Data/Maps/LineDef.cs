using System;
using UnityEngine;


namespace DIY_DOOM.WADs.Data.Maps
{
    [Flags]
    public enum LineDefFlags
    {
        BlocksPlayersAndMonsters = 1,
        BlocksMonsters = 2,
        TwoSided = 4,
        UpperTextureIsUnpegged = 8,
        LowerTextureIsUnpegged = 16,
        Secret = 32,
        BlocksSound = 64,
        NeverShowsOnAutoMap = 128,
        AlwaysShowsOnAutoMap = 256,
    }

    public struct LineDef
    {
        public uint StartVertexID;
        public uint EndVertexID;
        public LineDefFlags Flags;
        public uint LineType;
        public uint SectorTag;

        public int LeftSideDefIndex;
        public int RightSideDefIndex;


        public void DEBUG_Print()
        {
            Debug.Log("LINEDEF");
            Debug.Log(new string('-', 256));
            Debug.Log($"Start Vertex: {StartVertexID}");
            Debug.Log($"End Vertex: {EndVertexID}");
            Debug.Log($"Flags: {Flags}");
            Debug.Log($"Line Type: {LineType}");
            Debug.Log($"Sector Tag: {SectorTag}");
            Debug.Log($"Right Side Def: {RightSideDefIndex}");
            Debug.Log($"Left Side Def: {LeftSideDefIndex}");
            Debug.Log(new string('-', 256));
        }
    }


}
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

    public class LineDef
    {
        public uint StartVertexID;
        public uint EndVertexID;
        public LineDefFlags Flags;
        public uint LineType;
        public uint SectorTag;

        public int BackSideDefIndex;  // aka LeftSideDefIndex (relative to the direction the side def is going in)
        public int FrontSideDefIndex; // aka RightSideDefIndex (relative to the direction the side def is going in)


        public void DEBUG_Print()
        {
            Debug.Log("LINEDEF");
            Debug.Log(new string('-', 256));
            Debug.Log($"Start Vertex: {StartVertexID}");
            Debug.Log($"End Vertex: {EndVertexID}");
            Debug.Log($"Flags: {Flags}");
            Debug.Log($"Line Type: {LineType}");
            Debug.Log($"Sector Tag: {SectorTag}");
            Debug.Log($"Right Side Def: {FrontSideDefIndex}");
            Debug.Log($"Left Side Def: {BackSideDefIndex}");
            Debug.Log(new string('-', 256));
        }
    }


}
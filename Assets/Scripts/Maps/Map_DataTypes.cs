using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace DIY_DOOM.Maps
{
    public enum MapLumpIndices
    {
        Things = 1,
        LineDefs,
        SideDefs,
        Vertices,
        Segs,
        SubSectors, // AKA SSectors in the notes
        Nodes,
        Sectors,
        Reject,
        BlockMap,
        Count
    }

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
        public int StartVertex;
        public int EndVertex;
        public LineDefFlags Flags;
        public int LineType;
        public int SectorTag;
        public int LeftSideDef;
        public int RightSideDef;


        public void DEBUG_Print()
        {
            Debug.Log("LINEDEF");
            Debug.Log(new string('-', 256));
            Debug.Log($"Start Vertex: {StartVertex}");
            Debug.Log($"End Vertex: {EndVertex}");
            Debug.Log($"Flags: {Flags}");
            Debug.Log($"Line Type: {LineType}");
            Debug.Log($"Sector Tag: {SectorTag}");
            Debug.Log($"Right Side Def: {RightSideDef}");
            Debug.Log($"Left Side Def: {LeftSideDef}");
            Debug.Log(new string('-', 256));
        }
    }

    public struct ThingDef
    {
        public Vector2 Position;
        public uint Angle;
        public uint Type;
        public uint Flags;


        public void DEBUG_Print()
        {
            Debug.Log("THING");
            Debug.Log(new string('-', 256));
            Debug.Log($"Position: {Position}");
            Debug.Log($"Angle: {Angle}");
            Debug.Log($"Type: {Type}");
            Debug.Log($"Flags: {Flags}");
            Debug.Log(new string('-', 256));
        }
    }

    public struct NodeDef
    {
        // These two vectors define the line (binary space partition) that is dividing the space this node represents.
        public Vector2 PartitionStart;
        public Vector2 DeltaToPartitionEnd; // This represents the distance and direction from the PartitionStart point to the end point of the partition line. So PartitionStart plus this value equals the end point of the line.

        // Opposite corners of the bounding box of the right side of the space partition for this node.
        public Vector2 RightBox_BottomLeft;
        public Vector2 RightBox_TopRight;

        // Opposite corners of the bounding box of the left side of the space partition for this node.
        public Vector2 LeftBox_BottomLeft;
        public Vector2 LeftBox_TopRight;

        // Node IDs of the children of this node.
        public int RightChildID;
        public int LeftChildID;


        public void DEBUG_Print()
        {
            Debug.Log("NODE");
            Debug.Log(new string('-', 256));
            Debug.Log($"Partition Start: {PartitionStart}");
            Debug.Log($"Delta To Partition End: {DeltaToPartitionEnd}");
            Debug.Log($"Right Box Bottom Left: {RightBox_BottomLeft}");
            Debug.Log($"Right Box Top Right: {RightBox_TopRight}");
            Debug.Log($"Left Box Bottom Left: {LeftBox_BottomLeft}");
            Debug.Log($"Left Box Top Right: {LeftBox_TopRight}");
            Debug.Log($"Right Child ID: {RightChildID}");
            Debug.Log($"Left Child ID: {LeftChildID}");
            Debug.Log(new string('-', 256));
        }
    }

}
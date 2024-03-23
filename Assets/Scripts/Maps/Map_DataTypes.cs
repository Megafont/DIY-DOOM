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

    public struct ThingDef
    {
        public Vector3 Position;
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
        public Vector3 PartitionStart;
        public Vector3 PartitionEnd;
        public Vector3 DeltaToPartitionEnd; // This represents the distance and direction from the PartitionStart point to the end point of the partition line. So PartitionStart plus this value equals the end point of the line.
        
        // Opposite corners of the bounding box of the right side of the space partition for this node.
        public Vector3 RightBox_BottomLeft;
        public Vector3 RightBox_TopRight;

        // Opposite corners of the bounding box of the left side of the space partition for this node.
        public Vector3 LeftBox_BottomLeft;
        public Vector3 LeftBox_TopRight;

        // Node IDs of the children of this node.
        public uint RightChildID;
        public uint LeftChildID;


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

    /// <summary>
    /// A sub sector is a convex subsection of a sector.
    /// </summary>
    public struct SubSectorDef
    {
        public uint SegCount;
        public uint FirstSegID;


        public void DEBUG_Print()
        {
            Debug.Log("SUBSECTOR");
            Debug.Log(new string('-', 256));
            Debug.Log($"Seg Count: {SegCount}");
            Debug.Log($"First Seg ID: {FirstSegID}");
            Debug.Log(new string('-', 256));
        }
    }

    /// <summary>
    /// A seg is a segment of a LineDef, or sometimes an entire lineDef.
    /// </summary>
    public struct SegDef
    {
        public uint StartVertexID;
        public uint EndVertexID;
        public int Angle;
        public uint LineDefID;
        public uint Direction; // Facing direction: 0 = same as lineDef, and 1 = opposite of lineDef
        public uint Offset; // Distance from start of lineDef to start of this seg


        public void DEBUG_Print()
        {
            Debug.Log("SEG");
            Debug.Log(new string('-', 256));
            Debug.Log($"Start Vertex: {StartVertexID}");
            Debug.Log($"End Vertex: {EndVertexID}");
            Debug.Log($"Angle: {Angle}");
            Debug.Log($"Line Def ID: {LineDefID}");
            Debug.Log($"Direction: {Direction}");
            Debug.Log($"Offset: {Offset}");
            Debug.Log(new string('-', 256));
        }
    }

    /// <summary>
    /// This struct holds a 256 color DOOM palette.
    /// </summary>
    public struct PaletteDef
    {
        private Color32[] _Colors;



        public PaletteDef(Color32[] colors)
        {
            _Colors = colors;
        }



        public Color32 this[int i]
        {
            get
            {
                return _Colors[i];
            }
            set 
            { 
                _Colors[i] = value;
            }
        }
    }


    // TEXTURE RELATED TYPES
    // ========================================================================================================================================================================================================

    public struct WAD_PatchHeader
    {
        public uint Width;
        public uint Height;
        public int X_Offset;
        public int Y_Offset;
        
        private uint[] _ColumnOffsets;



        public void SetColumnOffsets(uint[] columnOffsets)
        {
            _ColumnOffsets = columnOffsets;
        }

        public uint GetColumnOffset(int index)
        {
            return _ColumnOffsets[index];
        }

        public void DEBUG_Print(bool printColumnOffsets = false)
        {
            Debug.Log("PATCH HEADER");
            Debug.Log(new string('-', 256));
            Debug.Log($"Width: {Width}");
            Debug.Log($"Height: {Height}");
            Debug.Log($"X Offset: {X_Offset}");
            Debug.Log($"Y Offset: {Y_Offset}");

            if (printColumnOffsets)
            {
                Debug.Log("COLUMN OFFSETS:");
                for (int i = 0; i < _ColumnOffsets.Length; i++)
                {
                    Debug.Log($"[{i}]: {_ColumnOffsets[i]}");
                }
            }

            Debug.Log(new string('-', 256));
        }


        public int GetColumnOffsetsCount { get {  return _ColumnOffsets.Length;} }
    }

    public struct WAD_PatchColumn
    {
        public byte TopDelta;
        public byte Length;
        public byte PaddingPre;
        public byte PaddingPost;


        private byte[] _ColumnData;


        public void DEBUG_Print()
        {
            Debug.Log("PATCH COLUMN");
            Debug.Log(new string('-', 256));
            Debug.Log($"Top Delta: {TopDelta}");
            Debug.Log($"Length: {Length}");
            Debug.Log($"Padding Pre: {PaddingPre}");
            Debug.Log($"Padding Post: {PaddingPost}");

            Debug.Log("COLUMN DATA:");
            string colData = "";
            for (int i = 0; i < _ColumnData.Length; i++)
            {
                colData += $"{(int) _ColumnData[i]} ";
            }
            Debug.Log(colData);

            Debug.Log(new string('-', 256));
        }

        public void SetColumnData(byte[] columnData)
        {
            _ColumnData = columnData;
        }



        public byte this[int i]
        {
            get
            {
                return _ColumnData[i];
            }
            set
            {
                _ColumnData[i] = value;
            }
        }   
    }


}
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


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
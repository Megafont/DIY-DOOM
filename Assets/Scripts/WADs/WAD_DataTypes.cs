using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Analytics;


public struct Directory
{
    public int LumpOffset;
    public int LumpSize;
    public string LumpName;


    public void DEBUG_Print()
    {
        Debug.Log("WAD DIRECTORY");
        Debug.Log(new string('-', 256));
        Debug.Log($"Lump Name: {LumpName}");
        Debug.Log($"Lump Size: {LumpSize}");
        Debug.Log($"Lump Offset: {LumpOffset}");
        Debug.Log(new string('-', 256));
        Debug.Log("");
    }
}

public struct WAD_Header
{
    /// <summary>
    /// There are two WAD types. IWAD is for official WADs released by ID software. PWAD is for custom WADs.
    /// </summary>
    public string WAD_Type;

    public int DirectoryCount;
    public int DirectoryOffset;


    public void DEBUG_Print()
    {
        Debug.Log("WAD HEADER");
        Debug.Log(new string('-', 256));
        Debug.Log($"WAD Type: {WAD_Type}");
        Debug.Log($"Directory Count: {DirectoryCount}");
        Debug.Log($"Directory Offset: {DirectoryOffset}");
        Debug.Log(new string('-', 256));
        Debug.Log("");
    }
}

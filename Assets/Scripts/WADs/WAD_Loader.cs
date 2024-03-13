using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEditor;
using UnityEngine;


public class WAD_Loader
{
    private string _FilePath;
    private byte[] _WAD_Data; // Stores the loaded file contents.
    private List<Directory> _WAD_Directories; // The directories inside the WAD file.



    public WAD_Loader(string filePath, string _WAD_Data = null)
    {
        _FilePath = filePath;
        _WAD_Data = null;
    }


    public bool LoadWAD()
    {
        if (!OpenAndLoad())
            return false;

        if (!ReadDirectories())
            return false;


        return true;
    }

    protected bool OpenAndLoad()
    {
        if (!File.Exists(_FilePath)) 
        {
            Debug.LogError($"ERROR: Failed to open WAD file \"{_FilePath}\"!");
            return false;
        }


        _WAD_Data = File.ReadAllBytes(_FilePath);
        Debug.Log($"Loaded \"{_FilePath}\": {_WAD_Data.Length} byes of data");

        return true;
    }

    protected bool ReadDirectories()
    {
        WAD_Reader reader = new WAD_Reader();
        WAD_Header header = reader.ReadHeaderData(_WAD_Data, 0);

        header.DEBUG_Print();

        Directory directory = new Directory();

        for (int i = 0; i < header.DirectoryCount; i++)
        {
            directory = reader.ReadDirectoryData(_WAD_Data, header.DirectoryOffset + i * 16);
            directory.DEBUG_Print();
        }

        return true;
    }
}

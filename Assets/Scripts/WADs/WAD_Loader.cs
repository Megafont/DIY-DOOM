using DIY_DOOM.Maps;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;


namespace DIY_DOOM.WADs
{
    public class WAD_Loader
    {
        private string _FilePath;
        private byte[] _WAD_Data; // Stores the loaded file contents.
        private List<Directory> _WAD_Directories; // The directories inside the WAD file.

        private WAD_Reader _Reader;



        public WAD_Loader()
        {
            Clear();
        }


        public bool LoadWAD(string filePath)
        {
            Clear();
            _FilePath = filePath;

            _Reader = new WAD_Reader();


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
            Debug.Log($"Loaded \"{_FilePath}\": {_WAD_Data.Length} bytes of data.");

            return true;
        }

        protected bool ReadDirectories()
        {
            WAD_Header header = _Reader.ReadHeaderData(_WAD_Data, 0);

            //header.DEBUG_Print();

            _WAD_Directories = new List<Directory>();

            Directory directory = new Directory();
            for (int i = 0; i < header.DirectoryCount; i++)
            {
                directory = _Reader.ReadDirectoryData(_WAD_Data, header.DirectoryOffset + i * 16);

                _WAD_Directories.Add(directory);

                //directory.DEBUG_Print();
            }


            Debug.Log($"Loaded {_WAD_Directories.Count} WAD directories.");

            return true;
        }

        public bool LoadMapData(string mapName, out Map map)
        {
            map = new Map(mapName);


            if (!ReadMapVertices(map))
            {
                Debug.LogError($"Failed to load vertices data for the map ({map.Name})!");
                map = null;
                return false;
            }

            if (!ReadMapLineDefs(map))
            {
                Debug.LogError($"Failed to load linedefs data for the map ({map.Name})!");
                map = null;
                return false;
            }


            return true;
        }

        int FindMapIndex(Map map)
        {
            for (int i = 0; i < _WAD_Directories.Count; i++)
            {
                //Debug.Log($"COMP: [{i}]\"{_WAD_Directories[i].LumpName}\" == \"{map.Name}\"");
                if (_WAD_Directories[i].LumpName == map.Name)
                    return i;
            }

            return -1;
        }

        bool ReadMapVertices(Map map)
        {
            int mapIndex = FindMapIndex(map);
            if (mapIndex == -1)
            {
                Debug.LogError("Failed to load vertices! The map index is invalid!");
                return false;
            }

            mapIndex += (int)MapLumpIndices.Vertices;

            if (_WAD_Directories[mapIndex].LumpName.CompareTo("VERTEXES") != 0)
            {
                Debug.LogError("Failed to load vertices! The map's vertices lump index is invalid!");
                return false;
            }


            int vertexSizeInBytes = 4;
            int verticesCount = _WAD_Directories[mapIndex].LumpSize / vertexSizeInBytes;


            Vector2 vertex;
            for (int i = 0; i < verticesCount; i++)
            {
                vertex = _Reader.ReadVertexData(_WAD_Data, _WAD_Directories[mapIndex].LumpOffset + i * vertexSizeInBytes);

                map.AddVertex(vertex);

                //Debug.Log($"VERTEX[{i}]: {_WAD_Directories[mapIndex].LumpOffset + i * vertexSizeInBytes}    {vertex}");
            }


            Debug.Log($"Loaded {verticesCount} vertices for {map.Name}.");

            return true;
        }

        bool ReadMapLineDefs(Map map)
        {
            int mapIndex = FindMapIndex(map);
            if (mapIndex == -1)
            {
                Debug.LogError("Failed to load vertices! The map index is invalid!");
                return false;
            }

            mapIndex += (int)MapLumpIndices.LineDefs;

            if (_WAD_Directories[mapIndex].LumpName.CompareTo("LINEDEFS") != 0)
            {
                Debug.LogError("Failed to load linedefs! The map's linedefs lump index is invalid!");
                return false;
            }


            int lineDefSizeInBytes = 14;
            int lineDefsCount = _WAD_Directories[mapIndex].LumpSize / lineDefSizeInBytes;

            LineDef lineDef;
            for (int i = 0; i < lineDefsCount; i++)
            {
                lineDef = _Reader.ReadLineDefData(_WAD_Data, _WAD_Directories[mapIndex].LumpOffset + i * lineDefSizeInBytes);

                map.AddLineDef(lineDef);

                //lineDef.DEBUG_Print();
            }


            Debug.Log($"Loaded {lineDefsCount} lineDefs for {map.Name}.");

            return true;
        }

        private void Clear()
        {
            _WAD_Data = null;
            _WAD_Directories = null;
        }

    }
}
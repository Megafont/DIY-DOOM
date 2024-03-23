using DIY_DOOM.Maps;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;


namespace DIY_DOOM.WADs
{
    public class WAD_Loader
    {
        private string _FilePath;
        private byte[] _WAD_Data; // Stores the loaded file contents.
        private List<Directory> _WAD_Directories; // The directories inside the WAD file.

        private WAD_Reader _Reader;

        private int _MapLumpIndex = -1;

        private AssetManager _AssetManager;



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
                directory = _Reader.ReadDirectoryData(_WAD_Data, (int) (header.DirectoryOffset + i * 16));

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
                DisplayLoadMapDataFailedError("vertices", map);
                return false;
            }

            if (!ReadMapLineDefs(map))
            {
                DisplayLoadMapDataFailedError("lineDefs", map);
                return false;
            }

            if (!ReadMapThings(map))
            {
                DisplayLoadMapDataFailedError("things", map);
                return false;
            }

            if (!ReadMapNodes(map))
            {
                DisplayLoadMapDataFailedError("nodes", map);
                return false;
            }

            if (!ReadMapSubSectors(map))
            {
                DisplayLoadMapDataFailedError("subSectors", map);
                return false;
            }

            if (!ReadMapSegs(map))
            {
                DisplayLoadMapDataFailedError("segs", map);
                return false;
            }

            if (!ReadPaletteData(map))
            {
                DisplayLoadMapDataFailedError("palettes", map);
                return false;
            }


            // Unload the raw wad data, since we don't need it anymore.
            // NOTE: This is commented out since we need to keep it around for whenever we load patches.
            //UnloadRawWadData(); 


            map.DoFinalProcessing();

            return true;
        }


        public void DisplayLoadMapDataFailedError(string type, Map map)
        {
            Debug.LogError($"Failed to load {type} data for the map ({map.Name})!");
            map = null;
        }

        private void UnloadRawWadData()
        {
            _WAD_Data = null;
            _WAD_Directories = null;
        }

        int FindMapIndex(Map map)
        {
            // Was the map's lump index previously found & cached?
            if (_MapLumpIndex > -1)
                return _MapLumpIndex;


            // The map's lump index was not previously found and cached, so find it.
            for (int i = 0; i < _WAD_Directories.Count; i++)
            {
                //Debug.Log($"COMP: [{i}]\"{_WAD_Directories[i].LumpName}\" == \"{map.Name}\"");
                if (_WAD_Directories[i].LumpName == map.Name)
                {
                    // Cache the map's lump index and then return it.
                    _MapLumpIndex = i;
                    return i;
                }
            }


            // Return -1 as an error code.
            _MapLumpIndex = -1;
            return -1;
        }

        /// <summary>
        /// This function finds the lump with the specified name and returns it's index.
        /// </summary>
        /// <param name="lumpName">The name of the data lump to find.</param>
        /// <returns>The index of the specified lump, or -1 if it was not found.</returns>
        private int FindLumpByName(string lumpName)
        {
            for (int i = 0; i < _WAD_Directories.Count; i++)
            {
                if (_WAD_Directories[i].LumpName == lumpName)
                {
                    return i;
                }
            }

            return -1;
        }

        bool ReadMapVertices(Map map)
        {
            int mapIndex = FindMapIndex(map);
            if (mapIndex == -1)
            {
                Debug.LogError("Failed to load vertices! The map's lump index is invalid!");
                return false;
            }

            mapIndex += (int)MapLumpIndices.Vertices;

            if (_WAD_Directories[mapIndex].LumpName.CompareTo("VERTEXES") != 0)
            {
                Debug.LogError("Failed to load vertices! The map's vertices lump index is invalid!");
                return false;
            }


            int vertexSizeInBytes = 4;
            int verticesCount = (int) (_WAD_Directories[mapIndex].LumpSize / vertexSizeInBytes);


            Vector3 vertex;
            for (int i = 0; i < verticesCount; i++)
            {
                vertex = _Reader.ReadVertexData(_WAD_Data, (int) (_WAD_Directories[mapIndex].LumpOffset + i * vertexSizeInBytes));

                map.AddVertexDef(vertex);

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
                Debug.LogError("Failed to load lineDefs! The map's lump index is invalid!");
                return false;
            }

            mapIndex += (int)MapLumpIndices.LineDefs;

            if (_WAD_Directories[mapIndex].LumpName.CompareTo("LINEDEFS") != 0)
            {
                Debug.LogError("Failed to load linedefs! The map's linedefs lump index is invalid!");
                return false;
            }


            int lineDefSizeInBytes = 14;
            int lineDefsCount = (int) (_WAD_Directories[mapIndex].LumpSize / lineDefSizeInBytes);

            LineDef lineDef;
            for (int i = 0; i < lineDefsCount; i++)
            {
                lineDef = _Reader.ReadLineDefData(_WAD_Data, (int) (_WAD_Directories[mapIndex].LumpOffset + i * lineDefSizeInBytes));

                map.AddLineDef(lineDef);

                //lineDef.DEBUG_Print();
            }


            Debug.Log($"Loaded {lineDefsCount} lineDefs for {map.Name}.");

            return true;
        }

        bool ReadMapThings(Map map)
        {
            int mapIndex = FindMapIndex(map);
            if (mapIndex == -1)
            {
                Debug.LogError("Failed to load things! The map's lump index is invalid!");
                return false;
            }

            mapIndex += (int)MapLumpIndices.Things;

            if (_WAD_Directories[mapIndex].LumpName.CompareTo("THINGS") != 0)
            {
                Debug.LogError("Failed to load things! The map's things lump index is invalid!");
                return false;
            }


            int thingSizeInBytes = 10;
            int thingsCount = (int) (_WAD_Directories[mapIndex].LumpSize / thingSizeInBytes);

            ThingDef thing;
            for (int i = 0; i < thingsCount; i++)
            {
                thing = _Reader.ReadThingData(_WAD_Data, (int) (_WAD_Directories[mapIndex].LumpOffset + i * thingSizeInBytes));

                map.AddThingDef(thing);

                //thing.DEBUG_Print();
            }


            Debug.Log($"Loaded {thingsCount} things for {map.Name}.");

            return true;
        }

        bool ReadMapNodes(Map map)
        {
            int mapIndex = FindMapIndex(map);
            if (mapIndex == -1)
            {
                Debug.LogError("Failed to load nodes! The map's lump index is invalid!");
                return false;
            }

            mapIndex += (int)MapLumpIndices.Nodes;

            if (_WAD_Directories[mapIndex].LumpName.CompareTo("NODES") != 0)
            {
                Debug.LogError("Failed to load nodes! The map's nodes lump index is invalid!");
                return false;
            }


            int nodeSizeInBytes = 28;
            int nodesCount = (int) (_WAD_Directories[mapIndex].LumpSize / nodeSizeInBytes);

            NodeDef node;
            for (int i = 0; i < nodesCount; i++)
            {
                node = _Reader.ReadNodeData(_WAD_Data, (int) (_WAD_Directories[mapIndex].LumpOffset + i * nodeSizeInBytes));

                map.AddNodeDef(node);

                //node.DEBUG_Print();
            }


            Debug.Log($"Loaded {nodesCount} nodes for {map.Name}.");

            return true;
        }

        bool ReadMapSubSectors(Map map)
        {
            int mapIndex = FindMapIndex(map);
            if (mapIndex == -1)
            {
                Debug.LogError("Failed to load subSectors! The map's lump index is invalid!");
                return false;
            }

            mapIndex += (int)MapLumpIndices.SubSectors;

            if (_WAD_Directories[mapIndex].LumpName.CompareTo("SSECTORS") != 0)
            {
                Debug.LogError("Failed to load subSectors! The map's subSectors lump index is invalid!");
                return false;
            }


            int subSectorSizeInBytes = 4;
            int subSectorsCount = (int) (_WAD_Directories[mapIndex].LumpSize / subSectorSizeInBytes);

            SubSectorDef subSector;
            for (int i = 0; i < subSectorsCount; i++)
            {
                subSector = _Reader.ReadSubSectorData(_WAD_Data, (int) (_WAD_Directories[mapIndex].LumpOffset + i * subSectorSizeInBytes));

                map.AddSubSectorDef(subSector);

                //subSector.DEBUG_Print();
            }


            Debug.Log($"Loaded {subSectorsCount} subSectors for {map.Name}.");

            return true;
        }

        bool ReadMapSegs(Map map)
        {
            int mapIndex = FindMapIndex(map);
            if (mapIndex == -1)
            {
                Debug.LogError("Failed to load segs! The map's lump index is invalid!");
                return false;
            }

            mapIndex += (int)MapLumpIndices.Segs;

            if (_WAD_Directories[mapIndex].LumpName.CompareTo("SEGS") != 0)
            {
                Debug.LogError("Failed to load segs! The map's segs lump index is invalid!");
                return false;
            }


            int segSizeInBytes = 12;
            int segsCount = (int) (_WAD_Directories[mapIndex].LumpSize / segSizeInBytes);

            SegDef seg;
            for (int i = 0; i < segsCount; i++)
            {
                seg = _Reader.ReadSegData(_WAD_Data, (int) (_WAD_Directories[mapIndex].LumpOffset + i * segSizeInBytes));

                map.AddSegDef(seg);

                //seg.DEBUG_Print();
            }


            Debug.Log($"Loaded {segsCount} segs for {map.Name}.");

            return true;
        }

        bool ReadPaletteData(Map map)
        {
            if (_AssetManager == null)
                _AssetManager = AssetManager.Instance;


            int palettesLumpIndex = FindLumpByName("PLAYPAL");
            if (_WAD_Directories[palettesLumpIndex].LumpName.CompareTo("PLAYPAL") != 0)
            {
                Debug.LogError("Failed to load palettes! The palettes lump index is invalid!");
                return false;
            }


            int paletteSizeInBytes = 256 * 3;
            int palettesCount = (int) (_WAD_Directories[palettesLumpIndex].LumpSize / paletteSizeInBytes);

            PaletteDef palette;
            for (int i = 0; i < palettesCount; i++)
            {
                palette = _Reader.ReadPaletteData(_WAD_Data, (int) (_WAD_Directories[palettesLumpIndex].LumpOffset + i * paletteSizeInBytes));

                _AssetManager.AddPalette(palette);

                //palette.DEBUG_Print();
            }


            Debug.Log($"Loaded {palettesCount} palettes.");

            return true;
        }

        /// <summary>
        /// Loads a texture.
        /// </summary>
        /// <param name="map">The map we're loading.</param>
        /// <returns>True if sueccessfull or false otherwise.</returns>
        public bool ReadPatchData(string patchName, out Patch patch)
        {
            // This just sets the variable in case we fail to read in the specified patch and thus return earl
            patch = new Patch("DUMMY", new WAD_PatchHeader());


            int patchLumpIndex = FindLumpByName(patchName);
            if (_WAD_Directories[patchLumpIndex].LumpName.CompareTo(patchName) != 0)
            {
                Debug.LogError("Failed to load patch! The patch's lump index is invalid!");
                return false;
            }


            WAD_PatchHeader patchHeader =_Reader.ReadPatchHeader(_WAD_Data, (int) _WAD_Directories[patchLumpIndex].LumpOffset);


            patch = new Patch(patchName, patchHeader);

            WAD_PatchColumn patchColumn = new WAD_PatchColumn();
            for (int i = 0; i < patchHeader.Width; i++)
            {
                int offset = (int) (_WAD_Directories[patchLumpIndex].LumpOffset + patchHeader.GetColumnOffset(i));

                while (patchColumn.TopDelta != 0xFF)
                {
                    patchColumn = _Reader.ReadPatchColumn(_WAD_Data, offset, out int nextColumnOffset);
                    offset = nextColumnOffset;
                    patch.AddPatchColumn(patchColumn);
                }

                // Reset this so the while loop can run again.
                patchColumn.TopDelta = 0;
            }
            

            Debug.Log($"Loaded patch \"{patchName}\".");

            return true;
        }

        private void Clear()
        {
            _WAD_Data = null;
            _WAD_Directories = null;
        }


    }
}
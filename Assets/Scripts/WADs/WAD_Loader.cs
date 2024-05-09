using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

using DIY_DOOM.Maps;
using DIY_DOOM.WADs;
using DIY_DOOM.WADs.Data;
using DIY_DOOM.WADs.Data.Maps;
using DIY_DOOM.WADs.Data.Textures;
using JetBrains.Annotations;

namespace DIY_DOOM.WADs
{
    public class WAD_Loader
    {
        private string _FilePath;
        private byte[] _WAD_Data; // Stores the loaded file contents.
        private List<WAD_DirectoryDef> _WAD_Directories; // The directories inside the WAD file.

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

        private void Clear()
        {
            _WAD_Data = null;
            _WAD_Directories = null;
        }

        protected bool ReadDirectories()
        {
            WAD_Header header = _Reader.ReadHeaderData(_WAD_Data, 0);

            //header.DEBUG_Print();

            _WAD_Directories = new List<WAD_DirectoryDef>();

            WAD_DirectoryDef directory = new WAD_DirectoryDef();
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
            if (_AssetManager == null)
                _AssetManager = AssetManager.Instance;


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

            if (!ReadMapSectors(map))
            {
                DisplayLoadMapDataFailedError("sectors", map);
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

            if (!ReadMapSideDefs(map))
            {
                DisplayLoadMapDataFailedError("sideDefs", map);
                return false;
            }


            map.DoFinalProcessing();

            ReadTextureData();


            // Unload the raw wad data, since we don't need it anymore.
            // NOTE: This is commented out since we need to keep it around for whenever we load patches.
            //UnloadRawWadData(); 


            return true;
        }

        private bool ReadTextureData()
        {
            if (!ReadPaletteData())
            {
                DisplayLoadDataFailedError("PLAYPAL");
                return false;
            }

            if (!ReadPatchNames())
            {
                DisplayLoadDataFailedError("PNAMES");
                return false;
            }

            if (!ReadPatchesData())
            {
                Debug.LogError($"Failed to load all patch data entries!");
                return false;
            }

            if (!ReadTexturesData("TEXTURE1"))
            {
                DisplayLoadDataFailedError("TEXTURE1");
                return false;
            }

            if (!ReadTexturesData("TEXTURE2"))
            {
                DisplayLoadDataFailedError("TEXTURE2");
                return false;
            }

            if (!ReadFlatsData())
            {
                DisplayLoadDataFailedError("FLATS");
                return false;
            }
            

            return true;
        }

        public void DisplayLoadMapDataFailedError(string type, Map map)
        {
            Debug.LogError($"Failed to load {type} data for the map ({map.Name})!");
            map = null;
        }

        public void DisplayLoadDataFailedError(string lumpName)
        {
            Debug.LogError($"Failed to load from lump \"{lumpName}\"!");
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
                Debug.LogError($"Failed to load vertices! The map's lump index ({mapIndex}) is invalid!");
                return false;
            }

            mapIndex += (int)MapLumpIndices.Vertices;

            if (_WAD_Directories[mapIndex].LumpName.CompareTo("VERTEXES") != 0)
            {
                Debug.LogError($"Failed to load vertices! The map's vertices lump index ({mapIndex}) is invalid!");
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
                Debug.LogError($"Failed to load lineDefs! The map's lump index ({mapIndex}) is invalid!");
                return false;
            }

            mapIndex += (int)MapLumpIndices.LineDefs;

            if (_WAD_Directories[mapIndex].LumpName.CompareTo("LINEDEFS") != 0)
            {
                Debug.LogError($"Failed to load linedefs! The map's linedefs lump index ({mapIndex}) is invalid!");
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
                Debug.LogError($"Failed to load things! The map's lump index ({mapIndex}) is invalid!");
                return false;
            }

            mapIndex += (int)MapLumpIndices.Things;

            if (_WAD_Directories[mapIndex].LumpName.CompareTo("THINGS") != 0)
            {
                Debug.LogError($"Failed to load things! The map's things lump index ({mapIndex}) is invalid!");
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
                Debug.LogError($"Failed to load nodes! The map's lump index ({mapIndex}) is invalid!");
                return false;
            }

            mapIndex += (int)MapLumpIndices.Nodes;

            if (_WAD_Directories[mapIndex].LumpName.CompareTo("NODES") != 0)
            {
                Debug.LogError($"Failed to load nodes! The map's nodes lump index ({mapIndex}) is invalid!");
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

        bool ReadMapSectors(Map map)
        {
            int mapIndex = FindMapIndex(map);
            if (mapIndex == -1)
            {
                Debug.LogError($"Failed to load sectors! The map's lump index ({mapIndex}) is invalid!");
                return false;
            }

            mapIndex += (int)MapLumpIndices.Sectors;

            if (_WAD_Directories[mapIndex].LumpName.CompareTo("SECTORS") != 0)
            {
                Debug.LogError($"Failed to load sectors! The map's sectors lump index ({mapIndex}) is invalid!");
                return false;
            }


            int sectorSizeInBytes = 26;
            int sectorsCount = (int)(_WAD_Directories[mapIndex].LumpSize / sectorSizeInBytes);

            SectorDef sector;
            for (int i = 0; i < sectorsCount; i++)
            {
                sector = _Reader.ReadSectorData(_WAD_Data, (int)(_WAD_Directories[mapIndex].LumpOffset + i * sectorSizeInBytes));

                map.AddSectorDef(sector);

                //sector.DEBUG_Print();
            }


            Debug.Log($"Loaded {sectorsCount} sectors for {map.Name}.");

            return true;
        }

        bool ReadMapSubSectors(Map map)
        {
            int mapIndex = FindMapIndex(map);
            if (mapIndex == -1)
            {
                Debug.LogError($"Failed to load subSectors! The map's lump index ({mapIndex}) is invalid!");
                return false;
            }

            mapIndex += (int)MapLumpIndices.SubSectors;

            if (_WAD_Directories[mapIndex].LumpName.CompareTo("SSECTORS") != 0)
            {
                Debug.LogError($"Failed to load subSectors! The map's subSectors lump index ({mapIndex}) is invalid!");
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
                Debug.LogError($"Failed to load segs! The map's lump index ({mapIndex}) is invalid!");
                return false;
            }

            mapIndex += (int)MapLumpIndices.Segs;

            if (_WAD_Directories[mapIndex].LumpName.CompareTo("SEGS") != 0)
            {
                Debug.LogError($"Failed to load segs! The map's segs lump index ({mapIndex}) is invalid!");
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

        bool ReadMapSideDefs(Map map)
        {
            int mapIndex = FindMapIndex(map);
            if (mapIndex == -1)
            {
                Debug.LogError($"Failed to load sideDefs! The map's lump index ({mapIndex}) is invalid!");
                return false;
            }

            mapIndex += (int)MapLumpIndices.SideDefs;

            if (_WAD_Directories[mapIndex].LumpName.CompareTo("SIDEDEFS") != 0)
            {
                Debug.LogError($"Failed to load sideDefs! The map's sideDefs lump index ({mapIndex}) is invalid!");
                return false;
            }


            int sideDefSizeInBytes = 30;
            int sideDefsCount = (int)(_WAD_Directories[mapIndex].LumpSize / sideDefSizeInBytes);

            SideDef sideDef;
            for (int i = 0; i < sideDefsCount; i++)
            {
                sideDef = _Reader.ReadSideDefData(_WAD_Data, (int)(_WAD_Directories[mapIndex].LumpOffset + i * sideDefSizeInBytes));

                map.AddSideDef(sideDef);

                //sideDef.DEBUG_Print();
            }


            Debug.Log($"Loaded {sideDefsCount} sideDefs for {map.Name}.");

            return true;
        }

        bool ReadPaletteData()
        {
            if (_AssetManager == null)
                _AssetManager = AssetManager.Instance;


            int palettesLumpIndex = FindLumpByName("PLAYPAL");
            if (_WAD_Directories[palettesLumpIndex].LumpName.CompareTo("PLAYPAL") != 0)
            {
                Debug.LogError($"Failed to load palettes! The palettes lump index ({palettesLumpIndex}) is invalid!");
                return false;
            }


            int paletteSizeInBytes = 256 * 3;
            int palettesCount = (int) (_WAD_Directories[palettesLumpIndex].LumpSize / paletteSizeInBytes);

            Palette palette;
            for (int i = 0; i < palettesCount; i++)
            {
                palette = _Reader.ReadPaletteData(_WAD_Data, (int) (_WAD_Directories[palettesLumpIndex].LumpOffset + i * paletteSizeInBytes));

                _AssetManager.AddPalette(palette);

                //palette.DEBUG_Print();
            }


            Debug.Log($"Loaded {palettesCount} palettes.");

            return true;
        }

        public bool ReadPatchNames()
        {
            int patchNamesLumpIndex = FindLumpByName("PNAMES");
            if (_WAD_Directories[patchNamesLumpIndex].LumpName.CompareTo("PNAMES") != 0)
            {
                Debug.LogError($"Failed to load patch names! The patch names lump index ({patchNamesLumpIndex}) is invalid!");
                return false;
            }



            PatchNamesHeader patchNamesHeader = _Reader.ReadPatchNamesHeader(_WAD_Data, (int)_WAD_Directories[patchNamesLumpIndex].LumpOffset);
            string name = "";
            for (int i = 0; i < patchNamesHeader.PatchNamesCount; i++)
            {
                name = _Reader.Read8ByteString(_WAD_Data, (int)patchNamesHeader.PatchNamesOffset);
                _AssetManager.AddPatchName(name.ToUpper()); // The ToUpper() here is needed as one of the patches has its name entered wrong in the names list in DOOM.WAD, starting with a lower case letter.
                patchNamesHeader.PatchNamesOffset += 8;
            }

            Debug.Log($"Loaded {patchNamesHeader.PatchNamesCount} patch names from lump \"PNAMES\".");

            return true;
        }

        /// <summary>
        /// This function reads in data for all patches.
        /// </summary>
        /// <returns>True if successful or false otherwise.</returns>
        public bool ReadPatchesData()
        {
            for (int i = 0; i < _AssetManager.PatchNamesCount; i++) 
            {
                if (!ReadPatchData(_AssetManager.GetPatchName(i), out Patch patch))
                    return false;
            }

            Debug.Log($"Loaded {_AssetManager.PatchNamesCount} patch data entries.");

            return true;
        }

        /// <summary>
        /// Loads a single patch, or returns it if it is already loaded.
        /// </summary>
        /// <param name="map">The map we're loading.</param>
        /// <returns>True if sueccessfull or false otherwise.</returns>
        public bool ReadPatchData(string patchName, out Patch patch)
        {
            // Check if this patch is already loaded.
            if (_AssetManager.ContainsPatch(patchName))
            {
                patch = _AssetManager.GetRawPatchData(patchName);
                return true;
            }


            // This just sets the variable in case we fail to read in the specified patch and thus return earl
            patch = new Patch("DUMMY", new PatchHeader());


            int patchLumpIndex = FindLumpByName(patchName);
            if (patchLumpIndex < 0 || patchLumpIndex > _WAD_Directories.Count)
                return false;
            if (_WAD_Directories[patchLumpIndex].LumpName.CompareTo(patchName) != 0)
            {
                Debug.LogError($"Failed to load patch! The patch's lump index ({patchLumpIndex}) is invalid!");
                return false;
            }


            PatchHeader patchHeader =_Reader.ReadPatchHeader(_WAD_Data, (int) _WAD_Directories[patchLumpIndex].LumpOffset);


            patch = new Patch(patchName, patchHeader);

            PatchColumn patchColumn = new PatchColumn();
            for (int i = 0; i < patchHeader.Width; i++)
            {
                int offset = (int) (_WAD_Directories[patchLumpIndex].LumpOffset + patchHeader.GetColumnOffset(i));
                
                patch.AppendColumnStartIndex();

                while (true)
                {
                    patchColumn = _Reader.ReadPatchColumn(_WAD_Data, offset, out int nextColumnOffset);
                    offset = nextColumnOffset;
                    patch.AddPatchColumn(patchColumn);

                    if (patchColumn.TopDelta == 0xFF)
                        break;
                }

            }


            _AssetManager.AddRawPatchData(patchName, patch);


            //Debug.Log($"Loaded patch \"{patchName}\".");

            return true;
        }

        public bool ReadTexturesData(string texturesLumpName)
        {
            int textureLumpIndex = FindLumpByName(texturesLumpName);
            if (_WAD_Directories[textureLumpIndex].LumpName.CompareTo(texturesLumpName) != 0)
            {
                Debug.LogError($"Failed to load texture data for texture \"{texturesLumpName}\"! The texture's data lump index ({textureLumpIndex}) is invalid!");
                return false;
            }


            TextureHeader textureHeader = _Reader.ReadTextureHeader(_WAD_Data, (int) _WAD_Directories[textureLumpIndex].LumpOffset);

            for (int i = 0; i <textureHeader.TexturesCount; i++)
            {
                TextureData textureData = _Reader.ReadTextureData(_WAD_Data, (int) _WAD_Directories[textureLumpIndex].LumpOffset + (int) textureHeader.GetTextureDataOffset(i));
                _AssetManager.AddRawTextureData(textureData.TextureName,textureData);
            }

            Debug.Log($"Loaded {textureHeader.TexturesCount} texture data entries from lump \"{texturesLumpName}\".");

            return true;
        }

        /// <summary>
        /// This function reads in data for all flats (floor/ceiling textures).
        /// </summary>
        /// <returns>True if successful or false otherwise.</returns>
        public bool ReadFlatsData()
        {
            // Find the empty lump named "F_START". This is a marker that denotes where in the WAD file
            // the data entries for the flats begin (flats are floor/ceiling textures).
            int flatsLumpIndex = FindLumpByName("F_START");

            for (int i = flatsLumpIndex; i < _WAD_Directories.Count; i++)
            {
                WAD_DirectoryDef dirDef = _WAD_Directories[i];
                
                // If the current directory is empty, skip it. It's a marker and not a flat.
                // NOTE: This may not be true in all WAD files, but in DOOM.WAD, the flats are the last data lumps in the file along with a few markers.
                if (dirDef.LumpSize < 1)
                    continue;


                byte[] flatData = _Reader.ReadFlatData(_WAD_Data, (int)dirDef.LumpOffset);
                if (flatData == null || flatData.Length != 4096)
                {
                    Debug.LogError($"Failed to read in flat data for flat \"{dirDef.LumpName}\"!");

                    return false;
                }

                Flat flat = new Flat(dirDef.LumpName, flatData);
                _AssetManager.AddRawFlatData(dirDef.LumpName, flat);
            }

            Debug.Log($"Loaded {_AssetManager.FlatCount} flats (floor/ceiling texture) data entries.");

            return true;
        }

        public bool ReadFlatData(string flatName, out Flat flat)
        {
            flat = null;


            // Check if this flat is already loaded.
            if (_AssetManager.ContainsFlat(flatName))
            {
                flat = _AssetManager.GetRawFlatData(flatName);
                return true;
            }


            // Find the empty lump named "F_START". This is a marker that denotes where in the WAD file
            // the data entries for the flats begin (flats are floor/ceiling textures).
            int flatsLumpIndex = FindLumpByName("F_START");

            for (int i = flatsLumpIndex; i < _WAD_Directories.Count; i++)
            {
                WAD_DirectoryDef dirDef = _WAD_Directories[i];


                // Is this flat the one we're looking for?
                if (dirDef.LumpName != flatName)
                    continue;

                // If the current directory is empty, skip it. It's a marker and not a flat.
                // NOTE: This may not be true in all WAD files, but in DOOM.WAD, the flats are the last data lumps in the file along with a few markers.
                if (dirDef.LumpSize < 1)
                    continue;


                byte[] flatData = _Reader.ReadFlatData(_WAD_Data, (int)dirDef.LumpOffset);
                if (flatData == null || flatData.Length != 4096)
                {
                    Debug.LogError($"Failed to read in flat data for flat \"{flatName}\"!    " + (flatData == null) + "    " + (flatData != null ? flatData.Length : 0));

                    return false;
                }


                flat = new Flat(flatName, flatData);

                _AssetManager.AddRawFlatData(flatName, flat);

                return true;
            }

            return false;

        }

    }
}
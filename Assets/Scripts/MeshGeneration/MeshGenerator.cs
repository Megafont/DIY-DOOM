using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DIY_DOOM.Maps;
using DIY_DOOM.WADs.Data.Maps;
using DIY_DOOM.Utils.Textures;


namespace DIY_DOOM.MeshGeneration
{
    public struct MeshGenOutput
    {
        public List<Mesh> MapSubMeshes;
        public List<Material> MapSubMeshMaterials;
    }


    public struct MeshData
    {
        public List<Vector3> Vertices;
        public List<int> Triangles;
        public List<Vector2> UVs;

        public Material Material;

        public string TextureName;



        public MeshData(string textureName, Material material)
        {
            Vertices = new List<Vector3>();
            Triangles = new List<int>();
            UVs = new List<Vector2>();

            Material = material;
            TextureName = textureName;
        }


        public void ClearGeometry()
        {
            Vertices.Clear();
            Triangles.Clear();
            UVs.Clear();
        }
    }



    // ********************************************************************************************************************************************************************************************************
    // *  I could implement lighting by creating different versions of the textures for different light levels similarly to how I did it for palettes already.
    // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // *  An improvement to that code would be to get rid of adding a number to the end of the string, and instead just add a couple bytes on the end.
    // *  The first added byte is the palette index, and the 2nd added byte is the light level.
    // *  This will make it far easier to extract that extra data from the end of the name string, which is currently formatted as <TextureName>_<PaletteIndex>.   
    // ********************************************************************************************************************************************************************************************************



    /// <summary>
    /// This class holds data used during the mesh generation process.
    /// </summary>
    /// <remarks>
    /// The code in this class was written with help from the following resources:
    /// * https://doom.fandom.com/wiki/Texture_alignment
    /// * https://wiki.srb2.org/wiki/SRB2_Doom_Builder_tutorial/Sectors_and_textures
    /// * and the repo linked in the Readme.md file in the root folder of this project.
    /// </remarks>
    public static class MeshGenerator
    {
        private static Material _DOOM_MaterialPrefab; // This is the material we use to create a material instance for each of our textures;
        private static Material _DOOM_MissingTextureMaterial;


        public static int _CurVertexIndex; // The index of the next vertex to be added

        public static LineDef _CurLineDef;
        public static Vector3 _LineDefStart;
        public static Vector3 _LineDefEnd;

        public static SideDef _CurLeftSideDef;
        public static SideDef _CurRightSideDef; // Front side?

        public static SectorDef _CurLeftSectorDef;
        public static SectorDef _CurRightSectorDef;

        public static MeshData _CurMeshData;

        public static Map _Map;

        public static Mesh _MapMesh;

        public static float _PixelSizeInWorldUnits;
    
        public static Dictionary<string, MeshData> _SubMeshLookup;



        private static void InitMeshGeneration(Map map)
        {
            _Map = map;

            _MapMesh = new Mesh();

            // Initialize the mesh data.
            _SubMeshLookup = new Dictionary<string, MeshData>();

            // Calculate the size of a pixel in world units. See the comments for Map.ScaleFactor for more information on this.
            _PixelSizeInWorldUnits = 16 / _Map.ScaleFactor;

            _DOOM_MaterialPrefab = DoomEngine.Settings.DOOM_MaterialPrefab;
            _DOOM_MissingTextureMaterial = DoomEngine.Settings.DOOM_MissingTextureMaterial;
        }


        private static void ResetList<T>(ref List<T> list)
        {
            if (list == null)
                list = new List<T>();
            else
                list.Clear();
        }

        public static MeshGenOutput GenerateMapMesh(Map map)
        {
            InitMeshGeneration(map);


            for (uint i = 0; i < map.LineDefsCount; i++)
            {
                _CurLineDef = map.GetLineDef(i);
                GetLineDefInfo();


                if (!_CurLineDef.Flags.HasFlag(LineDefFlags.TwoSided))
                {
                    GenerateLineDefGeometry_SingleSided();
                }
                else
                {                    
                    if (TextureUtils.IsNameValid(_CurRightSideDef.LowerTextureName))
                        GenerateLineDefGeometry_DoubleSided_LowerTexture();
                    if (TextureUtils.IsNameValid(_CurRightSideDef.UpperTextureName))
                        GenerateLineDefGeometry_DoubleSided_UpperTexture();
                    if (TextureUtils.IsNameValid(_CurRightSideDef.MiddleTextureName))
                        GenerateLineDefGeometry_DoubleSided_MiddleTexture();
                }

            } // end for i


            return CreateOutputObject();            
        }

        private static void GetLineDefInfo()
        {
            _LineDefStart = _Map.GetVertex(_CurLineDef.StartVertexID);
            _LineDefEnd = _Map.GetVertex(_CurLineDef.EndVertexID);

            if (_CurLineDef.LeftSideDefIndex >= 0)
                _CurLeftSideDef = _Map.GetSideDef((uint) _CurLineDef.LeftSideDefIndex);
            if (_CurLineDef.RightSideDefIndex >= 0)
                _CurRightSideDef = _Map.GetSideDef((uint) _CurLineDef.RightSideDefIndex);

            _CurLeftSectorDef = _Map.GetSectorDef((uint) _CurLeftSideDef.SectorIndex);
            _CurRightSectorDef = _Map.GetSectorDef((uint) _CurRightSideDef.SectorIndex);

            
        }

        private static void GenerateLineDefGeometry_SingleSided()
        {
            float floorHeight = _CurRightSectorDef.FloorHeight;
            float ceilingHeight = _CurRightSectorDef.CeilingHeight;


            //_CurLeftSideDef.DEBUG_Print();
            //_CurRightSideDef.DEBUG_Print();

            // Get the appropriate MeshData object, and store it in _CurMeshData.
            GetMeshData(_CurRightSideDef.MiddleTextureName);

            int firstVertIndex = _CurMeshData.Vertices.Count;


            // NOTE: We do NOT scale these vertices. Remember that the vertex gets scaled after being passed into Map.AddVertex().
            //       The floor and ceiling heights are scaled when the SectorDef was passed into Map.AddSectorDef().

            _CurMeshData.Vertices.Add(new Vector3(_LineDefStart.x, floorHeight, _LineDefStart.z));
            _CurMeshData.Vertices.Add(new Vector3(_LineDefStart.x, ceilingHeight, _LineDefStart.z));
            _CurMeshData.Vertices.Add(new Vector3(_LineDefEnd.x, ceilingHeight, _LineDefEnd.z));

            _CurMeshData.Vertices.Add(new Vector3(_LineDefStart.x, floorHeight, _LineDefStart.z));
            _CurMeshData.Vertices.Add(new Vector3(_LineDefEnd.x, ceilingHeight, _LineDefEnd.z));
            _CurMeshData.Vertices.Add(new Vector3(_LineDefEnd.x, floorHeight, _LineDefEnd.z));

            _CurMeshData.Triangles.AddRange(new int[] { firstVertIndex, firstVertIndex + 1, firstVertIndex + 2 });
            _CurMeshData.Triangles.AddRange(new int[] { firstVertIndex + 3, firstVertIndex + 4, firstVertIndex + 5 });


            GenerateUVsForSingleSidedLineDef(_CurLineDef.Flags,
                                             Vector3.Distance(_LineDefStart, _LineDefEnd),
                                             ceilingHeight - floorHeight);
        }

        private static void GenerateLineDefGeometry_DoubleSided_LowerTexture()
        {
            float highestFloor = Mathf.Max(_CurLeftSectorDef.FloorHeight, _CurRightSectorDef.FloorHeight);
            float lowestFloor = Mathf.Min(_CurLeftSectorDef.FloorHeight, _CurRightSectorDef.FloorHeight);

            // Get the appropriate MeshData object, and store it in _CurMeshData.
            GetMeshData(_CurRightSideDef.LowerTextureName);

            int firstVertIndex = _CurMeshData.Vertices.Count;


            // NOTE: We do NOT scale these vertices. Remember that the vertex gets scaled after being passed into Map.AddVertex().
            //       The floor and ceiling heights are scaled when the SectorDef was passed into Map.AddSectorDef().

            _CurMeshData.Vertices.Add(new Vector3(_LineDefStart.x, lowestFloor, _LineDefStart.z));
            _CurMeshData.Vertices.Add(new Vector3(_LineDefStart.x, highestFloor, _LineDefStart.z));
            _CurMeshData.Vertices.Add(new Vector3(_LineDefEnd.x, highestFloor, _LineDefEnd.z));

            _CurMeshData.Vertices.Add(new Vector3(_LineDefStart.x, lowestFloor, _LineDefStart.z));
            _CurMeshData.Vertices.Add(new Vector3(_LineDefEnd.x, highestFloor, _LineDefEnd.z));
            _CurMeshData.Vertices.Add(new Vector3(_LineDefEnd.x, lowestFloor, _LineDefEnd.z));

            _CurMeshData.Triangles.AddRange(new int[] { firstVertIndex, firstVertIndex + 1, firstVertIndex + 2 });
            _CurMeshData.Triangles.AddRange(new int[] { firstVertIndex + 3, firstVertIndex + 4, firstVertIndex + 5 });


            GenerateUVsForDoubleSidedLineDef_LowerTexture(_CurLineDef.Flags,
                                                           Vector3.Distance(_LineDefStart, _LineDefEnd),
                                                           highestFloor - lowestFloor);
        }

        private static void GenerateLineDefGeometry_DoubleSided_UpperTexture()
        {
            float highestCeiling = Mathf.Max(_CurLeftSectorDef.CeilingHeight, _CurRightSectorDef.CeilingHeight);
            float lowestCeiling = Mathf.Min(_CurLeftSectorDef.CeilingHeight, _CurRightSectorDef.CeilingHeight);

            // Get the appropriate MeshData object, and store it in _CurMeshData.
            GetMeshData(_CurRightSideDef.UpperTextureName);

            int firstVertIndex = _CurMeshData.Vertices.Count;


            // NOTE: We do NOT scale these vertices. Remember that the vertex gets scaled after being passed into Map.AddVertex().
            //       The floor and ceiling heights are scaled when the SectorDef was passed into Map.AddSectorDef().

            _CurMeshData.Vertices.Add(new Vector3(_LineDefStart.x, lowestCeiling, _LineDefStart.z));
            _CurMeshData.Vertices.Add(new Vector3(_LineDefStart.x, highestCeiling, _LineDefStart.z));
            _CurMeshData.Vertices.Add(new Vector3(_LineDefEnd.x, highestCeiling, _LineDefEnd.z));

            _CurMeshData.Vertices.Add(new Vector3(_LineDefStart.x, lowestCeiling, _LineDefStart.z));
            _CurMeshData.Vertices.Add(new Vector3(_LineDefEnd.x, highestCeiling, _LineDefEnd.z));
            _CurMeshData.Vertices.Add(new Vector3(_LineDefEnd.x, lowestCeiling, _LineDefEnd.z));

            _CurMeshData.Triangles.AddRange(new int[] { firstVertIndex, firstVertIndex + 1, firstVertIndex + 2 });
            _CurMeshData.Triangles.AddRange(new int[] { firstVertIndex + 3, firstVertIndex + 4, firstVertIndex + 5 });


            GenerateUVsForDoubleSidedLineDef_UpperTexture(_CurLineDef.Flags,
                                                           Vector3.Distance(_LineDefStart, _LineDefEnd),
                                                           highestCeiling - lowestCeiling);
        }

        private static void GenerateLineDefGeometry_DoubleSided_MiddleTexture()
        {
            float highestFloor = Mathf.Max(_CurLeftSectorDef.FloorHeight, _CurRightSectorDef.FloorHeight);
            float lowestCeiling = Mathf.Min(_CurLeftSectorDef.CeilingHeight, _CurRightSectorDef.CeilingHeight);

            // Get the appropriate MeshData object, and store it in _CurMeshData.
            GetMeshData(_CurRightSideDef.MiddleTextureName);

            int firstVertIndex = _CurMeshData.Vertices.Count;


            //Debug.Log($"\"{_CurMeshData.Material.mainTexture.name}\"    FloorL: {_CurLeftSectorDef.FloorHeight}    FloorR: {_CurRightSectorDef.FloorHeight}    CeilL: {_CurLeftSectorDef.CeilingHeight}    CeilR: {_CurRightSectorDef.CeilingHeight}    HighestFloor: {highestFloor}    LowestCeil: {lowestCeiling}");


            // NOTE: We do NOT scale these vertices. Remember that the vertex gets scaled after being passed into Map.AddVertex().
            //       The floor and ceiling heights are scaled when the SectorDef was passed into Map.AddSectorDef().

            _CurMeshData.Vertices.Add(new Vector3(_LineDefStart.x, lowestCeiling, _LineDefStart.z));
            _CurMeshData.Vertices.Add(new Vector3(_LineDefStart.x, highestFloor, _LineDefStart.z));
            _CurMeshData.Vertices.Add(new Vector3(_LineDefEnd.x, highestFloor, _LineDefEnd.z));

            _CurMeshData.Vertices.Add(new Vector3(_LineDefStart.x, lowestCeiling, _LineDefStart.z));
            _CurMeshData.Vertices.Add(new Vector3(_LineDefEnd.x, highestFloor, _LineDefEnd.z));
            _CurMeshData.Vertices.Add(new Vector3(_LineDefEnd.x, lowestCeiling, _LineDefEnd.z));

            _CurMeshData.Triangles.AddRange(new int[] { firstVertIndex, firstVertIndex + 1, firstVertIndex + 2 });
            _CurMeshData.Triangles.AddRange(new int[] { firstVertIndex + 3, firstVertIndex + 4, firstVertIndex + 5 });


            GenerateUVsForDoubleSidedLineDef_MiddleTexture(_CurLineDef.Flags,
                                                           Vector3.Distance(_LineDefStart, _LineDefEnd),
                                                           lowestCeiling - highestFloor);
        }

        private static void GenerateUVsForSingleSidedLineDef(LineDefFlags faceFlags, float faceWidth, float faceHeight)
        {
            float top = 0;
            float bottom = 0;
            float left = 1;
            float right = 1;


            // We multiply faceWidth by the scaleFactor to convert back to DOOM units. This is because a 1m section of wall is 64 pixels wide.
            // Then we divide by the texture width to find out how many texture repeats will fit across the length of the wall.
            float textureRepeatsX = (faceWidth * _Map.ScaleFactor) / _CurMeshData.Material.mainTexture.width;
            float textureRepeatsY = (faceHeight * _Map.ScaleFactor) / _CurMeshData.Material.mainTexture.height;

            float xOffset = (float)_CurRightSideDef.X_Offset / _CurMeshData.Material.mainTexture.width;
            float yOffset = (float)_CurRightSideDef.Y_Offset / _CurMeshData.Material.mainTexture.height;

            //Debug.Log($"{_CurMeshData.Material.mainTexture.width}x{_CurMeshData.Material.mainTexture.height}    {_PixelSizeInWorldUnits}    {faceWidth}    {textureRepeatsX}");

            if (!faceFlags.HasFlag(LineDefFlags.LowerTextureIsUnpegged))
            {
                // The top of the texture is snapped to the ceiling.
                left = 0;
                right = textureRepeatsX;
                top = 1;
                bottom = top - textureRepeatsY;
            }
            else
            {
                // The bottom of the texture is snapped to the floor.
                left = 0;
                right = textureRepeatsX;
                top = textureRepeatsY;
                bottom = 0;
            }

            left += xOffset;
            right += xOffset;
            top -= yOffset;
            bottom -= yOffset;


            _CurMeshData.UVs.Add(new Vector2(left, bottom));
            _CurMeshData.UVs.Add(new Vector2(left, top));
            _CurMeshData.UVs.Add(new Vector2(right, top));

            _CurMeshData.UVs.Add(new Vector2(left, bottom));
            _CurMeshData.UVs.Add(new Vector2(right, top));
            _CurMeshData.UVs.Add(new Vector2(right, bottom));
        }

        private static void GenerateUVsForDoubleSidedLineDef_LowerTexture(LineDefFlags faceFlags, float faceWidth, float faceHeight)
        {
            float top = 1;
            float bottom = 0;
            float left = 0;
            float right = 1;


            // We multiply faceWidth by the scaleFactor to convert back to DOOM units. This is because a 1m section of wall is 64 pixels wide.
            // Then we divide by the texture width to find out how many texture repeats will fit across the length of the wall.
            float textureRepeatsX = (faceWidth * _Map.ScaleFactor) / _CurMeshData.Material.mainTexture.width;
            float textureRepeatsY = (faceHeight * _Map.ScaleFactor) / _CurMeshData.Material.mainTexture.height;

            float xOffset = (float)_CurRightSideDef.X_Offset / _CurMeshData.Material.mainTexture.width;
            float yOffset = (float)_CurRightSideDef.Y_Offset / _CurMeshData.Material.mainTexture.height;

            //Debug.Log($"{_CurMeshData.Material.mainTexture.width}x{_CurMeshData.Material.mainTexture.height}    {_PixelSizeInWorldUnits}    {faceWidth}    {textureRepeatsX}");

            if (!faceFlags.HasFlag(LineDefFlags.LowerTextureIsUnpegged))
            {
                // The bottom of the texture is snapped to the lower floor.
                left = 0;
                right = textureRepeatsX;
                top = 1;
                bottom = top - textureRepeatsY;
            }
            else if (faceFlags.HasFlag(LineDefFlags.LowerTextureIsUnpegged))
            {
                // This is using the left sector's floor height on purpose, as we need to compare the floor height on the back side of this wall to the ceiling height of the sector directly in front of this wall.
                float heightFromFloorToCeiling = (_CurRightSectorDef.CeilingHeight - _CurLeftSectorDef.FloorHeight) * _Map.ScaleFactor;
                float textureRepeatsFromCeilingToFloor = heightFromFloorToCeiling / _CurMeshData.Material.mainTexture.height;

                //Debug.Log($"\"{_CurMeshData.Material.mainTexture.name}\"    {_CurLeftSectorDef.FloorHeight}    {_CurLeftSectorDef.CeilingHeight}    {_CurRightSectorDef.FloorHeight}    {_CurRightSectorDef.CeilingHeight}    {heightFromFloorToCeiling}    {textureRepeatsFromCeilingToFloor}    {_CurMeshData.Material.mainTexture.height}");

                // The top of the texture is snapped to the ceiling.
                left = 0;
                right = textureRepeatsX;
                top = 1 - textureRepeatsFromCeilingToFloor;
                bottom = top - textureRepeatsY;
            }

            left += xOffset;
            right += xOffset;
            top -= yOffset;
            bottom -= yOffset;


            _CurMeshData.UVs.Add(new Vector2(left, bottom));
            _CurMeshData.UVs.Add(new Vector2(left, top));
            _CurMeshData.UVs.Add(new Vector2(right, top));

            _CurMeshData.UVs.Add(new Vector2(left, bottom));
            _CurMeshData.UVs.Add(new Vector2(right, top));
            _CurMeshData.UVs.Add(new Vector2(right, bottom));
        }

        private static void GenerateUVsForDoubleSidedLineDef_UpperTexture(LineDefFlags faceFlags, float faceWidth, float faceHeight)
        {
            float top = 1;
            float bottom = 0;
            float left = 0;
            float right = 1;


            // We multiply faceWidth by the scaleFactor to convert back to DOOM units. This is because a 1m section of wall is 64 pixels wide.
            // Then we divide by the texture width to find out how many texture repeats will fit across the length of the wall.
            float textureRepeatsX = (faceWidth * _Map.ScaleFactor) / _CurMeshData.Material.mainTexture.width;
            float textureRepeatsY = (faceHeight * _Map.ScaleFactor) / _CurMeshData.Material.mainTexture.height;

            float xOffset = (float)_CurRightSideDef.X_Offset / _CurMeshData.Material.mainTexture.width;
            float yOffset = (float)_CurRightSideDef.Y_Offset / _CurMeshData.Material.mainTexture.height;

            //Debug.Log($"{_CurMeshData.Material.mainTexture.width}x{_CurMeshData.Material.mainTexture.height}    {_PixelSizeInWorldUnits}    {faceWidth}    {textureRepeatsX}");

            if (!faceFlags.HasFlag(LineDefFlags.UpperTextureIsUnpegged))
            {
                // The bottom of the texture is aligned to the lowest ceiling
                left = 0;
                right = textureRepeatsX;
                top = textureRepeatsY;
                bottom = 0;
            }
            else if (faceFlags.HasFlag(LineDefFlags.UpperTextureIsUnpegged))
            {
                // The top of the texture is aligned to the highest ceiling.
                left = 0;
                right = textureRepeatsX;
                top = 1;
                bottom = top - textureRepeatsY;
            }

            left += xOffset;
            right += xOffset;
            top -= yOffset;
            bottom -= yOffset;


            _CurMeshData.UVs.Add(new Vector2(left, bottom));
            _CurMeshData.UVs.Add(new Vector2(left, top));
            _CurMeshData.UVs.Add(new Vector2(right, top));

            _CurMeshData.UVs.Add(new Vector2(left, bottom));
            _CurMeshData.UVs.Add(new Vector2(right, top));
            _CurMeshData.UVs.Add(new Vector2(right, bottom));
        }

        private static void GenerateUVsForDoubleSidedLineDef_MiddleTexture(LineDefFlags faceFlags, float faceWidth, float faceHeight)
        {
            float top = 1;
            float bottom = 0;
            float left = 0;
            float right = 1;


            // We multiply faceWidth by the scaleFactor to convert back to DOOM units. This is because a 1m section of wall is 64 pixels wide.
            // Then we divide by the texture width to find out how many texture repeats will fit across the length of the wall.
            float textureRepeatsX = (faceWidth * _Map.ScaleFactor) / _CurMeshData.Material.mainTexture.width;
            // NOTE: Middle textures do not repeat vertically, hence why the line that calculates textureRepeatsY is missing here.

            float xOffset = (float)_CurRightSideDef.X_Offset / _CurMeshData.Material.mainTexture.width;
            float yOffset = (float)_CurRightSideDef.Y_Offset / _CurMeshData.Material.mainTexture.height;

            //Debug.Log($"TextureSize: {_CurMeshData.Material.mainTexture.width}x{_CurMeshData.Material.mainTexture.height}    FaceSize: {faceWidth}x{faceHeight}    {textureRepeatsX}    {xOffset}    {yOffset}");

            // The top of the texture is snapped to the lowest ceiling.
            left = 0;
            right = textureRepeatsX;
            top = 1;
            bottom = 0;


            left += xOffset;
            right += xOffset;
            top -= yOffset;
            bottom -= yOffset;


            _CurMeshData.UVs.Add(new Vector2(left, bottom));
            _CurMeshData.UVs.Add(new Vector2(left, top));
            _CurMeshData.UVs.Add(new Vector2(right, top));

            _CurMeshData.UVs.Add(new Vector2(left, bottom));
            _CurMeshData.UVs.Add(new Vector2(right, top));
            _CurMeshData.UVs.Add(new Vector2(right, bottom));
        }

        private static Material GetMaterial(string textureName)
        {
            Material newMaterial;


            Texture2D texture = AssetManager.Instance.GetTexture(textureName, 0);
            if (texture != null)
            {
                newMaterial = new Material(_DOOM_MaterialPrefab);
                newMaterial.name = $"({textureName})";
                newMaterial.mainTexture = texture;
            }
            else
            {
                newMaterial = new Material(_DOOM_MissingTextureMaterial);
                newMaterial.name = $"({textureName})";
                Debug.LogError($"Could not find texture \"{textureName}\" in the AssetManager. This face will be rendered with the missing texture material.");
            }


            return newMaterial;
        }

        /// <summary>
        /// Checks if the specified texture name is already associated with a subMesh or not.
        /// If so, it is retrieved and stored in _CurMeshData.
        /// If not, then a new MeshData object is created for the specified texture name.
        /// </summary>
        /// <param name="textureName"></param>
        /// <returns>True if the texture name already has a MeshData object associated with it, false if not.</returns>
        private static bool GetMeshData(string textureName)
        {
            if (_SubMeshLookup.TryGetValue(textureName, out _CurMeshData))
                return true;

           
            _CurMeshData = new MeshData(textureName, GetMaterial(textureName));
            _SubMeshLookup.Add(textureName, _CurMeshData);

            return false;
        }

        private static MeshGenOutput CreateOutputObject()
        {
            MeshGenOutput meshGenOutput = new MeshGenOutput();
            meshGenOutput.MapSubMeshes = new List<Mesh>();
            meshGenOutput.MapSubMeshMaterials = new List<Material>();


            foreach (KeyValuePair<string, MeshData> pair in _SubMeshLookup)
            {
                Mesh mesh = new Mesh();

                mesh.vertices = pair.Value.Vertices.ToArray();
                mesh.triangles = pair.Value.Triangles.ToArray();
                mesh.uv = pair.Value.UVs.ToArray();

                meshGenOutput.MapSubMeshes.Add(mesh);
                meshGenOutput.MapSubMeshMaterials.Add(pair.Value.Material);

            } // end foreach


            return meshGenOutput;
        }
    }
}
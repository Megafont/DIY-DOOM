using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DIY_DOOM.Maps;
using DIY_DOOM.WADs.Data.Maps;


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



    /// <summary>
    /// This class holds data used during the mesh generation process.
    /// </summary>
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


                Debug.Log($"FLAGS: {_CurLineDef.Flags}");
                if (!_CurLineDef.Flags.HasFlag(LineDefFlags.TwoSided))
                {
                    GenerateFrontFace();
                }
                else
                {
                    Debug.Log($"Skipped two-sided lineDef[{i}]");

                    //Debug.Log($"Skipped left sideDef[{i}]");
                }


                //if (i >= 10) break;
            }


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

        private static void GenerateFrontFace()
        {
            float floorHeight = _CurRightSectorDef.FloorHeight;
            float ceilingHeight = _CurRightSectorDef.CeilingHeight;

            
            //_CurLeftSideDef.DEBUG_Print();
            if (_CurLineDef.RightSideDefIndex == 28)
                _CurRightSideDef.DEBUG_Print();

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

            //Debug.Log($"{_LineDefStart}    {_LineDefEnd}    {Vector3.Distance(_LineDefStart, _LineDefEnd)}");

            GenerateUVsForSingleSidedLineDef(_CurLineDef.Flags,
                                             Vector3.Distance(_LineDefStart, _LineDefEnd),
                                             ceilingHeight - floorHeight);



            /*
            _MapVertices.Add(MapUtils.Point2dTo3dXZ(_LineDefStart.x, _LineDefStart.y, floorHeight));
            _MapVertices.Add(MapUtils.Point2dTo3dXZ(_LineDefStart.x, _LineDefStart.y, ceilingHeight));
            _MapVertices.Add(MapUtils.Point2dTo3dXZ(_LineDefEnd.x, _LineDefEnd.y, ceilingHeight));

            _MapTris.AddRange(new int[] { firstVertIndex, firstVertIndex + 1, firstVertIndex + 2 });

            _MapVertices.Add(MapUtils.Point2dTo3dXZ(_LineDefStart.x, _LineDefStart.y, floorHeight));
            _MapVertices.Add(MapUtils.Point2dTo3dXZ(_LineDefEnd.x, _LineDefEnd.y, ceilingHeight));
            _MapVertices.Add(MapUtils.Point2dTo3dXZ(_LineDefEnd.x, _LineDefEnd.y, floorHeight));

            _MapTris.AddRange(new int[] { firstVertIndex + 3, firstVertIndex + 4, firstVertIndex + 5 });

            _CurVertexIndex += 6;
            */
        }

        private static void GenerateUVsForSingleSidedLineDef(LineDefFlags faceFlags, float faceWidth, float faceHeight)
        {
            float top;
            float bottom;
            float left;
            float right;


            // We multiply faceWidth by the scaleFactor to convert back to DOOM units. This is because a 1m section of wall is 64 pixels wide.
            // Then we divide by the texture width to find out how many texture repeats will fit across the length of the wall.
            float textureRepeatsX = (faceWidth * _Map.ScaleFactor) / _CurMeshData.Material.mainTexture.width;
            float textureRepeatsY = (faceHeight * _Map.ScaleFactor) / _CurMeshData.Material.mainTexture.height;

            float xOffset = (float) _CurRightSideDef.X_Offset / _CurMeshData.Material.mainTexture.width;
            float yOffset = (float) _CurRightSideDef.Y_Offset / _CurMeshData.Material.mainTexture.height;

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

            Debug.Log($"yOffset: {yOffset}    {_CurRightSideDef.Y_Offset}    {_CurMeshData.Material.mainTexture.height}");
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
            Debug.Log($"NAME2: \"{textureName}\"");

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
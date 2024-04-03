using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DIY_DOOM.Maps;
using DIY_DOOM.WADs.Data.Maps;
using Unity.VisualScripting;
using UnityEngine.UIElements;


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

        public static Map _Map;

        public static Mesh _MapMesh;
    
        public static Dictionary<string, MeshData> _SubMeshLookup;



        private static void InitMeshGeneration(Map map)
        {
            _Map = map;

            _MapMesh = new Mesh();

            // Initialize the mesh data.
            _SubMeshLookup = new Dictionary<string, MeshData>();

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
                    GenerateFrontFace();
                    break;
                }
                else
                {
                    Debug.Log($"Skipped two-sided lineDef[{i}]");

                    //Debug.Log($"Skipped left sideDef[{i}]");
                }


                //break;
            }


            return CreateOutputObject();            
        }

        private static void GetLineDefInfo()
        {
            _LineDefStart = _Map.GetVertex(_CurLineDef.StartVertexID);
            _LineDefEnd = _Map.GetVertex(_CurLineDef.EndVertexID);

            if (_CurLineDef.LeftSideDef >= 0)
                _CurLeftSideDef = _Map.GetSideDef((uint) _CurLineDef.LeftSideDef);
            if (_CurLineDef.RightSideDef >= 0)
                _CurRightSideDef = _Map.GetSideDef((uint) _CurLineDef.RightSideDef);

            _CurLeftSectorDef = _Map.GetSectorDef((uint) _CurLeftSideDef.SectorIndex);
            _CurRightSectorDef = _Map.GetSectorDef((uint) _CurRightSideDef.SectorIndex);

            
        }

        private static void GenerateFrontFace()
        {
            int firstVertIndex = _CurVertexIndex;

            float floorHeight = _CurRightSectorDef.FloorHeight;
            float ceilingHeight = _CurRightSectorDef.CeilingHeight;

            _CurRightSideDef.DEBUG_Print();

            MeshData meshData = GetMeshData(_CurRightSideDef.MiddleTextureName);

            // NOTE: We do NOT scale these vertices. Remember that the vertex gets scaled after being passed into Map.AddVertex().
            //       The floor and ceiling heights are scaled when the SectorDef was passed into Map.AddSectorDef().


            meshData.Vertices.Add(new Vector3(_LineDefStart.x, floorHeight, _LineDefStart.z));
            meshData.Vertices.Add(new Vector3(_LineDefStart.x, ceilingHeight, _LineDefStart.z));
            meshData.Vertices.Add(new Vector3(_LineDefEnd.x, ceilingHeight, _LineDefEnd.z));

            meshData.Triangles.AddRange(new int[] { firstVertIndex, firstVertIndex + 1, firstVertIndex + 2 });

            meshData.Vertices.Add(new Vector3(_LineDefStart.x, floorHeight, _LineDefStart.z));
            meshData.Vertices.Add(new Vector3(_LineDefEnd.x, ceilingHeight, _LineDefEnd.z));
            meshData.Vertices.Add(new Vector3(_LineDefEnd.x, floorHeight, _LineDefEnd.z));

            meshData.Triangles.AddRange(new int[] { firstVertIndex + 3, firstVertIndex + 4, firstVertIndex + 5 });

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

        private static Material GetMaterial(string textureName)
        {
            Material newMaterial;


            Texture2D texture = AssetManager.Instance.GetTexture(textureName, 0);
            if (texture != null)
            {
                newMaterial = new Material(_DOOM_MaterialPrefab);
                newMaterial.mainTexture = texture;
            }
            else
            {
                newMaterial = new Material(_DOOM_MissingTextureMaterial);
                Debug.LogError($"Could not find texture \"{textureName}\" in the AssetManager. This face will be rendered with the missing texture material.");
            }


            return newMaterial;
        }

        private static MeshData GetMeshData(string textureName)
        {
            if (_SubMeshLookup.TryGetValue(textureName, out MeshData meshData))
                return meshData;


            MeshData newMeshData;
            
            newMeshData = new MeshData(textureName, GetMaterial(textureName));
            _SubMeshLookup.Add(textureName, newMeshData);

            return newMeshData;
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
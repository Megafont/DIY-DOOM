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
    

    public struct FaceUvBounds
    {
        public float Bottom;
        public float Left;
        public float Top;
        public float Right;



        public static FaceUvBounds Default
        {
            get
            {
                return new FaceUvBounds() { Bottom = 0, Left = 0, Top = 1, Right = 1 };
            }
        }

        public void ApplyTextureOffset(Vector2 textureOffset)
        {
            Left += textureOffset.x;
            Right += textureOffset.x;

            Top -= textureOffset.y;
            Bottom -= textureOffset.y;
        }

        public override string ToString()
        {
            return $"({Left},{Bottom})-({Right}, {Top})";
        }
    }

    public struct FaceFrontAndBackData
    {
        public SideDef FrontSideDef;
        public SideDef BackSideDef;

        public SectorDef FrontSectorDef;
        public SectorDef BackSectorDef;
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

        public static FaceFrontAndBackData _CurFaceFrontAndBackData;

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


            // Generate the geometry for the walls (all vertical surfaces)
            for (uint i = 0; i < map.LineDefsCount; i++)
            {
                _CurLineDef = map.GetLineDef(i);
                GetLineDefInfo();


                if (DoomEngine.Settings.EnableWallGeneration_MiddleTextures && !_CurLineDef.Flags.HasFlag(LineDefFlags.TwoSided))
                {
                    GenerateLineDefGeometry_SingleSided(true);
                }
                else // This is a two-sided lineDef.
                {       
                    // Draw the front face(s).
                    if (DoomEngine.Settings.EnableWallGeneration_LowerTextures && TextureUtils.IsNameValid(_CurRightSideDef.LowerTextureName))
                        GenerateLineDefGeometry_DoubleSided_LowerTexture(true);
                    if (DoomEngine.Settings.EnableWallGeneration_UpperTextures && TextureUtils.IsNameValid(_CurRightSideDef.UpperTextureName))
                        GenerateLineDefGeometry_DoubleSided_UpperTexture(true);
                    if (DoomEngine.Settings.EnableWallGeneration_MiddleTextures && TextureUtils.IsNameValid(_CurRightSideDef.MiddleTextureName))
                        GenerateLineDefGeometry_DoubleSided_MiddleTexture(true);

                    // Draw the back face(s).
                    if (DoomEngine.Settings.EnableWallGeneration_LowerTextures && TextureUtils.IsNameValid(_CurLeftSideDef.LowerTextureName))
                        GenerateLineDefGeometry_DoubleSided_LowerTexture(false);
                    if (DoomEngine.Settings.EnableWallGeneration_UpperTextures && TextureUtils.IsNameValid(_CurLeftSideDef.UpperTextureName))
                        GenerateLineDefGeometry_DoubleSided_UpperTexture(false);
                    if (DoomEngine.Settings.EnableWallGeneration_MiddleTextures && TextureUtils.IsNameValid(_CurLeftSideDef.MiddleTextureName))
                        GenerateLineDefGeometry_DoubleSided_MiddleTexture(false);
                }

            } // end for i


            // Generate geometry for the floors
            //GenerateFloorsGeometry();

            return CreateOutputObject();            
        }

        private static void GenerateFloorsGeometry()
        {
            List<Vector3> vertices = new List<Vector3>();
            List<uint> vertexIndices = new List<uint>();
            List<string> textureNames = new List<string>();


            // Generate the geometry for the floors
            for (int i = 0; i < _Map.SubSectorsCount; i++)
            {
                SubSectorDef subSectorDef = _Map.GetSubSectorDef((uint) i);

                for (int j = 0; j < subSectorDef.SegCount; j++)
                {
                    SegDef segDef = _Map.GetSegDef((uint)j);
                    Vector3 vertex = _Map.GetVertex(segDef.StartVertexID);


                    vertexIndices.Add(segDef.StartVertexID);
                    vertices.Add(vertex);
                    LineDef lineDef = _Map.GetLineDef(segDef.LineDefID);
                    SideDef sideDef = _Map.GetSideDef((uint) lineDef.RightSideDefIndex);
                    SectorDef sectorDef = _Map.GetSectorDef((uint) sideDef.SectorIndex);
                    textureNames.Add(sectorDef.FloorTextureName);

                } // end for j


            } // end for i

            Debug.Log("Vertices: " + vertices.Count);


            int c = 0;

            // Triangulation will form (n - 2) triangles, so 3 * (n - 2) vertex indicies are needed.
            for (int j = 0, k = 1; j < vertices.Count; j += 3, k++)
            {
                if (vertices.Count - j < 3)
                    break;

                if (!TextureUtils.IsNameValid(textureNames[j]))
                {
                    Debug.LogError($"Texture name \"{textureNames[j]}\" is not valid!");
                    break;
                }

                Debug.Log("TEXTURE: " + textureNames[j]);
                GetMeshData(textureNames[j]);

                _CurMeshData.Triangles.Add((int) vertexIndices[0]);
                _CurMeshData.Triangles.Add((int) vertexIndices[k]);
                _CurMeshData.Triangles.Add((int) vertexIndices[k + 1]);

                _CurMeshData.UVs.Add(vertices[(int) vertexIndices[0]]);
                _CurMeshData.UVs.Add(vertices[(int) vertexIndices[k]]);
                _CurMeshData.UVs.Add(vertices[(int) vertexIndices[k + 1]]);

                c++;
                if (c >= 5)
                    return;
            }

        }

        private static void GetLineDefInfo()
        {
            _LineDefStart = _Map.GetVertex(_CurLineDef.StartVertexID);
            _LineDefEnd = _Map.GetVertex(_CurLineDef.EndVertexID);


            if (_CurLineDef.LeftSideDefIndex >= 0)
            {
                _CurLeftSideDef = _Map.GetSideDef((uint)_CurLineDef.LeftSideDefIndex);
                _CurLeftSectorDef = _Map.GetSectorDef((uint)_CurLeftSideDef.SectorIndex);
            }
            else
            {
                _CurLeftSideDef = null;
                _CurLeftSectorDef = null;
            }
            
            if (_CurLineDef.RightSideDefIndex >= 0)
            {
                _CurRightSideDef = _Map.GetSideDef((uint)_CurLineDef.RightSideDefIndex);
                _CurRightSectorDef = _Map.GetSectorDef((uint)_CurRightSideDef.SectorIndex);
            }
            else
            {
                _CurRightSideDef = null;
                _CurRightSectorDef = null;
            }
        }

        private static void GenerateLineDefGeometry_SingleSided(bool isFrontFace)
        {
            _CurFaceFrontAndBackData = GetFaceFrontAndBackData(isFrontFace);

            float floorHeight = _CurFaceFrontAndBackData.FrontSectorDef.FloorHeight;
            float ceilingHeight = _CurFaceFrontAndBackData.FrontSectorDef.CeilingHeight;


            // Get the appropriate MeshData object, and store it in _CurMeshData.
            GetMeshData(_CurFaceFrontAndBackData.FrontSideDef.MiddleTextureName);

            Vector2 faceSize = CalculateFaceSize(floorHeight, ceilingHeight);
            FaceUvBounds uvBounds = CalculateUvBoundsFor_SingleSidedLineDef_LowerTexture(_CurLineDef.Flags, faceSize);
            

            if (isFrontFace)
            {
                GenerateVerticesForFrontFace(floorHeight, ceilingHeight);
                GenerateUVsForFrontFace(uvBounds);
            }
            else
            {
                GenerateVerticesForBackFace(floorHeight, ceilingHeight);
                GenerateUVsForBackFace(uvBounds);
            }
        }

        private static void GenerateLineDefGeometry_DoubleSided_LowerTexture(bool isFrontFace)
        {
            _CurFaceFrontAndBackData = GetFaceFrontAndBackData(isFrontFace);

            float highestFloor = Mathf.Max(_CurFaceFrontAndBackData.BackSectorDef.FloorHeight, _CurFaceFrontAndBackData.FrontSectorDef.FloorHeight);
            float lowestFloor = Mathf.Min(_CurFaceFrontAndBackData.BackSectorDef.FloorHeight, _CurFaceFrontAndBackData.FrontSectorDef.FloorHeight);


            // Get the appropriate MeshData object, and store it in _CurMeshData.
            GetMeshData(_CurFaceFrontAndBackData.FrontSideDef.LowerTextureName);

            Vector2 faceSize = CalculateFaceSize(lowestFloor, highestFloor);
            FaceUvBounds uvBounds = CalculateUvBoundsFor_DoubleSidedLineDef_LowerTexture(_CurLineDef.Flags, faceSize);


            if (isFrontFace)
            {
                GenerateVerticesForFrontFace(lowestFloor, highestFloor);
                GenerateUVsForFrontFace(uvBounds);
            }
            else
            {
                GenerateVerticesForBackFace(lowestFloor, highestFloor);
                GenerateUVsForBackFace(uvBounds);
            }
        }

        private static void GenerateLineDefGeometry_DoubleSided_UpperTexture(bool isFrontFace)
        {
            _CurFaceFrontAndBackData = GetFaceFrontAndBackData(isFrontFace);

            float highestCeiling = Mathf.Max(_CurFaceFrontAndBackData.BackSectorDef.CeilingHeight, _CurFaceFrontAndBackData.FrontSectorDef.CeilingHeight);
            float lowestCeiling = Mathf.Min(_CurFaceFrontAndBackData.BackSectorDef.CeilingHeight, _CurFaceFrontAndBackData.FrontSectorDef.CeilingHeight);


            // Get the appropriate MeshData object, and store it in _CurMeshData.
            GetMeshData(_CurFaceFrontAndBackData.FrontSideDef.UpperTextureName);

            Vector2 faceSize = CalculateFaceSize(lowestCeiling, highestCeiling);
            FaceUvBounds uvBounds = CalculateUvBoundsFor_DoubleSidedLineDef_UpperTexture(_CurLineDef.Flags, faceSize);


            if (isFrontFace)
            {
                GenerateVerticesForFrontFace(lowestCeiling, highestCeiling);
                GenerateUVsForFrontFace(uvBounds);
            }
            else
            {
                GenerateVerticesForBackFace(lowestCeiling, highestCeiling);
                GenerateUVsForBackFace(uvBounds);
            }   
        }

        private static void GenerateLineDefGeometry_DoubleSided_MiddleTexture(bool isFrontFace)
        {
            _CurFaceFrontAndBackData = GetFaceFrontAndBackData(isFrontFace);

            float highestFloor = Mathf.Max(_CurFaceFrontAndBackData.BackSectorDef.FloorHeight, _CurFaceFrontAndBackData.FrontSectorDef.FloorHeight);
            float lowestCeiling = Mathf.Min(_CurFaceFrontAndBackData.BackSectorDef.CeilingHeight, _CurFaceFrontAndBackData.FrontSectorDef.CeilingHeight);
            float highestCeiling = Mathf.Max(_CurFaceFrontAndBackData.BackSectorDef.CeilingHeight, _CurFaceFrontAndBackData.FrontSectorDef.CeilingHeight);

            // Get the appropriate MeshData object, and store it in _CurMeshData.
            GetMeshData(_CurFaceFrontAndBackData.FrontSideDef.MiddleTextureName);

            Vector2 faceSize = CalculateFaceSize(highestFloor, lowestCeiling);
            FaceUvBounds uvBounds = CalculateUvBoundsFor_DoubleSidedLineDef_MiddleTexture(_CurLineDef.Flags, faceSize, highestCeiling - lowestCeiling);
          

            if (isFrontFace)
            {
                GenerateVerticesForFrontFace(highestFloor, lowestCeiling);
                GenerateUVsForFrontFace(uvBounds);
            }
            else
            {
                GenerateVerticesForBackFace(highestFloor, lowestCeiling);
                GenerateUVsForBackFace(uvBounds);
            }
        }

        private static FaceUvBounds CalculateUvBoundsFor_SingleSidedLineDef_LowerTexture(LineDefFlags faceFlags, Vector2 faceSize)
        {
            Vector2 textureRepeats = CalculateTextureRepeats(faceSize);
            Vector2 textureOffset = CalculateTextureOffset();

            FaceUvBounds uvBounds = FaceUvBounds.Default;


            if (!faceFlags.HasFlag(LineDefFlags.LowerTextureIsUnpegged))
            {
                // The top of the texture is snapped to the ceiling.
                uvBounds.Left = 0;
                uvBounds.Right = textureRepeats.x;
                uvBounds.Top = 1;
                uvBounds.Bottom = uvBounds.Top - textureRepeats.y;
            }
            else
            {
                // The bottom of the texture is snapped to the floor.
                uvBounds.Left = 0;
                uvBounds.Right = textureRepeats.x;
                uvBounds.Top = textureRepeats.y;
                uvBounds.Bottom = 0;
            }


            uvBounds.ApplyTextureOffset(textureOffset);

            return uvBounds;
        }

        private static FaceUvBounds CalculateUvBoundsFor_DoubleSidedLineDef_LowerTexture(LineDefFlags faceFlags, Vector2 faceSize)
        {
            Vector2 textureRepeats = CalculateTextureRepeats(faceSize);
            Vector2 textureOffset = CalculateTextureOffset();

            FaceUvBounds uvBounds = FaceUvBounds.Default;


            if (!faceFlags.HasFlag(LineDefFlags.LowerTextureIsUnpegged))
            {
                // The bottom of the texture is snapped to the lower floor.
                uvBounds.Left = 0;
                uvBounds.Right = textureRepeats.x;
                uvBounds.Top = 1;
                uvBounds.Bottom = uvBounds.Top - textureRepeats.y;
            }
            else // Lower Texture Is Unpegged))
            {
                // This is using the left sector's floor height on purpose, as we need to compare the floor height on the back side of this wall to the ceiling height of the sector directly in front of this wall.
                float heightFromFloorToCeiling = (_CurRightSectorDef.CeilingHeight - _CurLeftSectorDef.FloorHeight) * _Map.ScaleFactor;
                float textureRepeatsFromCeilingToFloor = heightFromFloorToCeiling / _CurMeshData.Material.mainTexture.height;

                // The top of the texture is snapped to the ceiling.
                uvBounds.Left = 0;
                uvBounds.Right = textureRepeats.x;
                uvBounds.Top = 1 - textureRepeatsFromCeilingToFloor;
                uvBounds.Bottom = uvBounds.Top - textureRepeats.y;
            }


            uvBounds.ApplyTextureOffset(textureOffset);

            return uvBounds;
        }

        private static FaceUvBounds CalculateUvBoundsFor_DoubleSidedLineDef_UpperTexture(LineDefFlags faceFlags, Vector2 faceSize)
        {
            Vector2 textureRepeats = CalculateTextureRepeats(faceSize);
            Vector2 textureOffset = CalculateTextureOffset();

            FaceUvBounds uvBounds = FaceUvBounds.Default;


            if (!faceFlags.HasFlag(LineDefFlags.UpperTextureIsUnpegged))
            {
                // The bottom of the texture is aligned to the lowest ceiling
                uvBounds.Left = 0;
                uvBounds.Right = textureRepeats.x;
                uvBounds.Top = textureRepeats.y;
                uvBounds.Bottom = 0;
            }
            else // Upper Texture Is Unpegged
            {
                // The top of the texture is aligned to the highest ceiling.
                uvBounds.Left = 0;
                uvBounds.Right = textureRepeats.x;
                uvBounds.Top = 1;
                uvBounds.Bottom = uvBounds.Top - textureRepeats.y;
            }


            uvBounds.ApplyTextureOffset(textureOffset);

            return uvBounds;
        }

        private static FaceUvBounds CalculateUvBoundsFor_DoubleSidedLineDef_MiddleTexture(LineDefFlags faceFlags, Vector2 faceSize, float ceilingHeightsDifference)
        {
            Vector2 textureRepeats = CalculateTextureRepeats(faceSize);
            Vector2 textureOffset = CalculateTextureOffset();

            FaceUvBounds uvBounds = FaceUvBounds.Default;

            // ******************************************************************************************
            // CHANGE THIS CODE SO THE TOP OF THE TEXTURE IS SNAPPED TO THE HIGHEST CEILING
            // ******************************************************************************************


            if (!faceFlags.HasFlag(LineDefFlags.LowerTextureIsUnpegged))
            {
                // The top of the texture is snapped to the ceiling.
                uvBounds.Left = 0;
                uvBounds.Right = textureRepeats.x;
                uvBounds.Top = 1;
                uvBounds.Bottom = uvBounds.Top - textureRepeats.y;
            }
            else
            {
                // The bottom of the texture is snapped to the floor.
                uvBounds.Left = 0;
                uvBounds.Right = textureRepeats.x;
                uvBounds.Top = textureRepeats.y;
                uvBounds.Bottom = 0;
            }

            /*
                // The top of the texture is snapped to the lowest ceiling.
                uvBounds.Left = 0;
                uvBounds.Right = textureRepeats.x;
                uvBounds.Top = 1;
                uvBounds.Bottom = 0;
            */


            uvBounds.ApplyTextureOffset(textureOffset);

            return uvBounds;
        }

        private static void GenerateUVsForSingleSidedLineDef(LineDefFlags faceFlags, Vector2 faceSize)
        {
            Vector2 textureRepeats = CalculateTextureRepeats(faceSize);
            Vector2 textureOffset = CalculateTextureOffset();

            FaceUvBounds uvBounds = FaceUvBounds.Default;


            if (!faceFlags.HasFlag(LineDefFlags.LowerTextureIsUnpegged))
            {
                // The top of the texture is snapped to the ceiling.
                uvBounds.Left = 0;
                uvBounds.Right = textureRepeats.x;
                uvBounds.Top = 1;
                uvBounds.Bottom = uvBounds.Top - textureRepeats.y;
            }
            else
            {
                // The bottom of the texture is snapped to the floor.
                uvBounds.Left = 0;
                uvBounds.Right = textureRepeats.x;
                uvBounds.Top = textureRepeats.y;
                uvBounds.Bottom = 0;
            }


            uvBounds.ApplyTextureOffset(textureOffset);

            GenerateUVsForFrontFace(uvBounds);
        }

        private static void GenerateUVsForDoubleSidedLineDef_LowerTexture(LineDefFlags faceFlags, Vector2 faceSize)
        {
            Vector2 textureRepeats = CalculateTextureRepeats(faceSize);
            Vector2 textureOffset = CalculateTextureOffset();

            FaceUvBounds uvBounds = FaceUvBounds.Default;


            if (!faceFlags.HasFlag(LineDefFlags.LowerTextureIsUnpegged))
            {
                // The bottom of the texture is snapped to the lower floor.
                uvBounds.Left = 0;
                uvBounds.Right = textureRepeats.x;
                uvBounds.Top = 1;
                uvBounds.Bottom = uvBounds.Top - textureRepeats.y;
            }
            else if (faceFlags.HasFlag(LineDefFlags.LowerTextureIsUnpegged))
            {
                // This is using the left sector's floor height on purpose, as we need to compare the floor height on the back side of this wall to the ceiling height of the sector directly in front of this wall.
                float heightFromFloorToCeiling = (_CurRightSectorDef.CeilingHeight - _CurLeftSectorDef.FloorHeight) * _Map.ScaleFactor;
                float textureRepeatsFromCeilingToFloor = heightFromFloorToCeiling / _CurMeshData.Material.mainTexture.height;

                // The top of the texture is snapped to the ceiling.
                uvBounds.Left = 0;
                uvBounds.Right = textureRepeats.x;
                uvBounds.Top = 1 - textureRepeatsFromCeilingToFloor;
                uvBounds.Bottom = uvBounds.Top - textureRepeats.y;
            }

            uvBounds.ApplyTextureOffset(textureOffset);

            GenerateUVsForFrontFace(uvBounds);
        }

        private static void GenerateUVsForDoubleSidedLineDef_UpperTexture(LineDefFlags faceFlags, Vector2 faceSize)
        {
            Vector2 textureRepeats = CalculateTextureRepeats(faceSize);
            Vector2 textureOffset = CalculateTextureOffset();

            FaceUvBounds uvBounds = FaceUvBounds.Default;


            if (!faceFlags.HasFlag(LineDefFlags.UpperTextureIsUnpegged))
            {
                // The bottom of the texture is aligned to the lowest ceiling
                uvBounds.Left = 0;
                uvBounds.Right = textureRepeats.x;
                uvBounds.Top = textureRepeats.y;
                uvBounds.Bottom = 0;
            }
            else if (faceFlags.HasFlag(LineDefFlags.UpperTextureIsUnpegged))
            {
                // The top of the texture is aligned to the highest ceiling.
                uvBounds.Left = 0;
                uvBounds.Right = textureRepeats.x;
                uvBounds.Top = 1;
                uvBounds.Bottom = uvBounds.Top - textureRepeats.y;
            }


            uvBounds.ApplyTextureOffset(textureOffset);

            GenerateUVsForFrontFace(uvBounds);
        }

        private static void GenerateUVsForDoubleSidedLineDef_MiddleTexture(LineDefFlags faceFlags, Vector2 faceSize)
        {
            Vector2 textureRepeats = CalculateTextureRepeats(faceSize);
            Vector2 textureOffset = CalculateTextureOffset();

            FaceUvBounds uvBounds = FaceUvBounds.Default;


            // The top of the texture is snapped to the lowest ceiling.
            uvBounds.Left = 0;
            uvBounds.Right = textureRepeats.x;
            uvBounds.Top = 1;
            uvBounds.Bottom = 0;


            uvBounds.ApplyTextureOffset(textureOffset);

            GenerateUVsForFrontFace(uvBounds);
        }

        private static void GenerateVerticesForFrontFace(float bottomHeight, float topHeight)
        {
            int firstVertIndex = _CurMeshData.Vertices.Count;

            // NOTE: We do NOT scale these vertices. Remember that the vertex gets scaled after being passed into Map.AddVertex().
            //       The floor and ceiling heights are scaled when the SectorDef was passed into Map.AddSectorDef().

            // Generate vertices
            _CurMeshData.Vertices.Add(new Vector3(_LineDefStart.x, bottomHeight, _LineDefStart.z));
            _CurMeshData.Vertices.Add(new Vector3(_LineDefStart.x, topHeight, _LineDefStart.z));
            _CurMeshData.Vertices.Add(new Vector3(_LineDefEnd.x, topHeight, _LineDefEnd.z));

            _CurMeshData.Vertices.Add(new Vector3(_LineDefStart.x, bottomHeight, _LineDefStart.z));
            _CurMeshData.Vertices.Add(new Vector3(_LineDefEnd.x, topHeight, _LineDefEnd.z));
            _CurMeshData.Vertices.Add(new Vector3(_LineDefEnd.x, bottomHeight, _LineDefEnd.z));

            // Generate triangles
            _CurMeshData.Triangles.AddRange(new int[] { firstVertIndex, firstVertIndex + 1, firstVertIndex + 2 });
            _CurMeshData.Triangles.AddRange(new int[] { firstVertIndex + 3, firstVertIndex + 4, firstVertIndex + 5 });
        }

        private static void GenerateVerticesForBackFace(float bottomHeight, float topHeight)
        {
            int firstVertIndex = _CurMeshData.Vertices.Count;

            // NOTE: We do NOT scale these vertices. Remember that the vertex gets scaled after being passed into Map.AddVertex().
            //       The floor and ceiling heights are scaled when the SectorDef was passed into Map.AddSectorDef().

            // Generate vertices
            _CurMeshData.Vertices.Add(new Vector3(_LineDefStart.x, bottomHeight, _LineDefStart.z));
            _CurMeshData.Vertices.Add(new Vector3(_LineDefEnd.x, topHeight, _LineDefEnd.z));
            _CurMeshData.Vertices.Add(new Vector3(_LineDefStart.x, topHeight, _LineDefStart.z));

            _CurMeshData.Vertices.Add(new Vector3(_LineDefStart.x, bottomHeight, _LineDefStart.z));
            _CurMeshData.Vertices.Add(new Vector3(_LineDefEnd.x, bottomHeight, _LineDefEnd.z));
            _CurMeshData.Vertices.Add(new Vector3(_LineDefEnd.x, topHeight, _LineDefEnd.z));

            // Generate triangles
            _CurMeshData.Triangles.AddRange(new int[] { firstVertIndex, firstVertIndex + 1, firstVertIndex + 2 });
            _CurMeshData.Triangles.AddRange(new int[] { firstVertIndex + 3, firstVertIndex + 4, firstVertIndex + 5 });
        }

        private static void GenerateUVsForFrontFace(FaceUvBounds uvBounds)
        {
            _CurMeshData.UVs.Add(new Vector2(uvBounds.Left, uvBounds.Bottom));
            _CurMeshData.UVs.Add(new Vector2(uvBounds.Left, uvBounds.Top));
            _CurMeshData.UVs.Add(new Vector2(uvBounds.Right, uvBounds.Top));

            _CurMeshData.UVs.Add(new Vector2(uvBounds.Left, uvBounds.Bottom));
            _CurMeshData.UVs.Add(new Vector2(uvBounds.Right, uvBounds.Top));
            _CurMeshData.UVs.Add(new Vector2(uvBounds.Right, uvBounds.Bottom));
        }

        private static void GenerateUVsForBackFace(FaceUvBounds uvBounds)
        {
            _CurMeshData.UVs.Add(new Vector2(uvBounds.Right, uvBounds.Bottom));
            _CurMeshData.UVs.Add(new Vector2(uvBounds.Left, uvBounds.Top));
            _CurMeshData.UVs.Add(new Vector2(uvBounds.Right, uvBounds.Top));

            _CurMeshData.UVs.Add(new Vector2(uvBounds.Right, uvBounds.Bottom));
            _CurMeshData.UVs.Add(new Vector2(uvBounds.Left, uvBounds.Bottom));
            _CurMeshData.UVs.Add(new Vector2(uvBounds.Left, uvBounds.Top));
        }

        private static Vector2 CalculateFaceSize(float bottomHeight, float topHeight)
        {
            return new Vector2(Vector3.Distance(_LineDefStart, _LineDefEnd),
                               topHeight - bottomHeight);
        }

        private static Vector2 CalculateTextureOffset()
        {
            return new Vector2((float) _CurFaceFrontAndBackData.FrontSideDef.X_Offset / _CurMeshData.Material.mainTexture.width,
                               (float) _CurFaceFrontAndBackData.FrontSideDef.Y_Offset / _CurMeshData.Material.mainTexture.height);
        }

        private static Vector2 CalculateTextureRepeats(Vector2 faceSize)
        {
            // We multiply faceWidth by the scaleFactor to convert back to DOOM units. This is because a 1m section of wall is 64 pixels wide.
            // Then we divide by the texture width to find out how many texture repeats will fit across the length of the wall.
            // I had to add the call to Mathf.Ceil() to fix a bug where sometimes there was a half-pixel on the right edge of a wall section.
            return new Vector2(Mathf.Ceil(faceSize.x * _Map.ScaleFactor) / _CurMeshData.Material.mainTexture.width,
                               Mathf.Ceil(faceSize.y * _Map.ScaleFactor) / _CurMeshData.Material.mainTexture.height);
        }

        /// <summary>
        /// Gets the front and back data for the current face.
        /// </summary>
        /// <param name="isFrontFace">Whether or not to get that data for the front or back side.</param>
        /// <returns>The front and back data for the current face</returns>
        private static FaceFrontAndBackData GetFaceFrontAndBackData(bool isFrontFace)
        {
            if (isFrontFace)
            {
                return new FaceFrontAndBackData()
                {
                    FrontSideDef = _CurRightSideDef,
                    BackSideDef = _CurLeftSideDef,

                    FrontSectorDef = _CurRightSectorDef,
                    BackSectorDef = _CurLeftSectorDef,
                };
            }
            else
            {
                return new FaceFrontAndBackData()
                {
                    FrontSideDef = _CurLeftSideDef,
                    BackSideDef = _CurRightSideDef,

                    FrontSectorDef = _CurLeftSectorDef,
                    BackSectorDef = _CurRightSectorDef,
                };

            }
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
        /// If not, then a new MeshData object is created for the specified texture name,
        /// and stored in _CurMeshData.
        /// </summary>
        /// <param name="textureName"></param>
        /// <returns>True if the texture name already has a MeshData object associated with it, false if not.</returns>
        private static bool GetMeshData(string textureName)
        {
            if (_SubMeshLookup.TryGetValue(textureName, out _CurMeshData))
                return true;

            Debug.Log("ZZZ");
           
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
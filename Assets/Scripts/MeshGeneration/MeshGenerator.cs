using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using DIY_DOOM.Maps;
using DIY_DOOM.MeshGeneration.Triangulation;
using DIY_DOOM.WADs.Data.Maps;
using DIY_DOOM.Utils.Maps;
using DIY_DOOM.Utils.Textures;


namespace DIY_DOOM.MeshGeneration
{

    // ********************************************************************************************************************************************************************************************************
    // *  TODO: I could implement lighting by creating different versions of the textures for different light levels similarly to how I did it for palettes already.
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

        public static SideDef _CurLeftSideDef; // Back side
        public static SideDef _CurRightSideDef; // Front side

        public static SectorDef _CurBackSectorDef;
        public static SectorDef _CurFrontSectorDef;

        public static FaceFrontAndBackData _CurFaceFrontAndBackData;

        public static MeshData _CurMeshData;

        public static Map _Map;

        public static Mesh _MapMesh;

        public const float _PixelsPerWorldUnit = 32f;
        public const float _HalfPixelsPerWorldUnit = _PixelsPerWorldUnit / 2f;
        public const float _PixelSizeInWorldUnits = 1f / _PixelsPerWorldUnit;

    
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


            // Generate geometry for the floors/ceilings
            for (int i = 0; i < _Map.SectorsCount; i++)
            {
                SectorDef sectorDef = _Map.GetSectorDef((uint) i);
                
                if (DoomEngine.Settings.EnableFloorGeneration)
                    GenerateSectorFloorGeometry(sectorDef);
                if (DoomEngine.Settings.EnableCeilingGeneration)
                    GenerateSectorCeilingGeometry(sectorDef);
                
            } // end for i
            
            return CreateOutputObject();            
        }

        private static bool GenerateSectorFloorGeometry(SectorDef sectorDef)
        {
            if (sectorDef.SectorOutline.Count < 3)
            {
                Debug.Log($"Sector with ID {sectorDef.ID} has less than 3 floor verticies! Skipping floor geometry generation for it.");
                return false;
            }

            // Get the appropriate MeshData objects to add the floor geometry of this sector to.
            GetMeshData(sectorDef.FloorTextureName, sectorDef.LightLevel, out MeshData floorMeshData);

            // Triangulate it to create the floor geometry.
            if (Triangulator_Polygon.Triangulate(sectorDef.SectorOutline, floorMeshData, sectorDef.FloorHeight))
            {
                Debug.LogWarning($"Triangulation of sector {sectorDef.ID} floor result: {Triangulator_Polygon.LastTriangulationResult}");
            }
            else
            {
                Debug.LogError($"Failed to triangulate the floor geometry of sector[{sectorDef.ID}]!");
            }

            return true;
        }
        
        private static bool GenerateSectorCeilingGeometry(SectorDef sectorDef)
        {
            if (sectorDef.SectorOutline.Count < 3)
            {
                Debug.Log($"Sector with ID {sectorDef.ID} has less than 3 ceiling verticies! Skipping floor geometry generation for it.");
                return false;
            }

            // Get the appropriate MeshData objects to add the ceiling geometry of this sector to.
            GetMeshData(sectorDef.CeilingTextureName, sectorDef.LightLevel, out MeshData ceilingMeshData);

            // Triangulate it to create the ceiling geometry.
            List<Vector2> invertedSectorOutline = new List<Vector2>();
            invertedSectorOutline.AddRange(sectorDef.SectorOutline);
            invertedSectorOutline.Reverse(); // Invert the order of the sector outline vertices. We need to make the ceiling outline go counterclockwise so these polygons face down instead of up.
            if (Triangulator_Polygon.Triangulate(invertedSectorOutline, ceilingMeshData, sectorDef.CeilingHeight, triangulateReversed: true))
            {
                Debug.LogWarning($"Triangulation of sector {sectorDef.ID} ceiling result: {Triangulator_Polygon.LastTriangulationResult}");
            }
            else
            {
                Debug.LogError($"Failed to triangulate the ceiling geometry of sector[{sectorDef.ID}]!");
            }

            return true;
        }
        
        private static void RemoveInvalidVertices(List<uint> vertexIndices)
        {
            List<int> invalidIndices = new List<int>();

            for (int i = 0; i < vertexIndices.Count; i += 3)
            {
                if (vertexIndices.Count - i < 3)
                    break;

                Vector3 v1 = _Map.GetVertex(vertexIndices[i]);
                Vector3 v2 = _Map.GetVertex(vertexIndices[i + 1]);
                Vector3 v3 = _Map.GetVertex(vertexIndices[i + 2]);

                // Check if all three are at the same x position.
                if (v1.x == v2.x && v2.x == v3.x)
                {
                    int max = (int) Mathf.Max(Mathf.Max(v1.z, v2.z), v3.z);                    
                    int min = (int) Mathf.Min(Mathf.Min(v1.z, v2.z), v3.z);

                    Debug.Log($"MIN: {min}    MAX: {max}    V1: {v1}    V2: {v2}    V3: {v3}");

                    if (v1.z > min && v1.z < max)
                    {
                        invalidIndices.Add(i);
                    }
                    else if (v2.z > min && v2.z < max)
                    {
                        invalidIndices.Add(i + 1);
                    }
                    else if (v3.z > min && v3.z < max)
                    {
                        invalidIndices.Add(i + 2);
                    }
                }
            } // end for i

            // Remove the invalid indices in reverse order so we don't mess up the loop iteration as we go
            Debug.Log("INVALID: " + invalidIndices.Count);
            for (int i = invalidIndices.Count - 1; i >= 0; i--)
            {
                vertexIndices.RemoveAt(invalidIndices[i]);
            }
        }

        private static Vector3 GetFlatVertex(uint vertexIndex, float height)
        {
            Vector3 vertex = _Map.GetVertex(vertexIndex);

            vertex.y = height;

            return vertex;
        }

        /// <summary>
        /// Transforms the coordinates of a vertex of a flat (floor / ceiling texture).
        /// This function is used to generate UV coordinates from the floor/ceiling vertices.
        /// </summary>
        /// <param name="v">The vertex to transform.</param>
        /// <returns>The transformed vertex.</returns>
        public static Vector2 TransformFlatPointToUV(MeshData meshData, Vector3 v)
        {
            // Convert to a 2D point with the x and z coords from the 3d point.
            Vector2 v2D = MapUtils.Point3dTo2d(v);
            
            float textureRepeatsPerXUnit = _PixelsPerWorldUnit / meshData.Material.mainTexture.width;
            float textureRepeatsPerZUnit = _PixelsPerWorldUnit / meshData.Material.mainTexture.height;

            v2D.x = v2D.x * textureRepeatsPerXUnit;
            v2D.y = v2D.y * textureRepeatsPerZUnit;
            
            // Translate a bit so the floor/ceiling texture is aligned the same as it is in the original game.
            return v2D + new Vector2(0.75f, 0f);
        }

        private static void GetLineDefInfo()
        {
            _LineDefStart = _Map.GetVertex(_CurLineDef.StartVertexID);
            _LineDefEnd = _Map.GetVertex(_CurLineDef.EndVertexID);


            if (_CurLineDef.BackSideDefIndex >= 0)
            {
                _CurLeftSideDef = _Map.GetSideDef((uint)_CurLineDef.BackSideDefIndex);
                _CurBackSectorDef = _Map.GetSectorDef((uint)_CurLeftSideDef.SectorIndex);
            }
            else
            {
                _CurLeftSideDef = null;
                _CurBackSectorDef = null;
            }
            
            if (_CurLineDef.FrontSideDefIndex >= 0)
            {
                _CurRightSideDef = _Map.GetSideDef((uint)_CurLineDef.FrontSideDefIndex);
                _CurFrontSectorDef = _Map.GetSectorDef((uint)_CurRightSideDef.SectorIndex);
            }
            else
            {
                _CurRightSideDef = null;
                _CurFrontSectorDef = null;
            }
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // The following a post from Reddit on the meaning of the "lower unpegged" and "upper unpegged" texture options.
        // https://www.reddit.com/r/Doom/comments/2dn835/comment/cjrcaad/?utm_source=share&utm_medium=web3x&utm_name=web3xcss&utm_term=1&utm_content=share_button
        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //  
        // The "lower unpegged"/"upper unpegged" used to confuse me, but this is how I started understanding them:
        //
        // Lower unpegged means the texture will align to the floor. So if the floor moves, the texture moves. If the ceiling moves, the texture doesn't move.
        //
        // Upper unpegged is the oppesite: if the ceiling moves, the texture follows it. If the floor moves, it doesn't move.
        //
        // If you use both, the texture will never move
        //
        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        
        private static void GenerateLineDefGeometry_SingleSided(bool isFrontFace)
        {
            _CurFaceFrontAndBackData = GetFaceFrontAndBackData(isFrontFace);

            float floorHeight = _CurFaceFrontAndBackData.FrontSectorDef.FloorHeight;
            float ceilingHeight = _CurFaceFrontAndBackData.FrontSectorDef.CeilingHeight;


            // Get the appropriate MeshData object, and store it in _CurMeshData.
            GetMeshData(_CurFaceFrontAndBackData.FrontSideDef.MiddleTextureName, 
                        isFrontFace ? _CurFaceFrontAndBackData.FrontSectorDef.LightLevel : _CurFaceFrontAndBackData.BackSectorDef.LightLevel, 
                        out _CurMeshData);

            Vector2 faceSize = CalculateWallFaceSize(floorHeight, ceilingHeight);
            FaceUvBounds uvBounds = CalculateUvBoundsFor_SingleSidedLineDef_LowerTexture(_CurLineDef.Flags, faceSize);
            

            if (isFrontFace)
            {
                GenerateVerticesForFrontFace(floorHeight, ceilingHeight);
                GenerateUVsForWallFrontFace(uvBounds);
            }
            else
            {
                GenerateVerticesForBackFace(floorHeight, ceilingHeight);
                GenerateUVsForWallBackFace(uvBounds);
            }
        }

        private static void GenerateLineDefGeometry_DoubleSided_LowerTexture(bool isFrontFace)
        {
            _CurFaceFrontAndBackData = GetFaceFrontAndBackData(isFrontFace);

            float highestFloor = Mathf.Max(_CurFaceFrontAndBackData.BackSectorDef.FloorHeight, _CurFaceFrontAndBackData.FrontSectorDef.FloorHeight);
            float lowestFloor = Mathf.Min(_CurFaceFrontAndBackData.BackSectorDef.FloorHeight, _CurFaceFrontAndBackData.FrontSectorDef.FloorHeight);


            // Get the appropriate MeshData object, and store it in _CurMeshData.
            GetMeshData(_CurFaceFrontAndBackData.FrontSideDef.LowerTextureName, 
                        isFrontFace ? _CurFaceFrontAndBackData.FrontSectorDef.LightLevel : _CurFaceFrontAndBackData.BackSectorDef.LightLevel, 
                        out _CurMeshData);

            Vector2 faceSize = CalculateWallFaceSize(lowestFloor, highestFloor);
            FaceUvBounds uvBounds = CalculateUvBoundsFor_DoubleSidedLineDef_LowerTexture(_CurLineDef.Flags, faceSize);


            if (isFrontFace)
            {
                GenerateVerticesForFrontFace(lowestFloor, highestFloor);
                GenerateUVsForWallFrontFace(uvBounds);
            }
            else
            {
                GenerateVerticesForBackFace(lowestFloor, highestFloor);
                GenerateUVsForWallBackFace(uvBounds);
            }
        }

        private static void GenerateLineDefGeometry_DoubleSided_UpperTexture(bool isFrontFace)
        {
            _CurFaceFrontAndBackData = GetFaceFrontAndBackData(isFrontFace);

            float highestCeiling = Mathf.Max(_CurFaceFrontAndBackData.BackSectorDef.CeilingHeight, _CurFaceFrontAndBackData.FrontSectorDef.CeilingHeight);
            float lowestCeiling = Mathf.Min(_CurFaceFrontAndBackData.BackSectorDef.CeilingHeight, _CurFaceFrontAndBackData.FrontSectorDef.CeilingHeight);


            // Get the appropriate MeshData object, and store it in _CurMeshData.
            GetMeshData(_CurFaceFrontAndBackData.FrontSideDef.UpperTextureName, 
                        isFrontFace ? _CurFaceFrontAndBackData.FrontSectorDef.LightLevel : _CurFaceFrontAndBackData.BackSectorDef.LightLevel, 
                        out _CurMeshData);

            Vector2 faceSize = CalculateWallFaceSize(lowestCeiling, highestCeiling);
            FaceUvBounds uvBounds = CalculateUvBoundsFor_DoubleSidedLineDef_UpperTexture(_CurLineDef.Flags, faceSize);


            if (isFrontFace)
            {
                GenerateVerticesForFrontFace(lowestCeiling, highestCeiling);
                GenerateUVsForWallFrontFace(uvBounds);
            }
            else
            {
                GenerateVerticesForBackFace(lowestCeiling, highestCeiling);
                GenerateUVsForWallBackFace(uvBounds);
            }   
        }

        private static void GenerateLineDefGeometry_DoubleSided_MiddleTexture(bool isFrontFace)
        {
            _CurFaceFrontAndBackData = GetFaceFrontAndBackData(isFrontFace);

            float highestFloor = Mathf.Max(_CurFaceFrontAndBackData.BackSectorDef.FloorHeight, _CurFaceFrontAndBackData.FrontSectorDef.FloorHeight);
            float lowestCeiling = Mathf.Min(_CurFaceFrontAndBackData.BackSectorDef.CeilingHeight, _CurFaceFrontAndBackData.FrontSectorDef.CeilingHeight);
            float highestCeiling = Mathf.Max(_CurFaceFrontAndBackData.BackSectorDef.CeilingHeight, _CurFaceFrontAndBackData.FrontSectorDef.CeilingHeight);

            // Get the appropriate MeshData object, and store it in _CurMeshData.
            GetMeshData(_CurFaceFrontAndBackData.FrontSideDef.MiddleTextureName, 
                        isFrontFace ? _CurFaceFrontAndBackData.FrontSectorDef.LightLevel : _CurFaceFrontAndBackData.BackSectorDef.LightLevel, 
                        out _CurMeshData);

            Vector2 faceSize = CalculateWallFaceSize(highestFloor, lowestCeiling);
            FaceUvBounds uvBounds = CalculateUvBoundsFor_DoubleSidedLineDef_MiddleTexture(_CurLineDef.Flags, faceSize, highestCeiling - lowestCeiling);
          

            if (isFrontFace)
            {
                GenerateVerticesForFrontFace(highestFloor, lowestCeiling);
                GenerateUVsForWallFrontFace(uvBounds);
            }
            else
            {
                GenerateVerticesForBackFace(highestFloor, lowestCeiling);
                GenerateUVsForWallBackFace(uvBounds);
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
                uvBounds.Left = 0f;
                uvBounds.Right = textureRepeats.x;
                uvBounds.Top = 1f;
                uvBounds.Bottom = uvBounds.Top - textureRepeats.y;
            }
            else
            {
                // The bottom of the texture is snapped to the floor.
                uvBounds.Left = 0f;
                uvBounds.Right = textureRepeats.x;
                uvBounds.Top = textureRepeats.y;
                uvBounds.Bottom = 0f;
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
                uvBounds.Left = 0f;
                uvBounds.Right = textureRepeats.x;
                uvBounds.Top = 1f;
                uvBounds.Bottom = uvBounds.Top - textureRepeats.y;
            }
            else // Lower Texture Is Unpegged))
            {
                float lowerFloorHeight = Mathf.Min(_CurBackSectorDef.FloorHeight, _CurFrontSectorDef.FloorHeight);
                float higherFloorHeight = Mathf.Max(_CurBackSectorDef.FloorHeight, _CurFrontSectorDef.FloorHeight);
                float lowerCeilingHeight = Mathf.Min(_CurBackSectorDef.CeilingHeight, _CurFrontSectorDef.CeilingHeight);
                float higherCeilingHeight = Mathf.Max(_CurBackSectorDef.CeilingHeight, _CurFrontSectorDef.CeilingHeight);

                uvBounds.Left = 0f;
                uvBounds.Right = textureRepeats.x;
                uvBounds.Top = 1f - Mathf.Abs(_CurFrontSectorDef.CeilingHeight - higherFloorHeight) * _Map.ScaleFactor / _CurMeshData.Material.mainTexture.height;
                //uvBounds.Top = 1f - Mathf.Abs(higherCeilingHeight - higherFloorHeight) * _Map.ScaleFactor / _CurMeshData.Material.mainTexture.height;
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
                uvBounds.Left = 0f;
                uvBounds.Right = textureRepeats.x;
                uvBounds.Top = textureRepeats.y;
                uvBounds.Bottom = 0f;
            }
            else // Upper Texture Is Unpegged
            {
                // The top of the texture is aligned to the highest ceiling.
                uvBounds.Left = 0f;
                uvBounds.Right = textureRepeats.x;
                uvBounds.Top = 1f;
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
                uvBounds.Left = 0f;
                uvBounds.Right = textureRepeats.x;
                uvBounds.Top = 1f;
                uvBounds.Bottom = uvBounds.Top - textureRepeats.y;
            }
            else
            {
                // The bottom of the texture is snapped to the floor.
                uvBounds.Left = 0f;
                uvBounds.Right = textureRepeats.x;
                uvBounds.Top = textureRepeats.y;
                uvBounds.Bottom = 0f;
            }

            uvBounds.ApplyTextureOffset(textureOffset);

            return uvBounds;
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

        private static void GenerateUVsForWallFrontFace(FaceUvBounds uvBounds)
        {
            _CurMeshData.UVs.Add(new Vector2(uvBounds.Left, uvBounds.Bottom));
            _CurMeshData.UVs.Add(new Vector2(uvBounds.Left, uvBounds.Top));
            _CurMeshData.UVs.Add(new Vector2(uvBounds.Right, uvBounds.Top));

            _CurMeshData.UVs.Add(new Vector2(uvBounds.Left, uvBounds.Bottom));
            _CurMeshData.UVs.Add(new Vector2(uvBounds.Right, uvBounds.Top));
            _CurMeshData.UVs.Add(new Vector2(uvBounds.Right, uvBounds.Bottom));
        }

        private static void GenerateUVsForWallBackFace(FaceUvBounds uvBounds)
        {
            _CurMeshData.UVs.Add(new Vector2(uvBounds.Right, uvBounds.Bottom));
            _CurMeshData.UVs.Add(new Vector2(uvBounds.Left, uvBounds.Top));
            _CurMeshData.UVs.Add(new Vector2(uvBounds.Right, uvBounds.Top));

            _CurMeshData.UVs.Add(new Vector2(uvBounds.Right, uvBounds.Bottom));
            _CurMeshData.UVs.Add(new Vector2(uvBounds.Left, uvBounds.Bottom));
            _CurMeshData.UVs.Add(new Vector2(uvBounds.Left, uvBounds.Top));
        }

        private static Vector2 CalculateWallFaceSize(float bottomHeight, float topHeight)
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

                    FrontSectorDef = _CurFrontSectorDef,
                    BackSectorDef = _CurBackSectorDef,
                };
            }
            else
            {
                return new FaceFrontAndBackData()
                {
                    FrontSideDef = _CurLeftSideDef,
                    BackSideDef = _CurRightSideDef,

                    FrontSectorDef = _CurBackSectorDef,
                    BackSectorDef = _CurFrontSectorDef,
                };

            }
        }

        private static Material CreateMaterial(string materialName, string textureName, byte lightLevel)
        {
            Material newMaterial;

            Texture2D texture = AssetManager.Instance.GetTexture(textureName, 0);
            if (texture != null)
            {
                newMaterial = new Material(_DOOM_MaterialPrefab);
                newMaterial.name = $"({materialName})";
                newMaterial.mainTexture = texture;
                newMaterial.color = new Color32(lightLevel, lightLevel, lightLevel, 255);
            }
            else
            {
                newMaterial = new Material(_DOOM_MissingTextureMaterial);
                newMaterial.name = $"({materialName})";
                newMaterial.color = new Color32(lightLevel, lightLevel, lightLevel, 255);
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
        /// <param name="textureName">The name of the texture to find the mesh for.</param>
        /// <param name="lightLevel">The light level of the sector that is using this texture.</param>
        /// <param name="meshData">This out parameter returns the MeshData object for the specified texture.</param>
        /// <returns>True if the texture name already has a MeshData object associated with it, false if not.</returns>
        private static bool GetMeshData(string textureName, int lightLevel, out MeshData meshData)
        {
            string materialName = textureName;
            if (DoomEngine.Settings.EnableSectorLighting)
            {
                materialName = $"{textureName}_{lightLevel}";
                lightLevel = Mathf.FloorToInt(((float) lightLevel) * DoomEngine.Settings.SectorLightingAdjustment);
            }

            if (_SubMeshLookup.TryGetValue(materialName, out meshData))
                return true;

            
            // Create a new MeshData object for the specified texture name and light level.            
            meshData = new MeshData(textureName, 
                CreateMaterial(materialName, textureName, DoomEngine.Settings.EnableSectorLighting ? (byte) lightLevel : (byte) 255));
            
            _SubMeshLookup.Add(materialName, meshData);

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

                Debug.Log($"Tex: \"{pair.Value.Material.name}\"    Verts: {pair.Value.Vertices.Count}    Tris: {pair.Value.Triangles.Count / 3}    UVs: {pair.Value.UVs.Count}");

                mesh.name = $"Mesh ({pair.Value.Material.name})";
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
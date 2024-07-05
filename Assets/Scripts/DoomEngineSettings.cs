using DIY_DOOM.MeshGeneration;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace DIY_DOOM
{
    [CreateAssetMenu(fileName = "NewDoomEngineSettings", menuName = "DoomEngineSettings Asset")]
    public class DoomEngineSettings : ScriptableObject
    {
        [Header("General")]
        
        [Tooltip("This is a relative path to the WAD to load. It is relative to Application.persistantDataPath.")]
        public string WAD_Path = "WADS/DOOM.wad";

        [Tooltip("The name of the map to load.")]
        public string MapToLoad = "E1M1";


        [Header("Mesh Generation")]

        [Tooltip("This controls the size of the map. Note that the scaling uses division rather than multiplication. The default value is 32, which makes the map 32x smaller than it would be if we pretend one DOOM unit is equal to one Unity unit. This constant comes from DOOM wiki (https://doom.fandom.com/wiki/Map_unit). The guy that made the repo linked in the readme file in the root folder of this project is using a scale factor of 15.")]
        [Range(0.001f, 1000f)]
        public float MapScaleFactor = 32;

        [Tooltip("This is the prefab used to instantiate empty mesh objects.")]
        public LevelSubMesh LevelMeshPrefab;

        [Tooltip("This is the material that is used to create a material instance for each of the textures.")]
        public Material DOOM_MaterialPrefab;

        [Tooltip("This is the material that is used to render faces with missing textures.")]
        public Material DOOM_MissingTextureMaterial;

        [Space(10)]


        [Header("Graphics")]
        
        public bool TextureAlphaIsTransparency = true;
        public FilterMode TextureFilterMode = FilterMode.Point;
        public TextureWrapMode TextureWrapMode = TextureWrapMode.Repeat;
        public Color32 NewTextureFillColor = Color.clear;

        [Space(10)]


        [Header("Runtime Objects (DoomEngine.cs sets these at runtime)")]

        [Tooltip("This is the GameObject that will hold the 3D geometry of the map. It must have a MeshFilter and a MeshRender component on it!")]
        public LevelGeometry LevelGeometryObject;

        [Space(10)]


        [Header("Debug - General")]

        [Tooltip("Whether or not to enable the minimap.")]
        public bool EnableAutoMap = true;

        [Tooltip("Whether or not to enable the texture rendering tester.")]
        public bool EnableTextureRenderingTester = false;


        [Header("Debug - Geometry Generation")]
        
        [Tooltip("Whether or not to enable geometry generation.")]
        public bool EnableGeometryGeneration = true;

        [Tooltip("Whether or not wall faces with lower textures on them will be generated.")]
        public bool EnableWallGeneration_LowerTextures = true;
        
        [Tooltip("Whether or not wall faces with middle textures on them will be generated.")]
        public bool EnableWallGeneration_MiddleTextures = true;
        
        [Tooltip("Whether or not wall faces with upper textures on them will be generated.")]
        public bool EnableWallGeneration_UpperTextures = true;

    }
}
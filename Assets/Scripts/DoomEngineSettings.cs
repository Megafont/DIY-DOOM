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
        [SerializeField] public string WAD_Path = "WADS/DOOM.wad";

        [Tooltip("The name of the map to load.")]
        [SerializeField] public string MapToLoad = "E1M1";


        [Header("Mesh Generation")]

        [Tooltip("This is the prefab used to instantiate empty mesh objects.")]
        [SerializeField] public LevelSubMesh LevelMeshPrefab;

        [Tooltip("This is the material that is used to create a material instance for each of the textures.")]
        [SerializeField] public Material DOOM_MaterialPrefab;

        [Tooltip("This is the material that is used to render faces with missing textures.")]
        [SerializeField] public Material DOOM_MissingTextureMaterial;

        [Space(10)]


        [Header("Graphics")]
        
        [SerializeField] public bool TextureAlphaIsTransparency = true;
        [SerializeField] public FilterMode TextureFilterMode = FilterMode.Point;
        [SerializeField] public TextureWrapMode TextureWrapMode = TextureWrapMode.Repeat;
        [SerializeField] public Color32 NewTextureFillColor = Color.clear;

        [Space(10)]


        [Header("Runtime Objects (DoomEngine.cs sets these at runtime)")]

        [Tooltip("This is the GameObject that will hold the 3D geometry of the map. It must have a MeshFilter and a MeshRender component on it!")]
        [SerializeField] public LevelGeometry LevelGeometryObject;

        [SerializeField] public MeshRenderer TextureTestObject;


    }
}
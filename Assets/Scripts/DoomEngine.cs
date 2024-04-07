using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DIY_DOOM.AutoMap;
using DIY_DOOM.Maps;
using DIY_DOOM.MeshGeneration;
using DIY_DOOM.WADs;


namespace DIY_DOOM
{
    public class DoomEngine : MonoBehaviour
    {

        [Tooltip("A DoomEngineSettings object that contains configuration settings to be used by the engine.")]
        [SerializeField] private DoomEngineSettings _Settings;

        [Space(10)]


        [Header("Mesh Generation")]

        [Tooltip("This is the GameObject that will hold the 3D geometry of the map. It must have a MeshFilter and a MeshRender component on it!")]
        [SerializeField] private LevelGeometry _LevelGeometryObject;

        [Space(10)]


        [Header("Testing")]

        [SerializeField] private MeshRenderer _TextureTestObject;


        private AssetManager _AssetManager;
        private WAD_Loader _WAD_Loader;
        private Map _Map;

        private bool _IsOver;



        private void Awake()
        {
            if (_LevelGeometryObject == null)
                throw new Exception("The LevelGeometryObject is not set in the inspector!");
            if (_TextureTestObject == null)
                throw new Exception("The TextureTestObject is not set in the inspector!");


            Settings = _Settings;

            Settings.LevelGeometryObject = _LevelGeometryObject;
            Settings.TextureTestObject = _TextureTestObject;

            _WAD_Loader = new WAD_Loader();
            _AssetManager = new AssetManager(_WAD_Loader);
        }

        // Start is called before the first frame update
        void Start()
        {
            Init();
        }

        // Update is called once per frame
        void Update()
        {

        }

        protected virtual bool Init()
        {
            bool loadedWAD = _WAD_Loader.LoadWAD(Application.persistentDataPath + "/" + _Settings.WAD_Path);
            bool loadedMapData = _WAD_Loader.LoadMapData(_Settings.MapToLoad, out Map map);

            InitAutoMap(map);

            DEBUG_DoTextureTest();

            _Settings.LevelGeometryObject.SetMap(map);

            return loadedWAD && loadedMapData;
        }

        private void InitAutoMap(Map map)
        {
            AutoMapRenderer autoMapRenderer = FindObjectOfType<AutoMapRenderer>();
            autoMapRenderer.DrawMap(map, Color.white);

            // Swaping these two lines changes which BSP traverser is used.
            //BSP_Traverser_A traverser = autoMapRenderer.GetComponent<BSP_Traverser_A>();
            BSP_Traverser_B traverser = autoMapRenderer.GetComponent<BSP_Traverser_B>();

            traverser.SetMap(map);
            traverser.RenderBspNodes();
        }

        protected virtual bool IsOver()
        {
            return _IsOver;
        }

        protected virtual void Quit()
        {
            _IsOver = true;
            Application.Quit();
        }

        private void DEBUG_DoTextureTest()
        {
            //_Settings.TextureTestObject.material.mainTexture = _AssetManager.GetTexture("PISGA0", 0);

            //_Settings.TextureTestObject.material.mainTexture = _AssetManager.GetTexture("AASTINKY", 0);
            //_Settings.TextureTestObject.material.mainTexture = _AssetManager.GetTexture("BROWN1", 0);
            //_Settings.TextureTestObject.material.mainTexture = _AssetManager.GetTexture("BROWNPIP", 0);
            _Settings.TextureTestObject.material.mainTexture = _AssetManager.GetTexture("BROWN144", 0);
            //_Settings.TextureTestObject.material.mainTexture = _AssetManager.GetTexture("BIGDOOR1", 0);
            //_Settings.TextureTestObject.material.mainTexture = _AssetManager.GetTexture("BIGDOOR2", 0);
            //_Settings.TextureTestObject.material.mainTexture = _AssetManager.GetTexture("BIGDOOR4", 0);
            //_Settings.TextureTestObject.material.mainTexture = _AssetManager.GetTexture("COMP2", 0);
            //_Settings.TextureTestObject.material.mainTexture = _AssetManager.GetTexture("BRNSMAL1", 0);
            //_Settings.TextureTestObject.material.mainTexture = _AssetManager.GetTexture("BRNBIGC", 0);
            //_Settings.TextureTestObject.material.mainTexture = _AssetManager.GetTexture("BRNPOIS", 0);
            //_Settings.TextureTestObject.material.mainTexture = _AssetManager.GetTexture("BRNPOIS2", 0);
            //_Settings.TextureTestObject.material.mainTexture = _AssetManager.GetTexture("EXITDOOR", 0);
            //_Settings.TextureTestObject.material.mainTexture = _AssetManager.GetTexture("SKY1", 0);
            //_Settings.TextureTestObject.material.mainTexture = _AssetManager.GetTexture("TEKWALL5", 0);
            //_Settings.TextureTestObject.material.mainTexture = _AssetManager.GetTexture("SW1DIRT", 0);
            //_Settings.TextureTestObject.material.mainTexture = _AssetManager.GetTexture("LITE3", 0);


        }



        public static DoomEngineSettings Settings { get; private set; }
    }
}
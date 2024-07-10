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


        private AssetManager _AssetManager;
        private WAD_Loader _WAD_Loader;
        private Map _Map;

        private bool _IsOver;



        private void Awake()
        {
            if (_LevelGeometryObject == null)
                throw new Exception("The LevelGeometryObject is not set in the inspector!");


            Settings = _Settings;

            Settings.LevelGeometryObject = _LevelGeometryObject;

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
            bool loadedMapData = _WAD_Loader.LoadMapData(_Settings.MapToLoad, _Settings.MapScaleFactor, out Map map);


            if (_Settings.EnableAutoMap)
                InitAutoMap(map);

            if (_Settings.EnableGeometryGeneration)
                _Settings.LevelGeometryObject.SetMap(map);


            if (_Settings.EnableTextureRenderingTester)
                Tester.RunTests();


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





        public static DoomEngineSettings Settings { get; private set; }
    }
}
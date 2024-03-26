using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DIY_DOOM.AutoMap;
using DIY_DOOM.Maps;
using DIY_DOOM.WADs;


namespace DIY_DOOM
{
    public class DoomEngine : MonoBehaviour
    {
        public MeshRenderer TextureTest;

        [Tooltip("This is a relative path to the WAD to load. It is relative to Application.persistantDataPath.")]
        [SerializeField]
        private string WAD_Path = "WADS/DOOM.wad";

        [Tooltip("The name of the map to load.")]
        [SerializeField]
        private string MapToLoad = "E1M1";


        private AssetManager _AssetManager;
        private WAD_Loader _WAD_Loader;
        private Map _Map;

        private bool _IsOver;



        private void Awake()
        {
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
            bool loadedWAD = _WAD_Loader.LoadWAD(Application.persistentDataPath + "/" + WAD_Path);
            bool loadedMapData = _WAD_Loader.LoadMapData(MapToLoad, out Map map);

            //DEBUG_DoTextureTest();

            AutoMapRenderer autoMapRenderer = FindObjectOfType<AutoMapRenderer>();
            autoMapRenderer.DrawMap(map, Color.white);

            // Swaping these two lines changes which BSP traverser is used.
            //BSP_Traverser_A traverser = autoMapRenderer.GetComponent<BSP_Traverser_A>();
            BSP_Traverser_B traverser = autoMapRenderer.GetComponent<BSP_Traverser_B>();

            traverser.SetMap(map);
            traverser.RenderBspNodes();

            return loadedWAD && loadedMapData;
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
            //TextureTest.material.mainTexture = _AssetManager.GetTexture("PISGA0", 0);

            //TextureTest.material.mainTexture = _AssetManager.GetTexture("AASTINKY", 0);
            //TextureTest.material.mainTexture = _AssetManager.GetTexture("BROWN1", 0);
            //TextureTest.material.mainTexture = _AssetManager.GetTexture("BROWNPIP", 0);
            //TextureTest.material.mainTexture = _AssetManager.GetTexture("BROWN144", 0);
            //TextureTest.material.mainTexture = _AssetManager.GetTexture("BIGDOOR1", 0);
            //TextureTest.material.mainTexture = _AssetManager.GetTexture("BIGDOOR2", 0);
            //TextureTest.material.mainTexture = _AssetManager.GetTexture("BIGDOOR4", 0);
            TextureTest.material.mainTexture = _AssetManager.GetTexture("COMP2", 0);
            //TextureTest.material.mainTexture = _AssetManager.GetTexture("BRNSMAL1", 0);
            //TextureTest.material.mainTexture = _AssetManager.GetTexture("BRNBIGC", 0);
            //TextureTest.material.mainTexture = _AssetManager.GetTexture("BRNPOIS", 0);
            //TextureTest.material.mainTexture = _AssetManager.GetTexture("BRNPOIS2", 0);
            //TextureTest.material.mainTexture = _AssetManager.GetTexture("EXITDOOR", 0);
            //TextureTest.material.mainTexture = _AssetManager.GetTexture("SKY1", 0);
            //TextureTest.material.mainTexture = _AssetManager.GetTexture("TEKWALL5", 0);
            //TextureTest.material.mainTexture = _AssetManager.GetTexture("SW1DIRT", 0);


        }
    }
}
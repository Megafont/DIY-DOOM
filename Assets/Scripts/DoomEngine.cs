using DIY_DOOM.AutoMap;
using DIY_DOOM.Maps;
using DIY_DOOM.WADs;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace DIY_DOOM
{
    public class DoomEngine : MonoBehaviour
    {
        [Tooltip("This is a relative path to the WAD to load. It is relative to Application.persistantDataPath.")]
        [SerializeField]
        private string WAD_Path = "WADS/DOOM.wad";

        [Tooltip("The name of the map to load.")]
        [SerializeField]
        private string MapToLoad = "E1M1";


        private WAD_Loader _WAD_Loader;
        private Map _Map;

        private bool _IsOver;



        private void Awake()
        {
            _WAD_Loader = new WAD_Loader();
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

            FindObjectOfType<AutoMapRenderer>().DrawMap(map);

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

    }
}
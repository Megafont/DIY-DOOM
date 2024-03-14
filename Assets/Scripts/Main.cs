using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class Main : MonoBehaviour
{
    [Tooltip("This is a relative path to the WAD to load. It is relative to Application.persistantDataPath.")]
    [SerializeField]
    private string WAD_Path = "WADS/DOOM.wad";

    [Tooltip("The name of the map to load.")]
    [SerializeField]
    private string WAD_Map = "E1M1";



    // Start is called before the first frame update
    void Start()
    {
        string wadPath = Application.persistentDataPath + "/" + WAD_Path;
        WAD_Loader wadLoader = new WAD_Loader(wadPath);
        wadLoader.LoadWAD();

        Map map = new Map(WAD_Map);
        wadLoader.LoadMapData(map);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

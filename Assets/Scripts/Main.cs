using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    [SerializeField]
    private string WAD_Path = "WADS/DOOM.wad";


    // Start is called before the first frame update
    void Start()
    {
        string wadPath = Application.persistentDataPath + "/" + WAD_Path;
        WAD_Loader wadLoader = new WAD_Loader(wadPath);
        wadLoader.LoadWAD();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

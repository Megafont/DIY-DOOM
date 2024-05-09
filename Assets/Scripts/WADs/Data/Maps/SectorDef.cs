using UnityEngine;


namespace DIY_DOOM.WADs.Data.Maps
{
    public class SectorDef
    {
        public float FloorHeight;
        public float CeilingHeight;

        public string FloorTextureName;
        public string CeilingTextureName;

        public int LightLevel;
        public int Type;
        public int Tag;



        public void DEBUG_Print()
        {
            Debug.Log("SECTOR");
            Debug.Log(new string('-', 256));
            Debug.Log($"Floor Height: {FloorHeight}");
            Debug.Log($"Ceiling Height: {CeilingHeight}");
            Debug.Log($"Floor Texture: {FloorTextureName}");
            Debug.Log($"Ceiling Texture: {CeilingTextureName}");
            Debug.Log($"Light Level: {LightLevel}");
            Debug.Log($"Type: {Type}");
            Debug.Log($"Tag: {Tag}");
            Debug.Log(new string('-', 256));
        }

    }

}
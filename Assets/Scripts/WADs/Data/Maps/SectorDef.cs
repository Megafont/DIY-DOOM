using UnityEngine;


namespace DIY_DOOM.WADs.Data.Maps
{
    public struct SectorDef
    {
        public float FloorHeight;
        public float CeilingHeight;

        public string FloorTexture;
        public string CeilingTexture;

        public int LightLevel;
        public int Type;
        public int Tag;



        public void DEBUG_Print()
        {
            Debug.Log("SECTOR");
            Debug.Log(new string('-', 256));
            Debug.Log($"Floor Height: {FloorHeight}");
            Debug.Log($"Ceiling Height: {CeilingHeight}");
            Debug.Log($"Floor Texture: {FloorTexture}");
            Debug.Log($"Ceiling Texture: {CeilingTexture}");
            Debug.Log($"Light Level: {LightLevel}");
            Debug.Log($"Type: {Type}");
            Debug.Log($"Tag: {Tag}");
            Debug.Log(new string('-', 256));
        }

    }

}
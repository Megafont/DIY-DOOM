using UnityEngine;


namespace DIY_DOOM.WADs.Data
{
    public struct WAD_DirectoryDef
    {
        public uint LumpOffset;
        public uint LumpSize;
        public string LumpName;


        public void DEBUG_Print()
        {
            Debug.Log("WAD DIRECTORY");
            Debug.Log(new string('-', 256));
            Debug.Log($"Lump Name: {LumpName}");
            Debug.Log($"Lump Size: {LumpSize}");
            Debug.Log($"Lump Offset: {LumpOffset}");
            Debug.Log(new string('-', 256));
            Debug.Log("");
        }
    }

}

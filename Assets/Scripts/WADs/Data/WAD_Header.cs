using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Analytics;


namespace DIY_DOOM.WADs.Data
{
    public struct WAD_Header
    {
        /// <summary>
        /// There are two WAD types. IWAD is for official WADs released by ID software. PWAD is for custom WADs.
        /// </summary>
        public string WAD_Type;

        public uint DirectoryCount;
        public uint DirectoryOffset;


        public void DEBUG_Print()
        {
            Debug.Log("WAD HEADER");
            Debug.Log(new string('-', 256));
            Debug.Log($"WAD Type: {WAD_Type}");
            Debug.Log($"Directory Count: {DirectoryCount}");
            Debug.Log($"Directory Offset: {DirectoryOffset}");
            Debug.Log(new string('-', 256));
            Debug.Log("");
        }
    }
}

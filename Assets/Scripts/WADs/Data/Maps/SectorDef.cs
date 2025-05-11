using DIY_DOOM.Maps;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace DIY_DOOM.WADs.Data.Maps
{
    public class SectorDef
    {
        public uint ID;

        public float FloorHeight;
        public float CeilingHeight;

        public string FloorTextureName;
        public string CeilingTextureName;

        public int LightLevel;
        public int Type;
        public int Tag;

        /// <summary>
        /// This list holds references to all segs linked to front SideDefs that reference this sector.
        /// This list can be thought of as defining the exterior outline of the sector.
        /// </summary>
        public List<SegDef> FrontSegs = new List<SegDef>();

        /// <summary>
        /// This list holds references to all segs linked to back SideDefs that reference this sector.
        /// </summary>
        public List<SegDef> BackSegs = new List<SegDef>();

        /// <summary>
        /// This list holds a list of segs for each hole that exists within this sector (aka another sector)
        /// </summary>
        public List<List<LineDef>> Holes = new List<List<LineDef>>();

        /// <summary>
        /// This list holds the outline of the sector.
        /// </summary>
        public List<Vector2> SectorOutline = new List<Vector2>();



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
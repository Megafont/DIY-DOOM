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
        /// This list holds references to all line defs linked to front SideDefs that reference this sector.
        /// This list can be thought of as defining the exterior outline of the sector.
        /// </summary>
        public List<LineDef> FrontLineDefs = new List<LineDef>();

        /// <summary>
        /// This list holds references to all LineDefs linked to back SideDefs that reference this sector.
        /// </summary>
        public List<LineDef> BackLineDefs = new List<LineDef>();
        
        /// <summary>
        /// This list holds references to all segs linked to front SideDefs that reference this sector.
        /// This list can be thought of as defining the exterior outline of the sector.
        /// </summary>
        public List<SegDef> FrontSegDefs = new List<SegDef>();

        /// <summary>
        /// This list holds references to all segs linked to back SideDefs that reference this sector.
        /// </summary>
        public List<SegDef> BackSegDefs = new List<SegDef>();

        /// <summary>
        /// This list holds a list of segs for each hole that exists within this sector (aka another sector)
        /// </summary>
        public List<List<LineDef>> Holes = new List<List<LineDef>>();

        /// <summary>
        /// This list holds the outline of the sector.
        /// </summary>
        public List<Vector2> SectorOutline = new List<Vector2>();


        public List<LineDef> GetLineDefs(Map map)
        {
            Dictionary<uint, LineDef> lineDefs = new Dictionary<uint, LineDef>();
            List<SegDef> segs = new List<SegDef>(FrontSegDefs);
            segs.AddRange(BackSegDefs);

            foreach (SegDef seg in segs)
            {
                if (!lineDefs.ContainsKey(seg.LineDefID))
                {
                    lineDefs.Add(seg.LineDefID, map.GetLineDef(seg.LineDefID));
                }
                
            } // end foreach
            
            
            return new List<LineDef>(lineDefs.Values);
        }

        /// <summary>
        /// Returns all sector defs that share line defs with this sector.
        /// </summary>
        /// <param name="map">The map data.</param>
        /// <returns>A list containing all sector defs that share a line def with this sector.</returns>
        public List<SectorDef> GetNeighboringSectors(Map map)
        {
            Dictionary<uint, SectorDef> neighborSectors = new Dictionary<uint, SectorDef>();

            foreach (SegDef segDef in FrontSegDefs)
            {
                LineDef lineDef = map.GetLineDef(segDef.LineDefID);
                SideDef front = map.GetSideDef(lineDef.FrontSideDefIndex);
                SideDef back = map.GetSideDef(lineDef.BackSideDefIndex);

                if (front.SectorIndex != ID &&
                    !neighborSectors.ContainsKey(front.SectorIndex))
                { 
                    neighborSectors.Add(front.SectorIndex, map.GetSectorDef(front.SectorIndex));
                }
                
                if (back.SectorIndex != ID &&
                    !neighborSectors.ContainsKey(back.SectorIndex))
                { 
                    neighborSectors.Add(back.SectorIndex, map.GetSectorDef(back.SectorIndex));
                }                
            }
            
            
            return new List<SectorDef>(neighborSectors.Values);
        }

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
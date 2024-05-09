using UnityEngine;


namespace DIY_DOOM.WADs.Data.Maps
{
    /// <summary>
    /// A sub sector is a convex subsection of a sector.
    /// </summary>
    public class SubSectorDef
    {
        public uint SegCount;
        public uint FirstSegID;


        public void DEBUG_Print()
        {
            Debug.Log("SUBSECTOR");
            Debug.Log(new string('-', 256));
            Debug.Log($"Seg Count: {SegCount}");
            Debug.Log($"First Seg ID: {FirstSegID}");
            Debug.Log(new string('-', 256));
        }
    }


}
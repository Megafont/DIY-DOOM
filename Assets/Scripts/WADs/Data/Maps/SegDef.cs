using UnityEngine;


namespace DIY_DOOM.WADs.Data.Maps
{
    /// <summary>
    /// A seg is a segment of a LineDef, or sometimes an entire lineDef.
    /// </summary>
    public struct SegDef
    {
        public uint StartVertexID;
        public uint EndVertexID;
        public int Angle;
        public uint LineDefID;
        public uint Direction; // Facing direction: 0 = same as lineDef, and 1 = opposite of lineDef
        public uint Offset; // Distance from start of lineDef to start of this seg


        public void DEBUG_Print()
        {
            Debug.Log("SEG");
            Debug.Log(new string('-', 256));
            Debug.Log($"Start Vertex: {StartVertexID}");
            Debug.Log($"End Vertex: {EndVertexID}");
            Debug.Log($"Angle: {Angle}");
            Debug.Log($"Line Def ID: {LineDefID}");
            Debug.Log($"Direction: {Direction}");
            Debug.Log($"Offset: {Offset}");
            Debug.Log(new string('-', 256));
        }
    }


}
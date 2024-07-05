using UnityEngine;


namespace DIY_DOOM.WADs.Data.Maps
{
    /// <summary>
    /// A seg is a segment of a LineDef, or sometimes an entire lineDef.
    /// </summary>
    public class SegDef
    {
        public uint StartVertexID;
        public uint EndVertexID;
        public int Angle;
        public uint LineDefID;
        public uint Direction; // Facing direction: 0 = same as lineDef (runs along the lineDef's front side), and 1 = opposite of lineDef (runs along the lineDef's back side)
        public uint Offset; // Distance along the lineDef to the start of this seg

        // TODO: Remove these fields once I get the floor/ceiling geometry working!
        public Vector3 StartPoint; // The start point calculated by using the Offset field.
        public float PercentStartShifted;
        public int ID;


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
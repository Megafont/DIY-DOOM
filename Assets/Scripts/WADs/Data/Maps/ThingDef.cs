using UnityEngine;


namespace DIY_DOOM.WADs.Data.Maps
{
    public class ThingDef
    {
        public Vector3 Position;
        public uint Angle;
        public uint Type;
        public uint Flags;


        public void DEBUG_Print()
        {
            Debug.Log("THING");
            Debug.Log(new string('-', 256));
            Debug.Log($"Position: {Position}");
            Debug.Log($"Angle: {Angle}");
            Debug.Log($"Type: {Type}");
            Debug.Log($"Flags: {Flags}");
            Debug.Log(new string('-', 256));
        }
    }


}
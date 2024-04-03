using System;
using UnityEngine;


namespace DIY_DOOM.WADs.Data.Maps
{
    public struct SideDef
    {
        public int X_Offset;
        public int Y_Offset;

        public string UpperTextureName;
        public string MiddleTextureName;
        public string LowerTextureName;

        public uint SectorIndex;



        public bool HasOneOrMoreTextures()
        {
            return !FieldIsEmptyOrADash(UpperTextureName) || 
                   !FieldIsEmptyOrADash(LowerTextureName) ||    
                   !FieldIsEmptyOrADash(MiddleTextureName);
        }

        private bool FieldIsEmptyOrADash(string field)
        {
            return string.IsNullOrWhiteSpace(field) || field.Trim() == "-";
        }

        public void DEBUG_Print()
        {
            Debug.Log("SIDEDEF");
            Debug.Log(new string('-', 256));
            Debug.Log($"X Offset: {X_Offset}");
            Debug.Log($"Y Offset: {Y_Offset}");
            Debug.Log($"Upper Texture: \"{UpperTextureName}\"");
            Debug.Log($"Lower Texture: \"{LowerTextureName}\"");
            Debug.Log($"Middle Texture: \"{MiddleTextureName}\"");
            Debug.Log($"Sector Index: {SectorIndex}");
            Debug.Log(new string('-', 256));
        }
    }

}
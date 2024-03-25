using UnityEngine;


namespace DIY_DOOM.WADs.Data.Textures
{
    public struct PatchHeader
    {
        public uint Width;
        public uint Height;
        public int X_Offset;
        public int Y_Offset;
        
        private uint[] _ColumnOffsets;



        public void SetColumnOffsets(uint[] columnOffsets)
        {
            _ColumnOffsets = columnOffsets;
        }

        public uint GetColumnOffset(int index)
        {
            return _ColumnOffsets[index];
        }

        public void DEBUG_Print(bool printColumnOffsets = false)
        {
            Debug.Log("PATCH HEADER");
            Debug.Log(new string('-', 256));
            Debug.Log($"Width: {Width}");
            Debug.Log($"Height: {Height}");
            Debug.Log($"X Offset: {X_Offset}");
            Debug.Log($"Y Offset: {Y_Offset}");

            if (printColumnOffsets)
            {
                Debug.Log("COLUMN OFFSETS:");
                for (int i = 0; i < _ColumnOffsets.Length; i++)
                {
                    Debug.Log($"[{i}]: {_ColumnOffsets[i]}");
                }
            }

            Debug.Log(new string('-', 256));
        }


        public int GetColumnOffsetsCount { get {  return _ColumnOffsets.Length;} }
    }


}
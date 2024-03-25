using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace DIY_DOOM.WADs.Data.Textures
{
    public struct PatchColumn
    {
        public byte TopDelta;
        public byte Length;
        public byte PaddingPre;
        public byte PaddingPost;


        private byte[] _ColumnData;



        public void DEBUG_Print()
        {
            Debug.Log("PATCH COLUMN");
            Debug.Log(new string('-', 256));
            Debug.Log($"Top Delta: {TopDelta}");
            Debug.Log($"Length: {Length}");
            Debug.Log($"Padding Pre: {PaddingPre}");
            Debug.Log($"Padding Post: {PaddingPost}");

            Debug.Log("COLUMN DATA:");
            string colData = "";
            for (int i = 0; i < _ColumnData.Length; i++)
            {
                colData += $"{(int) _ColumnData[i]} ";
            }
            Debug.Log(colData);

            Debug.Log(new string('-', 256));
        }

        public void SetColumnData(byte[] columnData)
        {
            _ColumnData = columnData;
        }

        public byte GetColumnData(int index)
        {
            return _ColumnData[index];
        }



        public byte this[int i]
        {
            get
            {
                return _ColumnData[i];
            }
            set
            {
                _ColumnData[i] = value;
            }
        }   
    }


}
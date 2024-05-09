using UnityEngine;


namespace DIY_DOOM.WADs.Data.Maps
{
    public class NodeDef
    {
        // These two vectors define the line (binary space partition) that is dividing the space this node represents.
        public Vector3 PartitionStart;
        public Vector3 PartitionEnd;
        public Vector3 DeltaToPartitionEnd; // This represents the distance and direction from the PartitionStart point to the end point of the partition line. So PartitionStart plus this value equals the end point of the line.
        
        // Opposite corners of the bounding box of the right side of the space partition for this node.
        public Vector3 RightBox_BottomLeft;
        public Vector3 RightBox_TopRight;

        // Opposite corners of the bounding box of the left side of the space partition for this node.
        public Vector3 LeftBox_BottomLeft;
        public Vector3 LeftBox_TopRight;

        // Node IDs of the children of this node.
        public uint RightChildID;
        public uint LeftChildID;


        public void DEBUG_Print()
        {
            Debug.Log("NODE");
            Debug.Log(new string('-', 256));
            Debug.Log($"Partition Start: {PartitionStart}");
            Debug.Log($"Delta To Partition End: {DeltaToPartitionEnd}");
            Debug.Log($"Right Box Bottom Left: {RightBox_BottomLeft}");
            Debug.Log($"Right Box Top Right: {RightBox_TopRight}");
            Debug.Log($"Left Box Bottom Left: {LeftBox_BottomLeft}");
            Debug.Log($"Left Box Top Right: {LeftBox_TopRight}");
            Debug.Log($"Right Child ID: {RightChildID}");
            Debug.Log($"Left Child ID: {LeftChildID}");
            Debug.Log(new string('-', 256));
        }
    }


}
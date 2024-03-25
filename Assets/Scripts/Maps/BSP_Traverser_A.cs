using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DIY_DOOM.Maps;
using DIY_DOOM.AutoMap;
using DIY_DOOM.WADs.Data.Maps;


/// <summary>
/// This is the first version of the BSP code in Notes008 in the repo linked in the
/// readme file in the root folder of this project.
/// 
/// This class uses the AutoMapDrawer to visualize the process of navigating the BSP tree
/// to get to the subsector that contains the player.
/// </summary>
public class BSP_Traverser_A : MonoBehaviour
{
    // This constant is used to check the last bit of the node ID to see if it is a leaf node (has no children).
    // 0x8000 in binary is: 1000000000000000
    private const uint SUBSECTOR_IDENTIFIER = 0x8000;


    private AutoMapRenderer _AutoMapRenderer;
    private Map _Map;



    void Awake()
    {
        _AutoMapRenderer = GetComponent<AutoMapRenderer>();
    }

    public void SetMap(Map map)
    {
        if (map == null)
            throw new ArgumentNullException("The passed in map is null!");


        _Map = map;
    }


    private IEnumerator TraverseAndRenderBspNodes(uint nodeID)
    {
        yield return new WaitForSeconds(3f);

        // Check if this node represents a subsector (aka leaf node).
        if ((nodeID & SUBSECTOR_IDENTIFIER) == SUBSECTOR_IDENTIFIER)
        {
            RenderSubSector(nodeID & (~SUBSECTOR_IDENTIFIER));
            yield break;
        }


        // Check if the player is on the left side of this node's binary space partition.
        bool isOnLeft = IsPointOnLeftSide(_Map.GetPlayerSpawn(0).Position, 
                                          nodeID);


        NodeDef node = _Map.GetNodeDef(nodeID);
        _AutoMapRenderer.DrawBinaryspacePartition(node);


        // Proceed to the child node representing the side the player is on.
        if (isOnLeft)
        {
            RenderBspNodes(node.LeftChildID);
        }
        else
        {
            RenderBspNodes(node.RightChildID);
        }
    }

    public void RenderBspNodes(uint nodeID)
    {
        StartCoroutine(TraverseAndRenderBspNodes(nodeID));
    }
    public void RenderBspNodes()
    {
        StartCoroutine(TraverseAndRenderBspNodes(_Map.NodesCount - 1));
    }

    private void RenderSubSector(uint subSectorID)
    {

    }

    /// <summary>
    /// This function uses the dot product to determine if the specified point is on the
    /// left side of the partition line.
    /// </summary>
    /// <param name="point">The point to check.</param>
    /// <param name="nodeID">The ID of the node whose space partition is being checked against.</param>
    /// <returns>True if the point is on the left side of the partition line, or false otherwise.</returns>
    private bool IsPointOnLeftSide(Vector3 point, uint nodeID)
    {
        NodeDef node = _Map.GetNodeDef(nodeID);

        Vector3 pointToPartition = MapUtils.Point3dToFlattened3D(point - node.PartitionStart);

        Vector3 spacePartitionLine = MapUtils.Point3dToFlattened3D(node.DeltaToPartitionEnd);


      
        return Vector3.Cross(spacePartitionLine, pointToPartition).y <= 0;


        // This line code how he calculates the cross product.
        // float cross = (pointToPartition.x * spacePartitionLine.y) - (pointToPartition.y * spacePartitionLine.x);
        // return cross <= 0;
    }
}

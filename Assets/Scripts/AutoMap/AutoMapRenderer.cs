using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DIY_DOOM.Maps;
using DIY_DOOM.WADs.Data.Maps;
using DIY_DOOM.Utils.Maps;


namespace DIY_DOOM.AutoMap
{
    public class AutoMapRenderer : MonoBehaviour
    {
        public const float LINE_DEPTH_INCREMENT = 0.1f;

        private List<Color> _Colors = new List<Color>()
        {
            new Color32(255, 0, 0, 255),
            new Color32(0, 255, 0, 255),
            new Color32(0, 0, 255, 255),
            new Color32(255, 255, 0, 255),
            new Color32(0, 255, 255, 255),
            new Color32(255, 0, 255, 255),
            new Color32(128, 0, 0, 255),
            new Color32(0, 128, 0, 255),
            new Color32(0, 0, 128, 255),
            new Color32(128, 128, 0, 255),
            new Color32(0, 128, 128, 255),
            new Color32(128, 0, 128, 255),
            new Color32(255, 255, 255, 255),
            new Color32(128, 128, 128, 255),
        };


        [Header("References")]
        [SerializeField]
        private Camera _AutoMapCamera;
        
        [Header("Prefabs")]
        [SerializeField]
        private LineRenderer _LineRendererPrefab;
        [SerializeField]
        GameObject _PlayerPrefab;


        private LineRendererPool _LineRendererPool;

        private Map _Map;

        private Transform _Container_Root;
        private Transform _Container_ActiveLineRenderers;
        private Transform _Container_InactiveLineRenderers;
        private Transform _Container_Things;

        private GameObject _PlayerObject;


        private void Awake()
        {
            CreateContainerObjects();

            _LineRendererPool = new LineRendererPool(_LineRendererPrefab.gameObject, 
                                                     _Container_ActiveLineRenderers,
                                                     _Container_InactiveLineRenderers);

            _PlayerObject = Instantiate(_PlayerPrefab);
            _PlayerObject.transform.SetParent(_Container_Things);
            _PlayerObject.SetActive(false);

            _AutoMapCamera.transform.SetParent(_PlayerObject.transform);
        }

        private void CreateContainerObjects()
        {
            _Container_Root = new GameObject("Walls").transform;
            _Container_Root.transform.SetParent(transform);

            _Container_ActiveLineRenderers = new GameObject("Active").transform;
            _Container_ActiveLineRenderers.transform.SetParent(_Container_Root.transform);
            
            _Container_InactiveLineRenderers = new GameObject("Inactive").transform;
            _Container_InactiveLineRenderers.transform.SetParent(_Container_Root.transform);

            _Container_Things = new GameObject("Things").transform;
            _Container_Things.transform.SetParent(transform);
        }

        public void DrawMap(Map map, Color color, float yOffset = 0f)
        {
            _Map = map;

            ClearAll();
            
            DrawWalls(color, yOffset);
            DrawPlayer();

            for (int i = 0; i <= 15; i++)
            {
                Color sectorColor = new Color32((byte)UnityEngine.Random.Range(0, 256), (byte)UnityEngine.Random.Range(0, 256), (byte)UnityEngine.Random.Range(0, 256), 255);
                DrawSector(_Map.GetSectorDef((uint) i), sectorColor, yOffset + 1 + (i * LINE_DEPTH_INCREMENT));
            }

            //DrawBinaryspacePartition(_Map.GetNodeDef(_Map.NodesCount - 1));
        }

        public void DrawWalls(Color color, float yOffset = 0f)
        {
            // Clear all current lines.
            _LineRendererPool.ReturnAllToPool();


            // Draw the _Map.
            for (uint i = 0; i < _Map.LineDefsCount - 1; i++)
            {
                LineDef lineDef = _Map.GetLineDef(i);

                DrawLine(_Map.GetVertex(lineDef.StartVertexID),
                         _Map.GetVertex(lineDef.EndVertexID),
                         color,
                         yOffset);
            }
        }

        public void DrawBinaryspacePartition(NodeDef node)
        {
            
            //  RIGHT BOX / SPACE PARTITION
            Vector2 rightBox_BottomLeft = node.RightBox_BottomLeft;
            Vector2 rightBox_TopRight = node.RightBox_TopRight;


            // LEFT BOX / SPACE PARTITION
            Vector2 leftBox_BottomLeft = node.LeftBox_BottomLeft;
            Vector2 leftBox_TopRight = node.LeftBox_TopRight;


            // Render the boxes.
            DrawBox(rightBox_BottomLeft,
                    rightBox_TopRight, 
                    Color.green,
                    LINE_DEPTH_INCREMENT * 1f);
            DrawBox(leftBox_BottomLeft,
                    leftBox_TopRight, 
                    Color.red,
                    LINE_DEPTH_INCREMENT * 2f);



            // THE SEPARATOR LINE THAT THE SPACE PARTITION IS BASED ON
            Vector2 partitionStart = node.PartitionStart;
            Vector2 partitionEnd = node.PartitionStart + node.DeltaToPartitionEnd;

            // Render the line.
            DrawLine(partitionStart, 
                     partitionEnd, 
                     Color.blue,
                     LINE_DEPTH_INCREMENT * 3f);
        }

        /// <summary>
        /// This function draws a sector's outline.
        /// </summary>
        /// <param name="sector">The sector whose outline is to be drawn.</param>
        /// <param name="color">The color of the outline.</param>
        /// <param name="yOffset">The y-position of the outline.</param>
        public void DrawSector(SectorDef sector, Color32 color, float yOffset = 0f)
        {
            if (sector.SectorOutline == null || sector.SectorOutline.Count < 3)
            {
                Debug.LogWarning($"Sector outline cannot be drawn, as it is null or has less than 3 vertices!");
                return;
            }

            color = Color.black;

            for (int i = 0; i < sector.SectorOutline.Count - 1; i++)
            {
                DrawLine(MapUtils.Point2dTo3dXZ(sector.SectorOutline[i]),
                         MapUtils.Point2dTo3dXZ(sector.SectorOutline[i + 1]),
                         color,
                         yOffset);

                color.b += 5;
            }


            // Draw one last segment to connect the start and end points.
            DrawLine(MapUtils.Point2dTo3dXZ(sector.SectorOutline[sector.SectorOutline.Count - 1]),
                     MapUtils.Point2dTo3dXZ(sector.SectorOutline[0]),
                     color,
                     yOffset);
        }


        /* The below functions are commented out as they were my attempts to get it to draw subsector outlines.
         * However, this isn't needed anyway, and it may not be possible.
         * 
         
        /// <summary>
        /// This function draws a subSector's outline.
        /// </summary>
        /// <param name="subSector">The subsector whose outline is to be drawn.</param>
        /// <param name="color">The color of the outline.</param>
        /// <param name="yOffset">The y-position of the outline.</param>
        public void DrawSubSector_Original(SubSectorDef subSector, Color color, float yOffset = 0f)
        {
            List<Vector3> vertices = new List<Vector3>();

            SegDef curSeg;
            Vector3 curStart = Vector3.zero;
            Vector3 curEnd = Vector3.zero;
            for (uint i = 0; i < subSector.SegCount; i++)
            {                
                curSeg = _Map.GetSegDef(subSector.FirstSegID + i);

                curStart = _Map.GetVertex(curSeg.StartVertexID);
                curEnd = _Map.GetVertex(curSeg.EndVertexID);

                LineRenderer r = DrawLine(curStart, curEnd, color, LINE_DEPTH_INCREMENT * 4f);

                vertices.Add(curStart);
                vertices.Add(curEnd);
            }
            
        }

        /// <summary>
        /// This function draws a subSector's outline.
        /// </summary>
        /// <remarks>Thios version of the function does not have the fillInMissingSegments parameter,
        /// because there is no way to draw a polygon with gaps using a list of consecutive line segments.
        /// </remarks>
        /// <param name="subSector">The subsector whose outline is to be drawn.</param>
        public void DrawSubSector_FullOutline(SubSectorDef subSector, Color color, float yOffset = 0f)
        {
            List<Vector3> verts = GetVertsInCorrectOrder(subSector);
            
            DrawPolygon(verts,
                        false, 
                        color, 
                        yOffset);
        }

        private List<Vector3> GetVertsInCorrectOrder(SubSectorDef subSector)
        {
            List<Vector3> vertsUnsorted = new List<Vector3>();
            List<Vector3> vertsSorted = new List<Vector3>();


            // Get all the vertices for this subSector.
            for (uint i = 0; i < subSector.SegCount; i++)
            {
                SegDef curSeg = _Map.GetSegDef(subSector.FirstSegID + i);

                Vector3 curSegStart = _Map.GetVertex(curSeg.StartVertexID);
                Vector3 curSegEnd = _Map.GetVertex(curSeg.EndVertexID);

                vertsUnsorted.Add(curSegStart);
                //vertsUnsorted.Add(curSegEnd);
            }


            AddVertexPair(vertsSorted, vertsUnsorted[0], vertsUnsorted[1]);
            //vertsSorted.Add(vertsUnsorted[0]);
            Vector3 lastVert = vertsUnsorted[0];
            RemoveVertexPair(vertsSorted, 0);

            while (vertsUnsorted.Count > 0)
            {
                int indexOfClosest = FindClosestVertTo(lastVert, vertsUnsorted);
                Debug.Log("CLOSEST: " + indexOfClosest);
                AddVertexPair(vertsSorted, vertsUnsorted[indexOfClosest], vertsUnsorted[indexOfClosest + 1]);
                //vertsSorted.Add(vertsUnsorted[indexOfClosest]);
                lastVert = vertsUnsorted[indexOfClosest];
                //vertsUnsorted.RemoveAt(indexOfClosest);
                RemoveVertexPair(vertsUnsorted, indexOfClosest);
            }

            return vertsSorted;
        }        
        
        private void AddVertexPair(List<Vector3>list, Vector3 first, Vector3 second)
        {
            list.Add(first);
            list.Add(second);
        }
        private void RemoveVertexPair(List<Vector3> list, int firstIndex)
        {
            list.RemoveAt(firstIndex + 1);
            list.RemoveAt(firstIndex);
        }

        private int FindClosestVertTo(Vector3 lastVert, List<Vector3> list)
        {
            float minDist = float.MaxValue;
            int indexOfNearest = -1;
               
            for (int i = 0; i < list.Count; i++)
            {
                float distance = Vector3.Distance(lastVert, list[i]);
                if (distance < minDist)
                {
                    minDist = distance;
                    indexOfNearest = i;
                }                    
            }

            return indexOfNearest;
        }

        */


        // PRIMITVE DRAWING FUNCTIONS
        // ========================================================================================================================================================================================================

        public void ClearAll()
        {
            ClearAllWalls();
            ClearAllThings();
        }

        public void ClearAllWalls()
        {
            _LineRendererPool.ReturnAllToPool();
        }

        public void ClearAllThings()
        {
            _PlayerObject.SetActive(false);
        }

        public void DrawPlayer()
        {
            _PlayerObject.transform.position = _Map.GetPlayerSpawn(0).Position;
            _PlayerObject.SetActive(true);
        }

        // *****************************************************************
        // TODO: REMOVE DRAWLINE RETURNING A LINE RENDERER!!!
        // *****************************************************************

        public LineRenderer DrawLine(Vector3 start, Vector3 end, Color color, float yOffset = 0f)
        {
            start.y += yOffset;
            end.y += yOffset;


            LineRenderer line = _LineRendererPool.GetLineRenderer();

            line.endColor = color;
            line.startColor = color;

            line.SetPositions(new Vector3[] { start,
                                              end });

            return line;
        }

        public void DrawBox(Vector3 bottomLeft, Vector3 topRight, Color color, float yOffset = 0f)
        {
            bottomLeft.y += yOffset;
            topRight.y += yOffset;


            LineRenderer line = _LineRendererPool.GetLineRenderer();
            line.positionCount = 4; // 4 points because we need 4 line segments.


            line.endColor = color;
            line.startColor = color;

            line.SetPosition(0, bottomLeft);
            line.SetPosition(1, new Vector3(bottomLeft.x, yOffset, topRight.z));
            line.SetPosition(2, topRight);
            line.SetPosition(3, new Vector3(topRight.x, yOffset, bottomLeft.z));
            line.loop = true;
        }

        public void DrawPolygon(List<Vector3> vertices, bool loop, Color color, float yOffset = 0f)
        {
            if (vertices == null)
                throw new ArgumentNullException("The passed in vertices list is null!");
            if (vertices != null && vertices.Count == 0)
                throw new ArgumentException("The passed in vertices list is empty!");

            
            LineRenderer line = _LineRendererPool.GetLineRenderer();
            line.positionCount = vertices.Count; // 4 points because we need 4 line segments.


            line.endColor = color;
            line.startColor = color;
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 vert = vertices[i];
                vert.y += yOffset;

                line.SetPosition(i, vert);                
            }


            // Whether or not the last vertex should connect back to the first one.
            line.loop = loop;
        }
    }
}
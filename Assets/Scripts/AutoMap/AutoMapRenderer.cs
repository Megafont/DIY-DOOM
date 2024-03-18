using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DIY_DOOM.Maps;
using Unity.VisualScripting;
using UnityEngine.UI;
using Unity.Properties;


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

                Vector2 start = _Map.GetVertex(lineDef.StartVertexID);
                Vector2 end = _Map.GetVertex(lineDef.EndVertexID);

                start = MapUtils.ScaleAndAdjustRawDoomPoint(start, _Map.AutoMapScaleFactor);
                end = MapUtils.ScaleAndAdjustRawDoomPoint(end,_Map.AutoMapScaleFactor);

                DrawLine(MapUtils.Point2dTo3dXZ(start),
                         MapUtils.Point2dTo3dXZ(end),
                         color,
                         yOffset);
            }
        }

        public void DrawBinaryspacePartition(NodeDef node)
        {
            
            //  RIGHT BOX / SPACE PARTITION
            Vector2 rightBox_BottomLeft = MapUtils.ScaleAndAdjustRawDoomPoint(node.RightBox_BottomLeft, _Map.AutoMapScaleFactor);
            Vector2 rightBox_TopRight = MapUtils.ScaleAndAdjustRawDoomPoint(node.RightBox_TopRight, _Map.AutoMapScaleFactor);


            // LEFT BOX / SPACE PARTITION
            Vector2 leftBox_BottomLeft = MapUtils.ScaleAndAdjustRawDoomPoint(node.LeftBox_BottomLeft, _Map.AutoMapScaleFactor);
            Vector2 leftBox_TopRight = MapUtils.ScaleAndAdjustRawDoomPoint(node.LeftBox_TopRight, _Map.AutoMapScaleFactor);


            // Render the boxes.
            DrawBox(MapUtils.Point2dTo3dXZ(rightBox_BottomLeft),
                    MapUtils.Point2dTo3dXZ(rightBox_TopRight), 
                    Color.green,
                    LINE_DEPTH_INCREMENT * 1f);
            DrawBox(MapUtils.Point2dTo3dXZ(leftBox_BottomLeft),
                    MapUtils.Point2dTo3dXZ(leftBox_TopRight), 
                    Color.red,
                    LINE_DEPTH_INCREMENT * 2f);



            // THE SEPARATOR LINE THAT THE SPACE PARTITION IS BASED ON
            Vector2 partitionStart = MapUtils.ScaleAndAdjustRawDoomPoint(node.PartitionStart, _Map.AutoMapScaleFactor);
            Vector2 partitionEnd = MapUtils.ScaleAndAdjustRawDoomPoint(node.PartitionStart + node.DeltaToPartitionEnd, _Map.AutoMapScaleFactor);

            // Render the line.
            DrawLine(MapUtils.Point2dTo3dXZ(partitionStart), 
                     MapUtils.Point2dTo3dXZ(partitionEnd), 
                     Color.blue,
                     LINE_DEPTH_INCREMENT * 3f);
        }

        /// <summary>
        /// This function draws a subSector's outline.
        /// </summary>
        /// <param name="subSector">The subsector whose outline is to be drawn.</param>
        /// <param name="color"></param>
        /// <param name="yOffset"></param>
        public void DrawSubSector_Original(SubSectorDef subSector, Color color, float yOffset = 0f)
        {
            List<Vector3> vertices = new List<Vector3>();

            SegDef curSeg, nextSeg;
            Vector3 curStart = Vector3.zero;
            Vector3 curEnd = Vector3.zero;
            for (uint i = 0; i < subSector.SegCount; i++)
            {                
                curSeg = _Map.GetSegDef(subSector.FirstSegID + i);

                curStart = MapUtils.Point2dTo3dXZ(MapUtils.ScaleAndAdjustRawDoomPoint(_Map.GetVertex(curSeg.StartVertexID), _Map.AutoMapScaleFactor));
                curEnd = MapUtils.Point2dTo3dXZ(MapUtils.ScaleAndAdjustRawDoomPoint(_Map.GetVertex(curSeg.EndVertexID), _Map.AutoMapScaleFactor));

                LineRenderer r = DrawLine(curStart, curEnd, color, LINE_DEPTH_INCREMENT * 4f);

                vertices.Add(curStart);
                vertices.Add(curEnd);
            }


            
        }


        // *****************************************************************
        // REMOVE DRAWLINE RETURNING A LINE RENDERER!!!
        // *****************************************************************


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

                Vector3 curSegStart = MapUtils.Point2dTo3dXZ(MapUtils.ScaleAndAdjustRawDoomPoint(_Map.GetVertex(curSeg.StartVertexID), _Map.AutoMapScaleFactor));
                Vector3 curSegEnd = MapUtils.Point2dTo3dXZ(MapUtils.ScaleAndAdjustRawDoomPoint(_Map.GetVertex(curSeg.EndVertexID), _Map.AutoMapScaleFactor));

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
            Vector2 position = MapUtils.ScaleAndAdjustRawDoomPoint(_Map.Player1Spawn.Position, _Map.AutoMapScaleFactor);

            _PlayerObject.transform.position = MapUtils.Point2dTo3dXZ(position);
            _PlayerObject.SetActive(true);
        }

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
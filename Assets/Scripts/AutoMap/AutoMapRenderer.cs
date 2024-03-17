using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DIY_DOOM.Maps;
using Unity.VisualScripting;


namespace DIY_DOOM.AutoMap
{
    public class AutoMapRenderer : MonoBehaviour
    {
        private const float LINE_DEPTH_INCREMENT = 0.1f;


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

        public void DrawMap(Map map)
        {
            _Map = map;

            ClearAll();

            DrawWalls();
            DrawPlayer();
            //DrawBinaryspacePartition(_Map.GetNodeDef(_Map.NodesCount - 1));
        }

        public void DrawWalls()
        {
            // Clear all current lines.
            _LineRendererPool.ReturnAllToPool();


            // Draw the _Map.
            for (int i = 0; i < _Map.LineDefsCount - 1; i++)
            {
                LineDef lineDef = _Map.GetLineDef(i);

                Vector2 start = _Map.GetVertex(lineDef.StartVertex);
                Vector2 end = _Map.GetVertex(lineDef.EndVertex);

                start = MapUtils.ScaleAndAdjustRawDoomPoint(start, _Map.AutoMapScaleFactor);
                end = MapUtils.ScaleAndAdjustRawDoomPoint(end,_Map.AutoMapScaleFactor);

                DrawLine(MapUtils.Point2dTo3dXZ(start),
                         MapUtils.Point2dTo3dXZ(end),
                         Color.white);
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

        public void DrawLine(Vector3 start, Vector3 end, Color color, float yOffset = 0f)
        {
            start.y = yOffset;
            end.y = yOffset;


            LineRenderer line = _LineRendererPool.GetLineRenderer();

            line.endColor = color;
            line.startColor = color;

            line.SetPositions(new Vector3[] { start,
                                              end });
        }

        public void DrawBox(Vector3 bottomLeft, Vector3 topRight, Color color, float yOffset = 0f)
        {
            bottomLeft.y = yOffset;
            topRight.y = yOffset;


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

    }
}
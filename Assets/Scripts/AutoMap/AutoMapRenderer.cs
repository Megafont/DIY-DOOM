using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DIY_DOOM.Maps;


namespace DIY_DOOM.AutoMap
{
    public class AutoMapRenderer : MonoBehaviour
    {
        [SerializeField]
        private LineRenderer _LineRendererPrefab;


        private LineRendererPool _LineRendererPool;



        private void Awake()
        {
            GameObject lineRenderers = new GameObject("Line Renderers");
            lineRenderers.transform.SetParent(transform);

            GameObject active = new GameObject("Active");
            active.transform.SetParent(lineRenderers.transform);
            GameObject inactive = new GameObject("Inactive");
            inactive.transform.SetParent(lineRenderers.transform);

            _LineRendererPool = new LineRendererPool(_LineRendererPrefab.gameObject, active.transform, inactive.transform);
        }

        public void DrawMap(Map map)
        {
            // Clear all current lines.
            _LineRendererPool.ReturnAllToPool();

            // Draw the map.
            for (int i = 0; i < map.LineDefsCount - 1; i++)
            {
                LineDef lineDef = map.GetLineDef(i);

                Vector2 start = map.GetVertex(lineDef.StartVertex);
                Vector2 end = map.GetVertex(lineDef.EndVertex);

                start /= map.AutoMapScaleFactor;
                end /= map.AutoMapScaleFactor;

                DrawLine(MapUtils.Point2dTo3dXZ(start), 
                         MapUtils.Point2dTo3dXZ(end));
            }
        }

        public void DrawLine(Vector3 start, Vector3 end)
        {
            LineRenderer line = _LineRendererPool.GetLineRenderer();

            line.SetPositions(new Vector3[] { start, 
                                              end });
        }

        public void ClearAllLines()
        {
            _LineRendererPool.ReturnAllToPool();
        }
    }
}
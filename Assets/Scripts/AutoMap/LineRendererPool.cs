using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Pool;

namespace DIY_DOOM.AutoMap
{
    public class LineRendererPool
    {
        private ObjectPool<LineRenderer> _Pool;

        private LineRenderer _LineRendererPrefab;

        private Transform _ActiveLineRenderersParent;
        private Transform _InactiveLineRenderersParent;



        public LineRendererPool(GameObject lineRendererPrefab, Transform activeLineRenderersParent, Transform inactiveLineRenderersParent)
        {
            _ActiveLineRenderersParent = activeLineRenderersParent;
            _InactiveLineRenderersParent = inactiveLineRenderersParent;

            _LineRendererPrefab = lineRendererPrefab.GetComponent<LineRenderer>();

            _Pool = new ObjectPool<LineRenderer>(CreateLineRenderer, OnTakeFromPool, OnReturnToPool, OnDestroyLineRenderer);
        }

        public LineRenderer GetLineRenderer()
        {
            LineRenderer lineRenderer = _Pool.Get();
            lineRenderer.gameObject.SetActive(true);

            return lineRenderer;
        }

        public void ReturnToPool(LineRenderer lineRenderer)
        {
            // If this line has more than one line segement, reset it to just one segment (2 points).
            lineRenderer.positionCount = 2;

            // Make sure loop is off, too.
            lineRenderer.loop = false;


            _Pool.Release(lineRenderer);
        }

        public void ReturnAllToPool()
        {
            for (int i = _ActiveLineRenderersParent.childCount - 1; i >= 0; i--)
            {
                ReturnToPool(_ActiveLineRenderersParent.GetChild(i).GetComponent<LineRenderer>());
            }
        }


        // Pool event handlers
        // ====================================================================================================

        private LineRenderer CreateLineRenderer()
        {
            return Object.Instantiate(_LineRendererPrefab, _ActiveLineRenderersParent);
        }

        private void OnTakeFromPool(LineRenderer lineRenderer)
        {
            lineRenderer.gameObject.SetActive(true);
        }

        private void OnReturnToPool(LineRenderer lineRenderer)
        {
            lineRenderer.gameObject.SetActive(false);

            lineRenderer.transform.SetParent(_InactiveLineRenderersParent);
        }

        private void OnDestroyLineRenderer(LineRenderer lineRenderer)
        {
            Object.Destroy(lineRenderer.gameObject);
        }



        public int CountActive { get { return _Pool.CountActive; } }
        public int CountAll { get { return _Pool.CountAll; } }
        public int CountInactive { get { return _Pool.CountInactive; } }
    }
}
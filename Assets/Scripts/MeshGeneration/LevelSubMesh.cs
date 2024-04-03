using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace DIY_DOOM.MeshGeneration
{
    public class LevelSubMesh : MonoBehaviour
    {
        private MeshFilter _MeshFilter;
        private MeshRenderer _MeshRenderer;



        private void Awake()
        {
            _MeshFilter = GetComponent<MeshFilter>();
            _MeshRenderer = GetComponent<MeshRenderer>();
        }



        public MeshFilter MeshFilter { get { return _MeshFilter; } }
        public MeshRenderer MeshRenderer { get { return _MeshRenderer; } }
    }
}
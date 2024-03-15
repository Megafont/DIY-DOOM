using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

namespace DIY_DOOM.Maps
{
    public class Map
    {
        private string _Name;
        private List<Vector2> _Vertices;
        private List<LineDef> _LineDefs;

        private Vector2 _MinExtents;
        private Vector2 _MaxExtents;
        private float _AutoMapScaleFactor;



        public Map(string name)
        {
            _Name = name;

            _Vertices = new List<Vector2>();
            _LineDefs = new List<LineDef>();

            _MinExtents = new Vector2(float.MaxValue, float.MaxValue);
            _MaxExtents = new Vector2(float.MinValue, float.MinValue);

            _AutoMapScaleFactor = 15f;
        }

        public void AddVertex(Vector2 vertex)
        {
            _Vertices.Add(vertex);

            UpdateExtents(vertex);
        }

        public void AddLineDef(LineDef lineDef)
        {
            _LineDefs.Add(lineDef);
        }

        private void UpdateExtents(Vector2 vertex)
        {
            if (vertex.x < _MinExtents.x)
                _MinExtents.x = vertex.x;
            else if (vertex.x > _MaxExtents.x)
                _MaxExtents.x = vertex.x;

            if (vertex.y < _MinExtents.y)
                _MinExtents.y = vertex.y;
            else if (vertex.y > _MaxExtents.y)
                _MaxExtents.y = vertex.y;
        }

        public Vector2 GetVertex(int index)
        {
            return _Vertices[index];
        }

        public LineDef GetLineDef(int index)
        {
            return _LineDefs[index];
        }



        public int VerticesCount { get { return _Vertices.Count; } }        
        public int LineDefsCount { get { return _LineDefs.Count; } }

        public float AutoMapScaleFactor { get { return _AutoMapScaleFactor; } }
        public Vector2 MinExtents { get { return _MinExtents; } }
        public Vector2 MaxExtents { get { return _MaxExtents; } }
        public string Name { get { return _Name; } }

    }
}
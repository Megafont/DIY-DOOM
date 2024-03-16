using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace DIY_DOOM.Maps
{
    public class Map
    {
        private string _Name;
        
        private List<Vector2> _VertexDefs;
        private List<LineDef> _LineDefs;
        private List<ThingDef> _ThingsDefs;
        private List<NodeDef> _NodeDefs;

        private Vector2 _MinExtents;
        private Vector2 _MaxExtents;
        private float _AutoMapScaleFactor;



        public Map(string name)
        {
            _Name = name;

            _VertexDefs = new List<Vector2>();
            _LineDefs = new List<LineDef>();
            _ThingsDefs = new List<ThingDef>();
            _NodeDefs = new List<NodeDef>();

            _MinExtents = new Vector2(float.MaxValue, float.MaxValue);
            _MaxExtents = new Vector2(float.MinValue, float.MinValue);

            _AutoMapScaleFactor = 15f;
        }

        public void AddVertex(Vector2 vertex)
        {
            _VertexDefs.Add(vertex);

            UpdateExtents(vertex);
        }

        public void AddLineDef(LineDef lineDef)
        {
            _LineDefs.Add(lineDef);
        }

        public void AddThing(ThingDef thing)
        {
            if (thing.Type == 1)
                Player1Spawn = thing;

            _ThingsDefs.Add(thing);
        }

        public void AddNode(NodeDef node)
        {
            _NodeDefs.Add(node);
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
            return _VertexDefs[index];
        }

        public LineDef GetLineDef(int index)
        {
            return _LineDefs[index];
        }

        public ThingDef GetThing(int index)
        {            
            return _ThingsDefs[index];
        }

        public NodeDef GetNodeDef(int index)
        {
            return _NodeDefs[index];
        }



        public int VerticesCount { get { return _VertexDefs.Count; } }        
        public int LineDefsCount { get { return _LineDefs.Count; } }
        public int ThingsCount { get { return _ThingsDefs.Count; } }
        public int NodesCount { get { return _NodeDefs.Count; } }

        public float AutoMapScaleFactor { get { return _AutoMapScaleFactor; } }
        public Vector2 MinExtents { get { return _MinExtents; } }
        public Vector2 MaxExtents { get { return _MaxExtents; } }
        public string Name { get { return _Name; } }

        public ThingDef Player1Spawn {get; private set; }

    }
}
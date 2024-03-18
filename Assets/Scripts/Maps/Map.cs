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
        private List<SubSectorDef> _SubSectorDefs;
        private List<SegDef> _SegDefs;

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
            _SubSectorDefs = new List<SubSectorDef>();
            _SegDefs = new List<SegDef>();

            _MinExtents = new Vector2(float.MaxValue, float.MaxValue);
            _MaxExtents = new Vector2(float.MinValue, float.MinValue);

            // This constant comes from DOOM wiki: https://doom.fandom.com/wiki/Map_unit
            // The guy that made the repo linked in the readme file in the root folder of this project using 15f.   
            _AutoMapScaleFactor = 32f; // 15f;
        }

        public void AddVertexDef(Vector2 vertex)
        {
            _VertexDefs.Add(vertex);

            UpdateExtents(vertex);
        }

        public void AddLineDef(LineDef lineDef)
        {
            _LineDefs.Add(lineDef);
        }

        public void AddThingDef(ThingDef thing)
        {
            if (thing.Type == 1)
                Player1Spawn = thing;

            _ThingsDefs.Add(thing);
        }

        public void AddNodeDef(NodeDef node)
        {
            _NodeDefs.Add(node);
        }

        public void AddSubSectorDef(SubSectorDef subSector)
        {
            _SubSectorDefs.Add(subSector);
        }

        public void AddSegDef(SegDef seg)
        {
            _SegDefs.Add(seg);
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

        public Vector2 GetVertex(uint index)
        {
            return _VertexDefs[(int) index];
        }

        public LineDef GetLineDef(uint index)
        {
            return _LineDefs[(int) index];
        }

        public ThingDef GetThingDef(uint index)
        {            
            return _ThingsDefs[(int) index];
        }

        public NodeDef GetNodeDef(uint index)
        {
            return _NodeDefs[(int) index];
        }

        public SubSectorDef GetSubSectorDef(uint index)
        {
            return _SubSectorDefs[(int) index];
        }

        public SegDef GetSegDef(uint index)
        {
            return _SegDefs[(int) index];
        }



        public uint VerticesCount { get { return (uint) _VertexDefs.Count; } }        
        public uint LineDefsCount { get { return (uint) _LineDefs.Count; } }
        public uint ThingsCount { get { return (uint) _ThingsDefs.Count; } }
        public uint NodesCount { get { return (uint) _NodeDefs.Count; } }
        public uint SubSectorsCount { get { return (uint) _SubSectorDefs.Count; } }
        public uint SegsCount { get { return (uint) _SegDefs.Count; } }

        public float AutoMapScaleFactor { get { return _AutoMapScaleFactor; } }
        public Vector2 MinExtents { get { return _MinExtents; } }
        public Vector2 MaxExtents { get { return _MaxExtents; } }
        public string Name { get { return _Name; } }

        public ThingDef Player1Spawn {get; private set; }

    }
}
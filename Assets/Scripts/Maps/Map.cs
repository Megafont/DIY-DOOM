using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DIY_DOOM.WADs.Data.Maps;


namespace DIY_DOOM.Maps
{
    public class Map
    {
        private string _Name;

        private bool _CenterMapOnOrigin;
        private bool _IsFullyLoaded;
        private bool _ScaleAndAjustVertices;

        private List<Vector3> _VertexDefs;
        private List<LineDef> _LineDefs;
        private List<ThingDef> _ThingsDefs;
        private List<NodeDef> _NodeDefs;
        private List<SectorDef> _SectorDefs;
        private List<SubSectorDef> _SubSectorDefs;
        private List<SegDef> _SegDefs;
        private List<SideDef> _SideDefs;

        private Vector3 _MinExtents;
        private Vector3 _MaxExtents;
        private Vector3 _MapSize;


        private ThingDef[] _PlayerSpawns;

        // This controls the size of the map. Note that the scaling uses division rather than multiplication.
        // The default value is 32, which makes the map 32x smaller than it would be if we pretend one DOOM unit is equal to one Unity unit.
        // NOTE: In DOOM, one grid cell is 64 units. The grid lines mark every 16 grid cells (1024 DOOM units).
        //       Lastly, a pixel is 16 DOOM units, so four of them is 64 units (one grid cell).
        private float _ScaleFactor; 

        private int _ActivePaletteIndex = 0;
        


        public Map(string name, bool scaleAndAjustVertices = true, bool centerMapOnOrigin = true)
        {
            _PlayerSpawns = new ThingDef[4];

            _Name = name;
            _ScaleAndAjustVertices = scaleAndAjustVertices;
            _CenterMapOnOrigin = centerMapOnOrigin;

            _VertexDefs = new List<Vector3>();
            _LineDefs = new List<LineDef>();
            _ThingsDefs = new List<ThingDef>();
            _NodeDefs = new List<NodeDef>();
            _SectorDefs = new List<SectorDef>();
            _SubSectorDefs = new List<SubSectorDef>();
            _SegDefs = new List<SegDef>();
            _SideDefs = new List<SideDef>();

            _MinExtents = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            _MaxExtents = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            // This constant comes from DOOM wiki: https://doom.fandom.com/wiki/Map_unit
            // The guy that made the repo linked in the readme file in the root folder of this project using 15f.   
            _ScaleFactor = 32f; // 15f;
        }

        public void AddVertexDef(Vector3 vertex)
        {
            if (_ScaleAndAjustVertices)
            {
                vertex = MapUtils.ScaleAndAdjustRawDoomPoint(vertex, _ScaleFactor);
            }


            _VertexDefs.Add(vertex);

            UpdateExtents(vertex);
        }

        public void AddLineDef(LineDef lineDef)
        {
            _LineDefs.Add(lineDef);
        }

        public void AddThingDef(ThingDef thing)
        {
            if (_ScaleAndAjustVertices)
            {
                thing.Position = MapUtils.ScaleAndAdjustRawDoomPoint(thing.Position, _ScaleFactor);
            }

            if (thing.Type >= 1 && thing.Type <= 4)
            {
                _PlayerSpawns[thing.Type - 1] = thing;
            }


            _ThingsDefs.Add(thing);
        }

        public void AddNodeDef(NodeDef node)
        {
            if (_ScaleAndAjustVertices)
            {
                node.PartitionStart = MapUtils.ScaleAndAdjustRawDoomPoint(node.PartitionStart, _ScaleFactor);
                node.DeltaToPartitionEnd = MapUtils.ScaleAndAdjustRawDoomPoint(node.DeltaToPartitionEnd, _ScaleFactor);
                node.RightBox_BottomLeft = MapUtils.ScaleAndAdjustRawDoomPoint(node.RightBox_BottomLeft, _ScaleFactor);
                node.RightBox_TopRight = MapUtils.ScaleAndAdjustRawDoomPoint(node.RightBox_TopRight, _ScaleFactor);
                node.LeftBox_BottomLeft = MapUtils.ScaleAndAdjustRawDoomPoint(node.LeftBox_BottomLeft, _ScaleFactor);
                node.LeftBox_TopRight = MapUtils.ScaleAndAdjustRawDoomPoint(node.LeftBox_TopRight, _ScaleFactor);
            }


            _NodeDefs.Add(node);
        }

        public void AddSectorDef(SectorDef sector)
        {
            sector.FloorHeight = MapUtils.ScaleSingleValue(sector.FloorHeight, _ScaleFactor);
            sector.CeilingHeight = MapUtils.ScaleSingleValue(sector.CeilingHeight, _ScaleFactor);

            _SectorDefs.Add(sector);
        }

        public void AddSubSectorDef(SubSectorDef subSector)
        {
            _SubSectorDefs.Add(subSector);
        }

        public void AddSegDef(SegDef seg)
        {
            _SegDefs.Add(seg);
        }   

        public void AddSideDef(SideDef sideDef)
        {
            _SideDefs.Add(sideDef);
        }

        private void UpdateExtents(Vector3 vertex)
        {
            if (vertex.x < _MinExtents.x)
                _MinExtents.x = vertex.x;
            else if (vertex.x > _MaxExtents.x)
                _MaxExtents.x = vertex.x;

            if (vertex.y < _MinExtents.y)
                _MinExtents.y = vertex.y;
            else if (vertex.y > _MaxExtents.y)
                _MaxExtents.y = vertex.y;

            if (vertex.z < _MinExtents.z)
                _MinExtents.z = vertex.z;
            else if (vertex.z > _MaxExtents.z)
                _MaxExtents.z = vertex.z;
        }

        public Vector3 GetVertex(uint index)
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

        public SectorDef GetSectorDef(uint index)
        {
            return _SectorDefs[(int) index];
        }

        public SubSectorDef GetSubSectorDef(uint index)
        {
            return _SubSectorDefs[(int) index];
        }

        public SegDef GetSegDef(uint index)
        {
            return _SegDefs[(int) index];
        }

        public SideDef GetSideDef(uint index)
        {
            return _SideDefs[(int) index];
        }

        /// <summary>
        /// This function gets player spawn data for player 1, 2, 3, or 4.
        /// </summary>
        /// <param name="index">The index (0-3) of the player whose spawn data is to be retreived.</param>
        /// <returns>The spawn data for the specified player.</returns>
        public ThingDef GetPlayerSpawn(int index)
        {
            return _PlayerSpawns[index];
        }

        public void DoFinalProcessing()
        {
            if (_CenterMapOnOrigin)
                CenterMapOnOrigin();

            _IsFullyLoaded = true;
        }

        public void CenterMapOnOrigin()
        {
            _MapSize = _MaxExtents - _MinExtents;

            // Find the map's center point. This tells us the map's current offset from the origin.
            Vector3 currentOffset = Vector3.Lerp(_MinExtents, _MaxExtents, 0.5f);


            // Apply the offset to all vertices.
            for (int i = 0; i < _VertexDefs.Count; i++)
            {
                _VertexDefs[i] = _VertexDefs[i] - currentOffset;

            } // end for VertexDefs


            // Apply the offset to all things' positions.
            for (int i = 0; i < _ThingsDefs.Count; i++)
            {
                ThingDef def = _ThingsDefs[i];

                def.Position = _ThingsDefs[i].Position - currentOffset;

                _ThingsDefs[i] = def;

                // If this is one of the player spawn datas, then update it.
                if (def.Type >= 1 && def.Type <= 4)
                {
                    _PlayerSpawns[def.Type - 1] = def;
                }

            } // end for ThingDefs


            // Apply the offset to all nodes' position values.
            for (int i = 0; i < _NodeDefs.Count; i++)
            {
                NodeDef def = _NodeDefs[i];
                def.PartitionStart = _NodeDefs[i].PartitionStart - currentOffset;
                def.DeltaToPartitionEnd = _NodeDefs[i].DeltaToPartitionEnd - currentOffset;
                def.RightBox_BottomLeft = _NodeDefs[i].RightBox_BottomLeft - currentOffset;
                def.RightBox_TopRight = _NodeDefs[i].RightBox_TopRight - currentOffset;
                def.LeftBox_BottomLeft = _NodeDefs[i].LeftBox_BottomLeft - currentOffset;
                def.LeftBox_TopRight = _NodeDefs[i].LeftBox_TopRight - currentOffset;

                _NodeDefs[i] = def;

            } // end for NodeDefs
        }



        public bool IsFullyLoaded { get { return _IsFullyLoaded;} }

        public uint VerticesCount { get { return (uint) _VertexDefs.Count; } }        
        public uint LineDefsCount { get { return (uint) _LineDefs.Count; } }
        public uint ThingsCount { get { return (uint) _ThingsDefs.Count; } }
        public uint NodesCount { get { return (uint) _NodeDefs.Count; } }
        public uint SectorsCount { get { return (uint) _SectorDefs.Count; } }
        public uint SubSectorsCount { get { return (uint) _SubSectorDefs.Count; } }
        public uint SegsCount { get { return (uint) _SegDefs.Count; } }
        public uint SideDefsCount { get { return (uint) _SideDefs.Count; } }

        public int ActivePaletteIndex 
        { 
            get { return _ActivePaletteIndex; } 
            set { _ActivePaletteIndex = value; }
        }

        public float ScaleFactor { get { return _ScaleFactor; } }
        public Vector3 MapDimensions { get { return _MapSize; } }
        public Vector3 MinExtents { get { return _MinExtents; } }
        public Vector3 MaxExtents { get { return _MaxExtents; } }
        public string Name { get { return _Name; } }

    }
}
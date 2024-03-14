using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class Map
{
    private string _Name;
    private List<Vector2> _Vertices;
    private List<LineDef> _LineDefs;



    public Map(string name)
    {
        _Name = name;

        _Vertices = new List<Vector2>();
        _LineDefs = new List<LineDef>();
    }

    public void AddVertex(Vector2 vertex)
    {
        _Vertices.Add(vertex);
    }

    public void AddLineDef(LineDef lineDef)
    {
        _LineDefs.Add(lineDef);
    }



    public string Name { get { return _Name; } }
}

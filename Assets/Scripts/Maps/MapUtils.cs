using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace DIY_DOOM.Maps
{ 
    public static class MapUtils
    {
        public static Vector3 Point2dTo3dXZ(Vector2 point2D)
        {
            return new Vector3(point2D.x, 0f, point2D.y);
        }
    }
}


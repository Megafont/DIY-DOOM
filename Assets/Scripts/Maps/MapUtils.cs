using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace DIY_DOOM.Maps
{ 
    public static class MapUtils
    {
        public static Vector2 ScaleAndAdjustRawDoomPoint(Vector2 point, float scaleFactor)
        {
            point /= scaleFactor;

            return point;
        }

        public static Vector3 ScaleAndAdjustRawDoomPoint(Vector3 point, float scaleFactor)
        {
            point /= scaleFactor;

            return point;
        }

        public static Vector3 Point2dTo3dXZ(Vector2 point2D, float yOffset = 0f)
        {
            return new Vector3(point2D.x, yOffset, point2D.y);
        }
    }
}


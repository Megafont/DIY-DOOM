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

        public static float ScaleSingleValue(float value, float scaleFactor)
        {
            return value / scaleFactor;
        }

        public static Vector3 Point2dTo3dXZ(Vector2 point2D, float verticalPosition = 0f)
        {
            return new Vector3(point2D.x, verticalPosition, point2D.y);
        }

        public static Vector3 Point2dTo3dXZ(float x, float y, float verticalPosition = 0f)
        {
            return new Vector3(x, verticalPosition, y);
        }

        /// <summary>
        /// This function sets the y value to 0, so we have only the horizontal components of the vector left.
        /// </summary>
        /// <param name="point">The vector to flatten.</param>
        /// <returns>The flattened vector.</returns>
        public static Vector3 Point3dToFlattened3D(Vector3 point)
        {
            return new Vector3(point.x, 0, point.z);
        }
    }
}


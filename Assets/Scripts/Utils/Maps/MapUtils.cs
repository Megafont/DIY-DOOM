using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace DIY_DOOM.Utils.Maps
{ 
    public static class MapUtils
    {
        public static Vector2 ScaleAndAdjustRawDoomPoint(Vector2 point, float scaleFactor)
        {            
            return point / scaleFactor;
        }

        public static Vector3 ScaleAndAdjustRawDoomPoint(Vector3 point, float scaleFactor)
        {  
            return point / scaleFactor;
        }

        public static float ScaleSingleValue(float value, float scaleFactor)
        {
            return value / scaleFactor;
        }

        public static uint ScaleSingleValue(uint value, float scaleFactor)
        {
            float result = (float) value / scaleFactor;

            return (uint) result;
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
        /// <param name="verticalPosition">The y-position to set the vertex to.</param>
        /// <returns>The flattened vector.</returns>
        public static Vector3 Point3dToFlattened3d(Vector3 point, float verticalPosition = 0f)
        {
            return new Vector3(point.x, verticalPosition, point.z);
        }

        /// <summary>
        /// This function creates a Vector2 from the x and z coordinates of the passed in Vector3.
        /// </summary>
        /// <param name="v">The 3D vector to convert</param>
        /// <returns>The 2D vector</returns>
        public static Vector2 Point3dTo2d(Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }
    }
}


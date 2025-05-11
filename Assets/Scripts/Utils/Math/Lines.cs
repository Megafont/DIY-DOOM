using DIY_DOOM.Utils.Maps;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace DIY_DOOM
{
    public static class Lines
    {
        /// <summary>
        /// This function finds the intersection point of two lines.
        /// </summary>
        /// <param name="line1StartPoint">Line 1 starting point.</param>
        /// <param name="line1Direction">Line 1 direction vector.</param>
        /// <param name="line2StartPoint">Line 2 starting point.</param>
        /// <param name="line2Direction">Line 2 direction vector.</param>
        /// <param name="intersectionPoint">Returns the intersection point.</param>
        /// NOTE: When the lines do not intersect (are paralell or the same line), the intersection parameter will return (0,0,0) since there are no intersection points in that case (or every point is an intersection point if they are the same line).
        /// <remarks>
        /// Source: https://forum.unity.com/threads/line-intersection.17384/
        /// </remarks>
        /// <returns>True if the lines intersect, and the intersection point is returned via the <see cref="intersectionPoint"/> out parameter.</returns>
        public static bool GetIntersectionPoint_FromStartAndDirections(Vector3 line1StartPoint, Vector3 line1Direction, Vector3 line2StartPoint, Vector3 line2Direction, out Vector3 intersectionPoint)
        {
            Vector3 lineVec3 = line2StartPoint - line1StartPoint;
            Vector3 crossVec1and2 = Vector3.Cross(line1Direction, line2Direction);
            Vector3 crossVec3and2 = Vector3.Cross(lineVec3, line2Direction);

            float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

            // Is not coplanar, and not parallel
            if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
            {
                float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
                intersectionPoint = line1StartPoint + (line1Direction * s);
                return true;
            }
            else
            {
                intersectionPoint = Vector3.zero;
                return false;
            }
        }

        /// <summary>
        /// This function finds the intersection point of two lines.
        /// </summary>
        /// <param name="line1StartPoint">Line 1 starting point.</param>
        /// <param name="line1EndPoint">Line 1 end point.</param>
        /// <param name="line2StartPoint">Line 2 starting point.</param>
        /// <param name="line2endPoint">Line 2 end point.</param>
        /// <param name="intersectionPoint">Returns the intersection point.</param>
        /// NOTE: When the lines do not intersect (are paralell or the same line), the intersection parameter will return (0,0,0) since there are no intersection points in that case (or every point is an intersection point if they are the same line).
        /// <remarks>
        /// Source: https://forum.unity.com/threads/line-intersection.17384/
        /// </remarks>
        /// <returns>True if the lines intersect, and the intersection point is returned via the <see cref="intersectionPoint"/> out parameter.</returns>
        public static bool GetIntersectionPoint_FromStartAndEndPoints(Vector3 line1StartPoint, Vector3 line1EndPoint, Vector3 line2StartPoint, Vector3 line2EndPoint, out Vector3 intersectionPoint)
        {
            intersectionPoint = Vector3.zero;

            return GetIntersectionPoint_FromStartAndDirections(line1StartPoint,
                                                               line1EndPoint - line1StartPoint,
                                                               line2StartPoint,
                                                               line2EndPoint - line2StartPoint,
                                                               out intersectionPoint);
        }

        /// <summary>
        /// Calculates the intersection point of two line segments.
        /// </summary>
        /// <param name="line1_Start"></param>
        /// <param name="line1_End"></param>
        /// <param name="line2_Start"></param>
        /// <param name="line2_End"></param>
        /// <param name="intersectionPoint">This out parameter returns the intersection point if there is one.</param>
        /// <returns>
        /// True if the intersection point of the lines exists within both line segments. 
        /// NOTE: When the lines do not intersect (are paralell or the same line), the intersection parameter will return (0,0,0) since there are no intersection points in that case (or every point is an intersection point if they are the same line segment).
        /// </returns>
        public static bool GetLineSegmentIntersectionPoint(Vector3 line1_Start, Vector3 line1_End, Vector3 line2_Start, Vector3 line2_End, out Vector3 intersectionPoint, bool ignoreEndPoints = false)
        {
            intersectionPoint = Vector3.zero;


            // Find the intersection point of the two lines
            bool result = GetIntersectionPoint_FromStartAndEndPoints(line1_Start, line1_End, line2_Start, line2_End, out Vector3 intersection);
            if (!result)
                return false;


            float line1_MinX = Mathf.Min(line1_Start.x, line1_End.x);
            float line1_MaxX = Mathf.Max(line1_Start.x, line1_End.x);
            float line1_MinY = Mathf.Min(line1_Start.y, line1_End.y);
            float line1_MaxY = Mathf.Max(line1_Start.y, line1_End.y);

            float line2_MinX = Mathf.Min(line2_Start.x, line2_End.x);
            float line2_MaxX = Mathf.Max(line2_Start.x, line2_End.x);
            float line2_MinY = Mathf.Min(line2_Start.y, line2_End.y);
            float line2_MaxY = Mathf.Max(line2_Start.y, line2_End.y);


            bool isOnLineSegment1 = false;
            bool isOnLineSegment2 = false;


            // Is the intersection point inside both of the line segments?
            if (!ignoreEndPoints)
            {
                isOnLineSegment1 = (intersection.x >= line1_MinX && intersection.x <= line1_MaxX) && 
                                   (intersection.y >= line1_MinY && intersection.y <= line1_MaxY); 

                isOnLineSegment2 = (intersection.x >= line2_MinX && intersection.x <= line2_MaxX) && 
                                   (intersection.y >= line2_MinY && intersection.y <= line2_MaxY); 
            }
            else
            {
                // I had to add an extra condition at the start of each of the four tests here (the section before the || operator). Otherwise there is a bug where
                // the collision got ignored if one of the lines is a straight line on x or y axis. This is because in such a case,
                // the collision point's x position is guaranteed to be the same as that of the straight line. Thus we through it
                // out since the collision 
                //Debug.Log($"<color=brown>DEBUG: ({line1_MinX == line1_MaxX} && {intersection.x == line1_MinX}) || ({intersection.x > line1_MinX} && {intersection.x < line1_MaxX})</color>");
                //Debug.Log($"<color=brown>DEBUG: ({line1_MinY == line1_MaxY} && {intersection.y == line1_MinY}) || ({intersection.y > line1_MinY} && {intersection.y < line1_MaxY})</color>");
                //Debug.Log($"<color=brown>DEBUG: ({line2_MinX == line2_MaxX} && {intersection.x == line2_MinX}) || ({intersection.x > line2_MinX} && {intersection.x < line2_MaxX})</color>");
                //Debug.Log($"<color=brown>DEBUG: ({line2_MinY == line2_MaxY} && {intersection.y == line2_MinY}) || ({intersection.y > line2_MinY} && {intersection.y < line2_MaxY})</color>");
                isOnLineSegment1 = ((line1_MinX != line1_MaxX) || (intersection.x > line1_MinX && intersection.x < line1_MaxX)) &&
                                   ((line1_MinY != line1_MaxY) || (intersection.y > line1_MinY && intersection.y < line1_MaxY));
                isOnLineSegment2 = ((line2_MinX != line2_MaxX) || (intersection.x > line2_MinX && intersection.x < line2_MaxX)) &&
                                   ((line2_MinY != line2_MaxY) || (intersection.y > line2_MinY && intersection.y < line2_MaxY));
                //Debug.Log($"<color=brown>DEBUG: ({isOnLineSegment1}    {isOnLineSegment2})</color>");

            }


            intersectionPoint = intersection;

            if (isOnLineSegment1 && isOnLineSegment2)
            {
                return true;
            }
            else
            {
                // There two lines would intersect if they were infinitely long, but the intersection point does not fall within both line segments.
                // So return false.
                return false;
            }
        }


        /// <summary>
        /// Calculates the intersection point of two line segments.
        /// </summary>
        /// <param name="line1_Start"></param>
        /// <param name="line1_End"></param>
        /// <param name="line2_Start"></param>
        /// <param name="line2_End"></param>
        /// <param name="intersectionPoint">This out parameter returns the intersection point if there is one.</param>
        /// <returns>
        /// True if the intersection point of the lines exists within both line segments. 
        /// NOTE: When the lines do not intersect (are paralell or the same line), the intersection parameter will return (0,0,0) since there are no intersection points in that case (or every point is an intersection point if they are the same line segment).
        /// </returns>
        public static bool GetLineSegmentIntersectionPoint(Vector2 line1_Start, Vector2 line1_End, Vector2 line2_Start, Vector2 line2_End, out Vector2 intersectionPoint, bool ignoreEndPoints = false)
        {
            intersectionPoint = Vector3.zero;

            return GetLineSegmentIntersectionPoint(MapUtils.Point2dTo3dXZ(line1_Start),
                                                   MapUtils.Point2dTo3dXZ(line1_End),
                                                   MapUtils.Point2dTo3dXZ(line2_Start),
                                                   MapUtils.Point2dTo3dXZ(line2_End),
                                                   out Vector3 intersection,
                                                   ignoreEndPoints);
        }
    }
}


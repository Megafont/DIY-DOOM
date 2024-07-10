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


            float line1_minX = Mathf.Min(line1_Start.x, line1_End.x);
            float line1_maxX = Mathf.Max(line1_Start.x, line1_End.x);
            float line1_minY = Mathf.Min(line1_Start.y, line1_End.y);
            float line1_maxY = Mathf.Max(line1_Start.y, line1_End.y);

            float line2_minX = Mathf.Min(line2_Start.x, line2_End.x);
            float line2_maxX = Mathf.Max(line2_Start.x, line2_End.x);
            float line2_minY = Mathf.Min(line2_Start.y, line2_End.y);
            float line2_maxY = Mathf.Max(line2_Start.y, line2_End.y);

            // Is the intersection point inside both of the line segments?
            if (!ignoreEndPoints)
            {
                //Debug.Log($"1    {intersection.x >= line1_minX} && {intersection.x <= line1_maxX} && {intersection.y >= line1_minY} && {intersection.y <= line1_maxY}");
                //Debug.Log($"1    {intersection.x >= line2_minX} && {intersection.x <= line2_maxX} && {intersection.y >= line2_minY} && {intersection.y <= line2_maxY}");

                if ((intersection.x >= line1_minX && intersection.x <= line1_maxX && intersection.y >= line1_minY && intersection.y <= line1_maxY) && // Does the intersection point fall between the start and end points of line 1?
                    (intersection.x >= line2_minX && intersection.x <= line2_maxX && intersection.y >= line2_minY && intersection.y <= line2_maxY))   // Does the intersection point fall between the start and end points of line 2?
                {
                    intersectionPoint = intersection;
                    return true;
                }
            }
            else
            {
                //Debug.Log($"2    {intersection.x > line1_minX} && {intersection.x < line1_maxX} && {intersection.y > line1_minY} && {intersection.y < line1_maxY}");
                //Debug.Log($"2    {intersection.x > line2_minX} && {intersection.x < line2_maxX} && {intersection.y > line2_minY} && {intersection.y < line2_maxY}");

                if ((intersection.x > line1_minX && intersection.x < line1_maxX && intersection.y > line1_minY && intersection.y < line1_maxY) && // Does the intersection point fall between the start and end points of line 1?
                    (intersection.x > line2_minX && intersection.x < line2_maxX && intersection.y > line2_minY && intersection.y < line2_maxY))   // Does the intersection point fall between the start and end points of line 2?
                {
                    intersectionPoint = intersection;
                    return true;
                }
            }


            // There two lines would intersect if they were infintely long, but the intersection point does not fall within both line segments.
            // So return false.
            intersectionPoint = intersection;
            return false;
        }
    }
}

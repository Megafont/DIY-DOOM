using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DIY_DOOM
{
    public static class MathTests
    {
        public static void DoMathTests()
        {
            DoLineIntersectTests();
        }

        public static void DoLineIntersectTests()
        {
            Debug.Log("");
            Debug.Log("BEGIN LINE TESTS");
            Debug.Log(new string('-', 128));

            Vector3 intersectionPoint = Vector3.zero;

            Debug.Log($"Lines Intersect Test #01: {Lines.GetIntersectionPoint_FromStartAndEndPoints(new Vector3(-4, 0, -4), new Vector3(4, 0, 4), new Vector3(-4, 0, 4), new Vector3(4, 0, -4), out intersectionPoint)}    {intersectionPoint}");
            Debug.Log($"Lines Intersect Test #02: {Lines.GetIntersectionPoint_FromStartAndEndPoints(new Vector3(2, 0, 10), new Vector3(2, 0, -10), new Vector3(-10, 0, 6), new Vector3(10, 0, 6), out intersectionPoint)}    {intersectionPoint}");
            Debug.Log($"Lines Intersect Test #03: {Lines.GetIntersectionPoint_FromStartAndEndPoints(new Vector3(2, 0, 5), new Vector3(8, 0, 10), new Vector3(2, 0, 10), new Vector3(8, 0, 15), out intersectionPoint)}    {intersectionPoint}");
            Debug.Log($"Lines Intersect Test #04: {Lines.GetIntersectionPoint_FromStartAndEndPoints(new Vector3(-10, 0, 3), new Vector3(10, 0, -3), new Vector3(-10, 0, 3), new Vector3(10, 0, -3), out intersectionPoint)}    {intersectionPoint}");
            Debug.Log($"Lines Intersect Test #05: {Lines.GetIntersectionPoint_FromStartAndEndPoints(new Vector3(-10, 0, 3), new Vector3(10, 0, -3), new Vector3(10, 0, -3), new Vector3(-10, 0, 3), out intersectionPoint)}    {intersectionPoint}");

            Debug.Log(new string('-', 128));

            Debug.Log($"Line Segments Intersect Test #01: {Lines.GetLineSegmentIntersectionPoint(new Vector3(-4, 0, -4), new Vector3(4, 0, 4), new Vector3(-4, 0, 4), new Vector3(4, 0, -4), out intersectionPoint)}    {intersectionPoint}");
            Debug.Log($"Line Segments Intersect Test #02: {Lines.GetLineSegmentIntersectionPoint(new Vector3(2, 0, 10), new Vector3(2, 0, -10), new Vector3(-10, 0, 6), new Vector3(10, 0, 6), out intersectionPoint)}    {intersectionPoint}");
            Debug.Log($"Line Segments Intersect Test #03: {Lines.GetLineSegmentIntersectionPoint(new Vector3(2, 0, 5), new Vector3(8, 0, 10), new Vector3(2, 0, 10), new Vector3(8, 0, 15), out intersectionPoint)}    {intersectionPoint}");
            Debug.Log($"Line Segments Intersect Test #04: {Lines.GetLineSegmentIntersectionPoint(new Vector3(-10, 0, 3), new Vector3(10, 0, -3), new Vector3(-10, 0, 3), new Vector3(10, 0, -3), out intersectionPoint)}    {intersectionPoint}");
            Debug.Log($"Line Segments Intersect Test #05: {Lines.GetLineSegmentIntersectionPoint(new Vector3(-10, 0, 3), new Vector3(10, 0, -3), new Vector3(10, 0, -3), new Vector3(-10, 0, 3), out intersectionPoint)}    {intersectionPoint}");
            Debug.Log($"Line Segments Intersect Test #01: {Lines.GetLineSegmentIntersectionPoint(new Vector3(-6, 0, -6), new Vector3(-2, 0, -2), new Vector3(8, 0, -8), new Vector3(4, 0, -4), out intersectionPoint)}    {intersectionPoint}");

        }


    }
}

using DIY_DOOM.MeshGeneration.Triangulation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DIY_DOOM
{
    public static class MathTests
    {
        public static void DoMathTests()
        {
            DoWrapIndexTests();
            DoLineIntersectTests();
        }

        public static void DoWrapIndexTests()
        {
            Debug.Log("");
            Debug.Log("BEGIN WRAP INDEX TESTS");
            Debug.Log(new string('-', 128));
            Debug.Log($"Wrap Index Test #01: WrapIndex( 0, 10)={Triangulator_Polygon.WrapIndex(0, 10)}");
            Debug.Log($"Wrap Index Test #02: WrapIndex(-2, 10)={Triangulator_Polygon.WrapIndex(-2, 10)}");
            Debug.Log($"Wrap Index Test #03: WrapIndex(12, 10)={Triangulator_Polygon.WrapIndex(12, 10)}");
            Debug.Log($"Wrap Index Test #04: WrapIndex(10, 10)={Triangulator_Polygon.WrapIndex(10, 10)}");
            Debug.Log($"Wrap Index Test #05: WrapIndex(11, 10)={Triangulator_Polygon.WrapIndex(11, 10)}");
            Debug.Log($"Wrap Index Test #06: WrapIndex(-1, 10)={Triangulator_Polygon.WrapIndex(-1, 10)}");
            Debug.Log($"Wrap Index Test #07: WrapIndex( 5, 10)={Triangulator_Polygon.WrapIndex(5, 10)}");
            Debug.Log($"Wrap Index Test #08: WrapIndex(-5, 10)={Triangulator_Polygon.WrapIndex(-5, 10)}");
        }

        public static void DoLineIntersectTests()
        {
            Debug.Log("");
            Debug.Log("BEGIN LINE TESTS");
            Debug.Log(new string('-', 128));

            Vector3 intersectionPoint = Vector3.zero;

            Debug.Log($"Lines 2D Colinear Test #01 (-5, 0)-( 0, 0)-( 5, 0): {Triangulator_Polygon.AreColinear(new Vector2(-5, 0), new Vector2(0, 0), new Vector2(5, 0))}");
            Debug.Log($"Lines 2D Colinear Test #02 ( 5, 0)-( 0, 0)-(-5, 0): {Triangulator_Polygon.AreColinear(new Vector2( 5, 0), new Vector2(0, 0), new Vector2(-5, 0))}");
            Debug.Log($"Lines 2D Colinear Test #03 ( 0,-5)-( 0, 0)-( 0, 5): {Triangulator_Polygon.AreColinear(new Vector2(0, -5), new Vector2(0, 0), new Vector2(0, 5))}");
            Debug.Log($"Lines 2D Colinear Test #04 ( 0, 5)-( 0, 0)-( 0,-5): {Triangulator_Polygon.AreColinear(new Vector2(0, 5), new Vector2(0, 0), new Vector2(0, -5))}");
            Debug.Log($"Lines 2D Colinear Test #05 (-5,-5)-( 0, 0)-( 5, 5): {Triangulator_Polygon.AreColinear(new Vector2(-5, -5), new Vector2(0, 0), new Vector2(5, 5))}");
            Debug.Log($"Lines 2D Colinear Test #06 ( 5, 5)-( 0, 0)-(-5,-5): {Triangulator_Polygon.AreColinear(new Vector2(5, 5), new Vector2(0, 0), new Vector2(-5, -5))}");
            Debug.Log($"Lines 2D Colinear Test #07 (-5,-5)-( 0, 0)-( 0, 5): {Triangulator_Polygon.AreColinear(new Vector2(-5, -5), new Vector2(0, 0), new Vector2(0, 5))}");
            Debug.Log($"Lines 2D Colinear Test #08 ( 0, 5)-( 0, 0)-(-5,-5): {Triangulator_Polygon.AreColinear(new Vector2(-5, -5), new Vector2(0, 0), new Vector2(0, 5))}");
            
            Debug.Log($"Lines 3D Colinear Test #01 (-5, 0, 0)-( 0, 0, 0)-( 5, 0, 0): {Triangulator_Polygon.AreColinear(new Vector3(-5, 0, 0), new Vector3(0, 0, 0), new Vector3( 5, 0, 0))}");
            Debug.Log($"Lines 3D Colinear Test #02 ( 5, 0, 0)-( 0, 0, 0)-(-5, 0, 0): {Triangulator_Polygon.AreColinear(new Vector3( 5, 0, 0), new Vector3(0, 0, 0), new Vector3(-5, 0, 0))}");
            Debug.Log($"Lines 3D Colinear Test #03 ( 0,-5, 0)-( 0, 0, 0)-( 0, 5, 0): {Triangulator_Polygon.AreColinear(new Vector3( 0,-5, 0), new Vector3(0, 0, 0), new Vector3( 0, 5, 0))}");
            Debug.Log($"Lines 3D Colinear Test #04 ( 0, 5, 0)-( 0, 0, 0)-( 0,-5, 0): {Triangulator_Polygon.AreColinear(new Vector3( 0, 5, 0), new Vector3(0, 0, 0), new Vector3( 0,-5, 0))}");
            Debug.Log($"Lines 3D Colinear Test #05 ( 0 ,0,-5)-( 0, 0, 0)-( 0, 0, 5): {Triangulator_Polygon.AreColinear(new Vector3( 0, 0,-5), new Vector3(0, 0, 0), new Vector3( 0, 0, 5))}");
            Debug.Log($"Lines 3D Colinear Test #06 ( 0, 0, 5)-( 0, 0, 0)-( 0, 0,-5): {Triangulator_Polygon.AreColinear(new Vector3( 0, 0, 5), new Vector3(0, 0, 0), new Vector3( 0, 0,-5))}");
            Debug.Log($"Lines 3D Colinear Test #07 (-5,-5,-5)-( 0, 0, 0)-( 5, 5, 5): {Triangulator_Polygon.AreColinear(new Vector3(-5,-5,-5), new Vector3(0, 0, 0), new Vector3( 5, 5, 5))}");
            Debug.Log($"Lines 3D Colinear Test #08 ( 5, 5, 5)-( 0, 0, 0)-(-5,-5,-5): {Triangulator_Polygon.AreColinear(new Vector3( 5, 5, 5), new Vector3(0, 0, 0), new Vector3(-5,-5,-5))}");
            Debug.Log($"Lines 3D Colinear Test #09 (-5, 0,-5)-( 0, 0, 0)-( 0, 0, 5): {Triangulator_Polygon.AreColinear(new Vector3(-5, 0,-5), new Vector3(0, 0, 0), new Vector3( 0, 0, 5))}");
            Debug.Log($"Lines 3D Colinear Test #10 ( 0, 0, 5)-( 0, 0, 0)-(-5, 0,-5): {Triangulator_Polygon.AreColinear(new Vector3( 0, 0, 5), new Vector3(0, 0, 0), new Vector3(-5, 0,-5))}");
            
            Debug.Log(new string('-', 128));
            
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
            Debug.Log($"Line Segments Intersect Test #06: {Lines.GetLineSegmentIntersectionPoint(new Vector3(-6, 0, -6), new Vector3(-2, 0, -2), new Vector3(8, 0, -8), new Vector3(4, 0, -4), out intersectionPoint)}    {intersectionPoint}");
            Debug.Log($"Line Segments Intersect Test #07: {Lines.GetLineSegmentIntersectionPoint(new Vector3(8.25f, 0, -28.25f), new Vector3(8.25f, 0, 25.75f), new Vector3(14.75f, 0, 25.75f), new Vector3(4.5f, 0, 28.25f), out intersectionPoint)}    {intersectionPoint}");
            Debug.Log($"Line Segments Intersect Test #08: {Lines.GetLineSegmentIntersectionPoint(new Vector3(8.25f, 0, 25.75f), new Vector3(8.25f, 0, -28.25f), new Vector3(4.5f, 0, 28.25f), new Vector3(14.75f, 0, 25.75f), out intersectionPoint)}    {intersectionPoint}");
            Debug.Log($"Line Segments Intersect Test #09: {Lines.GetLineSegmentIntersectionPoint(new Vector3(8.25f, 0, -28.25f), new Vector3(8.25f, 0, 25.75f), new Vector3(14.75f, 0, 25.75f), new Vector3(4.5f, 0, 28.25f), out intersectionPoint, true)}    {intersectionPoint}");
            Debug.Log($"Line Segments Intersect Test #10: {Lines.GetLineSegmentIntersectionPoint(new Vector3(8.25f, 0, 25.75f), new Vector3(8.25f, 0, -28.25f), new Vector3(4.5f, 0, 28.25f), new Vector3(14.75f, 0, 25.75f), out intersectionPoint, true)}    {intersectionPoint}");
            
            Debug.Log(new string('-', 128));

            Debug.Log($"IsClockwise Test #01 (Square_ConvexClockwise): {Triangulator_Polygon.IsClockwise(TestPolygons.Square_ConvexClockwise, 0, 0)}");
            Debug.Log($"IsClockwise Test #02 (Square_ConcaveClockwise): {Triangulator_Polygon.IsClockwise(TestPolygons.Square_ConcaveClockwise, 0, 0)}");
            Debug.Log($"IsClockwise Test #03 (Octagon_ConvexClockwise): {Triangulator_Polygon.IsClockwise(TestPolygons.Octagon_ConvexClockwise, 0, 0)}");
            Debug.Log($"IsClockwise Test #04 (Octagon_ConvexClockwise_ColinearTest): {Triangulator_Polygon.IsClockwise(TestPolygons.Octagon_ConvexClockwise_ColinearTest, 0, 0)}");
            
            Debug.Log($"IsClockwise Test #05 (Star_Clockwise): {Triangulator_Polygon.IsClockwise(TestPolygons.Star_Clockwise, 0, 0)}");
            
            List<Vector2> star_CounterClockwise = new List<Vector2>(TestPolygons.Star_Clockwise);
            star_CounterClockwise.Reverse();
            Debug.Log($"IsClockwise Test #06 (Star_CounterClockwise): {Triangulator_Polygon.IsClockwise(star_CounterClockwise, 0, 0)}");
            
            Debug.Log($"IsClockwise Test #07 (Seven_Clockwise): {Triangulator_Polygon.IsClockwise(TestPolygons.Seven_Clockwise, 0, 0)}");
            
            List<Vector2> seven_CounterClockwise = new List<Vector2>(TestPolygons.Star_Clockwise);
            seven_CounterClockwise.Reverse();
            Debug.Log($"IsClockwise Test #07 (Seven_Clockwise): {Triangulator_Polygon.IsClockwise(TestPolygons.Seven_Clockwise, 0, 0)}");

            Debug.Log($"IsClockwise Test #08 (CrossingLines): {Triangulator_Polygon.IsClockwise(TestPolygons.CrossingLines, 0, 0)}");
        }


    }
}

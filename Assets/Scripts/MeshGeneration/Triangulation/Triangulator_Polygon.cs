using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Android;

using DIY_DOOM.MeshGeneration.Triangulation.Base;
using DIY_DOOM.Utils.Maps;


// **************************************************************************************************************************************************
// PLAN
// --------------------------------------------------------------------------------------------------------------------------------------------------
// I think I figured out a way to solve the triangulation problem.
//
// 1. Finish implementing and testing the triangulation code I started in this file.
// 2. Find all segs for each sector, and store them in a list in the SectorDef. Now, we just need to put the segs in order. We can probably just
//    start with whatever happens to be the first one, and then find the one that connects to its end point. Repeat this until we've got a new
//    list containing all segs in order.
// 3. Next, we need to figure out which segs represent holes in the sector. We can do this by checking the parent sector. If it isn't this sector,
//    then we know it must be a smaller sector that is inside this one.
// 4. Determine which segs are holes. We may need to link them to their parent subsectors for this step. Then we can use that to easily determine
//    if the seg is part of a hole in this sector, or if it is just a neighboring sector. It it part of a subsector in this sector, then it's a hole.
// 5. Create a bridge to each hole. It should be placed at whichever of its vertices is closest to the outer border of the sector:
//    https://alienryderflex.com/triangulation/
// 6. Once that process is complete and the holes have been made part of the sector's polygon, we can pass it into our triangulation code.
// 7. Then run the resulting vertices list through a function that generates all the UVs.
// **************************************************************************************************************************************************


namespace DIY_DOOM.MeshGeneration.Triangulation
{
    /// <summary>
    /// This class triangulates a polygon.
    /// </summary>
    public static class Triangulator_Polygon
    {
        /// <summary>
        /// Returns true if the last polygon passed into Triangulate was convex.
        /// </summary>
        public static bool PolygonIsConvex { get; private set; }
        /// <summary>
        /// Returns true if the last polygon passed into Triangulate() had its vertices in clockwise order.
        /// </summary>
        public static bool PolygonIsClockwise { get; private set; }

        /// <summary>
        /// Returns the number of left turns in the polygon.
        /// </summary>
        public static int LeftTurns { get; private set; }

        /// <summary>
        /// Returns the number of right turns in the polygon
        /// </summary>
        public static int RightTurns { get; private set; }

        /// <summary>
        /// Returns the number of colinear sections in the polygon. A colinear section is two consecutive line segments that point in the exact same direction so there is no turn.
        /// </summary>
        public static int ColinearSections { get; private set; }

        /// <summary>
        /// Triangulates the polygon defined by the passed in vertices list.
        /// This also calls another function that determines if whether the polygon is convex/concave, and the winding order of its vertices (clockwise or counter-clockwise).
        /// </summary>
        /// <param name="vertices">The vertices of the polygon.</param>
        /// <param name="meshData">The <see cref="MeshData"/> object to store the generated triangles in.</param>
        /// <param name="yValue">The y position or elevation of the polygon.</param>
        /// <param name="removeColinearLineSegments">If true, colinear line segments are removed from the polygon since they are unnecessary.</param>
        /// <param name="invertOrderIfCounterClockwise">If true, the winding order of the polygon is inverted if it is counter-clockwise.</param>
        /// <param name="removeDegenerateTriangles">If true, degenerate triangles are removed from the polygon.</param>
        /// <returns>True if successful.</returns>
        public static bool Triangulate(List<Vector2> vertices, MeshData meshData, float yValue = 0.0f, bool removeColinearLineSegments = true, bool invertOrderIfCounterClockwise = true, bool removeDegenerateTriangles = true)
        {
            if (vertices.Count < 3)
                throw new ArgumentException("A polygon must have at least 3 vertices!");

            if (meshData == null)
                throw new ArgumentNullException(nameof(meshData));


            Debug.Log("ZERO Triangulator_Polygon.Triangulate(): " + vertices[0]);

            int originVertCount = vertices.Count;

            GetPolygonDetails(vertices, removeColinearLineSegments);

            // Invert the winding order if the option is enabled and the polygon is counter-clockwise
            if (!PolygonIsClockwise && invertOrderIfCounterClockwise)
                vertices.Reverse();

            // Triangulate the polygon appropriately depending on whether it is convex or concave.
            bool result = false;
            if (PolygonIsConvex)
                result = Triangulator_Convex.Triangulate(vertices, meshData, yValue);
            else
                result = Triangulator_Concave.Triangulate(vertices, meshData, yValue);


            // Remove degenerate triangles where all three vertices are at the same position on the x, or z axis.
            if (removeDegenerateTriangles)
                RemoveDegenerateTriangles(meshData);


            Debug.Log($"POLYGON INFO:    Original Vertex Count: {originVertCount}    Final Vertex Count: {meshData.Vertices.Count}    IsConvex: {PolygonIsConvex}    IsClockwise: {PolygonIsClockwise}    LeftTurns: {LeftTurns}    RightTurns: {RightTurns}    ColinearSections: {ColinearSections}");

            return result;
        }

        /// <summary>
        /// Scans the polygon to determine if it is convex, and whether the vertices are
        /// in clockwise or counter-clockwise order. These values are stored in private member variables.
        /// </summary>
        /// <param name="vertices">The vertices of the polygon.</param>
        /// <param name="removeColinearLineSegments">If true, colinear line segments are removed from the polygon since they are unnecessary.</param>
        /// <returns></returns>
        private static void GetPolygonDetails(List<Vector2> vertices, bool removeColinearLineSegments = true)
        {
            List<int> indicesToRemove = new List<int>();

            int rightTurns = 0;
            int leftTurns = 0;
            int colinearSections = 0; // A section where two consecutive line segments are pointing in the exact same direction so there is no turn.

            for (int i = 0; i < vertices.Count; i++)
            {
                // Get the current vertex, and the previous and next vertices
                Vector2 vPrev = i > 0 ? vertices[i - 1] : vertices[vertices.Count - 1];
                Vector2 vCurrent = vertices[i];
                Vector2 vNext = i < vertices.Count - 1 ? vertices[i + 1] : vertices[0];

                // Calculate the vectors of the line segments between them
                Vector2 line1 = vCurrent - vPrev;
                Vector2 line2 = vNext - vCurrent;

                // Check the direction of the corner formed by them
                float crossProduct = CalculateCrossProductYOnly(line1, line2);
                if (crossProduct < 0)
                    leftTurns++;
                else if (crossProduct > 0)
                    rightTurns++;
                else if (AreColinear(vPrev, vCurrent, vNext))
                {
                    colinearSections++;

                    // Mark the vertex between these two line segments (vCurrent) for removal, as it is unnecessary.
                    indicesToRemove.Add(i);
                }

            } // end for i


            // Remove the center vertex in each colinear section to turn it into a single line segement.
            if (removeColinearLineSegments)
            {
                for (int i = indicesToRemove.Count - 1; i >= 0; i--)
                {
                    vertices.RemoveAt(indicesToRemove[i]);
                }

                Debug.Log($"Removed {indicesToRemove.Count} colinear line segments.");
            }


            // Set the private member variables based on information obtained from the polygon
            PolygonIsConvex = (leftTurns == 0 && rightTurns > 0) || (rightTurns == 0 && leftTurns > 0);
            PolygonIsClockwise = IsClockwise(vertices);
            LeftTurns = leftTurns;
            RightTurns = rightTurns;
            ColinearSections = colinearSections;
        }

        /// <summary>
        /// This function uses a simple algorithm to determine whether the polygon is clockwise or
        /// counter-clockwise.
        /// </summary>
        /// <remarks>
        /// The algorithm is as follows.
        ///     1. Calculate the sum of the edges of the polygon
        ///     2. If the sum is positive, the polygon is clockwise
        ///     3. If the sum is negative, the polygon is counter-clockwise
        ///     
        ///     NOTE: The sum is equal to double the area of the polygon
        ///     
        /// I found this algorithm in this StackOverflow answer:
        /// https://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order#:~:text=Here's%20a%20simple%20one%20that,the%20curve%20is%20counter%2Dclockwise.
        /// </remarks>
        private static bool IsClockwise(List<Vector2> vertices)
        {
            float sumOfEdges = 0f;

            for (int i = 0; i < vertices.Count - 1; i++)
            {
                int nextIndex = i + 1;
                if (nextIndex >= vertices.Count)
                    nextIndex = 0;

                sumOfEdges += (vertices[nextIndex].x - vertices[i].x) * (vertices[nextIndex].y + vertices[i].y);

            } // end for i


            //Debug.Log($"Sum: {sumOfEdges}");


            // I'm treating 0 as an odd value right now, since I'm not sure if it is even possible to get a sum of 0
            // from this algorithm. So this will alert me should that happen.
            // The return statement below will just consider 0 as positive for now.
            if (sumOfEdges == 0)
                Debug.LogWarning("Triangulate_Polygon.IsClockwise() got a sum of 0!");


            return sumOfEdges >= 0;
        }

        /// <summary>
        /// Removes degenerate triangles where all three vertices are at the same position on the x or z axis.
        /// </summary>
        /// <returns></returns>
        private static void RemoveDegenerateTriangles(MeshData meshData)
        {
            int c = 0;
            for (int i = meshData.Triangles.Count - 1; i > 0; i -= 3)
            {
                Vector3 v1 = meshData.Vertices[meshData.Triangles[i]];
                Vector3 v2 = meshData.Vertices[meshData.Triangles[i - 1]];
                Vector3 v3 = meshData.Vertices[meshData.Triangles[i - 2]];

                // Check if the vertices of this triangle are colinear.
                // We skip the y-axis here since all the vertices will be at y=0, since the polygon is 2D.
                
                if (AreColinear(v1, v2, v3))
                {
                    // This triangle is colinear (has all three vertices at the same position on a single axis).
                    // So remove it.

                    // First, remove the vertices of this triangle from the mesh data.
                    meshData.Vertices.RemoveAt(meshData.Triangles[i]);
                    meshData.Vertices.RemoveAt(meshData.Triangles[i - 1]);
                    meshData.Vertices.RemoveAt(meshData.Triangles[i - 2]);

                    // Now remove the triangle vertex indices for this triangle.
                    meshData.Triangles.RemoveAt(i);
                    meshData.Triangles.RemoveAt(i - 1);
                    meshData.Triangles.RemoveAt(i - 2);

                    // Now remove the UVs for this triangle.
                    meshData.UVs.RemoveAt(i);
                    meshData.UVs.RemoveAt(i - 1);
                    meshData.UVs.RemoveAt(i - 2);

                    c++;
                }

            } // end for i

            Debug.Log($"Removed {c} degenerate triangles.");           
        }

        /// <summary>
        /// Checks if the two line segments defined by the three specified vertices are colinear (two sections of the same line).
        /// The y-axis is ignored since a polygon is 2D.
        /// </summary>
        /// <param name="v1">The 1st vertex</param>
        /// <param name="v2">The 2nd vertex</param>
        /// <param name="v3">The 3rd vertex</param>
        /// <returns></returns>
        public static bool AreColinear(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            return ((v1.x == v2.x && v2.x == v3.x) || (v1.z == v2.z && v2.z == v3.z));
        }

        public static bool AreColinear(Vector2 v1, Vector2 v2, Vector2 v3)
        {
            return ((v1.x == v2.x && v2.x == v3.x) || (v1.y == v2.y && v2.y == v3.y));
        }

        /// <summary>
        /// This calculates the cross product of two passed in vectors, but only the y-component of it.
        /// We don't need the other two components: https://stackoverflow.com/questions/27635188/algorithm-to-detect-left-or-right-turn-from-x-y-co-ordinates
        /// If the second vector turns left relative to the first one, the returned value is negative.
        /// If the second vector turns right relative to the first one, the returned value is positive.
        /// https://www.khanacademy.org/math/multivariable-calculus/thinking-about-multivariable-function/x786f2022:vectors-and-matrices/a/cross-products-mvc
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static float CalculateCrossProductYOnly(Vector3 line1, Vector3 line2)
        {
            return (line1.z * line2.x) - (line1.x * line2.z);
        }

        public static float CalculateCrossProductYOnly(Vector2 line1, Vector2 line2)
        {
            return CalculateCrossProductYOnly(new Vector3(line1.x, 0, line1.y),
                                              new Vector3(line2.x, 0, line2.y));
        }

        /// <summary>
        /// Takes an index and wraps it around to the appropriate value if it is negative
        /// or beyond the end of the list.
        /// </summary>
        /// <param name="index">The index to be wrapped</param>
        /// <param name="indexCount">The total number of indices in the list</param>
        /// <returns>
        /// The index wrapped around to the appropriate value at the opposite end of the list if it is negative or beyond the end of the list.
        /// Otherwise, the original index is just returned since no wrapping is needed.
        /// </returns>
        public static int WrapIndex(int index, int indexCount)
        {
            if (index >= 0 && index < indexCount)
                return index;
            else if (index < 0)
                return indexCount + (index % indexCount); // In this case we do the modulus again, but add it to indexCount. This is because the modulus of a negative number will be a negative remainder.
            else // if (index >= indexCount)
                return index % indexCount;
        }

        /// <summary>
        /// Generates a single triangle in the specified MeshData object.
        /// </summary>
        /// <param name="meshData">The MeshData object to create the triangle in.</param>
        /// <param name="v1">The first vertex</param>
        /// <param name="v2">The second vertex</param>
        /// <param name="v3">The third vertex</param>
        /// <param name="yValue">The y position or elevation of the polygon.</param>
        public static void GenerateTriangle(MeshData meshData, Vector2 v1, Vector2 v2, Vector2 v3, float yValue = 0.0f)
        {
            int nextIndex = meshData.Vertices.Count;

            // Add vertices for the next triangle.
            meshData.Vertices.Add(MapUtils.Point2dTo3dXZ(v1, yValue));
            meshData.Vertices.Add(MapUtils.Point2dTo3dXZ(v2, yValue));
            meshData.Vertices.Add(MapUtils.Point2dTo3dXZ(v3, yValue));

            // Add vertex indices for this triangle
            meshData.Triangles.Add(nextIndex);
            meshData.Triangles.Add(nextIndex + 1);
            meshData.Triangles.Add(nextIndex + 2);

            // Add UVs 
            meshData.UVs.Add(MeshGenerator.TransformFlatPointToUV(meshData.Vertices[nextIndex]));
            meshData.UVs.Add(MeshGenerator.TransformFlatPointToUV(meshData.Vertices[nextIndex + 1]));
            meshData.UVs.Add(MeshGenerator.TransformFlatPointToUV(meshData.Vertices[nextIndex + 2]));
        }
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using DIY_DOOM.MeshGeneration;

namespace DIY_DOOM.MeshGeneration.Triangulation.Base
{
    /// <summary>
    /// Triangulates a concave polygon using the ear clipping algorithm.
    /// https://alienryderflex.com/triangulation/
    /// </summary>
    internal static class Triangulator_Concave
    {
        /// <summary>
        /// Triangulates a concave polygon.
        /// </summary>
        /// <param name="vertices">The vertices of the polygon.</param>
        /// <param name="meshData">The <see cref="MeshData"/> object to store the generated triangles in.</param>
        /// <param name="yValue">The y position or elevation of the polygon.</param>
        /// <remarks>
        /// The passed in polygon MUST be clockwise, otherwise this algorithm will try to triangulate the area
        /// outside the polygon, which is no good! See the article linked above for information on what
        /// else can cause problems for this algorithm, such as a polygon with lines that cross.
        /// </remarks>
        /// <returns>The result code indicating whether the triangulation was successful or what error it failed with.</returns>
        public static TriangulationResults Triangulate(List<Vector2> vertices, MeshData meshData, float yValue = 0.0f)
        {
            List<int> indicesToRemove = new List<int>();

            int prevIndex = -1;
            int curIndex = -1;
            int nextIndex = -1;

            int curVertCount = vertices.Count;
            int earsClipped = 0;
            int consecutiveLoopsWithNoClippings = 0;


            // Loop until the ear clipping algorithm is complete.
            while(true)
            {
                // If we don't have enough vertices left to make another triangle, it means we are done!
                if (vertices.Count < 3)
                    break;


                // Check if we've looped all the way around the parent polygon without finding any good candidate corners.
                if (curIndex == 0) // Has curIndex looped back to the start of the list, so its value is now less than that of prevIndex?
                {
                    if (earsClipped == 0)
                    {
                        // We made no clippings during this trip around the parent polygon, so increment the counter.
                        consecutiveLoopsWithNoClippings++;
                    }
                    else
                    {
                        // We made some clippings during this trip around the parent polygon, so reset the counter.
                        consecutiveLoopsWithNoClippings = 0;
                    }


                    if (consecutiveLoopsWithNoClippings >= 3)
                    {
                        Debug.LogError("Failed to fully triangulate this concave polygon! The ear clipping algorithm could not create any more triangles.");
                        return TriangulationResults.Failed_EarClippingAlgorithmCouldntContinue;
                    }


                    // Reset the counters to prepare for our next trip around the parent polygon.
                    curVertCount = vertices.Count;
                    earsClipped = 0;
                }


                // Get the indices of the three vertices of the current corner, which can potentially form a new triangle.
                curIndex++;
                //int old = curIndex;
                curIndex = Triangulator_Polygon.WrapIndex(curIndex, vertices.Count);
                prevIndex = Triangulator_Polygon.WrapIndex(curIndex - 1, vertices.Count);
                nextIndex = Triangulator_Polygon.WrapIndex(curIndex + 1, vertices.Count);

                //Debug.Log($"WRAPPED {old} to {curIndex}.    Prev={prevIndex}    Next={nextIndex}    Count={vertices.Count}");


                // Get the current vertex, and the previous and next vertices
                Vector2 vPrev = vertices[prevIndex];
                Vector2 vCurrent = vertices[curIndex];
                Vector2 vNext = vertices[nextIndex];



                // Calculate the vectors of the line segments between them
                Vector2 line1 = vCurrent - vPrev;
                Vector2 line2 = vNext - vCurrent;

                
                // Have we found a good candidate corner that meets the following conditions?
                // 1. It's convex (assuming the polygon has its vertices in clockwise order).
                // 2. No other vertex in the polygon is inside this triangle.


                // Check the direction of the corner formed by them
                float crossProduct = Triangulator_Polygon.CalculateCrossProductYOnly(line1, line2);
                if (crossProduct < 0)
                {
                    // This corner is concave, so simply continue to the next iteration.
                    continue;
                }
                else if (crossProduct > 0)
                {
                    //Debug.Log("Clipping ear...");

                    // This corner is convex, so check if this triangle overlaps any other corner of the parent polygon.
                    // If so, simply jump to the next iteration of this loop.
                    if (ContainsPolygonVertexThatIsntPartOfThisTriangle(vertices, prevIndex, curIndex, nextIndex))
                        continue;

                    // We've found a good candidate corner (aka ear), so generate the triangle.
                    Triangulator_Polygon.GenerateTriangle(meshData, vPrev, vCurrent, vNext, yValue);

                    // Clip the ear (corner) off of the parent polygon by removing the center vertex.
                    vertices.RemoveAt(curIndex);

                    // Track how many "ears" we clipped during this trip around the parent polygon.
                    earsClipped++;
                }
                // NOTE: We don't have a third clause here to check for colinear line segments, as those were already removed before Triangulator_Polygon called this function.


            } // end while


            return TriangulationResults.Succeeded;
        }

        /// <summary>
        /// Checks if the triangle defined by vertices v1, v2, and v3 contains any other vertices of the parent polygon that are not part of this triangle.
        /// In otherwords, this triangle overlaps some other part of the polygon.
        /// </summary>
        /// <param name="vertices">The list of vertices in the parent polygon.</param>
        /// <param name="v1">The index of the 1st vertex of the triangle.</param>
        /// <param name="v2">The index of the 2nd vertex of the triangle.</param>
        /// <param name="v3">The index of the 3rd vertex of the triangle.</param>
        /// <returns>True if the triangle intersects with some other corner of the parent polygon</returns>
        private static bool ContainsPolygonVertexThatIsntPartOfThisTriangle(List<Vector2> vertices, int v1Index, int v2Index, int v3Index)
        {
            // Get the vertices of the triangle.
            Vector2 v1 = vertices[v1Index];
            Vector2 v2 = vertices[v2Index];
            Vector2 v3 = vertices[v3Index];


            // Check all other vertices to see if they fall inside this triangle.
            for (int i = 0; i < vertices.Count; i++)
            {
                // If the current vertex is one of the triangle's vertices, then simply skip it.
                if (i == v1Index || i == v2Index || i == v3Index)
                    continue;

                // This vertex is not one of the triangle's vertices, so check if it falls inside the
                // triangle to detect if this triangle overlaps another part of the parent triangle.
                if (IsPointInTriangle(vertices[i], v1, v2, v3))
                {
                    return true;
                }

            } // end for i


            // Since we found no overlaps between this triangle and any other corner of the parent polygon,
            // return false.
            return false;
        }

        /// <summary>
        /// Checks if point p1 lies inside the triangle formed by vertices v1, v2, and v3.
        /// </summary>
        /// <param name="p1">The point to check</param>
        /// <param name="v1">The 1st vertex of the triangle</param>
        /// <param name="v2">The 2nd vertex of the triangle</param>
        /// <param name="v3">The 3rd vertex of the triangle</param>
        /// <returns>True if the point lies inside the triangle</returns>
        public static bool IsPointInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3) 
        { 
            bool b1, b2, b3; 
            
            b1 = Sign(pt, v1, v2) < 0.0f; 
            b2 = Sign(pt, v2, v3) < 0.0f; 
            b3 = Sign(pt, v3, v1) < 0.0f; 
            
            return ((b1 == b2) && (b2 == b3)); 
        }

        /// <summary>
        /// This function is essentially a cross product. It is the same as taking two 2D vectors, making them 3D by setting z = 0,
        /// and them passing them into the Vector3.Cross() function. The return value of this function is the z value of that 3D cross product operation.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }

    }
}

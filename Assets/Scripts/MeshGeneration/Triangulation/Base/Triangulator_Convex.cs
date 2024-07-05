using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using DIY_DOOM.Maps;
using DIY_DOOM.MeshGeneration;
using UnityEngine.UI;

namespace DIY_DOOM.MeshGeneration.Triangulation.Base
{
    internal static class Triangulator_Convex
    {
        /// <summary>
        /// Triangulates a convex polygon into a triangle fan (with no shared vertices). The number of triangles will be vertexCount - 2.
        /// </summary>
        /// <remarks>
        /// To triangulate a convex polygon, you simply create a line segment between the first vertex and every other
        /// one (except for the two nearest neighbors of the first vertex).
        /// https://alienryderflex.com/triangulation/
        /// </remarks>
        /// <param name="vertices">The vertices of the polygon.</param>
        /// <param name="meshData">The <see cref="MeshData"/> object to store the generated triangles in.</param>
        /// <param name="yValue">The y position or elevation of the polygon.</param>
        /// <returns>True if successful.</returns>
        public static bool Triangulate(List<Vector2> vertices, MeshData meshData, float yValue = 0.0f)
        {
            for (int i = 1; i < vertices.Count - 1; i++)
            {
                // Generate the triangle.
                Triangulator_Polygon.GenerateTriangle(meshData, 
                                                      vertices[0], 
                                                      vertices[i], 
                                                      vertices[i + 1], 
                                                      yValue);

            } // end for i, j

            return true;
        }
    }
}

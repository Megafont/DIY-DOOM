using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace DIY_DOOM.MeshGeneration.Triangulation
{
    /// <summary>
    /// This class just contains various polygons used for testing the Triangulation classes.
    /// </summary>
    /// <remarks>
    /// 
    /// IMPORTANT: You can create a counterclockwise version of any shape by converting its
    ///            vertex array to a list, and then calling its Reverse() method.
    /// 
    /// </remarks>
    public static class TestPolygons
    {
        // ----------------------------------------------------------------------------------------------------
        // Squares
        // ----------------------------------------------------------------------------------------------------

        public static Vector2[] Square_ConvexClockwise = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0),
        };

        public static Vector2[] Square_ConcaveClockwise = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(0.5f, 0.75f),
            new Vector2(1, 1),
            new Vector2(1, 0),
        };


        // ----------------------------------------------------------------------------------------------------
        // Octagons
        // ----------------------------------------------------------------------------------------------------

        public static Vector2[] Octagon_ConvexClockwise = new Vector2[]
        {
            new Vector2(-1, -2),
            new Vector2(-2, -1),
            new Vector2(-2, 1),
            new Vector2(-1, 2),
            new Vector2(1, 2),
            new Vector2(2, 1),
            new Vector2(2, -1),
            new Vector2(1, -2),
        };

        public static Vector2[] Octagon_ConvexClockwise_ColinearTest = new Vector2[]
{
            new Vector2(-1, -2),
            new Vector2(-2, -1),
            new Vector2(-2, -0),
            new Vector2(-2, 1),
            new Vector2(-1, 2),
            new Vector2(0, 2),
            new Vector2(1, 2),
            new Vector2(2, 1),
            new Vector2(2, 0),
            new Vector2(2, -1),
            new Vector2(1, -2),
            new Vector2(0, -2),
        };

        // ----------------------------------------------------------------------------------------------------
        // Miscellaneous
        // ----------------------------------------------------------------------------------------------------

        public static Vector2[] Star_Clockwise = new Vector2[]
        {
            new Vector2(0, -1),
            new Vector2(-2, -2),
            new Vector2(-1, 0),
            new Vector2(-3, 1),
            new Vector2(-1, 1),
            new Vector2(0, 3),
            new Vector2(1, 1),
            new Vector2(3, 1),
            new Vector2(1, 0),
            new Vector2(2, -2),
        };

        public static Vector2[] Seven_Clockwise = new Vector2[]
        {
            new Vector2(-2, 4),
            new Vector2(2, 4),
            new Vector2(2, 3),
            new Vector2(0.5f, 0),
            new Vector2(-0.5f, 0),
            new Vector2(1, 3),
            new Vector2(-2, 3),
        };

        

        // ----------------------------------------------------------------------------------------------------
        // Invalid
        // ----------------------------------------------------------------------------------------------------

        public static Vector2[] CrossingLines = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 1),
            new Vector2(0, 1),
            new Vector2(1, 0),
        };

    }
}

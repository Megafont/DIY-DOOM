namespace DIY_DOOM.MeshGeneration.Triangulation.Base
{
    public class PolygonDetails
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="isConvex"></param>
        /// <param name="isClockwise"></param>
        /// <param name="leftTurns"></param>
        /// <param name="rightTurns"></param>
        /// <param name="colinearSections"></param>
        public PolygonDetails(bool isConvex, bool isClockwise, int leftTurns, int rightTurns, int colinearSections)
        {
            IsConvex = isConvex;
            IsClockwise = isClockwise;
            LeftTurns = leftTurns;
            RightTurns = rightTurns;
            ColinearSections = colinearSections;
        }
      


        /// <summary>
        /// Returns true if the last polygon passed into Triangulate was convex.
        /// </summary>
        public bool IsConvex { get; private set; }
        /// <summary>
        /// Returns true if the last polygon passed into Triangulate() had its vertices in clockwise order.
        /// </summary>
        public bool IsClockwise { get; private set; }

        /// <summary>
        /// Returns the number of left turns in the polygon.
        /// </summary>
        public int LeftTurns { get; private set; }

        /// <summary>
        /// Returns the number of right turns in the polygon
        /// </summary>
        public int RightTurns { get; private set; }

        /// <summary>
        /// Returns the number of colinear sections in the polygon. A colinear section is two consecutive line segments that point in the exact same direction so there is no turn.
        /// </summary>
        public int ColinearSections { get; private set; }

    }
}
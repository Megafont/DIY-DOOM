
namespace DIY_DOOM.MeshGeneration.Triangulation.Base
{
    public enum TriangulationResults
    {
        Succeeded = 0,

        Failed_VerticesListIsNull,
        Failed_NotEnoughVertices,
        Failed_IntersectingLineSegments,
        Failed_EarClippingAlgorithmCouldntContinue,
    }

}

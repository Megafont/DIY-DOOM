using DIY_DOOM.WADs.Data.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DIY_DOOM.MeshGeneration
{
    public class MeshGenOutput
    {
        public List<Mesh> MapSubMeshes;
        public List<Material> MapSubMeshMaterials;
    }


    public class MeshData
    {
        public List<Vector3> Vertices;
        public List<int> Triangles;
        public List<Vector2> UVs;

        public Material Material;

        public string TextureName;



        public MeshData(string textureName, Material material)
        {
            Vertices = new List<Vector3>();
            Triangles = new List<int>();
            UVs = new List<Vector2>();

            Material = material;
            TextureName = textureName;
        }


        public void ClearGeometry()
        {
            Vertices.Clear();
            Triangles.Clear();
            UVs.Clear();
        }
    }


    public struct FaceUvBounds
    {
        public float Bottom;
        public float Left;
        public float Top;
        public float Right;



        public static FaceUvBounds Default
        {
            get
            {
                return new FaceUvBounds() { Bottom = 0, Left = 0, Top = 1, Right = 1 };
            }
        }

        public void ApplyTextureOffset(Vector2 textureOffset)
        {
            Left += textureOffset.x;
            Right += textureOffset.x;

            Top -= textureOffset.y;
            Bottom -= textureOffset.y;
        }

        public override string ToString()
        {
            return $"({Left},{Bottom})-({Right}, {Top})";
        }
    }

    public class FaceFrontAndBackData
    {
        public SideDef FrontSideDef;
        public SideDef BackSideDef;

        public SectorDef FrontSectorDef;
        public SectorDef BackSectorDef;
    }

}

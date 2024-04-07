using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DIY_DOOM.Maps;
using DIY_DOOM.MeshGeneration;


namespace DIY_DOOM.MeshGeneration
{
    public class LevelGeometry : MonoBehaviour
    {
        private Map _Map;



        public void SetMap(Map map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            _Map = map;

            MeshGenOutput output = MeshGenerator.GenerateMapMesh(map);

            GenerateSubMeshObjects(output);
        }


        private void GenerateSubMeshObjects(MeshGenOutput meshGenOutput)
        {
            LevelSubMesh prefab = DoomEngine.Settings.LevelMeshPrefab;

            for (int i = 0; i < meshGenOutput.MapSubMeshes.Count; i++)
            {
                
                LevelSubMesh subMesh = Instantiate(prefab, transform);
                subMesh.gameObject.name = $"LevelMesh - {meshGenOutput.MapSubMeshMaterials[i].name}";
                subMesh.MeshFilter.mesh = meshGenOutput.MapSubMeshes[i];
                subMesh.MeshRenderer.material = meshGenOutput.MapSubMeshMaterials[i];
            }
        }

    }
}
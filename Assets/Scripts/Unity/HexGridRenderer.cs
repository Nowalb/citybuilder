using System.Collections.Generic;
using CityBuilder.Simulation;
using UnityEngine;

namespace CityBuilder.Unity
{
    /// <summary>
    /// Renders simulation hex tiles using a shared procedural mesh/material setup.
    /// </summary>
    public sealed class HexGridRenderer : MonoBehaviour
    {
        [SerializeField] private Material roadMaterial;
        [SerializeField] private Material groundMaterial;

        private readonly Dictionary<HexCoord, MeshRenderer> _tileRenderers = new();
        private Mesh _sharedHexMesh;
        private Transform _container;
        private Material _fallbackRoadMaterial;
        private Material _fallbackGroundMaterial;

        public void Initialize(GridSystem grid, float hexSize)
        {
            if (grid == null)
            {
                Debug.LogError("HexGridRenderer.Initialize called with null grid.");
                return;
            }

            Cleanup();

            _sharedHexMesh = CreatePointyTopHexMesh(hexSize);
            _container = new GameObject("HexGridRoot").transform;
            _container.SetParent(transform, false);

            foreach (var tile in grid.Tiles)
            {
                var hexGo = new GameObject($"Hex_{tile.Coord.Q}_{tile.Coord.R}");
                hexGo.transform.SetParent(_container, false);
                hexGo.transform.position = HexGridMath.HexToWorldPosition(tile.Coord, hexSize);

                var meshFilter = hexGo.AddComponent<MeshFilter>();
                var meshRenderer = hexGo.AddComponent<MeshRenderer>();
                var meshCollider = hexGo.AddComponent<MeshCollider>();

                meshFilter.sharedMesh = _sharedHexMesh;
                meshCollider.sharedMesh = _sharedHexMesh;
                meshRenderer.sharedMaterial = ResolveTileMaterial(tile.IsRoad);

                _tileRenderers[tile.Coord] = meshRenderer;
            }
        }

        public void RefreshTile(HexCoord coord, bool isRoad)
        {
            if (_tileRenderers.TryGetValue(coord, out var renderer))
            {
                renderer.sharedMaterial = ResolveTileMaterial(isRoad);
            }
        }

        private Material ResolveTileMaterial(bool isRoad)
        {
            if (isRoad)
            {
                if (roadMaterial != null) return roadMaterial;
                if (_fallbackRoadMaterial == null)
                {
                    _fallbackRoadMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = Color.gray };
                }

                return _fallbackRoadMaterial;
            }

            if (groundMaterial != null) return groundMaterial;
            if (_fallbackGroundMaterial == null)
            {
                _fallbackGroundMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(0.22f, 0.24f, 0.22f) };
            }

            return _fallbackGroundMaterial;
        }

        private static Mesh CreatePointyTopHexMesh(float size)
        {
            var mesh = new Mesh { name = "HexTile" };
            var vertices = new Vector3[7];
            var triangles = new int[18];

            vertices[0] = Vector3.zero;
            for (var i = 0; i < 6; i++)
            {
                var angleDeg = 60f * i - 30f;
                var angleRad = Mathf.Deg2Rad * angleDeg;
                vertices[i + 1] = new Vector3(size * Mathf.Cos(angleRad), 0f, size * Mathf.Sin(angleRad));
            }

            for (var i = 0; i < 6; i++)
            {
                var t = i * 3;
                triangles[t] = 0;
                triangles[t + 1] = i + 1;
                triangles[t + 2] = i == 5 ? 1 : i + 2;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private void Cleanup()
        {
            _tileRenderers.Clear();
            if (_container != null)
            {
                Destroy(_container.gameObject);
                _container = null;
            }

            _sharedHexMesh = null;
            _fallbackRoadMaterial = null;
            _fallbackGroundMaterial = null;
        }
    }
}

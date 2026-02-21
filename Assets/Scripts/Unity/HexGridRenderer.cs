using System.Collections.Generic;
using CityBuilder.Simulation;
using UnityEngine;

namespace CityBuilder.Unity
{
    /// <summary>
    /// Renders simulation hex tiles using shared procedural mesh/materials.
    /// </summary>
    public sealed class HexGridRenderer : MonoBehaviour
    {
        [SerializeField] private Material roadMaterial;
        [SerializeField] private Material grassMaterial;
        [SerializeField] private Material forestMaterial;
        [SerializeField] private Material hillMaterial;
        [SerializeField] private Material waterMaterial;

        private readonly Dictionary<HexCoord, MeshRenderer> _tileRenderers = new();
        private readonly Dictionary<TerrainType, Material> _fallbackTerrainMaterials = new();
        private Mesh _sharedHexMesh;
        private Transform _container;

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
                meshRenderer.sharedMaterial = ResolveTileMaterial(tile);

                _tileRenderers[tile.Coord] = meshRenderer;
            }
        }

        public void RefreshTile(Tile tile)
        {
            if (tile == null)
            {
                return;
            }

            if (_tileRenderers.TryGetValue(tile.Coord, out var renderer))
            {
                renderer.sharedMaterial = ResolveTileMaterial(tile);
            }
        }

        private Material ResolveTileMaterial(Tile tile)
        {
            if (tile.IsRoad)
            {
                if (roadMaterial != null)
                {
                    return roadMaterial;
                }

                return GetOrCreateFallback(TerrainType.Hill, Color.gray);
            }

            return tile.TerrainType switch
            {
                TerrainType.Grass when grassMaterial != null => grassMaterial,
                TerrainType.Forest when forestMaterial != null => forestMaterial,
                TerrainType.Hill when hillMaterial != null => hillMaterial,
                TerrainType.Water when waterMaterial != null => waterMaterial,
                TerrainType.Grass => GetOrCreateFallback(TerrainType.Grass, new Color(0.20f, 0.45f, 0.20f)),
                TerrainType.Forest => GetOrCreateFallback(TerrainType.Forest, new Color(0.10f, 0.32f, 0.14f)),
                TerrainType.Hill => GetOrCreateFallback(TerrainType.Hill, new Color(0.42f, 0.35f, 0.24f)),
                TerrainType.Water => GetOrCreateFallback(TerrainType.Water, new Color(0.10f, 0.24f, 0.55f)),
                _ => GetOrCreateFallback(TerrainType.Grass, new Color(0.20f, 0.45f, 0.20f))
            };
        }

        private Material GetOrCreateFallback(TerrainType terrainType, Color color)
        {
            if (_fallbackTerrainMaterials.TryGetValue(terrainType, out var material))
            {
                return material;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
            material = new Material(shader) { color = color };
            _fallbackTerrainMaterials[terrainType] = material;
            return material;
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
            _fallbackTerrainMaterials.Clear();
        }
    }
}

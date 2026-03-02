using UnityEngine;

namespace CityCore
{
    /// <summary>
    /// Generates and renders a procedural heightmap terrain using seeded Perlin noise.
    /// </summary>
    public class TerrainSystem : MonoBehaviour
    {
        private const float HeightAmplitude = 6f;

        [SerializeField] private MeshFilter terrainMeshFilter;
        [SerializeField] private MeshRenderer terrainMeshRenderer;

        private float[,] heightMap;
        private int width;
        private int height;
        private int currentSeed;

        /// <summary>
        /// Last generated seed value.
        /// </summary>
        public int CurrentSeed => currentSeed;

        /// <summary>
        /// Generates the heightmap and updates terrain mesh.
        /// </summary>
        public void Generate(int seed, int mapWidth, int mapHeight, float scale)
        {
            currentSeed = seed;
            width = mapWidth;
            height = mapHeight;
            heightMap = new float[width, height];

            float offsetX = seed * 0.001f;
            float offsetY = seed * 0.002f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float nx = x * scale + offsetX;
                    float ny = y * scale + offsetY;
                    heightMap[x, y] = Mathf.PerlinNoise(nx, ny);
                }
            }

            BuildMesh();
        }

        /// <summary>
        /// Returns terrain height at map coordinate.
        /// </summary>
        public float GetHeightAt(int x, int y)
        {
            if (!IsInBounds(x, y)) return 0f;
            return heightMap[x, y] * HeightAmplitude;
        }

        /// <summary>
        /// Returns normalized local slope estimate (0 flat, 1 steep).
        /// </summary>
        public float GetSlopeAt(int x, int y)
        {
            if (!IsInBounds(x, y)) return 1f;

            float h = GetHeightAt(x, y);
            float hx = Mathf.Abs(GetHeightAt(Mathf.Min(x + 1, width - 1), y) - h);
            float hy = Mathf.Abs(GetHeightAt(x, Mathf.Min(y + 1, height - 1)) - h);
            return Mathf.Clamp01((hx + hy) / HeightAmplitude);
        }

        private bool IsInBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < width && y < height;
        }

        private void BuildMesh()
        {
            if (terrainMeshFilter == null)
            {
                terrainMeshFilter = gameObject.GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();
            }

            if (terrainMeshRenderer == null)
            {
                terrainMeshRenderer = gameObject.GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();
                terrainMeshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
            }

            Vector3[] vertices = new Vector3[width * height];
            int[] triangles = new int[(width - 1) * (height - 1) * 6];
            Vector2[] uvs = new Vector2[vertices.Length];

            int vi = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    vertices[vi] = new Vector3(x, GetHeightAt(x, y), y);
                    uvs[vi] = new Vector2((float)x / width, (float)y / height);
                    vi++;
                }
            }

            int ti = 0;
            for (int y = 0; y < height - 1; y++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    int idx = y * width + x;
                    triangles[ti++] = idx;
                    triangles[ti++] = idx + width;
                    triangles[ti++] = idx + 1;
                    triangles[ti++] = idx + 1;
                    triangles[ti++] = idx + width;
                    triangles[ti++] = idx + width + 1;
                }
            }

            Mesh mesh = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            terrainMeshFilter.sharedMesh = mesh;
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
[RequireComponent(typeof(WaterTrigger))]
public class Water : MonoBehaviour
{
    [Header("Mesh Generation")]
    [Range(2, 500)] public int NumOfXVertices = 70;
    public float Width = 10f;
    public float Height = 4f;
    public Material WaterMaterial;
    private const int NUM_OF_Y_VERTICES = 2;

    [Header("Wave Physics")]
    [Range(0f, 1f)] public float Damping = 0.025f;
    [Range(0f, 1f)] public float Tension = 0.1f;
    [Range(0f, 1f)] public float Spread = 0.1f;
    public float SpringConstant = 0.02f;

    [Header("Splash")]
    public float SplashVelocityThreshold = 2f;
    public float SplashForceMultiplier = 1.5f;

    [Header("Gizmo")]
    public Color GizmoColor = Color.white;

    private Mesh _mesh;
    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    private Vector3[] _vertices;
    private int[] _topVerticesIndex;
    private float[] _velocity;

    private EdgeCollider2D _edgeCollider;

    private void Start()
    {
        GenerateMesh();
    }

    private void Reset()
    {
        _edgeCollider = GetComponent<EdgeCollider2D>();
        if (_edgeCollider != null)
        {
            _edgeCollider.isTrigger = true;
        }
    }

    public void ResetEdgeCollider()
    {
        if (_edgeCollider == null) return;

        _edgeCollider = GetComponent<EdgeCollider2D>();

        int count = _topVerticesIndex.Length;
        Vector2[] newPoints = new Vector2[count];

        for (int i = 0; i < count; i++)
        {
            Vector3 v = _vertices[_topVerticesIndex[i]];
            newPoints[i] = new Vector2(v.x, v.y);
        }

        _edgeCollider.offset = Vector2.zero;
        _edgeCollider.points = newPoints;
    }

    public void GenerateMesh()
    {
        if (_meshRenderer == null)
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }
        if (_meshFilter == null)
        {
            _meshFilter = GetComponent<MeshFilter>();
        }

        _mesh = new Mesh();
        _mesh.name = "WaterMesh";

        int xVertCount = NumOfXVertices;
        int totalVertices = xVertCount * NUM_OF_Y_VERTICES;

        _vertices = new Vector3[totalVertices];
        _topVerticesIndex = new int[xVertCount];
        _velocity = new float[xVertCount];

        // 先建底部顶点，再建顶部顶点
        for (int y = 0; y < NUM_OF_Y_VERTICES; y++)
        {
            for (int x = 0; x < xVertCount; x++)
            {
                int index = y * xVertCount + x;
                float xPos = (x / (float)(xVertCount - 1)) * Width - Width / 2f;
                float yPos = (y / (float)(NUM_OF_Y_VERTICES - 1)) * Height - Height / 2f;
                _vertices[index] = new Vector3(xPos, yPos, 0f);

                if (y == NUM_OF_Y_VERTICES - 1)
                    _topVerticesIndex[x] = index;
            }
        }

        // 三角形（顺时针绕序，法线朝 +Z）
        int[] triangles = new int[(xVertCount - 1) * (NUM_OF_Y_VERTICES - 1) * 6];
        int triIndex = 0;
        for (int y = 0; y < NUM_OF_Y_VERTICES - 1; y++)
        {
            for (int x = 0; x < xVertCount - 1; x++)
            {
                int bottomLeft = y * xVertCount + x;
                int bottomRight = bottomLeft + 1;
                int topLeft = bottomLeft + xVertCount;
                int topRight = topLeft + 1;

                triangles[triIndex++] = bottomLeft;
                triangles[triIndex++] = bottomRight;
                triangles[triIndex++] = topLeft;

                triangles[triIndex++] = topLeft;
                triangles[triIndex++] = bottomRight;
                triangles[triIndex++] = topRight;
            }
        }

        // UVs
        Vector2[] uvs = new Vector2[_vertices.Length];
        for (int i = 0; i < _vertices.Length; i++)
        {
            uvs[i] = new Vector2((_vertices[i].x + Width / 2f) / Width, (_vertices[i].y + Height / 2f) / Height);
        }

        if (WaterMaterial == null)
        {
            WaterMaterial = new Material(Shader.Find("Sprites/Default"));
            WaterMaterial.color = new Color(0.2f, 0.5f, 1f, 0.6f);
        }

        _meshRenderer.material = WaterMaterial;
        _mesh.vertices = _vertices;
        _mesh.triangles = triangles;
        _mesh.uv = uvs;

        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        _meshFilter.mesh = _mesh;
    }

    private void FixedUpdate()
    {
        if (_vertices == null || _velocity == null) return;

        SimulateWaves();
        UpdateMesh();
        UpdateEdgeColliderPoints();
    }

    private void SimulateWaves()
    {
        int count = NumOfXVertices;
        float[] acceleration = new float[count];

        for (int i = 0; i < count; i++)
        {
            float posY = _vertices[_topVerticesIndex[i]].y;
            float force = -SpringConstant * posY;
            force += -_velocity[i] * Damping;
            acceleration[i] = force;
        }

        for (int i = 0; i < count; i++)
        {
            _velocity[i] += acceleration[i];
            _vertices[_topVerticesIndex[i]].y += _velocity[i];
        }

        float[] leftDeltas = new float[count];
        float[] rightDeltas = new float[count];

        for (int j = 0; j < 4; j++)
        {
            for (int i = 0; i < count; i++)
            {
                float currentY = _vertices[_topVerticesIndex[i]].y;

                if (i > 0)
                {
                    float leftY = _vertices[_topVerticesIndex[i - 1]].y;
                    leftDeltas[i] = Spread * (currentY - leftY);
                    _velocity[i - 1] += leftDeltas[i] * Tension;
                }

                if (i < count - 1)
                {
                    float rightY = _vertices[_topVerticesIndex[i + 1]].y;
                    rightDeltas[i] = Spread * (currentY - rightY);
                    _velocity[i + 1] += rightDeltas[i] * Tension;
                }
            }

            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                    _velocity[i] -= leftDeltas[i] * Tension;
                if (i < count - 1)
                    _velocity[i] -= rightDeltas[i] * Tension;

                _vertices[_topVerticesIndex[i]].y += _velocity[i] * 0.5f;
            }
        }
    }

    private void UpdateMesh()
    {
        _mesh.vertices = _vertices;
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
    }

    private void UpdateEdgeColliderPoints()
    {
        if (_edgeCollider == null || _vertices == null || _topVerticesIndex == null) return;

        int count = NumOfXVertices;
        Vector2[] colliderPoints = new Vector2[count];

        for (int i = 0; i < count; i++)
        {
            Vector3 v = _vertices[_topVerticesIndex[i]];
            colliderPoints[i] = new Vector2(v.x, v.y);
        }

        _edgeCollider.points = colliderPoints;
    }

    public void Splash(Vector3 worldPosition, float velocity)
    {
        if (_velocity == null || _vertices == null) return;

        int count = NumOfXVertices;
        int closestIndex = GetClosestVertexIndex(worldPosition);
        if (closestIndex < 0) return;

        float force = -Mathf.Abs(velocity) * SplashForceMultiplier * 0.01f;
        _velocity[closestIndex] += force;

        int radius = Mathf.Clamp(Mathf.RoundToInt(Mathf.Abs(velocity) * 0.5f), 1, 5);
        for (int i = 1; i <= radius; i++)
        {
            float falloff = force * (1f - (float)i / (radius + 1));
            if (closestIndex - i >= 0)
                _velocity[closestIndex - i] += falloff;
            if (closestIndex + i < count)
                _velocity[closestIndex + i] += falloff;
        }
    }

    private int GetClosestVertexIndex(Vector3 worldPosition)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPosition);
        float vertexSpacing = Width / (NumOfXVertices - 1);
        int index = Mathf.RoundToInt((localPos.x + Width * 0.5f) / vertexSpacing);
        return Mathf.Clamp(index, 0, NumOfXVertices - 1);
    }

    private void OnDrawGizmos()
    {
        if (_vertices == null || _topVerticesIndex == null) return;

        Gizmos.color = GizmoColor;
        Vector3 offset = transform.position;

        for (int i = 0; i < _topVerticesIndex.Length - 1; i++)
        {
            Vector3 a = offset + _vertices[_topVerticesIndex[i]];
            Vector3 b = offset + _vertices[_topVerticesIndex[i + 1]];
            Gizmos.DrawLine(a, b);
        }
    }
}

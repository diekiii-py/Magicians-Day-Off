using UnityEngine;

/// <summary>
/// Procedurally builds a double-sided quad mesh on its MeshFilter so the mirror plane never
/// needs a manually-created Quad primitive. Runs in edit mode too, so the plane is visible
/// immediately in the Scene view. Just drag a material (e.g. MIrrorMat) onto the MeshRenderer.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
public class MirrorPlaneBuilder : MonoBehaviour
{
    [Tooltip("Width/height of the mirror plane in meters.")]
    public Vector2 size = new Vector2(0.7f, 1.2f);

    Vector2 _builtSize;

    void OnEnable() => Build();
    void OnValidate() => Build();

    void Build()
    {
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null) return;
        if (meshFilter.sharedMesh != null && meshFilter.sharedMesh.name == "MirrorPlaneQuad" && _builtSize == size) return;

        meshFilter.sharedMesh = BuildQuad(size);
        _builtSize = size;
    }

    static Mesh BuildQuad(Vector2 size)
    {
        float hw = size.x * 0.5f;
        float hh = size.y * 0.5f;

        var mesh = new Mesh { name = "MirrorPlaneQuad" };
        mesh.vertices = new[]
        {
            new Vector3(-hw, -hh, 0f),
            new Vector3(hw, -hh, 0f),
            new Vector3(hw, hh, 0f),
            new Vector3(-hw, hh, 0f),
        };
        mesh.uv = new[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1),
        };
        // Both windings included so the plane renders regardless of which way it ends up facing.
        mesh.triangles = new[] { 0, 1, 2, 0, 2, 3, 0, 2, 1, 0, 3, 2 };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}

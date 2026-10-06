using UnityEngine;

/// <summary>
/// Builds a flat rectangular "picture frame" mesh with a rectangular hole cut out of it at
/// runtime - no DCC tool needed. The frame occludes the passthrough view of the real wall
/// around the punched-through opening, while the hole itself is left empty so whatever is
/// placed behind it (the forest environment) is visible through the gap.
/// </summary>
public static class ProceduralHoleFrameMesh
{
    /// <summary>
    /// Builds the frame mesh in local space, centered at the origin, lying on the XY plane
    /// (matching a wall anchor whose forward axis is the wall normal).
    /// </summary>
    public static Mesh Build(Vector2 outerSize, Vector2 holeSize, Vector2 holeOffset)
    {
        float ow = outerSize.x * 0.5f;
        float oh = outerSize.y * 0.5f;
        float hw = holeSize.x * 0.5f;
        float hh = holeSize.y * 0.5f;
        Vector2 c = holeOffset;

        // 4 outer corners + 4 inner (hole) corners -> 8 verts, 8 triangles forming a ring.
        Vector3[] vertices =
        {
            new Vector3(-ow, -oh, 0f), new Vector3(ow, -oh, 0f), new Vector3(ow, oh, 0f), new Vector3(-ow, oh, 0f),
            new Vector3(c.x - hw, c.y - hh, 0f), new Vector3(c.x + hw, c.y - hh, 0f),
            new Vector3(c.x + hw, c.y + hh, 0f), new Vector3(c.x - hw, c.y + hh, 0f),
        };

        int[] triangles =
        {
            0, 4, 1, 1, 4, 5, // bottom strip
            1, 5, 2, 2, 5, 6, // right strip
            2, 6, 3, 3, 6, 7, // top strip
            3, 7, 0, 0, 7, 4, // left strip
        };

        var mesh = new Mesh { name = "WallHoleFrame" };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = new[]
        {
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
            new Vector2(0.25f, 0.25f), new Vector2(0.75f, 0.25f), new Vector2(0.75f, 0.75f), new Vector2(0.25f, 0.75f),
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}

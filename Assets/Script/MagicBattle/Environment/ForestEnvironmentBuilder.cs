using UnityEngine;

/// <summary>
/// Builds a simple low-poly forest out of Unity primitives at runtime - a stand-in for a real
/// forest asset pack. Replace by disabling <see cref="buildPlaceholderIfEmpty"/> and parenting
/// your own environment art under this transform instead.
/// </summary>
public class ForestEnvironmentBuilder : MonoBehaviour
{
    public bool buildPlaceholderIfEmpty = true;
    public int treeCount = 24;
    public float radius = 6f;
    public float minTreeHeight = 2f;
    public float maxTreeHeight = 4.5f;
    public Material groundMaterial;
    public Material trunkMaterial;
    public Material foliageMaterial;

    bool _built;

    public void BuildIfNeeded()
    {
        if (_built || !buildPlaceholderIfEmpty || transform.childCount > 0) return;
        _built = true;

        BuildGround();
        for (int i = 0; i < treeCount; i++) BuildTree(i);
    }

    void BuildGround()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ground.name = "ForestGround";
        ground.transform.SetParent(transform, false);
        ground.transform.localScale = new Vector3(radius * 2f, 0.05f, radius * 2f);
        ground.transform.localPosition = new Vector3(0f, -0.05f, radius * 0.5f);
        if (groundMaterial != null) ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;
        else SetColor(ground, new Color(0.18f, 0.3f, 0.12f));
    }

    void BuildTree(int index)
    {
        Vector2 offset = Random.insideUnitCircle * radius;
        Vector3 basePos = new Vector3(offset.x, 0f, Mathf.Abs(offset.y) + 0.5f);
        float height = Random.Range(minTreeHeight, maxTreeHeight);

        var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = $"TreeTrunk_{index}";
        trunk.transform.SetParent(transform, false);
        trunk.transform.localPosition = basePos + Vector3.up * height * 0.3f;
        trunk.transform.localScale = new Vector3(0.25f, height * 0.3f, 0.25f);
        if (trunkMaterial != null) trunk.GetComponent<Renderer>().sharedMaterial = trunkMaterial;
        else SetColor(trunk, new Color(0.3f, 0.2f, 0.1f));

        var foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        foliage.name = $"TreeFoliage_{index}";
        foliage.transform.SetParent(transform, false);
        foliage.transform.localPosition = basePos + Vector3.up * height * 0.75f;
        foliage.transform.localScale = Vector3.one * height * 0.55f;
        if (foliageMaterial != null) foliage.GetComponent<Renderer>().sharedMaterial = foliageMaterial;
        else SetColor(foliage, new Color(0.1f, 0.4f, 0.15f));
    }

    static void SetColor(GameObject go, Color color)
    {
        var renderer = go.GetComponent<Renderer>();
        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", color);
        renderer.SetPropertyBlock(block);
    }
}

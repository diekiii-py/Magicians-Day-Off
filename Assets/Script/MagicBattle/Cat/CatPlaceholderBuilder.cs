using UnityEngine;

/// <summary>
/// Builds a simple primitive placeholder cat (body/head/ears/tail) if no real model/renderer
/// has been parented under this transform. Replace by assigning a proper rigged cat model as
/// a child and this will do nothing.
/// </summary>
public class CatPlaceholderBuilder : MonoBehaviour
{
    public Color furColor = new Color(0.35f, 0.25f, 0.2f);

    void Awake()
    {
        if (GetComponentInChildren<Renderer>() != null) return; // real model already present
        Build();
    }

    void Build()
    {
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "CatBody_Placeholder";
        body.transform.SetParent(transform, false);
        body.transform.localPosition = new Vector3(0f, 0.3f, 0f);
        body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        body.transform.localScale = new Vector3(0.3f, 0.45f, 0.3f);
        Tint(body, furColor);

        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "CatHead_Placeholder";
        head.transform.SetParent(transform, false);
        head.transform.localPosition = new Vector3(0f, 0.55f, 0.35f);
        head.transform.localScale = Vector3.one * 0.3f;
        Tint(head, furColor);

        BuildEar(new Vector3(-0.12f, 0.75f, 0.4f));
        BuildEar(new Vector3(0.12f, 0.75f, 0.4f));

        var tail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tail.name = "CatTail_Placeholder";
        tail.transform.SetParent(transform, false);
        tail.transform.localPosition = new Vector3(0f, 0.35f, -0.4f);
        tail.transform.localRotation = Quaternion.Euler(60f, 0f, 0f);
        tail.transform.localScale = new Vector3(0.06f, 0.25f, 0.06f);
        Tint(tail, furColor);
    }

    void BuildEar(Vector3 localPos)
    {
        var ear = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ear.name = "CatEar_Placeholder";
        ear.transform.SetParent(transform, false);
        ear.transform.localPosition = localPos;
        ear.transform.localScale = new Vector3(0.08f, 0.1f, 0.03f);
        ear.transform.localRotation = Quaternion.Euler(-20f, 0f, 0f);
        Tint(ear, furColor * 0.8f);
    }

    static void Tint(GameObject go, Color color)
    {
        var renderer = go.GetComponent<Renderer>();
        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", color);
        renderer.SetPropertyBlock(block);
    }
}

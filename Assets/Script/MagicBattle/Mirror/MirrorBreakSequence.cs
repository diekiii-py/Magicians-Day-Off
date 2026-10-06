using System.Collections;
using UnityEngine;

/// <summary>
/// Drives the "mirror suddenly shatters" beat. After <see cref="crackDuration"/> the intact
/// mirror visual is swapped out for a pile of physically-simulated shard fragments (generated
/// procedurally - no fractured 3D model required) and the cat is revealed behind it.
/// </summary>
public class MirrorBreakSequence : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("Seconds of calm reflection before the mirror starts to crack.")]
    public float crackDuration = 4f;
    [Tooltip("Seconds the crack SFX/visual plays before the mirror actually shatters.")]
    public float shatterDelay = 1.5f;

    [Header("References")]
    [Tooltip("The intact mirror GameObject (quad/plane using MirrorReflectionCamera + MIrrorMat).")]
    public GameObject intactMirrorVisual;
    [Tooltip("Optional static mesh renderer swapped in to show cracks before shattering.")]
    public GameObject crackedMirrorVisual;
    [Tooltip("Rect transform/plane bounds (in local space, meters) of the mirror surface used to scatter shards.")]
    public Vector2 mirrorSize = new Vector2(0.7f, 1.2f);
    public int shardCountX = 4;
    public int shardCountY = 6;
    [Tooltip("Material applied to generated shards. Leave empty to reuse the intact mirror's material.")]
    public Material shardMaterialOverride;

    [Header("Audio")]
    [Tooltip("Drag a short creak/crack sound here. The AudioSource is created automatically.")]
    public AudioClip crackClip;
    [Tooltip("Drag a glass-shatter sound here. The AudioSource is created automatically.")]
    public AudioClip shatterClip;

    [Header("Cat reveal")]
    [Tooltip("Cat GameObject, hidden behind the mirror until it breaks.")]
    public GameObject cat;

    AudioSource _audioSource;
    bool _started;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 1f;
    }

    public void BeginSequence()
    {
        if (_started) return;
        _started = true;
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        yield return new WaitForSeconds(crackDuration);

        GameEvents.RaiseMirrorCrackStart();
        if (crackClip != null) _audioSource.PlayOneShot(crackClip);
        if (crackedMirrorVisual != null) crackedMirrorVisual.SetActive(true);

        yield return new WaitForSeconds(shatterDelay);

        Shatter();
    }

    void Shatter()
    {
        if (shatterClip != null) _audioSource.PlayOneShot(shatterClip);
        if (intactMirrorVisual != null) intactMirrorVisual.SetActive(false);
        if (crackedMirrorVisual != null) crackedMirrorVisual.SetActive(false);

        SpawnShards();

        if (cat != null) cat.SetActive(true);
        GameEvents.RaiseCatRevealed();
        GameEvents.RaiseMirrorBroken();
    }

    void SpawnShards()
    {
        Material mat = shardMaterialOverride;
        if (mat == null && intactMirrorVisual != null)
        {
            var renderer = intactMirrorVisual.GetComponentInChildren<Renderer>();
            if (renderer != null) mat = renderer.sharedMaterial;
        }

        float cellW = mirrorSize.x / shardCountX;
        float cellH = mirrorSize.y / shardCountY;

        var shardsRoot = new GameObject("MirrorShards");
        shardsRoot.transform.SetPositionAndRotation(transform.position, transform.rotation);

        for (int ix = 0; ix < shardCountX; ix++)
        {
            for (int iy = 0; iy < shardCountY; iy++)
            {
                float localX = (ix + 0.5f) * cellW - mirrorSize.x * 0.5f;
                float localY = (iy + 0.5f) * cellH - mirrorSize.y * 0.5f;
                float localU0 = (float)ix / shardCountX;
                float localU1 = (float)(ix + 1) / shardCountX;
                float localV0 = (float)iy / shardCountY;
                float localV1 = (float)(iy + 1) / shardCountY;

                var shard = new GameObject($"Shard_{ix}_{iy}");
                shard.transform.SetParent(shardsRoot.transform, false);
                shard.transform.localPosition = new Vector3(localX, localY, 0f);

                var meshFilter = shard.AddComponent<MeshFilter>();
                meshFilter.mesh = BuildShardMesh(cellW, cellH, localU0, localU1, localV0, localV1);

                var meshRenderer = shard.AddComponent<MeshRenderer>();
                if (mat != null) meshRenderer.sharedMaterial = mat;

                var collider = shard.AddComponent<BoxCollider>();
                collider.size = new Vector3(cellW, cellH, 0.02f);

                var rb = shard.AddComponent<Rigidbody>();
                rb.mass = 0.05f;
                Vector3 outward = (shard.transform.position - transform.position).normalized;
                if (outward.sqrMagnitude < 0.001f) outward = transform.forward;
                rb.AddForce((outward * 0.6f + transform.forward * 1.2f) * Random.Range(0.6f, 1.4f), ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);

                shard.AddComponent<DestroyAfterSeconds>().seconds = 10f;
            }
        }
    }

    static Mesh BuildShardMesh(float width, float height, float u0, float u1, float v0, float v1)
    {
        var mesh = new Mesh { name = "MirrorShard" };
        float hw = width * 0.5f;
        float hh = height * 0.5f;

        mesh.vertices = new[]
        {
            new Vector3(-hw, -hh, 0f),
            new Vector3(hw, -hh, 0f),
            new Vector3(hw, hh, 0f),
            new Vector3(-hw, hh, 0f),
        };
        mesh.uv = new[]
        {
            new Vector2(u0, v0),
            new Vector2(u1, v0),
            new Vector2(u1, v1),
            new Vector2(u0, v1),
        };
        mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}

using UnityEngine;

/// <summary>
/// Executes the actual wall break: spawns the hole-frame occluder, scatters physics debris
/// around the opening and reveals the forest environment behind it.
/// </summary>
public class WallBreakEffect : MonoBehaviour
{
    [Header("Hole")]
    public Vector2 holeSize = new Vector2(1.4f, 2.0f);
    public Material frameMaterial;

    [Header("Debris")]
    public int debrisCount = 18;
    public float debrisScatterRadius = 1.0f;
    public Material debrisMaterial;

    [Header("Forest")]
    [Tooltip("Prefab or scene object placed/activated just behind the hole once it opens.")]
    public ForestEnvironmentBuilder forestEnvironment;
    public float forestDistanceBehindWall = 2.5f;

    [Header("Audio/FX")]
    [Tooltip("Drag a wall-break crash/rumble sound here. The AudioSource is created automatically.")]
    public AudioClip breakClip;
    [Tooltip("Optional - a dust/debris particle effect. Leave empty to skip.")]
    public ParticleSystem dustParticles;

    AudioSource _audioSource;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 1f;
    }

    public void Break(IWallLocator wallLocator)
    {
        if (wallLocator == null || !wallLocator.HasWall) return;

        Vector3 center = wallLocator.WallCenter;
        Vector3 normal = wallLocator.WallNormal;
        Vector2 wallSize = wallLocator.WallSize;
        Quaternion rotation = Quaternion.LookRotation(normal, Vector3.up);

        if (breakClip != null)
        {
            transform.position = center;
            _audioSource.PlayOneShot(breakClip);
        }
        if (dustParticles != null)
        {
            dustParticles.transform.SetPositionAndRotation(center, rotation);
            dustParticles.Play();
        }

        BuildFrame(center, rotation, wallSize);
        SpawnDebris(center, rotation);
        RevealForest(center, normal, rotation);

        GameEvents.RaiseWallBroken();
    }

    void BuildFrame(Vector3 center, Quaternion rotation, Vector2 wallSize)
    {
        var frameObj = new GameObject("WallHoleFrame");
        frameObj.transform.SetPositionAndRotation(center, rotation);

        var meshFilter = frameObj.AddComponent<MeshFilter>();
        meshFilter.mesh = ProceduralHoleFrameMesh.Build(wallSize, holeSize, Vector2.zero);

        var meshRenderer = frameObj.AddComponent<MeshRenderer>();
        if (frameMaterial != null) meshRenderer.sharedMaterial = frameMaterial;
    }

    void SpawnDebris(Vector3 center, Quaternion rotation)
    {
        var root = new GameObject("WallDebris");
        root.transform.SetPositionAndRotation(center, rotation);

        for (int i = 0; i < debrisCount; i++)
        {
            var chunk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chunk.name = $"DebrisChunk_{i}";
            chunk.transform.SetParent(root.transform, false);

            Vector2 edgePos = Random.insideUnitCircle.normalized * Random.Range(holeSize.x * 0.4f, holeSize.x * 0.4f + debrisScatterRadius);
            chunk.transform.localPosition = new Vector3(edgePos.x, edgePos.y, Random.Range(-0.05f, 0.05f));
            chunk.transform.localScale = Vector3.one * Random.Range(0.05f, 0.18f);
            chunk.transform.localRotation = Random.rotation;

            if (debrisMaterial != null)
            {
                var r = chunk.GetComponent<Renderer>();
                r.sharedMaterial = debrisMaterial;
            }

            var rb = chunk.AddComponent<Rigidbody>();
            rb.mass = Random.Range(0.3f, 1.2f);
            rb.AddForce((root.transform.forward * Random.Range(0.5f, 1.5f)) + Random.insideUnitSphere, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 3f, ForceMode.Impulse);

            chunk.AddComponent<DebrisPiece>();
        }
    }

    void RevealForest(Vector3 center, Vector3 normal, Quaternion rotation)
    {
        if (forestEnvironment == null) return;
        forestEnvironment.transform.SetPositionAndRotation(center + normal * forestDistanceBehindWall, rotation);
        forestEnvironment.gameObject.SetActive(true);
        forestEnvironment.BuildIfNeeded();
    }
}

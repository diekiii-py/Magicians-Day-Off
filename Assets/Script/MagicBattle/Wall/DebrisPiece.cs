using UnityEngine;

/// <summary>Marks a spawned rubble chunk and cleans it up once it has settled.</summary>
public class DebrisPiece : MonoBehaviour
{
    public float maxLifetime = 12f;
    public float settleSpeedThreshold = 0.03f;
    public float settleCheckDelay = 2f;

    Rigidbody _rb;
    float _spawnTime;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _spawnTime = Time.time;
    }

    void Update()
    {
        if (Time.time - _spawnTime > maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (Time.time - _spawnTime > settleCheckDelay && _rb != null && _rb.linearVelocity.sqrMagnitude < settleSpeedThreshold * settleSpeedThreshold)
        {
            _rb.isKinematic = true;
        }
    }
}

using UnityEngine;

/// <summary>Destroys this GameObject after a fixed delay. Used by procedurally spawned debris/shards.</summary>
public class DestroyAfterSeconds : MonoBehaviour
{
    public float seconds = 5f;

    void Start()
    {
        Destroy(gameObject, seconds);
    }
}

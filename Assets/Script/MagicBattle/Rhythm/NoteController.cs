using UnityEngine;

/// <summary>
/// A single flying spell orb. Travels in a straight line from its spawn point towards (and
/// slightly past) the hit zone; while inside the hit zone the player must be holding the
/// required gesture to score a hit, otherwise it counts as a miss once it passes by.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class NoteController : MonoBehaviour
{
    public GestureType requiredGesture;
    public float travelTime = 1.6f;
    public RhythmGameManager manager;

    Vector3 _start;
    Vector3 _end;
    float _elapsed;
    bool _inZone;
    bool _resolved;

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    public void Launch(Vector3 start, Vector3 end, GestureType gesture, float travel, RhythmGameManager owner)
    {
        _start = start;
        _end = end;
        requiredGesture = gesture;
        travelTime = Mathf.Max(0.1f, travel);
        manager = owner;
        transform.position = start;

        var renderer = GetComponent<Renderer>();
        var block = new MaterialPropertyBlock();
        block.SetColor(BaseColorId, HandGestureRecognizer.ColorForGesture(gesture));
        renderer.SetPropertyBlock(block);
    }

    void Update()
    {
        if (_resolved) return;

        _elapsed += Time.deltaTime;
        float t = _elapsed / travelTime;
        transform.position = Vector3.LerpUnclamped(_start, _end, t);

        if (_inZone && GestureMatches())
        {
            Resolve(hit: true);
            return;
        }

        if (t >= 1f)
        {
            Resolve(hit: false);
        }
    }

    bool GestureMatches()
    {
        if (manager == null) return false;
        return manager.PlayerIsPerforming(requiredGesture);
    }

    public void OnEnterHitZone() => _inZone = true;
    public void OnExitHitZone() => _inZone = false;

    void Resolve(bool hit)
    {
        _resolved = true;
        if (manager != null)
        {
            if (hit) manager.NotifyHit(this);
            else manager.NotifyMiss(this);
        }
        Destroy(gameObject, 0.05f);
    }
}

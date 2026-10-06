#if MAGICBATTLE_MRUK
using UnityEngine;

/// <summary>
/// Real wall detection using Meta's Mixed Reality Utility Kit (MRUK), the same room-scan data
/// used by Meta's "First Encounters" tech demo. Finds the wall anchor nearest to and roughly
/// behind <see cref="playerHead"/> and exposes it through <see cref="IWallLocator"/>.
///
/// This file only compiles when the "MAGICBATTLE_MRUK" scripting define symbol is set (Project
/// Settings > Player > Other Settings > Scripting Define Symbols), which you should only do
/// after installing the "Room Mesh" Building Block / com.meta.xr.sdk.mrutilitykit package - see
/// the setup guide. Until then the project builds fine using ManualWallLocator only.
/// </summary>
public class MRUKWallLocator : MonoBehaviour, IWallLocator
{
    public Transform playerHead;

    Vector3 _center;
    Vector3 _normal = Vector3.forward;
    Vector2 _size = new Vector2(2.5f, 2.5f);
    bool _hasWall;

    public Vector3 WallCenter => _center;
    public Vector3 WallNormal => _normal;
    public Vector2 WallSize => _size;
    public bool HasWall => _hasWall;

    void OnEnable()
    {
        if (MRUK.Instance != null)
        {
            MRUK.Instance.RegisterSceneLoadedListener(OnSceneLoaded);
        }
    }

    void OnSceneLoaded()
    {
        FindWallBehindPlayer();
    }

    void FindWallBehindPlayer()
    {
        var room = MRUK.Instance != null ? MRUK.Instance.GetCurrentRoom() : null;
        if (room == null || playerHead == null) return;

        MRUKAnchor best = null;
        float bestScore = float.NegativeInfinity;

        foreach (var anchor in room.WallAnchors)
        {
            if (anchor == null) continue;
            Vector3 toAnchor = (anchor.transform.position - playerHead.position);
            toAnchor.y = 0f;
            Vector3 behind = -playerHead.forward;
            behind.y = 0f;
            float score = Vector3.Dot(toAnchor.normalized, behind.normalized);
            if (score > bestScore)
            {
                bestScore = score;
                best = anchor;
            }
        }

        if (best == null) return;

        _center = best.transform.position;
        _normal = best.transform.forward;
        if (best.PlaneRect.HasValue)
        {
            _size = new Vector2(best.PlaneRect.Value.width, best.PlaneRect.Value.height);
        }
        _hasWall = true;
    }
}
#endif

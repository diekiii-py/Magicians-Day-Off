using UnityEngine;

/// <summary>
/// Describes the real wall that will be punched open. Implementations can either be set
/// manually (<see cref="ManualWallLocator"/>, always available) or, on a real Quest 3 with
/// a room scan, backed by Meta's MR Utility Kit scene anchors (<see cref="MRUKWallLocator"/>).
/// </summary>
public interface IWallLocator
{
    /// <summary>World-space centre of the wall opening point.</summary>
    Vector3 WallCenter { get; }
    /// <summary>World-space normal of the wall, pointing INTO the room (towards the player).</summary>
    Vector3 WallNormal { get; }
    /// <summary>Width/height of the wall surface in meters.</summary>
    Vector2 WallSize { get; }
    bool HasWall { get; }
}

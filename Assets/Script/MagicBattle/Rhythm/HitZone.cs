using UnityEngine;

/// <summary>Trigger volume in front of the player; forwards enter/exit events to notes passing through.</summary>
[RequireComponent(typeof(Collider))]
public class HitZone : MonoBehaviour
{
    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        var note = other.GetComponent<NoteController>();
        if (note != null) note.OnEnterHitZone();
    }

    void OnTriggerExit(Collider other)
    {
        var note = other.GetComponent<NoteController>();
        if (note != null) note.OnExitHitZone();
    }
}

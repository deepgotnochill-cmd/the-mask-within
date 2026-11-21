using System.Collections.Generic;
using UnityEngine;

// The authored player has both a CharacterController and a trigger sphere.
// A zone stays occupied until the last player collider leaves it.
internal sealed class PlayerOverlap
{
    private readonly HashSet<Collider> colliders = new HashSet<Collider>();
    public bool Any { get { colliders.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy); return colliders.Count > 0; } }
    public void Enter(Collider other) { if (other.CompareTag("Player")) colliders.Add(other); }
    public void Exit(Collider other) { colliders.Remove(other); }
    public void Clear() { colliders.Clear(); }
}

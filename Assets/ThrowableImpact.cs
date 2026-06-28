using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ThrowableImpact : MonoBehaviour
{
    [Tooltip("Minimalna prędkość uderzenia, by NPC zareagował.")]
    public float minImpactSpeed = 2f;

    void OnCollisionEnter(Collision col)
    {
        if (col.relativeVelocity.magnitude < minImpactSpeed) return;

        var npc = col.collider.GetComponentInParent<NpcThrowReaction>();
        if (npc != null)
            npc.OnHitByObject();
    }
}
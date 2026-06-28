using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerBumpDetector : MonoBehaviour
{
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Szukamy reakcji na NPC (także gdy collider jest na dziecku).
        var npc = hit.collider.GetComponentInParent<NpcBumpReaction>();
        if (npc != null)
            npc.OnBumped();
    }
}
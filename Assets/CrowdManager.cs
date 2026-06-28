using System.Collections.Generic;
using UnityEngine;


public class CrowdManager : MonoBehaviour
{
    public static CrowdManager Instance { get; private set; }

    [Header("Zasięg paniki")]
    [Tooltip("Promień, w jakim panika rozchodzi się od źródła (metry).")]
    public float panicRadius = 8f;

    [Tooltip("Jeśli true - panikę łapie cały tłum, niezależnie od odległości.")]
    public bool panicAll = false;

    private readonly List<NpcAgent> npcs = new List<NpcAgent>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Register(NpcAgent npc)
    {
        if (!npcs.Contains(npc)) npcs.Add(npc);
    }

    public void Unregister(NpcAgent npc)
    {
        npcs.Remove(npc);
    }


    public void TriggerPanic(Vector3 origin)
    {
        foreach (var npc in npcs)
        {
            if (npc == null) continue;
            if (panicAll || Vector3.Distance(npc.Position, origin) <= panicRadius)
            {
                npc.StartFleeing();
            }
        }
    }

    // Podgląd zasięgu w edytorze.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, panicRadius);
    }
}
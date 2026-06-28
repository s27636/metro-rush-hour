using UnityEngine;


public class NpcBumpReaction : MonoBehaviour
{
    [Header("Kwestie na zderzenie")]
    [TextArea] public string[] bumpLines =
    {
        "Uważaj jak chodzisz!",
        "Hej, patrz jak leziesz",
    
    };

    [Tooltip("Odstęp między reakcjami (s), żeby nie powtarzał w kółko.")]
    public float cooldown = 3f;

    private DialogueUI ui;
    private string npcName = "Pasażer";
    private float lastBump = -99f;

    void Start()
    {
        ui = FindObjectOfType<DialogueUI>();
        var dt = GetComponent<DialogueTrigger>();
        if (dt != null) npcName = dt.npcName; // użyj tego samego imienia co dialog
    }

    /// <summary>Wywoływane przez gracza przy zderzeniu.</summary>
    public void OnBumped()
    {
        if (Time.time - lastBump < cooldown) return;
        lastBump = Time.time;

        if (ui != null && bumpLines.Length > 0)
            ui.Show(npcName, bumpLines[Random.Range(0, bumpLines.Length)]);
    }
}
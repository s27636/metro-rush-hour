using UnityEngine;


public class NpcThrowReaction : MonoBehaviour
{
    [Header("Kwestie po trafieniu")]
    [TextArea] public string[] lines =
    {
        "Coś z tobą nie tak?!",
        "Oszalałeś?!"
    };

    private DialogueUI ui;
    private string npcName = "Pasażer";
    private bool reacted = false;

    void Start()
    {
        ui = FindObjectOfType<DialogueUI>();
        var dt = GetComponent<DialogueTrigger>();
        if (dt != null) npcName = dt.npcName;
    }

    public void OnHitByObject()
    {
        if (reacted) return; // reaguje tylko raz
        reacted = true;

        if (ui != null && lines.Length > 0)
            ui.Show(npcName, lines[Random.Range(0, lines.Length)]);

        var agent = GetComponent<NpcAgent>();
        if (agent != null) agent.StartFleeing();
    }
}
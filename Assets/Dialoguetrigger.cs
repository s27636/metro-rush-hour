using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public enum NpcType { Innocent, Bomber }

    [Header("Postać")]
    public string npcName = "Pasażer";
    public NpcType type = NpcType.Innocent;

    [Header("Kwestie")]
    [TextArea] public string[] innocentLines =
    {
        "Czego pan ode mnie chce?!",
        "Niech mnie pan zostawi w spokoju!"
    };
    [TextArea] public string[] bomberLines =
    {
        "...nie... to nie ja!",
        "*porzuca torbę i ucieka*"
    };

    [Header("Interakcja")]
    public float interactRange = 3f;
    public KeyCode interactKey = KeyCode.F;

    [Header("Bomber - opcjonalnie")]
    [Tooltip("Obiekt bomby porzucany przez zamachowca (aktywowany przy wykryciu).")]
    public GameObject droppedBomb;

    [Header("Debug")]
    public bool debug = true;

    private Camera playerCam;
    private Transform playerRoot;
    private DialogueUI ui;
    private bool used = false;

    void Start()
    {
        playerCam = Camera.main;
        ui = FindObjectOfType<DialogueUI>();

        if (playerCam != null)
        {
            var cc = playerCam.GetComponentInParent<CharacterController>();
            playerRoot = cc != null ? cc.transform : playerCam.transform.root;
        }

        if (debug && playerCam == null)
            Debug.LogWarning("[Dialogue] Camera.main = null! Oznacz kamerę gracza tagiem 'MainCamera'.", this);
        if (debug && ui == null)
            Debug.LogWarning("[Dialogue] Nie znaleziono DialogueUI na scenie! Dodaj komponent Dialogue UI do Canvasu.", this);
    }

    void Update()
    {
        if (used && type == NpcType.Bomber) return;
        if (Input.GetKeyDown(interactKey) && PlayerIsLookingAtMe())
        {
            Interact();
        }
    }

    bool PlayerIsLookingAtMe()
    {
        if (playerCam == null) return false;

        Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);
        if (debug) Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.cyan, 0.5f);

        RaycastHit[] hits = Physics.RaycastAll(ray, interactRange);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var h in hits)
        {
            if (playerRoot != null && (h.collider.transform == playerRoot || h.collider.transform.IsChildOf(playerRoot)))
                continue;

            bool isMe = h.collider.gameObject == gameObject || h.collider.transform.IsChildOf(transform);
            if (debug && Input.GetKeyDown(interactKey))
                Debug.Log($"[Dialogue] Patrzysz na '{h.collider.name}', to ja ({npcName}): {isMe}", h.collider.gameObject);
            return isMe;
        }
        return false;
    }

    void Interact()
    {
        if (type == NpcType.Innocent)
        {
            string line = innocentLines[Random.Range(0, innocentLines.Length)];
            if (ui != null) ui.Show(npcName, line);
        }
        else // Bomber
        {
            used = true;
            string line = bomberLines.Length > 0 ? bomberLines[bomberLines.Length - 1] : "...";
            if (ui != null) ui.Show(npcName, line);

            if (droppedBomb != null)
            {
                droppedBomb.transform.position = transform.position + Vector3.up * 0.2f;
                droppedBomb.SetActive(true);
                if (debug) Debug.Log("[Dialogue] Bomba upuszczona w pozycji zamachowcy.", droppedBomb);
            }
            else if (debug)
            {
                Debug.LogWarning("[Dialogue] Pole 'Dropped Bomb' jest puste - nie ma czego upuścić!", this);
            }

            if (CrowdManager.Instance != null)
                CrowdManager.Instance.TriggerPanic(transform.position);

            var self = GetComponent<NpcAgent>();
            if (self != null) self.StartFleeing();
        }
    }
}
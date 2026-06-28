using UnityEngine;


public class PickupThrow : MonoBehaviour
{
    [Header("Ustawienia")]
    [Tooltip("Maksymalny zasięg podnoszenia (metry)")]
    public float pickupRange = 3f;

    [Tooltip("Jak daleko przed kamerą trzymany jest przedmiot")]
    public float holdDistance = 1.5f;

    [Tooltip("Siła rzutu")]
    public float throwForce = 12f;

    [Tooltip("Tag obiektów, które można podnieść")]
    public string pickableTag = "Pickable";

    private Camera cam;
    private Rigidbody heldObject;
    private Transform playerRoot; // gracz, którego promień ma ignorować

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;

        // Znajdź korzeń gracza (obiekt z Character Controllerem), żeby pomijać go w raycaście.
        var cc = GetComponentInParent<CharacterController>();
        playerRoot = cc != null ? cc.transform : transform.root;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null) TryPickup();
            else Drop();
        }

        if (heldObject != null && Input.GetMouseButtonDown(0))
        {
            Throw();
        }
    }

    void FixedUpdate()
    {
        if (heldObject != null)
        {
            // Płynne przyciąganie przedmiotu do punktu przed kamerą.
            Vector3 target = cam.transform.position + cam.transform.forward * holdDistance;
            Vector3 toTarget = target - heldObject.position;
            heldObject.linearVelocity = toTarget * 10f; // im wyżej, tym sztywniej trzyma
        }
    }

    [Header("Debug")]
    [Tooltip("Wypisuje w Console, w co trafia promień i czego brakuje.")]
    public bool debug = true;

    void TryPickup()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        // Rysuje czerwoną linię promienia w widoku Scene (widoczne podczas Play, gdy Gizmos włączone).
        if (debug) Debug.DrawRay(ray.origin, ray.direction * pickupRange, Color.red, 1f);

        // RaycastAll + pomijanie samego gracza: bierzemy pierwsze trafienie, które NIE jest graczem.
        RaycastHit[] hits = Physics.RaycastAll(ray, pickupRange);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        bool found = false;
        RaycastHit hit = default;
        foreach (var h in hits)
        {
            if (playerRoot != null && (h.collider.transform == playerRoot || h.collider.transform.IsChildOf(playerRoot)))
                continue; // to gracz - pomiń
            hit = h;
            found = true;
            break;
        }

        if (!found)
        {
            if (debug) Debug.Log("[Pickup] Promień w nic (poza graczem) nie trafił. Zasięg: " + pickupRange);
            return;
        }

        // Rigidbody może być na rodzicu collidera - to OK.
        Rigidbody rb = hit.collider.attachedRigidbody;

        // Tag sprawdzamy na colliderze ALBO na obiekcie z Rigidbody (odporne na GLB).
        bool taggedCollider = hit.collider.CompareTag(pickableTag);
        bool taggedBody = rb != null && rb.CompareTag(pickableTag);

        if (debug)
        {
            Debug.Log($"[Pickup] Trafiono: '{hit.collider.name}' | tag collidera: '{hit.collider.tag}' " +
                      $"| Rigidbody: {(rb != null ? rb.name : "BRAK")} " +
                      $"| tag pasuje: {(taggedCollider || taggedBody)}", hit.collider.gameObject);
        }

        if (!taggedCollider && !taggedBody)
        {
            if (debug) Debug.LogWarning($"[Pickup] '{hit.collider.name}' nie ma taga '{pickableTag}'. Ustaw tag na obiekcie z colliderem LUB z Rigidbody.", hit.collider.gameObject);
            return;
        }

        if (rb == null)
        {
            if (debug) Debug.LogWarning($"[Pickup] '{hit.collider.name}' ma tag, ale BRAK Rigidbody (na nim lub na rodzicu). Dodaj Rigidbody.", hit.collider.gameObject);
            return;
        }

        heldObject = rb;
        heldObject.useGravity = false;
        heldObject.angularVelocity = Vector3.zero;
        heldObject.interpolation = RigidbodyInterpolation.Interpolate;
        if (debug) Debug.Log($"[Pickup] Podniesiono: {rb.name}", rb.gameObject);
    }

    void Drop()
    {
        if (heldObject == null) return;
        heldObject.useGravity = true;
        heldObject.linearVelocity = Vector3.zero;
        heldObject = null;
    }

    void Throw()
    {
        if (heldObject == null) return;
        Rigidbody rb = heldObject;
        heldObject = null;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(cam.transform.forward * throwForce, ForceMode.Impulse);
    }
}
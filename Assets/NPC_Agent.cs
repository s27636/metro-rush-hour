using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public class NpcAgent : MonoBehaviour
{
    public enum State { Patrol, Flee }

    [Header("Prędkości")]
    public float patrolSpeed = 1.4f;
    public float fleeSpeed = 4.5f;

    [Header("Patrol")]
    [Tooltip("Punkty, między którymi NPC chodzi. Jeśli puste - błąka się losowo.")]
    public List<Transform> patrolPoints = new List<Transform>();
    [Tooltip("Losowy czas postoju w punkcie (min, max sekundy) - dla naturalności.")]
    public Vector2 waitAtPoint = new Vector2(1.5f, 4f);

    private float currentWait = 2f;

    [Header("Ucieczka")]
    [Tooltip("Punkty wyjścia/ucieczki. NPC biegnie do najbliższego.")]
    public List<Transform> exitPoints = new List<Transform>();

    private enum Phase { Moving, Waiting }

    private NavMeshAgent agent;
    private Animator anim;
    private State state = State.Patrol;
    private Phase phase = Phase.Moving;
    private int currentPoint = 0;
    private float waitTimer = 0f;
    private float departTimer = 0f; // czas od wyruszenia (chroni przed fałszywym "dotarłem")

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        agent.speed = patrolSpeed;

        // Ruchem steruje NavMesh, NIE animacja - wymuś wyłączenie Root Motion
        // (gdy włączone, postać po jednym cyklu potrafi "zamarznąć").
        if (anim != null) anim.applyRootMotion = false;

        // Bez dystansu zatrzymania agent "orbituje" wokół punktu - wymuś minimalny.
        if (agent.stoppingDistance < 0.3f) agent.stoppingDistance = 0.5f;
        agent.autoBraking = true;

        // Zarejestruj się w managerze tłumu, żeby reagować na panikę.
        if (CrowdManager.Instance != null)
            CrowdManager.Instance.Register(this);

        GoToNextPatrolPoint();
    }

    void Update()
    {
        // Aktualizuj animator
        if (anim != null)
        {
            anim.SetFloat("speed", agent.velocity.magnitude);
            anim.SetBool("isFleeing", state == State.Flee);
        }

        if (state == State.Patrol) UpdatePatrol();
        else if (state == State.Flee) UpdateFlee();
    }

    void UpdatePatrol()
    {
        if (phase == Phase.Waiting)
        {
            // Stoi w punkcie i odlicza postój.
            waitTimer += Time.deltaTime;
            if (waitTimer >= currentWait)
            {
                waitTimer = 0f;
                GoToNextPatrolPoint();
            }
            return;
        }

        // phase == Moving
        departTimer += Time.deltaTime;
        if (departTimer < 0.3f) return;      // chwila na policzenie ścieżki i ruszenie
        if (agent.pathPending) return;       // ścieżka jeszcze liczona

        // Dotarł do celu?
        if (agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            phase = Phase.Waiting;
            agent.isStopped = true;
            agent.velocity = Vector3.zero;   // zeruje prędkość -> przejście w Idle
            waitTimer = 0f;
            currentWait = Random.Range(waitAtPoint.x, waitAtPoint.y);
        }
    }

    void GoToNextPatrolPoint()
    {
        // Wznów ruch i wyzeruj licznik wyruszenia.
        phase = Phase.Moving;
        departTimer = 0f;
        agent.isStopped = false;

        if (patrolPoints.Count > 0)
        {
            currentPoint = (currentPoint + 1) % patrolPoints.Count;
            agent.SetDestination(patrolPoints[currentPoint].position);
        }
        else
        {
            // Brak punktów - losowy punkt w pobliżu (błąkanie się).
            Vector3 random = transform.position + Random.insideUnitSphere * 6f;
            if (NavMesh.SamplePosition(random, out NavMeshHit hit, 6f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }
    }

    void UpdateFlee()
    {
        departTimer += Time.deltaTime;
        if (departTimer < 0.5f) return;   // chwila na ruszenie (chroni przed natychmiastowym despawnem)
        if (agent.pathPending) return;

        // Dobiegł do wyjścia - znika ze sceny.
        if (agent.remainingDistance <= agent.stoppingDistance + 0.3f)
        {
            if (CrowdManager.Instance != null) CrowdManager.Instance.Unregister(this);
            Destroy(gameObject);
        }
    }

    /// <summary>Wywoływane przez CrowdManager. NPC wpada w panikę i ucieka.</summary>
    public void StartFleeing()
    {
        if (state == State.Flee) return;
        state = State.Flee;
        agent.speed = fleeSpeed;
        agent.isStopped = false; // na wypadek, gdyby stał na postoju
        phase = Phase.Moving;
        departTimer = 0f;

        Transform exit = GetNearestExit();
        if (exit != null)
            agent.SetDestination(exit.position);
        else
        {
            // Brak wyjść - po prostu biegnij "od środka".
            Vector3 away = (transform.position - Vector3.zero).normalized * 20f;
            agent.SetDestination(transform.position + away);
        }
    }

    Transform GetNearestExit()
    {
        Transform nearest = null;
        float best = Mathf.Infinity;
        foreach (var e in exitPoints)
        {
            if (e == null) continue;
            float d = Vector3.Distance(transform.position, e.position);
            if (d < best) { best = d; nearest = e; }
        }
        return nearest;
    }

    public Vector3 Position => transform.position;
}
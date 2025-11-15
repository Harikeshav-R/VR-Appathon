using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class WanderAI : MonoBehaviour
{
    private static readonly int Speed = Animator.StringToHash("Speed");

    [Header("Wander Settings")] [SerializeField]
    private float wanderRadius = 10f;

    [SerializeField] private float wanderTimer = 5f;

    [Header("References")] [SerializeField]
    private NavMeshAgent agent;

    [SerializeField] private Animator animator; // Reference to the Animator

    private float _timer;

    private Vector3 _wanderCenter;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        _wanderCenter = transform.position;
        _timer = wanderTimer;
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= wanderTimer)
        {
            var newPos = GetRandomNavMeshPoint(_wanderCenter, wanderRadius);
            agent.SetDestination(newPos);
            _timer = 0; // Reset the timer
        }

        // --- ANIMATOR LOGIC ---
        var currentSpeed = agent.velocity.magnitude;

        var maxSpeed = agent.speed;

        var normalizedSpeed = currentSpeed / maxSpeed;

        animator.SetFloat(Speed, normalizedSpeed);
    }

    /// <summary>
    ///     Finds a random valid point on the NavMesh within a given radius.
    /// </summary>
    public static Vector3 GetRandomNavMeshPoint(Vector3 center, float radius)
    {
        var randomDirection = Random.insideUnitSphere * radius;
        randomDirection += center;

        return NavMesh.SamplePosition(randomDirection, out var hit, radius, NavMesh.AllAreas) ? hit.position : center;
    }
}
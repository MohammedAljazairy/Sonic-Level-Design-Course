using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMotobug : MonoBehaviour
{
    public enum AIState { Patrol, Chase, Wait }
    public AIState currentState = AIState.Patrol;

    [Header("Targeting")]
    public Transform player;
    public float detectionRadius = 15f;
    
    [Header("Movement Speeds")]
    public float patrolSpeed = 4f;   
    public float chaseSpeed = 18f;   
    
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    public AudioClip destroyS;
    [Header("Timers")]
    public float waitTimeAfterHit = 2f;
    private float waitTimer = 0f;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        if (player == null) 
            player = FindFirstObjectByType<ImprovedPlayerController>().transform;

        GoToNextPatrolPoint();
    }

    void Update()
    {
        // NEW: If Sonic is frozen (Win/Game Over), the Motobug freezes too!
        if (player != null)
        {
            ImprovedPlayerController ipc = player.GetComponent<ImprovedPlayerController>();
            if (ipc != null && !ipc.canControl)
            {
                if (agent != null) agent.isStopped = true;
                return;
            }
        }

        switch (currentState)
        {
            case AIState.Patrol:
                PatrolLogic();
                break;
            case AIState.Chase:
                ChaseLogic();
                break;
            case AIState.Wait:
                WaitLogic();
                break;
        }
    }

    private void PatrolLogic()
    {
        agent.isStopped = false; 
        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextPatrolPoint();
        }

        if (Vector3.Distance(transform.position, player.position) <= detectionRadius)
        {
            currentState = AIState.Chase;
        }
    }

    private void ChaseLogic()
    {
        agent.isStopped = true;

        Vector3 targetPos = player.position;
        targetPos.y = transform.position.y;

        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetPos, 
            chaseSpeed * Time.deltaTime
        );

        Vector3 lookDir = (targetPos - transform.position).normalized;
        if (lookDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, player.position) > detectionRadius * 1.5f)
        {
            currentState = AIState.Patrol;
            GoToNextPatrolPoint();
        }
    }

    private void WaitLogic()
    {
        agent.isStopped = true;

        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0f)
        {
            currentState = AIState.Chase;
        }
    }

    private void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    private void OnCollisionEnter(Collision collision)
    {
        ImprovedPlayerController sonic = collision.gameObject.GetComponent<ImprovedPlayerController>();
        
        if (sonic != null)
        {
            if (sonic.IsInBallForm)
            {
                if (destroyS != null) Camera.main.GetComponent<AudioSource>().PlayOneShot(destroyS);
                Destroy(gameObject);
            }
            else
            {
                sonic.TakeDamage(transform.position);
                currentState = AIState.Wait;
                waitTimer = waitTimeAfterHit;
            }
        }
    }
}
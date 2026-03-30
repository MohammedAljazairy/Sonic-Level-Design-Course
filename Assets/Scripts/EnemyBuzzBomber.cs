using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class EnemyBuzzBomber : MonoBehaviour
{
    public enum AIState { Patrol, Chase, Shoot, Cooldown }
    public AIState currentState = AIState.Patrol;

    [Header("Targeting")]
    public Transform player;
    public float detectionRadius = 25f;
    public float attackDistance = 12f;  
    public float hoverHeight = 4f;      

    [Header("Movement Speeds")]
    public float patrolSpeed = 5f;
    public float chaseSpeed = 15f;
    public float turnSpeed = 5f;
    public AudioClip destroyS;
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    [Header("Combat Settings")]
    public GameObject projectilePrefab; 
    public Transform firePoint;         
    public float cooldownTime = 3f;     
    private float cooldownTimer = 0f;

    void Start()
    {
        if (player == null)
            player = FindFirstObjectByType<ImprovedPlayerController>().transform;
    }

    void Update()
    {
        // NEW: If Sonic is frozen (Win/Game Over), the BuzzBomber freezes too!
        if (player != null)
        {
            ImprovedPlayerController ipc = player.GetComponent<ImprovedPlayerController>();
            if (ipc != null && !ipc.canControl) return;
        }

        switch (currentState)
        {
            case AIState.Patrol:
                PatrolLogic();
                break;
            case AIState.Chase:
                ChaseLogic();
                break;
            case AIState.Shoot:
                ShootLogic();
                break;
            case AIState.Cooldown:
                CooldownLogic();
                break;
        }
    }

    private void PatrolLogic()
    {
        if (patrolPoints.Length > 0)
        {
            Transform target = patrolPoints[currentPatrolIndex];
            
            transform.position = Vector3.MoveTowards(transform.position, target.position, patrolSpeed * Time.deltaTime);
            LookAtTarget(target.position);

            if (Vector3.Distance(transform.position, target.position) < 0.5f)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            }
        }

        if (Vector3.Distance(transform.position, player.position) <= detectionRadius)
        {
            currentState = AIState.Chase;
        }
    }

    private void ChaseLogic()
    {
        float distanceToSonic = Vector3.Distance(transform.position, player.position);

        Vector3 hoverTarget = player.position + (Vector3.up * hoverHeight);

        if (distanceToSonic > attackDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, hoverTarget, chaseSpeed * Time.deltaTime);
        }
        else
        {
            currentState = AIState.Shoot;
        }

        LookAtTarget(player.position);

        if (distanceToSonic > detectionRadius * 1.5f)
        {
            currentState = AIState.Patrol;
        }
    }

    private void ShootLogic()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        }

        currentState = AIState.Cooldown;
        cooldownTimer = cooldownTime;
    }

    private void CooldownLogic()
    {
        LookAtTarget(player.position);

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f)
        {
            currentState = AIState.Chase;
        }
    }

    private void LookAtTarget(Vector3 targetPos)
    {
        Vector3 lookDir = (targetPos - transform.position).normalized;
        if (lookDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
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
            }
        }
    }
}
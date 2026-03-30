using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class EnemyBuzzBomber : MonoBehaviour
{
    public enum AIState { Patrol, Chase, Shoot, Cooldown }
    public AIState currentState = AIState.Patrol;

    [Header("Targeting")]
    public Transform player;
    public float detectionRadius = 25f;
    public float attackDistance = 12f;  // How close it gets before shooting
    public float hoverHeight = 4f;      // How high above Sonic it hovers

    [Header("Movement Speeds")]
    public float patrolSpeed = 5f;
    public float chaseSpeed = 15f;
    public float turnSpeed = 5f;
    public AudioClip destroyS;
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    [Header("Combat Settings")]
    public GameObject projectilePrefab; // Drag your laser prefab here
    public Transform firePoint;         // An empty GameObject at the tip of the enemy's gun
    public float cooldownTime = 3f;     // Time between shots
    private float cooldownTimer = 0f;

    void Start()
    {
        if (player == null)
            player = FindFirstObjectByType<ImprovedPlayerController>().transform;
    }

    void Update()
    {
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
            
            // Fly towards the patrol point
            transform.position = Vector3.MoveTowards(transform.position, target.position, patrolSpeed * Time.deltaTime);
            LookAtTarget(target.position);

            if (Vector3.Distance(transform.position, target.position) < 0.5f)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            }
        }

        // Do we see Sonic?
        if (Vector3.Distance(transform.position, player.position) <= detectionRadius)
        {
            currentState = AIState.Chase;
        }
    }

    private void ChaseLogic()
    {
        float distanceToSonic = Vector3.Distance(transform.position, player.position);

        // We want to hover *above* Sonic
        Vector3 hoverTarget = player.position + (Vector3.up * hoverHeight);

        // If we are too far, fly closer
        if (distanceToSonic > attackDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, hoverTarget, chaseSpeed * Time.deltaTime);
        }
        else
        {
            // We are in range! Time to shoot.
            currentState = AIState.Shoot;
        }

        LookAtTarget(player.position);

        // Sonic ran away
        if (distanceToSonic > detectionRadius * 1.5f)
        {
            currentState = AIState.Patrol;
        }
    }

    private void ShootLogic()
    {
        // Spawn the projectile and point it at Sonic
        if (projectilePrefab != null && firePoint != null)
        {
            Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        }

        currentState = AIState.Cooldown;
        cooldownTimer = cooldownTime;
    }

    private void CooldownLogic()
    {
        // Just hover in place and stare at Sonic while reloading
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

    // --- Physical Collision (If Sonic jumps into the BuzzBomber) ---
    private void OnCollisionEnter(Collision collision)
    {
        ImprovedPlayerController sonic = collision.gameObject.GetComponent<ImprovedPlayerController>();
        
        if (sonic != null)
        {
            if (sonic.IsInBallForm)
            {
                AudioSource.PlayClipAtPoint(destroyS,transform.position);
                Destroy(gameObject); // Sonic destroyed the enemy!
            }
            else
            {
                sonic.TakeDamage(transform.position); // Sonic got hurt!
            }
        }
    }

    // private void OnDrawGizmosSelected()
    // {
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireSphere(transform.position, attackDistance);
    //     Gizmos.color = Color.yellow;
    //     Gizmos.DrawWireSphere(transform.position, detectionRadius);
    // }
}
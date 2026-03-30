using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))] 
public class ImprovedPlayerController : MonoBehaviour
{
    [Header("Models")]
    public GameObject sonicModel;
    public GameObject ballModel;

    [Header("Ring Spilling")]
    public GameObject spilledRingPrefab; 
    public int maxSpilledRings = 32;    
    public AudioClip loseRingA;

    [Header("Momentum Settings")]
    public float acceleration = 15f;
    public float deceleration = 20f;
    public float maxSpeed = 30f;
    public float turnSpeed = 15f;
    public AudioClip jumpA;

    [Header("Jump & Physics")]
    public float jumpForce = 10f;
    public float customGravityMultiplier = 2f; 
    
    [Header("Ground Check (CheckSphere)")]
    public float groundCheckRadius = 0.2f; 
    public Vector3 groundCheckOffset = new Vector3(0f, 0.1f, 0f); 
    public LayerMask groundMask; 

    [Header("Spin Dash & Boost")]
    public float spinChargeRate = 15f;
    public float maxSpinCharge = 50f;
    public float ballRotationMultiplier = 30f;
    public float boostDuration = 1.5f; 

    [Header("State Parameters")]
    public float idleTimeThreshold = 10f; 
    public bool isDamaged = false; 
    public AudioClip SpindashS1;
    public AudioClip SpindashS2;
    
    public bool canControl = true; // NEW: Controls if Sonic is allowed to move!

    private Rigidbody rb;
    private Animator animator;
    private Transform camTransform;

    private Vector3 moveDirection;
    private float currentSpeed = 0f;
    private float currentSpinCharge = 0f;
    private float currentBoostTimer = 0f; 
    private float idleTimer = 0f; 
    private float jumpCooldownTimer = 0f; 
    
    private bool isGrounded;
    private bool isSpinDashing = false;
    private bool jumpRequested = false;
    private bool isJumping = false; 

    public bool IsInBallForm 
    { 
        get { return isSpinDashing || currentBoostTimer > 0f || isJumping; } 
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (sonicModel != null) animator = sonicModel.GetComponent<Animator>(); 
        if (Camera.main != null) camTransform = Camera.main.transform;
    }

    void Update()
    {
        if (!canControl) return; // Freezes Update if Game Over or Win

        if (jumpCooldownTimer > 0f) jumpCooldownTimer -= Time.deltaTime;

        CheckGrounded();
        HandleInputsAndStates();
        HandleModelSwappingAndVisuals();
    }

    void FixedUpdate()
    {
        if (!canControl) return; // Freezes physics if Game Over or Win

        ApplyMomentumAndRotation(); 
        ApplyCustomGravity();

        if (jumpRequested)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); 
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpRequested = false;
        }
    }

    private void CheckGrounded()
    {
        Vector3 spherePosition = transform.position + groundCheckOffset;
        isGrounded = Physics.CheckSphere(spherePosition, groundCheckRadius, groundMask);

        if (isGrounded && rb.linearVelocity.y <= 0.1f && jumpCooldownTimer <= 0f)
        {
            isJumping = false;
        }
    }

    private void HandleInputsAndStates()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        bool hasMovementInput = Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f;

        if (!hasMovementInput && currentSpeed < 0.11f && isGrounded && !Input.GetKey(KeyCode.F) && !isDamaged)
            idleTimer += Time.deltaTime;
        else
            idleTimer = 0f; 

        if (currentBoostTimer > 0f) currentBoostTimer -= Time.deltaTime; 

        if (animator != null) animator.SetBool("isCrouch", false);

        if (Input.GetKeyDown(KeyCode.F) && isGrounded && currentSpeed > 5f && !isDamaged)
        {
            currentBoostTimer = boostDuration; 
            Camera.main.GetComponent<AudioSource>().PlayOneShot(SpindashS1);
        }
        else if (Input.GetKey(KeyCode.F) && isGrounded && currentSpeed <= 5f && currentBoostTimer <= 0f && !isDamaged)
        {
            currentSpeed -= (deceleration * 4f) * Time.deltaTime;
            if (currentSpeed < 0.11f) currentSpeed = 0.1f; 

            if (!isSpinDashing && currentSpeed <= 0.11f)
            {
                if (animator != null)
                {
                    animator.SetBool("isCrouch", true);
                    animator.SetBool("isRunning", false); 
                }
            }

            if (Input.GetKeyDown(KeyCode.Space) && currentSpeed <= 0.11f)
            {
                isSpinDashing = true;
                if (animator != null) animator.SetBool("isCrouch", false); 
                
                if (SpindashS1 != null) Camera.main.GetComponent<AudioSource>().PlayOneShot(SpindashS1);
                
                currentSpinCharge += spinChargeRate;
                currentSpinCharge = Mathf.Clamp(currentSpinCharge, 0, maxSpinCharge);
            }
            return; 
        }
        else if (isSpinDashing)
        {
            isSpinDashing = false;
            if (SpindashS2 != null) Camera.main.GetComponent<AudioSource>().PlayOneShot(SpindashS2);
            currentSpeed = Mathf.Max(currentSpinCharge, 15f); 
            currentSpinCharge = 0f;
            currentBoostTimer = boostDuration;

            if (moveDirection == Vector3.zero) moveDirection = transform.forward;
        }

        if (isDamaged) return; 

        Vector3 inputDir = Vector3.zero;
        if (camTransform != null)
        {
            Vector3 camForward = camTransform.forward;
            Vector3 camRight = camTransform.right;
            camForward.y = 0f; camRight.y = 0f;
            camForward.Normalize(); camRight.Normalize();
            inputDir = (camForward * z + camRight * x).normalized;
        }

        if (currentBoostTimer > 0f)
        {
            currentSpeed -= (deceleration * 0.1f) * Time.deltaTime; 
            
            if (inputDir.magnitude >= 0.1f)
                moveDirection = Vector3.MoveTowards(moveDirection, inputDir, (turnSpeed * 0.1f) * Time.deltaTime).normalized;
        }
        else
        {
            if (inputDir.magnitude >= 0.1f)
            {
                float turnAmount = Vector3.Dot(moveDirection, inputDir);
                
                if (turnAmount < -0.1f) 
                {
                    currentSpeed -= (deceleration * 3f) * Time.deltaTime;
                    if (currentSpeed < 0.1f) currentSpeed = 0f;
                }
                else
                {
                    currentSpeed += acceleration * Time.deltaTime;
                }

                float currentSteerSpeed = Mathf.Lerp(turnSpeed, turnSpeed * 0.5f, currentSpeed / maxSpeed); 
                if (moveDirection == Vector3.zero) moveDirection = inputDir; 
                moveDirection = Vector3.MoveTowards(moveDirection, inputDir, currentSteerSpeed * Time.deltaTime).normalized;
            }
            else
            {
                currentSpeed -= deceleration * Time.deltaTime;
            }
        }

        float activeSpeedLimit = (currentBoostTimer > 0f) ? Mathf.Max(maxSpeed, maxSpinCharge) : maxSpeed;
        currentSpeed = Mathf.Clamp(currentSpeed, 0, activeSpeedLimit);

        if (animator != null) 
        {
            animator.SetBool("isRunning", currentSpeed > 0.11f && !isSpinDashing && currentBoostTimer <= 0f && !Input.GetKey(KeyCode.F));
            animator.SetBool("ThatsEnough", idleTimer >= idleTimeThreshold);
            animator.SetBool("isDamage", isDamaged);
            animator.SetBool("isFalling", !isGrounded);
            
            animator.SetFloat("CurrentSpeed", currentSpeed);

            float speedRatio = currentSpeed / maxSpeed;
            float animSpeedMultiplier = Mathf.Lerp(1f, 1.8f, speedRatio);
            animator.SetFloat("RunSpeed", animSpeedMultiplier);
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !Input.GetKey(KeyCode.F) && !isDamaged)
        {
            jumpRequested = true;
            if (jumpA != null) Camera.main.GetComponent<AudioSource>().PlayOneShot(jumpA);
            isJumping = true; 
            jumpCooldownTimer = 0.1f; 
            currentBoostTimer = 0f; 
        }
    }

    private void ApplyMomentumAndRotation()
    {
        if (isDamaged) return; 
        if (isSpinDashing || (Input.GetKey(KeyCode.F) && currentSpeed <= 5f)) return; 
        
        Vector3 targetVelocity = moveDirection * currentSpeed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

        if (currentSpeed > 0.1f && moveDirection != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDirection);
            Quaternion smoothRot = Quaternion.Slerp(rb.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(smoothRot);
        }
    }

    private void ApplyCustomGravity()
    {
        if (!isGrounded && rb.linearVelocity.y < 0)
        {
            rb.AddForce(Physics.gravity * customGravityMultiplier, ForceMode.Acceleration);
        }
    }

    private void HandleModelSwappingAndVisuals()
    {
        bool isBoosting = currentBoostTimer > 0f;
        bool shouldBeBall = isSpinDashing || isBoosting || isJumping;

        if (sonicModel != null && ballModel != null)
        {
            sonicModel.SetActive(!shouldBeBall);
            ballModel.SetActive(shouldBeBall);
        }

        if (shouldBeBall && ballModel != null)
        {
            float visualSpinSpeed = isSpinDashing ? currentSpinCharge : currentSpeed;
            if (!isGrounded && !isSpinDashing && !isBoosting && visualSpinSpeed < 10f) visualSpinSpeed = 15f;
            ballModel.transform.Rotate(0, 0, visualSpinSpeed * ballRotationMultiplier * Time.deltaTime);
        }
    }

    public void TakeDamage(Vector3 hazardPosition)
    {
        if (isDamaged) return;
        
        if (loseRingA != null) Camera.main.GetComponent<AudioSource>().PlayOneShot(loseRingA);
        
        PlayerScore score = GetComponent<PlayerScore>();
        if (score != null)
        {
            int droppedRings = score.LoseAllRings();
            
            if (droppedRings > 0)
            {
                SpillRings(droppedRings);
            }
            else
            {
                isDamaged = true;
                Invoke("RecoverFromDamage", 1.5f);
                PlayerRespawn respawn = GetComponent<PlayerRespawn>();
                if (respawn != null) respawn.TakeLethalDamage(); 
                return;
            }
        }

        isDamaged = true;
        currentSpeed = 0f; 
        currentBoostTimer = 0f;
        isSpinDashing = false;

        if (animator != null)
        {
            animator.SetBool("isDamage", true);
            animator.SetBool("isRunning", false);
            animator.SetBool("isCrouch", false);
        }

        Vector3 knockbackDir = (transform.position - hazardPosition).normalized;
        knockbackDir.y = 1f; 
        
        rb.linearVelocity = Vector3.zero; 
        rb.AddForce(knockbackDir * 15f, ForceMode.Impulse); 

        Invoke("RecoverFromDamage", 1.5f);
    }

    private void SpillRings(int amountToDrop)
    {
        if (spilledRingPrefab == null) return;
        
        int ringsToSpawn = Mathf.Min(amountToDrop, maxSpilledRings);
        float angleStep = 360f / ringsToSpawn;

        for (int i = 0; i < ringsToSpawn; i++)
        {
            Vector3 spawnPos = transform.position + (Vector3.up * 1f);
            GameObject ring = Instantiate(spilledRingPrefab, spawnPos, Quaternion.identity);
            
            float angle = i * angleStep;
            Vector3 throwDir = Quaternion.Euler(0, angle, 0) * transform.forward;
            throwDir.y = 1f; 
            throwDir.Normalize();

            Rigidbody ringRb = ring.GetComponent<Rigidbody>();
            if (ringRb != null)
            {
                float force = Random.Range(1f,1.5f);
                ringRb.AddForce(throwDir * force, ForceMode.Impulse);
            }
        }
    }

    private void RecoverFromDamage()
    {
        isDamaged = false;
        if (animator != null) animator.SetBool("isDamage", false);
    }

    public void ResetMomentum()
    {
        if (rb != null) rb.linearVelocity = Vector3.zero;
        currentSpeed = 0f;
        moveDirection = Vector3.zero;
        currentSpinCharge = 0f;
        currentBoostTimer = 0f;
        isSpinDashing = false;
        jumpRequested = false;
        
        isDamaged = false;
        if (animator != null) animator.SetBool("isDamage", false);
    }

    // NEW: Call this when Game Over or Game Won to freeze Sonic!
    public void LockControls()
    {
        canControl = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true; 
        }
        if (animator != null) animator.speed = 0; 
    }
}
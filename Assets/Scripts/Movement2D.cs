using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FirstGearGames.SmoothCameraShaker;

public class Movement2D : MonoBehaviour
{
    #region Header: Components
    [Header("Components")]
    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D playerCollider;
    #endregion

    #region Header: Layer Masks
    [Header("Layer Masks")]
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private LayerMask enemyLayer;
    #endregion

    #region Header: Movement Variables
    [Header("Movement Variables")]
    [SerializeField] private float movementAcceleration = 70f;
    [SerializeField] private float maxMoveSpeed = 12f;
    [SerializeField] private float groundLinearDrag = 7f;
    private float horizontalDirection;
    private float verticalDirection;
    private bool changingDirection => (rb.velocity.x > 0f && horizontalDirection < 0f) || (rb.velocity.x < 0f && horizontalDirection > 0f);
    private bool facingRight = true;
    #endregion

    #region Header: Jump Variables
    [Header("Jump Variables")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float airLinearDrag = 2.5f;
    [SerializeField] private float fallMultiplier = 8f;
    [SerializeField] private float lowJumpFallMultiplier = 5f;
    [SerializeField] private float downMultiplier = 12f;
    [SerializeField] private int extraJumps = 1;
    [SerializeField] private float hangTime = .1f;
    [SerializeField] private float jumpBufferLength = .1f;
    private int extraJumpsValue;
    private float hangTimeCounter;
    private float jumpBufferCounter;
    private bool canJump => jumpBufferCounter > 0f && (hangTimeCounter > 0f || extraJumpsValue > 0);
    private bool isJumping = false;
    private bool isGroundedState;
    #endregion

    #region Header: Dash Variables
    [Header("Dash Variables")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashLength = .3f;
    [SerializeField] private float dashBufferLength = .1f;
    [SerializeField] private float dashCooldown = 2f;
    [SerializeField] private int dashDamage = 1;
    [SerializeField] private ShakeData dashShake;
    private float dashCooldownTimer = 0f;
    private float dashBufferCounter;
    private bool isDashing;
    private bool hasDashed;
    private bool canDash => dashBufferCounter > 0f && !hasDashed && dashCooldownTimer <= 0f;
    #endregion

    #region Header: Long Dash Variables
    [Header("Long Dash Variables")]
    [SerializeField] private float longDashSpeed = 25f;
    [SerializeField] private float longDashLength = 0.6f;
    [SerializeField] private float longDashBufferLength = 0.1f;
    [SerializeField] private float longDashCooldown = 3f;
    [SerializeField] private ShakeData longDashShake;
    [SerializeField] private int longDashDamage = 2;
    private bool longDashKillConfirmed = false;
    private TrailRenderer longDashTrail;
    private float longDashCooldownTimer = 0f;
    private float longDashBufferCounter;
    private bool isLongDashing;
    public bool IsLongDashing => isLongDashing;
    private bool hasLongDashed;
    private bool canLongDash => longDashBufferCounter > 0f && !hasLongDashed && longDashCooldownTimer <= 0f;
    private Ghost ghostScript;
    private List<Enemy> enemiesHitDuringLongDash = new List<Enemy>();
    #endregion

    #region Header: Ground Collision Variables
    [Header("Ground Collision Variables")]
    [SerializeField] private float distanceGroundCheck;
    [SerializeField] private Vector2 boxSize;
    private RaycastHit2D[] groundCheckResults = new RaycastHit2D[1];
    #endregion

    #region Header: Wall Collision Variables
    [Header("Wall Collision Variables")]
    private Vector2 lastDashDirection = Vector2.zero;
    #endregion

    #region Header: UI
    [Header("UI")]
    [SerializeField] private Image longDashFillImage;
    [SerializeField] private Image DashFillImage;
    [SerializeField] private GameObject[] heartIcons; // เพิ่ม Array สำหรับเก็บ GameObject หัวใจ
    #endregion
    
    #region UI Methods // เพิ่ม Header ใหม่สำหรับ UI methods
    private void UpdateHeartsUI()
    {
        // ตรวจสอบให้แน่ใจว่าจำนวนหัวใจใน UI ไม่เกินจำนวนหัวใจสูงสุดที่เรามีใน Array
        int heartsToShow = Mathf.Clamp(hp, 0, heartIcons.Length);

        // วนลูปเพื่อเปิด/ปิดการแสดงผลของหัวใจ
        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (heartIcons[i] != null) // ตรวจสอบว่ามี GameObject ในตำแหน่งนี้หรือไม่
            {
                // ถ้า Index (i) น้อยกว่าจำนวน HP ปัจจุบัน ให้เปิดการแสดงผล
                if (i < heartsToShow)
                {
                    heartIcons[i].SetActive(true);
                }
                // ถ้า Index (i) มากกว่าหรือเท่ากับจำนวน HP ปัจจุบัน ให้ปิดการแสดงผล
                else
                {
                    heartIcons[i].SetActive(false);
                }
            }
        }
    }
    #endregion
    
    #region Header: Health and Life
    [Header("Health and Life")]
    [SerializeField] private int hp = 3;
    [SerializeField] private int maxHp = 3;
    [SerializeField] private int life = 3;
    public int HP
    {
        get => hp;
        private set => hp = Mathf.Clamp(value, 0, maxHp);
    }
    public int MaxHP => maxHp;
    public int Life
    {
        get => life;
        private set => life = Mathf.Max(0, value);
    }
    private Vector3 currentCheckpoint;
    private bool isHit = false;
    #endregion

    #region Cached Variables
    private Vector2 velocity = Vector2.zero;
    #endregion

    #region Monobehaviour Callbacks
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        longDashTrail = GetComponent<TrailRenderer>();
        ghostScript = GetComponent<Ghost>();
        isGroundedState = IsGroundedCheck();
        currentCheckpoint = transform.position;
        playerCollider = GetComponent<Collider2D>();
        UpdateHeartsUI();
    }

    private void Update()
    {
        horizontalDirection = GetInput().x;
        verticalDirection = GetInput().y;
        HandleJumpInput();
        HandleDashInput();
        HandleLongDashInput();
        UpdateAnimatorParameters();
        FlipController();
    }

    private void FixedUpdate()
    {
        UpdateLongDashCooldown();
        UpdateDashCooldown();
        UpdateDashUI();

        bool wasGrounded = isGroundedState;
        isGroundedState = IsGroundedCheck();

        if (!isDashing && !isLongDashing)
        {
            HandleMovement(wasGrounded);
            HandleJump();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position - transform.up * distanceGroundCheck, boxSize);
    }
    #endregion

    #region Input Methods
    private Vector2 GetInput()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    private void HandleJumpInput()
    {
        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferLength;
        else
            jumpBufferCounter -= Time.deltaTime;
    }

    private void HandleDashInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
            dashBufferCounter = dashBufferLength;
        else
            dashBufferCounter -= Time.deltaTime;
    }

    private void HandleLongDashInput()
    {
        if (Input.GetKeyDown(KeyCode.T))
            longDashBufferCounter = longDashBufferLength;
        else
            longDashBufferCounter -= Time.deltaTime;
    }
    #endregion

    #region Movement Methods
    private void MoveCharacter()
    {
        velocity.Set(horizontalDirection * movementAcceleration, 0f);
        rb.AddForce(velocity);

        if (Mathf.Abs(rb.velocity.x) > maxMoveSpeed)
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxMoveSpeed, rb.velocity.y);
    }

    private void ApplyGroundLinearDrag()
    {
        rb.drag = (Mathf.Abs(horizontalDirection) < 0.4f || changingDirection) ? groundLinearDrag : 0f;
    }

    private void ApplyAirLinearDrag()
    {
        rb.drag = airLinearDrag;
    }

    private void HandleMovement(bool wasGrounded)
    {
        if (canDash)
            StartCoroutine(Dash(horizontalDirection, verticalDirection));
        else if (canLongDash)
            StartCoroutine(LongDash(horizontalDirection, verticalDirection));
        else
        {
            MoveCharacter();
            if (isGroundedState)
            {
                ApplyGroundLinearDrag();
                if (!wasGrounded)
                    ResetMovementValues();
                if (hasDashed && isGroundedState)
                {
                    dashCooldownTimer = 0f;
                    hasDashed = false;
                }
                hangTimeCounter = hangTime;
            }
            else
            {
                ApplyAirLinearDrag();
                FallMultiplier();
                hangTimeCounter -= Time.fixedDeltaTime;
                SetFallingAnimation();
            }
        }
    }

    private void ResetMovementValues()
    {
        extraJumpsValue = extraJumps;
        hasDashed = false;
        hasLongDashed = false;
        isJumping = false;
        anim.SetBool("isJumping", false);
        anim.SetBool("isFalling", false);
        dashCooldownTimer = 0f;
    }

    private void SetFallingAnimation()
    {
        if (rb.velocity.y < 0f)
        {
            isJumping = false;
            anim.SetBool("isFalling", true);
            anim.SetBool("isJumping", false);
        }
        else if (rb.velocity.y > 0f && isJumping)
            anim.SetBool("isFalling", false);
    }

    private void FallMultiplier()
    {
        if (verticalDirection < 0f)
            rb.gravityScale = downMultiplier;
        else if (rb.velocity.y < 0)
            rb.gravityScale = fallMultiplier;
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
            rb.gravityScale = lowJumpFallMultiplier;
        else
            rb.gravityScale = 1f;
    }
    #endregion

    #region Jump Methods
    private void HandleJump()
    {
        if (canJump)
            Jump(Vector2.up);
    }

    private void Jump(Vector2 direction)
    {
        if (!isGroundedState)
            extraJumpsValue--;

        ApplyAirLinearDrag();
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(direction * jumpForce, ForceMode2D.Impulse);
        hangTimeCounter = 0f;
        jumpBufferCounter = 0f;
        isJumping = true;
        anim.SetBool("isJumping", true);
        anim.SetBool("isFalling", false);
    }
    #endregion

    #region Dash Methods
    IEnumerator Dash(float x, float y)
    {
        float dashStartTime = Time.time;
        hasDashed = true;
        isDashing = true;
        isJumping = false;
        SetDashAnimation(true);
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0f;
        rb.drag = 0f;
        ghostScript.StartGhosting();
        //CameraShakerHandler.Shake(dashShake);
        lastDashDirection = (x != 0f || y != 0f) ? new Vector2(x, y).normalized : (facingRight ? Vector2.right : Vector2.left);

        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), true);

        List<GameObject> hitEnemiesDash = new List<GameObject>();

        while (Time.time < dashStartTime + dashLength)
        {
            rb.velocity = lastDashDirection * dashSpeed;

            Collider2D[] hitColliders = Physics2D.OverlapBoxAll(playerCollider.bounds.center, playerCollider.bounds.size, 0f, enemyLayer);
            foreach (Collider2D hitCollider in hitColliders)
            {
                GameObject enemyGameObject = hitCollider.gameObject;
                Enemy enemyScript = enemyGameObject.GetComponent<Enemy>();
                if (enemyScript != null && !hitEnemiesDash.Contains(enemyGameObject))
                {
                    enemyScript.TakeDamage(dashDamage);
                    Debug.Log("Player dashed through and hit " + enemyGameObject.name + " for " + dashDamage + " damage!");
                    hitEnemiesDash.Add(enemyGameObject);
                }
            }

            yield return null;
        }

        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), false);

        isDashing = false;
        SetDashAnimation(false);
        ghostScript.StopGhosting();
        dashCooldownTimer = dashCooldown;
        hasDashed = false;
        rb.gravityScale = 1f;
    }
    #endregion

   // Inside your Movement2D.cs script

    #region Long Dash Methods
    IEnumerator LongDash(float x, float y)
    {
        float longDashStartTime = Time.time;
        hasLongDashed = true;
        isLongDashing = true;
        isJumping = false;
        SetLongDashAnimation(true);
        longDashTrail.emitting = true;
        rb.gravityScale = 0f;
        rb.drag = 0f;
        ghostScript.StartGhosting();
        //CameraShakerHandler.Shake(longDashShake);
        Vector2 longDashDirection = (x != 0f || y != 0f) ? new Vector2(x, y).normalized : (facingRight ? Vector2.right : Vector2.left);
        lastDashDirection = longDashDirection;
        rb.velocity = longDashDirection * longDashSpeed;

        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), true);

        List<GameObject> hitEnemiesLongDash = new List<GameObject>();
        longDashKillConfirmed = false; // รีเซ็ต flag ก่อนเริ่ม Long Dash

        while (Time.time < longDashStartTime + longDashLength)
        {
            rb.velocity = longDashDirection * longDashSpeed;

            Collider2D[] hitColliders = Physics2D.OverlapBoxAll(playerCollider.bounds.center, playerCollider.bounds.size, 0f, enemyLayer);
            foreach (Collider2D hitCollider in hitColliders)
            {
                GameObject enemyGameObject = hitCollider.gameObject;
                Enemy enemyScript = enemyGameObject.GetComponent<Enemy>();
                // ตรวจสอบว่า enemyScript ยังไม่เป็น null และยังไม่ถูก process ใน dash ครั้งนี้
                if (enemyScript != null && !hitEnemiesLongDash.Contains(enemyGameObject))
                {
                    // เรียก TakeDamage และรับค่า return
                    bool killedByDash = enemyScript.TakeDamage(longDashDamage);

                    Debug.Log("Player long dashed through and hit " + enemyGameObject.name + " for " + longDashDamage + " damage!");
                    hitEnemiesLongDash.Add(enemyGameObject); // เพิ่มเข้า list ที่โดนชนแล้ว เพื่อไม่ให้โดนชนซ้ำใน dash เดียวกัน

                    // ถ้า killedByDash เป็น true แสดงว่าศัตรูตัวนี้ตายจากการโจมตีครั้งนี้
                    if (killedByDash)
                    {
                         longDashKillConfirmed = true; // ตั้งค่า flag
                         Debug.Log("Enemy confirmed killed by long dash: " + enemyGameObject.name); // เพิ่ม debug log ตรงนี้เพื่อยืนยัน
                    }
                }
            }
            yield return null;
        }

        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), false);

        isLongDashing = false;
        SetLongDashAnimation(false);
        anim.SetBool("isFalling", false);

        // ตรวจสอบ flag ที่ตั้งค่าในขณะที่ dash อยู่
        if (longDashKillConfirmed)
        {
            longDashCooldownTimer = 0f; // รีเซ็ต cooldown เป็น 0
            Debug.Log("Long Dash cooldown reset after kill."); // Log นี้ควรจะขึ้นแล้ว
        }
        else
            longDashCooldownTimer = longDashCooldown; // ถ้าไม่ตาย ให้ใช้ cooldown ปกติ

        longDashBufferCounter = 0f;
        longDashTrail.emitting = false;
        ghostScript.StopGhosting();
        hasLongDashed = false;
        rb.gravityScale = 1f;
        // enemiesHitDuringLongDash.Clear(); // รายการนี้ดูเหมือนจะไม่ได้ใช้งานในโค้ดที่ให้มา อาจจะลบทิ้งได้
    }
    #endregion

    #region Collision Methods
    private bool IsGroundedCheck()
    {
        return Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, distanceGroundCheck, layerMask);
    }
    #endregion

    #region Animation and Visuals
    void FlipController()
    {
        // ป้องกันการพลิกตัวระหว่างที่กำลัง Dash หรือ Long Dash
        if (isDashing || isLongDashing)
        {
            return; // ไม่ทำอะไรเลยถ้ากำลัง Dash อยู่
        }

        // พลิกตัวตามทิศทางการกด input
        // ถ้ากดไปทางขวา (horizontalDirection > 0) แต่หน้าหันไปทางซ้าย (!facingRight) -> ให้พลิก
        if (horizontalDirection > 0 && !facingRight)
        {
            Flip();
        }
        // ถ้ากดไปทางซ้าย (horizontalDirection < 0) แต่หน้าหันไปทางขวา (facingRight) -> ให้พลิก
        else if (horizontalDirection < 0 && facingRight)
        {
            Flip();
        }
        // หมายเหตุ: ถ้า horizontalDirection เป็น 0 จะไม่มีการพลิกตัว
        // ดังนั้นตัวละครจะหันหน้าไปทิศทางล่าสุดที่กดค้างไว้
    }

    void Flip()
    {
        facingRight = !facingRight;
        // วิธีการพลิกตัวด้วยการ Scale แกน x
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;

        // หรือจะยังใช้วิธี Rotate 180 องศาก็ได้ แต่ Scale แกน x มักจะนิยมกว่า
        // transform.Rotate(0f, 180f, 0f);
    }

    private void UpdateAnimatorParameters()
    {
        // อัปเดต Animator Parameters เหมือนเดิม
        bool isMoving = Mathf.Abs(horizontalDirection) > 0.1f; // ยังคงใช้ horizontalDirection สำหรับตรวจสอบการเคลื่อนที่
        bool onGround = isGroundedState && !isDashing && !isLongDashing;

        anim.SetBool("isRunning", isMoving && onGround);
        anim.SetBool("isIdle", !isMoving && onGround);
        // isJumping และ isFalling จัดการใน HandleJump และ SetFallingAnimation แล้ว
        // isDashing และ isLongDashing จัดการใน SetDashAnimation และ SetLongDashAnimation แล้ว
    }

    private void SetDashAnimation(bool isDashing)
    {
        anim.SetBool("isDashing", isDashing);
        anim.SetBool("isLongDashing", false);
        anim.SetBool("isJumping", false);
        anim.SetBool("isFalling", false);
    }

    private void SetLongDashAnimation(bool isLongDashing)
    {
        anim.SetBool("isLongDashing", isLongDashing);
        anim.SetBool("isDashing", false);
        anim.SetBool("isJumping", false);
        anim.SetBool("isFalling", false);
    }
    #endregion

    #region Cooldown and Timers
    private void UpdateLongDashCooldown()
    {
        if (longDashCooldownTimer > 0f)
        {
            longDashCooldownTimer -= Time.fixedDeltaTime;
            if (longDashCooldownTimer < 0f)
                longDashCooldownTimer = 0f;
        }
    }

    private void UpdateDashCooldown()
    {
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.fixedDeltaTime;
            if (dashCooldownTimer < 0f)
                dashCooldownTimer = 0f;
        }
    }

    private void UpdateDashUI()
    {
        longDashFillImage.fillAmount = 1f - (longDashCooldownTimer / longDashCooldown);
        DashFillImage.fillAmount = 1f - (dashCooldownTimer / dashCooldown);
    }
    #endregion

    #region Health and Respawn
    public void TakeDamage(int damageAmount)
    {
        if (!isHit)
        {
            HP -= damageAmount;
            Debug.Log("Player took " + damageAmount + " damage! HP: " + HP + " / Life: " + Life);
            UpdateHeartsUI();
            StartCoroutine(GetHurt());

            if (HP <= 0)
            {
                Life--;
                Debug.Log("HP depleted! Life remaining: " + Life);
                if (Life > 0)
                    Respawn();
                else
                    GameOver();
            }
        }
    }

    IEnumerator GetHurt()
    {
        Physics2D.IgnoreLayerCollision(7, 8);
        anim.SetLayerWeight(1, 1);
        isHit = true;

        yield return new WaitForSeconds(1);
        anim.SetLayerWeight(1, 0);
        Physics2D.IgnoreLayerCollision(7, 8, false);
        isHit = false;
    }

    private void Respawn()
    {
        currentCheckpoint = Checkpoint.GetLastCheckpointPosition();
        transform.position = currentCheckpoint;
        HP = maxHp;
        rb.velocity = Vector2.zero;
        anim.Play("Idle");
        isHit = false;
        Debug.Log("Player respawned at: " + currentCheckpoint + " with HP: " + HP + " and Life: " + Life);
        UpdateHeartsUI();
    }

    private void GameOver()
    {
        Debug.Log("Game Over! Player has run out of lives.");
        Destroy(gameObject);
    }

    public void ResetDashCooldown()
    {
        dashCooldownTimer = 0f;
        longDashCooldownTimer = 0f;
        Debug.Log("Dash cooldown reset from Oil Item.");
    }
    #endregion
}


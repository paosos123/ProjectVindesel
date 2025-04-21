using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using FirstGearGames.SmoothCameraShaker;

public class Movement2D : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rb;
    private Animator anim;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask layerMask;

    [Header("Movement Variables")]
    [SerializeField] private float movementAcceleration = 70f;
    [SerializeField] private float maxMoveSpeed = 12f;
    [SerializeField] private float groundLinearDrag = 7f;
    private float horizontalDirection;
    private float verticalDirection;
    private bool changingDirection => (rb.velocity.x > 0f && horizontalDirection < 0f) || (rb.velocity.x < 0f && horizontalDirection > 0f);
    private bool facingRight = true;

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
    private bool isGroundedState; // สถานะการติดพื้น

    [Header("Dash Variables")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashLength = .3f;
    [SerializeField] private float dashBufferLength = .1f;
    [SerializeField] private float dashCooldown = 2f; // เพิ่มตัวแปรคูลดาวน์
    [SerializeField] ShakeData dashShake; 
    private float dashCooldownTimer = 0f; // ตัวนับเวลาคูลดาวน์
    private float dashBufferCounter;
    private bool isDashing;
    private bool hasDashed;
    private bool canDash => dashBufferCounter > 0f && !hasDashed && dashCooldownTimer <= 0f; // เช็คคูลดาวน์

    [Header("Long Dash Variables")]
    [SerializeField] private float longDashSpeed = 25f;
    [SerializeField] private float longDashLength = 0.6f;
    [SerializeField] private float longDashBufferLength = 0.1f;
    [SerializeField] private float longDashCooldown = 3f;
    [SerializeField] ShakeData longDashShake;
    private TrailRenderer longDashTrail;
    private float longDashCooldownTimer = 0f;
    private float longDashBufferCounter;
    private bool isLongDashing;
    private bool hasLongDashed;
    private bool canLongDash => longDashBufferCounter > 0f && !hasLongDashed && longDashCooldownTimer <= 0f;
    private Ghost ghostScript;

    [Header("Ground Collision Variables")]
    [SerializeField] private float distanceGroundCheck;
    [SerializeField] private Vector2 boxSize;

    [Header("Wall Collision Variables")]
    private bool isWallCollisionDuringDash = false;
    private Vector2 collisionNormalDuringDash = Vector2.zero;
    private Vector2 lastDashDirection = Vector2.zero;
    private bool isHit = false;

    [Header("UI")]
    [SerializeField] private Image longDashFillImage;
    [SerializeField] private Image DashFillImage;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        longDashTrail = GetComponent<TrailRenderer>();
        ghostScript = GetComponent<Ghost>();
        isGroundedState = IsGroundedCheck();

    }

    private void Update()
    {
        horizontalDirection = GetInput().x;
        verticalDirection = GetInput().y;
        if (Input.GetButtonDown("Jump")) jumpBufferCounter = jumpBufferLength;
        else jumpBufferCounter -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.E)) dashBufferCounter = dashBufferLength;
        else dashBufferCounter -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.T)) longDashBufferCounter = longDashBufferLength;
        else longDashBufferCounter -= Time.deltaTime;

        UpdateAnimatorParameters();
        FlipController();
    }

    private void FixedUpdate()
    {
        UpdateLongDashCooldown();
        UpdateDashCooldown(); // อัปเดตคูลดาวน์ Dash
        longDashFillImage.fillAmount = 1f - (longDashCooldownTimer / longDashCooldown);
        DashFillImage.fillAmount = 1f - (dashCooldownTimer / dashCooldown); // อัปเดต UI คูลดาวน์ Dash

        bool wasGrounded = isGroundedState;
        isGroundedState = IsGroundedCheck();

        if (!isDashing && !isLongDashing)
        {
            if (canDash) StartCoroutine(Dash(horizontalDirection, verticalDirection));
            else if (canLongDash) StartCoroutine(LongDash(horizontalDirection, verticalDirection));
            else
            {
                MoveCharacter();
                if (isGroundedState)
                {
                    ApplyGroundLinearDrag();
                    if (!wasGrounded) // รีเซ็ตค่าเมื่อลงสู่พื้น
                    {
                        extraJumpsValue = extraJumps;
                        hasDashed = false;
                        hasLongDashed = false;
                        isJumping = false;
                        anim.SetBool("isJumping", false);
                        anim.SetBool("isFalling", false);
                        dashCooldownTimer = 0f; // รีเซ็ตคูลดาวน์ Dash เมื่อลงพื้น
                    }
                    if (hasDashed && isGroundedState)
                    {
                        dashCooldownTimer = 0f;
                        hasDashed = false; // รีเซ็ตสถานะการแดชแล้ว เพื่อให้สามารถแดชใหม่ได้
                    }
                    hangTimeCounter = hangTime;
                }
                else
                {
                    ApplyAirLinearDrag();
                    FallMultiplier();
                    hangTimeCounter -= Time.fixedDeltaTime;
                    if (rb.velocity.y < 0f)
                    {
                        isJumping = false;
                        anim.SetBool("isFalling", true);
                        anim.SetBool("isJumping", false);
                    }
                    else if (rb.velocity.y > 0f && isJumping)
                    {
                        anim.SetBool("isFalling", false);
                    }
                }
            }
            if (canJump)
            {
                Jump(Vector2.up);
            }
        }
        else if (isDashing || isLongDashing)
        {
            return;
        }
    }

    private Vector2 GetInput()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    private void MoveCharacter()
    {
        rb.AddForce(new Vector2(horizontalDirection, 0f) * movementAcceleration);

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

    void FlipController()
    {
        if ((rb.velocity.x < 0f && facingRight) || (rb.velocity.x > 0f && !facingRight))
            Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0f, 180f, 0f);
    }

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
        CameraShakerHandler.Shake(dashShake);
        lastDashDirection = (x != 0f || y != 0f) ? new Vector2(x, y).normalized : (facingRight ? Vector2.right : Vector2.left);

        while (Time.time < dashStartTime + dashLength)
        {
            rb.velocity = lastDashDirection * dashSpeed;
            yield return null;
        }

        isDashing = false;
        SetDashAnimation(false);
        ghostScript.StopGhosting();
        dashCooldownTimer = dashCooldown;
        hasDashed = false;

        // ตรวจสอบการชนหลังแดช และหยุดการเคลื่อนที่ในทิศทางของการชน
        if (isWallCollisionDuringDash)
        {
            // ปรับความเร็วในทิศทางที่ตั้งฉากกับทิศทางของการชนให้เป็น 0
            Vector2 wallTangent = Vector2.Perpendicular(collisionNormalDuringDash).normalized;
            float projection = Vector2.Dot(rb.velocity, wallTangent);
            rb.velocity -= projection * wallTangent;

            isWallCollisionDuringDash = false;
            collisionNormalDuringDash = Vector2.zero;
        }
    }

    IEnumerator LongDash(float x, float y)
    {
        float dashStartTime = Time.time;
        hasLongDashed = true;
        isLongDashing = true;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0f;
        rb.drag = 0f;
        SetLongDashAnimation(true);
        longDashTrail.emitting = true;
        ghostScript.StartGhosting();
        CameraShakerHandler.Shake(longDashShake);
        Vector2 longDashDirection = (x != 0f || y != 0f) ? new Vector2(x, y).normalized : (facingRight ? Vector2.right : Vector2.left);
        lastDashDirection = longDashDirection;

        while (Time.time < dashStartTime + longDashLength)
        {
            rb.velocity = longDashDirection * longDashSpeed;
            yield return null;
        }

        isLongDashing = false;
        SetLongDashAnimation(false);
        anim.SetBool("isFalling", false);
        longDashCooldownTimer = longDashCooldown;
        longDashTrail.emitting = false;
        ghostScript.StopGhosting();
        hasLongDashed = false;

        // ตรวจสอบการชนหลัง Long Dash และหยุดการเคลื่อนที่ในทิศทางของการชน
        if (isWallCollisionDuringDash)
        {
            // ปรับความเร็วในทิศทางที่ตั้งฉากกับทิศทางของการชนให้เป็น 0
            Vector2 wallTangent = Vector2.Perpendicular(collisionNormalDuringDash).normalized;
            float projection = Vector2.Dot(rb.velocity, wallTangent);
            rb.velocity -= projection * wallTangent;

            isWallCollisionDuringDash = false;
            collisionNormalDuringDash = Vector2.zero;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if ((isDashing || isLongDashing) && (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Wall")))
        {
            isWallCollisionDuringDash = true;
            collisionNormalDuringDash = collision.contacts[0].normal;
        }
        if (collision.gameObject.CompareTag("Enemy") && !isHit)
        {
            StartCoroutine(GetHurt());
        }
        
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if ((isDashing || isLongDashing) && (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Wall")))
        {
            isWallCollisionDuringDash = true;
            collisionNormalDuringDash = collision.contacts[0].normal;
        }
    }
    private bool IsGroundedCheck()
    {
        return Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, distanceGroundCheck, layerMask);
    }

   
    IEnumerator GetHurt()
    {
        Physics2D.IgnoreLayerCollision(7, 8);
        anim.SetLayerWeight(1, 1);
        isHit = true;
        yield return new WaitForSeconds(2);
        anim.SetLayerWeight(1, 0);
        Physics2D.IgnoreLayerCollision(7, 8, false);
        isHit = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position - transform.up * distanceGroundCheck, boxSize);


    }

   
    private void UpdateLongDashCooldown()
    {
        if (longDashCooldownTimer > 0f)
        {
            longDashCooldownTimer -= Time.fixedDeltaTime;
            if (longDashCooldownTimer < 0f)
            {
                longDashCooldownTimer = 0f;
            }
        }
    }

    private void UpdateDashCooldown()
    {
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.fixedDeltaTime;
            if (dashCooldownTimer < 0f)
            {
                dashCooldownTimer = 0f;
            }
        }
    }

    private void UpdateAnimatorParameters()
    {
        bool isMoving = Mathf.Abs(horizontalDirection) > 0.1f;
        bool onGround = isGroundedState && !isDashing && !isLongDashing;

        anim.SetBool("isRunning", isMoving && onGround);
        anim.SetBool("isIdle", !isMoving && onGround);
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
}
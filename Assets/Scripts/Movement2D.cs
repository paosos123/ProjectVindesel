using System.Collections;
 using System.Collections.Generic;
 using UnityEngine;
 using UnityEngine.UI;
 using FirstGearGames.SmoothCameraShaker;
 using TMPro;
 using Cinemachine;
 
 public class Movement2D : MonoBehaviour
 {
     #region Header: Components
     [Header("Components")]
     private Rigidbody2D rb;
     private Animator anim;
     private Collider2D playerCollider;
     private SpriteRenderer playerSpriteRenderer;
     private Color originalColor;
     public AudioSource audioSource;
     [SerializeField] private CameraZoomController cameraZoomController; // เพิ่มตัวแปรอ้างอิง CameraZoomController
     private CinemachineImpulseSource impulseSource;
     [SerializeField] private float scaleDashImpulse;
     [SerializeField] private float scaleLongDashImpulse;
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
     [SerializeField] private int extraJumps = 0;
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
     [SerializeField] private AudioClip dashSound;
     [SerializeField] private float dashZoomDuration = 0.2f; // ระยะเวลาการซูมสำหรับ Dash
     private float dashCooldownTimer = 0f;
     private float dashBufferCounter;
     private bool isDashing;
     private bool hasDashed;
     private bool canDash => dashBufferCounter > 0f && !hasDashed && dashCooldownTimer <= 0f;
     #endregion

     #region Long Dash Variables
     [Header("Long Dash Variables")]
     [SerializeField] private float longDashSpeed = 25f;
     [SerializeField] private float longDashLength = 0.6f;
     [SerializeField] private float longDashBufferLength = 0.1f;
     [SerializeField] private float longDashCooldown = 3f;
     [SerializeField] private ShakeData longDashShake;
     [SerializeField] private int longDashDamage = 2;
     [SerializeField] private AudioClip longDashSound;
     [SerializeField] private float longDashZoomDuration = 0.4f; // ระยะเวลาการซูมสำหรับ Long Dash
     private bool longDashKillConfirmed = false;
     private TrailRenderer longDashTrail;
     private float longDashCooldownTimer = 0f; // Changed to private
     private float longDashBufferCounter;
     private bool isLongDashing;
     public bool IsLongDashing => isLongDashing;
     private bool hasLongDashed;
     private bool canLongDash => longDashBufferCounter > 0f && !hasLongDashed && longDashCooldownTimer <= 0f;
     private Ghost ghostScript;
     private List<Enemy> enemiesHitDuringLongDash = new List<Enemy>();
     private bool oilItemCollectedDuringLongDash = false; // เพิ่ม Flag สำหรับตรวจสอบการเก็บ Oil Item
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
     [SerializeField] private GameObject[] heartIcons;
     [SerializeField] private GameObject upgradeUI;// เพิ่ม Array สำหรับเก็บ GameObject หัวใจ
     [SerializeField] private TMP_Text lifeText_TMP;
     [SerializeField] private GameObject gameWinPanel;
     [SerializeField] private GameObject gameLosePanel;
     #endregion

     #region UI Methods // เพิ่ม Header ใหม่สำหรับ UI methods
     private void UpdateHeartsUI()
     {
         lifeText_TMP.text = "Life : " + Life.ToString();
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
     public void Heal()
     {
         HP = MaxHP;
         UpdateHeartsUI();
         Debug.Log("Player healed to full HP via Heal() function.");
     }
     [SerializeField] private GameObject maxHp4UI; // GameObject สำหรับ UI Max HP 4
     [SerializeField] private string trapTag = "Trap";
     [SerializeField] private AudioClip getHurtSound;
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
         playerSpriteRenderer = GetComponent<SpriteRenderer>(); // GetComponent SpriteRenderer
         if (playerSpriteRenderer != null)
         {
             originalColor = playerSpriteRenderer.color; // เก็บสีเดิม
         }
         UpdateHeartsUI();
         if (cameraZoomController == null)
         {
             Debug.LogError("CameraZoomController is not assigned in the Inspector!");
         }
         else
         {
             Debug.Log("CameraZoomController assigned: " + cameraZoomController.gameObject.name);
         }
         // ตรวจสอบว่า CameraZoomController ถูก Assign ใน Inspector แล้วหรือยัง
         if (cameraZoomController == null)
         {
             Debug.LogError("CameraZoomController is not assigned in the Inspector!");
         }
         impulseSource = GetComponent<CinemachineImpulseSource>();
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
         // longDashCooldownTimer = 0f; // นำการรีเซ็ต Long Dash Timer ออกจากตรงนี้
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

     #region Dash Methods // เลื่อนลงมาเพื่อความสะดวกในการอ่าน
     IEnumerator Dash(float x, float y)
     {
         float dashStartTime = Time.time;
         hasDashed = true;
         isDashing = true;
         isJumping = false;
         SetDashAnimation(true, y > 0.1f); // ส่งค่า isDashingUp ไปด้วย
         rb.velocity = Vector2.zero;
         rb.gravityScale = 0f;
         rb.drag = 0f;
         ghostScript.StartGhosting();
         //CameraShakerHandler.Shake(dashShake);
         lastDashDirection = (x != 0f || y != 0f) ? new Vector2(x, y).normalized : (facingRight ? Vector2.right : Vector2.left);

         Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), true);

         List<GameObject> hitEnemiesDash = new List<GameObject>();
         audioSource.PlayOneShot(dashSound);
         impulseSource.GenerateImpulse(lastDashDirection * dashSpeed *scaleDashImpulse); // ปรับค่าแรงสั่นตามต้องการ
         // เรียกใช้ Zoom In และ Zoom Out สำหรับ Dash
         if (cameraZoomController != null)
         {
             cameraZoomController.ZoomInAndOut(dashZoomDuration);
         }

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
         SetDashAnimation(false, false); // รีเซ็ต isDashingUp
         ghostScript.StopGhosting();
         dashCooldownTimer = dashCooldown;
         hasDashed = false;
         rb.gravityScale = 1f;
     }

     #endregion

     #region Long Dash Methods
      IEnumerator LongDash(float x, float y)
     {
         float longDashStartTime = Time.time;
         hasLongDashed = true;
         isLongDashing = true;
         isJumping = false;
         SetLongDashAnimation(true, y > 0.1f); // ส่งค่า isDashingUp ไปด้วย
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
         longDashKillConfirmed = false;
         oilItemCollectedDuringLongDash = false;
         audioSource.PlayOneShot(longDashSound);
         impulseSource.GenerateImpulse(lastDashDirection * longDashSpeed * scaleLongDashImpulse); // ปรับค่าแรงสั่นตามต้องการ
         // เรียกใช้ Zoom In และ Zoom Out สำหรับ Long Dash
         if (cameraZoomController != null)
         {
             cameraZoomController.ZoomInAndOut(longDashZoomDuration);
         }

         while (Time.time < longDashStartTime + longDashLength)
         {
             rb.velocity = longDashDirection * longDashSpeed;

             Collider2D[] hitColliders = Physics2D.OverlapBoxAll(playerCollider.bounds.center, playerCollider.bounds.size, 0f, enemyLayer);
             foreach (Collider2D hitCollider in hitColliders)
             {
                 GameObject enemyGameObject = hitCollider.gameObject;
                 Enemy enemyScript = enemyGameObject.GetComponent<Enemy>();
                 if (enemyScript != null && !hitEnemiesLongDash.Contains(enemyGameObject))
                 {
                     bool killedByDash = enemyScript.TakeDamage(longDashDamage);
                     Debug.Log("Player long dashed through and hit " + enemyGameObject.name + " for " + longDashDamage + " damage!");
                     hitEnemiesLongDash.Add(enemyGameObject);
                     if (killedByDash)
                     {
                         longDashKillConfirmed = true;
                         Debug.Log("Enemy confirmed killed by long dash: " + enemyGameObject.name);
                     }
                 }
             }
             yield return null;
         }

         Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), false);

         isLongDashing = false;
         SetLongDashAnimation(false, false); // รีเซ็ต isDashingUp
         anim.SetBool("isFalling", false);

         if (oilItemCollectedDuringLongDash)
         {
             longDashCooldownTimer = 0f;
             Debug.Log("Long Dash cooldown reset after collecting Oil Item during dash.");
         }
         else if (longDashKillConfirmed)
         {
             longDashCooldownTimer = 0f;
             Debug.Log("Long Dash cooldown reset after kill.");
         }
         else
             longDashCooldownTimer = longDashCooldown;

         longDashBufferCounter = 0f;
         longDashTrail.emitting = false;
         ghostScript.StopGhosting();
         hasLongDashed = false;
         rb.gravityScale = 1f;
     }
     #endregion

     #region Collision Methods
     private bool IsGroundedCheck()
     {
         return Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, distanceGroundCheck, layerMask);
     }
     private void OnTriggerEnter2D(Collider2D other)
     {
         // ตรวจสอบว่า GameObject ที่เข้ามาชนมี Tag เป็น Trap หรือไม่
         if (other.CompareTag(trapTag))
         {
             // ลด HP จนหมด
             HP = 0;
             Debug.Log("Player hit a Trap! HP reduced to 0.");
             UpdateHeartsUI(); // อัปเดต UI ทันที

             // คุณอาจเพิ่ม Effect เสียง หรือการทำงานอื่น ๆ เมื่อโดน Trap ที่นี่

             // ตรวจสอบว่า HP หมดแล้วหรือไม่ เพื่อจัดการการตายหรือ Respawn
             if (HP <= 0)
             {
                 Life--;
                 Debug.Log("HP depleted by Trap! Life remaining: " + Life);
                 if (Life > 0)
                     Respawn();
                 else
                     GameOver();
             }
         }
         if (other.CompareTag("TuWall"))
         {
             Debug.Log("Player hit a Death Wall! Triggering Respawn.");
             Respawn();
         }
         if (other.CompareTag("Win"))
         {
             gameWinPanel.SetActive(true);
         }
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
         if (horizontalDirection > 0 && !facingRight)
         {
             Flip();
         }
         else if (horizontalDirection < 0 && facingRight)
         {
             Flip();
         }
     }

     void Flip()
     {
         facingRight = !facingRight;
         Vector3 localScale = transform.localScale;
         localScale.x *= -1f;
         transform.localScale = localScale;
     }

     private void UpdateAnimatorParameters()
     {
         bool isMoving = Mathf.Abs(horizontalDirection) > 0.1f;
         bool onGround = isGroundedState && !isDashing && !isLongDashing;

         anim.SetBool("isRunning", isMoving && onGround);
         anim.SetBool("isIdle", !isMoving && onGround);
         // isJumping และ isFalling จัดการใน HandleJump และ SetFallingAnimation แล้ว
         // isDashing และ isLongDashing จัดการใน SetDashAnimation และ SetLongDashAnimation แล้ว
     }

     private void SetDashAnimation(bool isDashing, bool isDashingUp)
     {
         anim.SetBool("isDashing", isDashing);
         anim.SetBool("isLongDashing", false);
         anim.SetBool("isJumping", false);
         anim.SetBool("isFalling", false);
         anim.SetBool("isDashingUp", isDashingUp); // ใช้ Parameter เดียวกัน
     }

     private void SetLongDashAnimation(bool isLongDashing, bool isLongDashingUp)
     {
         anim.SetBool("isLongDashing", isLongDashing);
         anim.SetBool("isDashing", false);
         anim.SetBool("isJumping", false);
         anim.SetBool("isFalling", false);
         anim.SetBool("isDashingUp", isLongDashingUp); // ใช้ Parameter เดียวกัน
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

         if (playerSpriteRenderer != null)
         {
             playerSpriteRenderer.color = Color.red;
         }
         audioSource.PlayOneShot(getHurtSound);

         yield return new WaitForSeconds(1);

         anim.SetLayerWeight(1, 0);
         Physics2D.IgnoreLayerCollision(7, 8, false);
         isHit = false;

         if (playerSpriteRenderer != null)
         {
             playerSpriteRenderer.color = originalColor;
         }
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
         gameLosePanel.SetActive(true);
         Destroy(gameObject);
     }

     public void ResetDashCooldown()
     {
         dashCooldownTimer = 0f;
         longDashCooldownTimer = 0f;
         Debug.Log("Dash cooldown reset from Oil Item.");
         oilItemCollectedDuringLongDash = true;
     }
     #endregion

     #region Upgrade Methods // เพิ่ม Region ใหม่สำหรับ Upgrade Methods
     public void AddExtraJump()
     {
         if (extraJumps == 0)
         {
             extraJumps = 1;
             Debug.Log("Extra Jump added! Current extra jumps: " + extraJumps);
         }
         else
         {
             Debug.Log("Extra Jump already enabled.");
         }
         upgradeUI.SetActive(false);
     }

     public void IncreaseDashDamage()
     {
         dashDamage = 2;
         longDashDamage = 3;
         upgradeUI.SetActive(false);
         Debug.Log("Dash damage increased! Normal Dash: " + dashDamage + ", Long Dash: " + longDashDamage);
     }
     public void IncreaseMaxHPTo4()
     {
         if (maxHp < 4)
         {
             maxHp = 4;
             HP = maxHp;
             if (maxHp4UI != null)
             {
                 maxHp4UI.SetActive(true);
             }
             UpdateHeartsUI();
             Debug.Log("Max HP increased to " + maxHp + ". Current HP is now " + HP + ".");
         }
         else
         {
             Debug.Log("Max HP is already at the maximum value.");
         }
         upgradeUI.SetActive(false);
     }
     #endregion
 }

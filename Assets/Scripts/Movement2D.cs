﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//////////SETUP INSTRUCTIONS//////////
//Attach this script a RigidBody2D to the player GameObject
//Set Body type to Dynamic, Collision detection to continuous and Freeze Z rotation
//Add a 2D Collider (Any will do, but 2D box collider)
//Define the ground and wall mask layers (In the script and in the GameObjects)
//Adjust and play around with the other variables (Some require you to activate gizmos in order to visualize)

public class Movement2D : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D _rb;
    private Animator _anim;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask layerMask;
    

    [Header("Movement Variables")]
    [SerializeField] private float _movementAcceleration = 70f;
    [SerializeField] private float _maxMoveSpeed = 12f;
    [SerializeField] private float _groundLinearDrag = 7f;
    private float _horizontalDirection;
    private float _verticalDirection;
    private bool _changingDirection => (_rb.velocity.x > 0f && _horizontalDirection < 0f) || (_rb.velocity.x < 0f && _horizontalDirection > 0f);
    private bool _facingRight = true;

    [Header("Jump Variables")]
    [SerializeField] private float _jumpForce = 12f;
    [SerializeField] private float _airLinearDrag = 2.5f;
    [SerializeField] private float _fallMultiplier = 8f;
    [SerializeField] private float _lowJumpFallMultiplier = 5f;
    [SerializeField] private float _downMultiplier = 12f;
    [SerializeField] private int _extraJumps = 1;
    [SerializeField] private float _hangTime = .1f;
    [SerializeField] private float _jumpBufferLength = .1f;
    private int _extraJumpsValue;
    private float _hangTimeCounter;
    private float _jumpBufferCounter;
    private bool _canJump => _jumpBufferCounter > 0f && (_hangTimeCounter > 0f || _extraJumpsValue > 0 );
    private bool _isJumping = false;
    

    [Header("Dash Variables")]
    [SerializeField] private float _dashSpeed = 15f;
    [SerializeField] private float _dashLength = .3f;
    [SerializeField] private float _dashBufferLength = .1f;
    private float _dashBufferCounter;
    private bool _isDashing;
    private bool _hasDashed;
    
    
   
    private bool _canDash => _dashBufferCounter > 0f && !_hasDashed;

    [Header("Long Dash Variables")]
    [SerializeField] private float _longDashSpeed = 25f;
    [SerializeField] private float _longDashLength = 0.6f;
    [SerializeField] private float _longDashBufferLength = 0.1f;
    private float _longDashBufferCounter;
    private bool _isLongDashing;
    private bool _hasLongDashed;

    private bool _canLongDash => _longDashBufferCounter > 0f && !_hasLongDashed;

    [Header("Ground Collision Variables")]
    [SerializeField] private float distanceGroundCheck ;
    [SerializeField] private Vector2 boxSize;
   
    [Header("Wall Collision Variables")]
    [SerializeField] private Vector2 boxWallSize;
    [SerializeField] private float yDetectWallBox;
    [SerializeField] private float xDetectWallBoxOffset = 10f;
    private bool isHit = false;
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
    }

    private void Update()
    {
        _horizontalDirection = GetInput().x;
        _verticalDirection = GetInput().y;
        if (Input.GetButtonDown("Jump")) _jumpBufferCounter = _jumpBufferLength;
        else _jumpBufferCounter -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.E)) _dashBufferCounter = _dashBufferLength;
        else _dashBufferCounter -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.T)) _longDashBufferCounter = _longDashBufferLength;
        else _longDashBufferCounter -= Time.deltaTime;
        
        FlipController();
        if (isWallDetected())
        {
            Debug.Log("Gaae");
        }
    }

    private void FixedUpdate()
    {
        if (!_isDashing && !_isLongDashing) // เพิ่มเงื่อนไขตรวจสอบสถานะการพุ่ง
        {
            if (_canDash) StartCoroutine(Dash(_horizontalDirection, _verticalDirection));
            else if (_canLongDash) StartCoroutine(LongDash(_horizontalDirection, _verticalDirection)); // แก้ไขลำดับการตรวจสอบ
            else
            {
                MoveCharacter();
                if (isGrounded())
                {
                    ApplyGroundLinearDrag();
                    _extraJumpsValue = _extraJumps;
                    _hangTimeCounter = _hangTime;
                    _hasDashed = false;
                    _hasLongDashed = false;
                }
                else
                {
                    ApplyAirLinearDrag();
                    FallMultiplier();
                    _hangTimeCounter -= Time.fixedDeltaTime;
                    if (_rb.velocity.y < 0f) _isJumping = false;
                }
            }
            if (_canJump)
            {
                Jump(Vector2.up);
            }
        }
    }

    private Vector2 GetInput()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    private void MoveCharacter()
    {
        _rb.AddForce(new Vector2(_horizontalDirection, 0f) * _movementAcceleration);

        if (Mathf.Abs(_rb.velocity.x) > _maxMoveSpeed)
            _rb.velocity = new Vector2(Mathf.Sign(_rb.velocity.x) * _maxMoveSpeed, _rb.velocity.y);
    }

    private void ApplyGroundLinearDrag()
    {
        if (Mathf.Abs(_horizontalDirection) < 0.4f || _changingDirection)
        {
            _rb.drag = _groundLinearDrag;
        }
        else
        {
            _rb.drag = 0f;
        }
    }

    private void ApplyAirLinearDrag()
    {
         _rb.drag = _airLinearDrag;
    }

    private void Jump(Vector2 direction)
    {
        if (!isGrounded())
            _extraJumpsValue--;

        ApplyAirLinearDrag();
        _rb.velocity = new Vector2(_rb.velocity.x, 0f);
        _rb.AddForce(direction * _jumpForce, ForceMode2D.Impulse);
        _hangTimeCounter = 0f;
        _jumpBufferCounter = 0f;
        _isJumping = true;
    }

   

   
    
    private void FallMultiplier()
    {
        if (_verticalDirection < 0f)
        {
            _rb.gravityScale = _downMultiplier;
        }
        else
        {
            if (_rb.velocity.y < 0)
            {
                _rb.gravityScale = _fallMultiplier;
            }
            else if (_rb.velocity.y > 0 && !Input.GetButton("Jump"))
            {
                _rb.gravityScale = _lowJumpFallMultiplier;
            }
            else
            {
                _rb.gravityScale = 1f;
            }
        }
    }

    void FlipController()
    {
        if(_rb.velocity.x < 0f &&  _facingRight)
            Flip();
        else if(_rb.velocity.x > 0f && !_facingRight)
            Flip();
    }
   

    void Flip()
    {
        _facingRight = !_facingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    IEnumerator Dash(float x, float y)
    {
        float dashStartTime = Time.time;
        _hasDashed = true;
        _isDashing = true;
        _isJumping = false;

        _rb.velocity = Vector2.zero;
        _rb.gravityScale = 0f;
        _rb.drag = 0f;

        Vector2 dir;
        if (x != 0f || y != 0f) dir = new Vector2(x, y);
        else
        {
            if (_facingRight) dir = new Vector2(1f, 0f);
            else dir = new Vector2(-1f, 0f);
        }

        while (Time.time < dashStartTime + _dashLength)
        {
            _rb.velocity = dir.normalized * _dashSpeed;
            if (isWallDetected())
            {
                Debug.Log("Dash hit wall");
                _isDashing = false;
                yield break; // หยุดโครูทีนเมื่อชนกำแพง
            }
            yield return null;
        }

        _isDashing = false;
    }

    IEnumerator LongDash(float x, float y)
    {
        float dashStartTime = Time.time;
        _hasLongDashed = true;
        _isLongDashing = true;
        _rb.velocity = Vector2.zero;
        _rb.gravityScale = 0f;
        _rb.drag = 0f;

        Vector2 dir;
        if (x != 0f || y != 0f) dir = new Vector2(x, y);
        else
        {
            if (_facingRight) dir = new Vector2(1f, 0f);
            else dir = new Vector2(-1f, 0f);
        }

        while (Time.time < dashStartTime + _longDashLength)
        {
            _rb.velocity = dir.normalized * _longDashSpeed;
            if (isWallDetected())
            {
                Debug.Log("Long dash hit wall");
                _isLongDashing = false;
                yield break; // หยุดโครูทีนเมื่อชนกำแพง
            }
            yield return null;
        }

        _isLongDashing = false;
    }

    private bool isGrounded()
    {
        if (Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, distanceGroundCheck,layerMask))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    IEnumerator GetHurt()
    {
        Physics2D.IgnoreLayerCollision(7,8);
        GetComponent<Animator>().SetLayerWeight(1,1);
        isHit = true;
        yield return new WaitForSeconds(2);
        GetComponent<Animator>().SetLayerWeight(1,0);
        Physics2D.IgnoreLayerCollision(7,8,false);
        isHit = false;
       
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position-transform.up*distanceGroundCheck, boxSize);
        // Calculate the shifted position for the Gizmos
        
        Vector2 gizmosWallOnePosition = new Vector2(transform.position.x + xDetectWallBoxOffset, transform.position.y - yDetectWallBox);
        Gizmos.DrawWireCube(gizmosWallOnePosition, boxWallSize);
        Vector2 gizmosWallTwoPosition = new Vector2(transform.position.x - xDetectWallBoxOffset, transform.position.y + yDetectWallBox);
        Gizmos.DrawWireCube(gizmosWallTwoPosition, boxWallSize);
    }
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Enemy"&&isHit==false)
        {
            StartCoroutine(GetHurt());
        }
    }
    private bool isWallDetected()
    {
        // Calculate the shifted position for the BoxCast
        Vector2 gizmosWallOnePosition = new Vector2(transform.position.x + xDetectWallBoxOffset, transform.position.y - yDetectWallBox);
        Vector2 gizmosWallTwoPosition = new Vector2(transform.position.x - xDetectWallBoxOffset, transform.position.y + yDetectWallBox);
        if (Physics2D.BoxCast(gizmosWallOnePosition, boxWallSize, 0, -transform.up, yDetectWallBox, layerMask)
            || Physics2D.BoxCast(gizmosWallTwoPosition, boxWallSize, 0, -transform.up, yDetectWallBox, layerMask))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}
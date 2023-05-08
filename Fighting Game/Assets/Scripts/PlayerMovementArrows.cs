using System.Collections;
using UnityEngine;

public class PlayerMovementArrows : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 5f;
    public float shieldSpeed = 2f;

    [Header("Jump")]
    public float jumpForce = 10f;
    public float jumpTime = 0.5f;

    private bool isGrounded = false;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;

    private float jumpTimeCounter;
    private float jumpForceMultiplier = 1f;

    public Transform feetPosition;
    public float checkRadius = 0.1f;
    public LayerMask whatIsGround;

    // Add this variable to set the delay before attacking
    private float attackDelay = 0.25f;
    private float lastAttackTime = 0f;

    // Add a circle collider and a trigger for attack detection
    public CircleCollider2D attackCollider;
    public Transform attackPoint;

    public PlayerManager playerManager;

    void Start()
    {
        playerManager = this.GetComponent<PlayerManager>();

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Set the circle collider to trigger mode
        attackCollider.isTrigger = true;
    }

    void Update()
    {
        if (playerManager.currentHP <= 0)
        {
            StartCoroutine(IsDead());
        }

        isGrounded = Physics2D.OverlapCircle(feetPosition.position, checkRadius, whatIsGround);

        float horizontal = Input.GetAxisRaw("HorizontalJKI");
        rb.velocity = new Vector2(horizontal * (playerManager.isShiled ? shieldSpeed : moveSpeed), rb.velocity.y);

        if (isGrounded)
        { 
            anim.SetBool("isRunning", horizontal != 0);
            if (Input.GetKeyDown(KeyCode.O) && isGrounded && Time.time - lastAttackTime >= attackDelay)
            {
                lastAttackTime = Time.time; // set last attack time
                anim.SetBool("isRunning", false);
                anim.SetTrigger("isAttacking");

                // Enable the circle collider to detect attacks
                attackCollider.enabled = true;
            }

            if (Input.GetKeyDown(KeyCode.I) && isGrounded && !playerManager.isShiled)
            {
                anim.SetBool("isJumping", true);
                rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
                isGrounded = false;
                anim.SetBool("isRunning", false);
                anim.SetTrigger("isJumping");

                jumpTimeCounter = jumpTime;
            }
        }
        else
        {
            anim.SetBool("isRunning", false);

            if (Input.GetKey(KeyCode.I) && jumpTimeCounter > 0)
            {
                jumpTimeCounter -= Time.deltaTime;

                jumpForceMultiplier = 1f + (jumpTime - jumpTimeCounter) / jumpTime * (jumpForceMultiplier - 1f);

                rb.velocity = new Vector2(rb.velocity.x, jumpForce * jumpForceMultiplier);
            }
            else
            {
                jumpTimeCounter = 0;
            }
        }

        //if (horizontal < 0) spriteRenderer.flipX = true;
        //else if (horizontal > 0) spriteRenderer.flipX = false;

        if (horizontal != 0)
        {
            Quaternion rot = new Quaternion(0, horizontal > 0 ? -180 : 0, 0, 0);

            if (horizontal < 0) spriteRenderer.transform.localRotation = rot;
            else if (horizontal > 0) spriteRenderer.transform.localRotation = rot;

            //this.GetComponent<PlayerManager>().isFlipped = spriteRenderer.flipX;
            this.GetComponent<PlayerManager>().isFlipped = horizontal > 0;
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground")) isGrounded = true;
    }

    IEnumerator IsDead()
    {
        playerManager.animator.SetBool("isDead", true);

        yield return new WaitForSeconds(0.25f);

        playerManager.animator.SetBool("isDead", false);
        playerManager.animator.SetBool("stayDead", true);

        this.enabled = false;
    }
}


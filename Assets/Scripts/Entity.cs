using System.Collections;
using UnityEngine;

public class Entity : MonoBehaviour
{
    protected Animator anim;
    protected Rigidbody2D rb;
    protected Collider2D col;
    protected SpriteRenderer sr;

    [Header("Health")]
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private int currentHealth;
    [SerializeField] private Material damageMaterial;
    [SerializeField] private float damageFeedbackDuraiton = .1f;
    private Coroutine damageFeedbackCorutine;

    [Header("Attack details")]
    [SerializeField] protected float attackRadius;
    [SerializeField] protected Transform attackPoint;
    [SerializeField] protected LayerMask whatIsTarget;

    [Header("Collision details")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    protected bool isGrounded;

    //Facing direction details
    protected int facingDir = 1;
    protected bool canMove = true;
    protected bool facingRight = true;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        anim = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();

        currentHealth = maxHealth;
    }

    protected virtual void Update()
    {
        HandleCollision();
        HandleMovement();
        HandleAnimations();
        HandleFlip();
    }

    public void DamageTargets()
    {
        Collider2D[] enemyColliders = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, whatIsTarget);

        foreach (Collider2D enemy in enemyColliders)
        {
            Entity entityTarget = enemy.GetComponent<Entity>();
            if (entityTarget != null)
                entityTarget.TakeDamage();
        }
    }

    private void TakeDamage()
    {
        currentHealth = currentHealth - 1;

        PlayDamageFeedback();

        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        // Die animation
        if (anim != null) anim.enabled = false;
        if (col != null) col.enabled = false;

        if (rb != null)
        {
            rb.gravityScale = 12;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 15);
        }

        Destroy(gameObject, 3);
    }

    private void PlayDamageFeedback()
    {
        if (damageFeedbackCorutine != null)
            StopCoroutine(damageFeedbackCorutine);

        StartCoroutine(DamageFeedbackCo());
    }

    private IEnumerator DamageFeedbackCo()
    {
        Material originalMat = sr.material;

        sr.material = damageMaterial;

        yield return new WaitForSeconds(damageFeedbackDuraiton);

        sr.material = originalMat;
    }

    public virtual void EnableMovement(bool enable)
    {
        canMove = enable;
    }

    protected void HandleAnimations()
    {
        if (anim == null) return;
        anim.SetFloat("xVelocity", rb.linearVelocity.x);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("isGrounded", isGrounded);
    }

    protected virtual void HandleAttack()
    {
        if (isGrounded)
        {
            anim.SetTrigger("attack");
        }
    }

    protected virtual void HandleMovement()
    {
        // default: nothing -- children implement movement
    }

    protected virtual void HandleCollision()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
    }

    // Keep base flip logic, but child classes (like ChasingEnemy) can override this if needed.
    protected virtual void HandleFlip()
    {
        if (rb.linearVelocity.x > 0 && facingRight == false)
            Flip();
        else if (rb.linearVelocity.x < 0 && facingRight == true)
            Flip();
    }

    public void Flip()
    {
        // Use localScale flip for stability
        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;

        facingRight = !facingRight;
        facingDir = facingDir * -1;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -groundCheckDistance));

        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }

}

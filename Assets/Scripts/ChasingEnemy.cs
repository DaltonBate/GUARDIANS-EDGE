using UnityEngine;

public class ChasingEnemy : Enemy
{
    [Header("Chase Settings")]
    [SerializeField] protected float detectionRange = 8f;

    protected Transform player;

    // Resolve player robustly and log result
    protected void Start()
    {
        // Turn off root motion from Animator (defensive)
        if (anim != null)
            anim.applyRootMotion = false;

        // Prefer FindFirstObjectByType if available in your Unity version (used elsewhere in the project)
        var p = FindFirstObjectByType<Player>();
        if (p != null)
        {
            player = p.transform;
            Debug.Log($"{name} found Player by type at {player.position}");
            return;
        }

        // fallback to tag lookup
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go != null)
        {
            player = go.transform;
            Debug.Log($"{name} found Player by tag at {player.position}");
            return;
        }

        Debug.LogWarning($"{name} did not find Player in scene. Ensure tag or Player component exists.");
    }

    protected override void HandleMovement()
    {
        if (rb == null)
        {
            Debug.LogWarning($"{name}: rb is null");
            return;
        }

        if (!canMove || player == null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            Vector2 rawDir = (player.position - transform.position);
            float dirX = rawDir.x;

            // Debug only when something interesting happens
            Debug.DrawLine(transform.position, player.position, Color.red, 0.1f);
            Debug.Log($"{name} chasing — player.x={player.position.x:F2}, me.x={transform.position.x:F2}, dirX={dirX:F3}, facingRight={facingRight}");

            // If dirX is very small treat as zero
            if (Mathf.Abs(dirX) < 0.01f)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            else
            {
                // Use Sign to ensure we move left when dirX < 0 and right when > 0
                float horizontal = Mathf.Sign(dirX) * Mathf.Abs(moveSpeed);
                rb.linearVelocity = new Vector2(horizontal, rb.linearVelocity.y);

                if (horizontal > 0 && !facingRight) Flip();
                else if (horizontal < 0 && facingRight) Flip();
            }

            return;
        }

        // Out of range: stop moving
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    // Prevent base HandleFlip from interfering with chase flips
    protected override void HandleFlip()
    {
        // Intentionally left empty so chase flips aren't overridden
    }

    private void OnDrawGizmosSelected()
    {
        // draw detection range in editor for easier debugging
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    // Extra diagnostic: show final velocity after all Update logic (helps find other overrides)
    private void LateUpdate()
    {
        if (rb != null)
            Debug.Log($"{name} final velocity.x={rb.linearVelocity.x:F3}");
    }
}

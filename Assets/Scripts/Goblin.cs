using UnityEngine;

public class Goblin : Enemy
{
    [Header("Goblin Chase Settings")]
    [SerializeField] private float detectionRange = 6f;

    private Transform player;

    protected void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    protected override void Update()
    {
        // DO NOT call base.Update(), it overrides movement
        HandleAttack();      // still use your Enemy attack logic
        HandleMovement();    // use Goblin's custom movement
        HandleCollision();   // needed so playerDetected updates
    }

    protected override void HandleMovement() //Ask if making a new chase player method with movement logic would fix
    {
        if (player == null)
            return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            // chase player
            Vector2 direction = (player.position - transform.position).normalized;

            rb.linearVelocity = new Vector2(
                direction.x * moveSpeed,
                rb.linearVelocity.y
            );

            if (direction.x > 0 && facingDir < 0)
                Flip();
            else if (direction.x < 0 && facingDir > 0)
                Flip();
        }
        else
        {
            // stop when player not in range
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }
}




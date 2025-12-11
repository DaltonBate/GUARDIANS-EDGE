using UnityEngine;

public class Enemy_Respawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject goblinPrefab;
    [SerializeField] private Transform[] respawnPoints;
    [SerializeField] private float cooldown = 0.5f;
    [Space]
    [SerializeField] private float coolDownDecrease = .05f;
    [SerializeField] private float cooldownCap = .7f;

    [Header("Spawn safety")]
    [SerializeField] private LayerMask groundLayer = ~0;
    [SerializeField] private LayerMask blockingLayer = 0; // default none — set to enemy layer in Inspector if you want occupancy checks
    [SerializeField] private float spawnYOffset = 0.08f; // small upward offset to avoid initial intersection
    [SerializeField] private Vector2 spawnCheckSize = new Vector2(0.5f, 0.9f); // box used to check occupancy at spawn
    [SerializeField] private int maxActiveEnemies = 12; // prevents flooding the scene

    private float timer;
    private Transform player;

    private void Awake()
    {
        var p = FindFirstObjectByType<Player>();
        if (p != null) player = p.transform;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer < 0)
        {
            timer = cooldown;
            TryCreateNewEnemy();

            cooldown = Mathf.Max(cooldownCap, cooldown - coolDownDecrease);
        }
    }

    private void TryCreateNewEnemy()
    {
        // Cap check (safe API)
        if (maxActiveEnemies > 0)
        {
            int current = FindObjectsOfType<Enemy>().Length;
            if (current >= maxActiveEnemies)
            {
                Debug.Log($"Enemy_Respawner: skip spawn — active enemies {current} >= cap {maxActiveEnemies}");
                return;
            }
        }

        if (respawnPoints == null || respawnPoints.Length == 0)
        {
            Debug.LogWarning("Enemy_Respawner: no respawnPoints assigned");
            return;
        }

        int respawnPointIndex = Random.Range(0, respawnPoints.Length);
        Transform resp = respawnPoints[respawnPointIndex];
        if (resp == null)
        {
            Debug.LogWarning($"Enemy_Respawner: respawnPoints[{respawnPointIndex}] is null, skipping");
            return;
        }

        Vector3 spawnPoint = resp.position;
        Vector3 finalSpawn = spawnPoint + Vector3.up * spawnYOffset;

        // If blockingLayer is set, do an occupancy check; otherwise skip occupancy check
        if (blockingLayer != 0)
        {
            Vector2 checkCenter = (Vector2)finalSpawn + Vector2.up * (spawnCheckSize.y * 0.5f + 0.02f);
            Collider2D overlap = Physics2D.OverlapBox(checkCenter, spawnCheckSize, 0f, blockingLayer);
            if (overlap != null)
            {
                Debug.Log($"Enemy_Respawner: spawn spot occupied at {finalSpawn} by {overlap.name}, skipping");
                return;
            }
        }

        GameObject prefabToSpawn = (Random.value > 0.5f) ? enemyPrefab : goblinPrefab;
        if (prefabToSpawn == null)
        {
            Debug.LogWarning("Enemy_Respawner: prefabToSpawn is null — check enemyPrefab/goblinPrefab in Inspector");
            return;
        }

        GameObject newEnemy = Instantiate(prefabToSpawn, finalSpawn, resp.rotation);
        if (newEnemy == null)
        {
            Debug.LogError("Enemy_Respawner: Instantiate returned null");
            return;
        }

        Debug.Log($"Enemy_Respawner: spawned '{newEnemy.name}' at {finalSpawn}");

        // Stabilize physics on spawn
        Rigidbody2D rb = newEnemy.GetComponentInChildren<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.Sleep();
            rb.WakeUp();
        }

        bool createdOnTheRight = player != null && newEnemy.transform.position.x > player.position.x;

        Enemy enemy = newEnemy.GetComponentInChildren<Enemy>();
        if (enemy == null)
        {
            Debug.LogWarning($"Enemy_Respawner: spawned object {newEnemy.name} has no Enemy component (GetComponentInChildren returned null)");
            return;
        }

        // Don't flip goblins here — their chase logic decides facing
        if (enemy.GetType() != typeof(Goblin) && createdOnTheRight)
        {
            enemy.Flip();
        }
    }

    // Editor visualization
    private void OnDrawGizmosSelected()
    {
        if (respawnPoints == null) return;
        Gizmos.color = Color.cyan;
        foreach (var t in respawnPoints)
        {
            if (t == null) continue;
            Gizmos.DrawWireCube(t.position + Vector3.up * spawnYOffset + Vector3.up * (spawnCheckSize.y * 0.5f + 0.02f), spawnCheckSize);
        }
    }
}

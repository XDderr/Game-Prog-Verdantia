using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyEncounterSpawner : MonoBehaviour
{
    [Header("Encounter Settings")]
    [SerializeField] private GameObject enemyPrefab; // Assign your EnemyAI prefab
    [SerializeField] private int enemiesToSpawn = 3;
    [SerializeField] private float triggerRadius = 12f;
    [SerializeField] private float spawnSpreadRadius = 4f;
    [SerializeField] private LayerMask playerUnitLayer; // Set to your player units' layer (e.g., clickable)

    [Header("Behavior Options")]
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private float respawnCooldown = 30f; // Used only if triggerOnce is false
    [SerializeField] private bool destroySpawnerAfterTrigger = false;

    private bool hasTriggered = false;
    private float cooldownTimer = 0f;
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private void Update()
    {
        if (hasTriggered)
        {
            if (triggerOnce) return;

            // Optional repeatable encounter cooldown once all spawned enemies are defeated
            spawnedEnemies.RemoveAll(enemy => enemy == null);
            if (spawnedEnemies.Count == 0)
            {
                cooldownTimer += Time.deltaTime;
                if (cooldownTimer >= respawnCooldown)
                {
                    hasTriggered = false;
                    cooldownTimer = 0f;
                }
            }
            return;
        }

        CheckForPlayerUnits();
    }

    private void CheckForPlayerUnits()
    {
        // OverlapSphere avoids adding a physical collider that could interfere with mouse hover/click raycasts
        Collider[] hits = Physics.OverlapSphere(transform.position, triggerRadius, playerUnitLayer, QueryTriggerInteraction.Ignore);

        foreach (Collider hit in hits)
        {
            // Verify the detected object is one of the player's Units (ignores enemy detection spheres)
            if (hit.GetComponent<Unit>() != null)
            {
                TriggerEncounter();
                break;
            }
        }
    }

    private void TriggerEncounter()
    {
        if (enemyPrefab == null) return;

        hasTriggered = true;

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Vector3 spawnPos = GetValidNavMeshPoint();
            Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            GameObject enemy = Instantiate(enemyPrefab, spawnPos, randomRotation);
            spawnedEnemies.Add(enemy);
        }

        if (destroySpawnerAfterTrigger)
        {
            Destroy(gameObject);
        }
    }

    private Vector3 GetValidNavMeshPoint()
    {
        // Pick a random offset around the spawner prefab
        Vector2 randomCircle = Random.insideUnitCircle * spawnSpreadRadius;
        Vector3 candidatePoint = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

        // Snap the point onto the baked NavMesh so NavMeshAgent initializes cleanly
        if (NavMesh.SamplePosition(candidatePoint, out NavMeshHit hit, spawnSpreadRadius + 2f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        // Fallback to the spawner's own position if no offset point sampled cleanly
        return transform.position;
    }

    // Visualizes trigger and spawn radii in the Scene view
    private void OnDrawGizmosSelected()
    {
        // Cyan = Player detection range to start the encounter
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);

        // Magenta = Area where enemies will appear around the structure
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, spawnSpreadRadius);
    }
}
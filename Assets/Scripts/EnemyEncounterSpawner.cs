using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyEncounterSpawner : MonoBehaviour
{
    public enum SpawnDistributionMode
    {
        ClusteredAtSpawner, // Enemies emerge directly around the spawner prefab
        SpreadOut           // Enemies appear dispersed across a wider area (ambush/camp layout)
    }

    [Header("Encounter Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int enemiesToSpawn = 3;
    [SerializeField] private float triggerRadius = 12f;
    [SerializeField] private LayerMask playerUnitLayer;

    [Header("Spawn Distribution")]
    [SerializeField] private SpawnDistributionMode spawnMode = SpawnDistributionMode.ClusteredAtSpawner;

    [Tooltip("Used in Clustered mode: tight radius around the spawner prefab.")]
    [SerializeField] private float clusteredRadius = 2.5f;

    [Tooltip("Used in Spread Out mode: outer radius enemies can appear in.")]
    [SerializeField] private float spreadOutRadius = 10f;

    [Tooltip("Used in Spread Out mode: minimum distance kept between each spawned enemy.")]
    [SerializeField] private float minSpacingBetweenEnemies = 3f;

    [Tooltip("Optional: If populated in Spread Out mode, enemies spawn at these transforms instead of random points.")]
    [SerializeField] private List<Transform> customSpawnPoints = new List<Transform>();

    [Header("Behavior Options")]
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private float respawnCooldown = 30f;
    [SerializeField] private bool destroySpawnerAfterTrigger = false;

    private bool hasTriggered = false;
    private float cooldownTimer = 0f;
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private void Update()
    {
        if (hasTriggered)
        {
            if (triggerOnce) return;

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
        Collider[] hits = Physics.OverlapSphere(transform.position, triggerRadius, playerUnitLayer, QueryTriggerInteraction.Ignore);

        foreach (Collider hit in hits)
        {
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
        List<Vector3> usedSpawnPositions = new List<Vector3>();

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Vector3 spawnPos = GetSpawnPosition(i, usedSpawnPositions);
            usedSpawnPositions.Add(spawnPos);

            Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, randomRotation);
            spawnedEnemies.Add(enemy);
        }

        if (destroySpawnerAfterTrigger)
        {
            Destroy(gameObject);
        }
    }

    private Vector3 GetSpawnPosition(int spawnIndex, List<Vector3> usedPositions)
    {
        // Mode 1: Clustered right at the spawner prefab
        if (spawnMode == SpawnDistributionMode.ClusteredAtSpawner)
        {
            return SampleNavMeshAround(transform.position, clusteredRadius);
        }

        // Mode 2A: Spread out using specific designer-placed child transforms (if assigned)
        if (customSpawnPoints != null && customSpawnPoints.Count > 0)
        {
            Transform point = customSpawnPoints[spawnIndex % customSpawnPoints.Count];
            if (point != null)
            {
                return SampleNavMeshAround(point.position, 1f);
            }
        }

        // Mode 2B: Procedurally spread out with minimum spacing between enemies
        float minSpacingSqr = minSpacingBetweenEnemies * minSpacingBetweenEnemies;
        Vector3 bestCandidate = transform.position;

        // Try up to 15 times per enemy to find a NavMesh point that isn't too close to another enemy
        for (int attempt = 0; attempt < 15; attempt++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spreadOutRadius;
            Vector3 rawPoint = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

            if (NavMesh.SamplePosition(rawPoint, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                bestCandidate = hit.position;
                bool tooClose = false;

                foreach (Vector3 existingPos in usedPositions)
                {
                    if ((hit.position - existingPos).sqrMagnitude < minSpacingSqr)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    return hit.position;
                }
            }
        }

        return bestCandidate;
    }

    private Vector3 SampleNavMeshAround(Vector3 center, float radius)
    {
        Vector2 randomCircle = Random.insideUnitCircle * radius;
        Vector3 candidatePoint = center + new Vector3(randomCircle.x, 0f, randomCircle.y);

        if (NavMesh.SamplePosition(candidatePoint, out NavMeshHit hit, radius + 2f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return center;
    }

    private void OnDrawGizmosSelected()
    {
        // Cyan = Player detection range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);

        // Magenta = Active spawn zone based on selected Spawn Mode
        Gizmos.color = Color.magenta;
        if (spawnMode == SpawnDistributionMode.ClusteredAtSpawner)
        {
            Gizmos.DrawWireSphere(transform.position, clusteredRadius);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, spreadOutRadius);

            if (customSpawnPoints != null)
            {
                foreach (Transform pt in customSpawnPoints)
                {
                    if (pt != null) Gizmos.DrawSphere(pt.position, 0.5f);
                }
            }
        }
    }
}
using UnityEngine;

public class HolyCow_Bomb_Spawner : MonoBehaviour, IRespawnResetable
{
    [Header("Bomb Settings")]
    [Tooltip("The bomb prefab to spawn.")]
    [SerializeField]
    private GameObject bombPrefab;

    [Tooltip("Time interval in seconds between bomb spawns.")]
    [SerializeField]
    private float spawnInterval = 2f;

    private Transform playerTransform;
    private float spawnTimer;

    void Start()
    {
        playerTransform = GameMaster.instance.playerInstance.transform;

        // Initialize the spawn timer.
        spawnTimer = spawnInterval * 3f;
    }

    void Update()
    {
        // Check if player transform exists.
        if (playerTransform == null) return;

        // Countdown timer for spawning bombs.
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnBomb();
            spawnTimer = spawnInterval; // Reset the timer.
        }
    }

    private void SpawnBomb()
    {
        if (bombPrefab != null && playerTransform != null)
        {
            // Instantiate the bomb prefab at the player's position with no rotation.
            Instantiate(bombPrefab, playerTransform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Bomb prefab or player transform is missing!");
        }
    }

    public void PlayerHasRespawned()
    {
        spawnTimer = spawnInterval * 3f;
    }
}
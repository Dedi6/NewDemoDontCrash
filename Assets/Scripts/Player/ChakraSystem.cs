using UnityEngine;
using UnityEngine.UI;

public class ChakraSystem : MonoBehaviour
{
    public static ChakraSystem instance;

    [Header("Settings")]
    public int maxChakra = 2;                  // Start with 2, can be upgraded
    public float chakraDuration = 5f;

    [Header("UI References")]
    public Image[] chakraFills;                // Assign ALL possible fills (e.g., 5 for max upgrades)
    public GameObject[] chakraContainers;      // The parent objects for each chakra slot

    [Header("Orb Visuals")]
    public GameObject orbPrefab;               // The orb prefab with OrbDepthOrbit component
    public Transform playerTransform;          // Reference to player (can also use GameMaster)
    public int maxPoolSize = 5;                // Pool size (match max possible chakra upgrades)

    private int currentChakra = 0;
    private float[] timers;
    private bool isTimerRunning = false;
    private int currentMaxChakra;              // Current actual max (can be less than array length)

    // Orb pooling
    private OrbDepthOrbit[] orbPool;           // Pre-instantiated orbs
    private OrbDepthOrbit[] activeOrbs;        // Tracks which orb corresponds to which chakra slot (LIFO)

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        currentMaxChakra = maxChakra;
        timers = new float[chakraFills.Length]; // Support for maximum possible chakra

        // Initialize UI based on starting max
        UpdateMaxChakraUI();

        // Initialize all fills to empty
        foreach (Image fill in chakraFills)
        {
            fill.fillAmount = 0f;
        }

        // Initialize orb pool
        InitializeOrbPool();
    }

    private void InitializeOrbPool()
    {
        if (orbPrefab == null)
        {
            Debug.LogWarning("[ChakraSystem] No orbPrefab assigned - orb visuals disabled.");
            return;
        }

        // Try to get player transform if not assigned
        if (playerTransform == null)
        {
            playerTransform = GameMaster.instance?.playerInstance?.transform;
        }

        orbPool = new OrbDepthOrbit[maxPoolSize];
        activeOrbs = new OrbDepthOrbit[maxPoolSize];

        for (int i = 0; i < maxPoolSize; i++)
        {
            GameObject orbObj = Instantiate(orbPrefab, transform);
            orbObj.SetActive(false);
            orbPool[i] = orbObj.GetComponent<OrbDepthOrbit>();
            
            if (orbPool[i] == null)
            {
                Debug.LogError($"[ChakraSystem] Orb prefab missing OrbDepthOrbit component!");
            }
        }
    }

    /// <summary>
    /// Get an available orb from the pool.
    /// </summary>
    private OrbDepthOrbit GetOrbFromPool()
    {
        if (orbPool == null) return null;

        foreach (var orb in orbPool)
        {
            if (orb != null && !orb.IsActive)
            {
                return orb;
            }
        }
        return null; // Pool exhausted
    }

    /// <summary>
    /// Spawn a visual orb for the given chakra index.
    /// </summary>
    private void SpawnOrb(int chakraIndex)
    {
        if (playerTransform == null) return;

        OrbDepthOrbit orb = GetOrbFromPool();
        if (orb != null)
        {
            orb.Initialize(playerTransform);
            activeOrbs[chakraIndex] = orb;
        }
    }

    /// <summary>
    /// Destroy the visual orb at the given chakra index.
    /// </summary>
    private void DestroyOrb(int chakraIndex)
    {
        if (activeOrbs == null || chakraIndex < 0 || chakraIndex >= activeOrbs.Length)
            return;

        OrbDepthOrbit orb = activeOrbs[chakraIndex];
        if (orb != null && orb.IsActive)
        {
            orb.TriggerDestroy();
            activeOrbs[chakraIndex] = null;
        }
    }

    // Call this when player successfully parries
    public void AddChakra()
    {
        if (currentChakra < currentMaxChakra)
        {
            // Reset all previous timers visually
            for (int i = 0; i < currentChakra; i++)
            {
                chakraFills[i].fillAmount = 1f;
            }

            // Add new chakra
            timers[currentChakra] = chakraDuration;
            chakraFills[currentChakra].fillAmount = 1f; // Start full
            
            // Spawn visual orb
            SpawnOrb(currentChakra);
            
            currentChakra++;
            isTimerRunning = true;
        }
        else
        {
            // At max capacity - refresh the newest chakra's timer
            int newestIndex = currentChakra - 1;
            timers[newestIndex] = chakraDuration;
            chakraFills[newestIndex].fillAmount = 1f;
            // Orb already exists, no need to spawn
        }
    }

    void Update()
    {
        if (isTimerRunning && currentChakra > 0)
        {
            // Only update the newest (active) chakra
            int activeIndex = currentChakra - 1;
            timers[activeIndex] -= Time.deltaTime;

            // Update the fill amount (1 = full, 0 = empty)
            chakraFills[activeIndex].fillAmount = timers[activeIndex] / chakraDuration;

            // Check if active chakra expired
            if (timers[activeIndex] <= 0)
            {
                // Destroy the visual orb for expired chakra
                DestroyOrb(activeIndex);
                
                currentChakra--;
                if (currentChakra > 0)
                {
                    // Start counting down the next chakra
                    timers[currentChakra - 1] = chakraDuration;
                }
                else
                {
                    isTimerRunning = false;
                }
            }
        }
    }

    // Call this to upgrade player's max chakra capacity
    public void UpgradeMaxChakra(int newMax)
    {
        if (newMax <= chakraFills.Length && newMax > currentMaxChakra)
        {
            currentMaxChakra = newMax;
            UpdateMaxChakraUI();
            Debug.Log($"Chakra capacity upgraded to {currentMaxChakra}!");
        }
    }

    // Update which chakra slots are visible/enabled
    private void UpdateMaxChakraUI()
    {
        // Enable containers up to currentMax, disable the rest
        for (int i = 0; i < chakraContainers.Length; i++)
        {
            if (chakraContainers[i] != null)
            {
                chakraContainers[i].SetActive(i < currentMaxChakra);
            }
        }

        // If current chakra exceeds new max, reduce it
        if (currentChakra > currentMaxChakra)
        {
            // Destroy orbs for chakra being removed
            for (int i = currentMaxChakra; i < currentChakra; i++)
            {
                DestroyOrb(i);
            }
            
            currentChakra = currentMaxChakra;
            // Reset timers for remaining chakra
            for (int i = 0; i < currentChakra; i++)
            {
                timers[i] = chakraDuration;
                chakraFills[i].fillAmount = 1f;
            }
        }
    }

    // Public property to check current max
    public int CurrentMaxChakra => currentMaxChakra;

    // Public property to check current chakra count
    public int CurrentChakra => currentChakra;

    // Consume all chakra for spinning attack
    public int ConsumeAllChakra()
    {
        int consumed = currentChakra;
        
        // Destroy all active orbs
        for (int i = 0; i < currentChakra; i++)
        {
            DestroyOrb(i);
        }
        
        currentChakra = 0;
        isTimerRunning = false;

        // Reset all fills visually
        foreach (Image fill in chakraFills)
        {
            fill.fillAmount = 0f;
        }

        return consumed;
    }

    /// <summary>
    /// Try to consume chakra (e.g., for teleportation).
    /// Consumes the newest (active) chakra point first.
    /// </summary>
    /// <param name="amount">Amount to consume (default 1)</param>
    /// <returns>True if successfully consumed, false if not enough chakra</returns>
    public bool TryConsumeChakra(int amount = 1)
    {
        if (currentChakra < amount)
            return false;
        
        // Consume from the newest (top of the stack)
        for (int i = 0; i < amount; i++)
        {
            currentChakra--;
            chakraFills[currentChakra].fillAmount = 0f;
            timers[currentChakra] = 0f;
            
            // Destroy the visual orb
            DestroyOrb(currentChakra);
        }
        
        // If chakra remains, the new "newest" gets a fresh countdown
        if (currentChakra > 0)
        {
            timers[currentChakra - 1] = chakraDuration;
            chakraFills[currentChakra - 1].fillAmount = 1f; // Reset fill to full
            isTimerRunning = true;
        }
        else
        {
            isTimerRunning = false;
        }
        
        return true;
    }

    /// <summary>
    /// Check if player has enough chakra without consuming it.
    /// Useful for UI feedback or ability availability checks.
    /// </summary>
    public bool HasChakra(int amount = 1)
    {
        return currentChakra >= amount;
    }
}

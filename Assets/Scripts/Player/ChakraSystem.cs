using UnityEngine;
using UnityEngine.UI;

public class ChakraSystem : MonoBehaviour
{
    [Header("Settings")]
    public int maxChakra = 2;                  // Start with 2, can be upgraded
    public float chakraDuration = 5f;

    [Header("UI References")]
    public Image[] chakraFills;                // Assign ALL possible fills (e.g., 5 for max upgrades)
    public GameObject[] chakraContainers;      // The parent objects for each chakra slot

    private int currentChakra = 0;
    private float[] timers;
    private bool isTimerRunning = false;
    private int currentMaxChakra;              // Current actual max (can be less than array length)

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
            currentChakra++;

            isTimerRunning = true;
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
        currentChakra = 0;
        isTimerRunning = false;

        // Reset all fills visually
        foreach (Image fill in chakraFills)
        {
            fill.fillAmount = 0f;
        }

        return consumed;
    }

    // For testing
    public void DebugAddChakra()
    {
        AddChakra();
    }

    // For testing upgrades
    public void DebugUpgradeMaxChakra()
    {
        if (currentMaxChakra < chakraFills.Length)
        {
            UpgradeMaxChakra(currentMaxChakra + 1);
        }
    }
}
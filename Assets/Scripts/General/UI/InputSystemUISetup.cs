using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Sets up the EventSystem to use the new Input System for UI navigation.
/// Attach this to your EventSystem GameObject or it will find the EventSystem automatically.
/// This enables joystick/controller navigation in menus.
/// </summary>
public class InputSystemUISetup : MonoBehaviour
{
    private void Awake()
    {
        SetupInputSystemUI();
    }

    [ContextMenu("Setup Input System UI")]
    public void SetupInputSystemUI()
    {
        // Find or get the EventSystem
        EventSystem eventSystem = GetComponent<EventSystem>();
        if (eventSystem == null)
        {
            eventSystem = EventSystem.current;
        }
        if (eventSystem == null)
        {
            eventSystem = FindObjectOfType<EventSystem>();
        }
        if (eventSystem == null)
        {
            Debug.LogWarning("InputSystemUISetup: No EventSystem found in scene!");
            return;
        }

        // Check if old StandaloneInputModule exists and remove it
        StandaloneInputModule oldModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (oldModule != null)
        {
            Debug.Log("InputSystemUISetup: Removing old StandaloneInputModule...");
            DestroyImmediate(oldModule);
        }

        // Check if InputSystemUIInputModule already exists
        InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
        {
            Debug.Log("InputSystemUISetup: Adding InputSystemUIInputModule...");
            inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        // Configure the InputSystemUIInputModule to use our Controls asset
        // The module will auto-detect actions from the default UI action map
        // You can also manually assign action references here if needed
        
        Debug.Log("InputSystemUISetup: Input System UI configured successfully!");
    }
}

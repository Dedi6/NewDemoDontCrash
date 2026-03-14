using UnityEngine;
using MyBox;
using System;

public class OrbDepthOrbit : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public Vector3 bellyOffset = new Vector3(0, 0.5f, 0);

    [Header("Orbit Parameters (set at runtime via Initialize)")]
    [ReadOnly] public float horizontalRadius = 0.35f;
    [ReadOnly] public float depthRadius = 0.2f;
    [ReadOnly] public float verticalSway = 0.05f;
    [ReadOnly] public float speed = 2f;

    [Header("Randomization Ranges")]
    [MinMaxRange(0.1f, 1f)]
    public RangedFloat horizontalRadiusRange = new RangedFloat(0.25f, 0.45f);
    
    [MinMaxRange(0.01f, 0.3f)]
    public RangedFloat verticalSwayRange = new RangedFloat(0.03f, 0.1f);
    
    [MinMaxRange(0.5f, 5f)]
    public RangedFloat speedRange = new RangedFloat(1.5f, 3f);

    [Header("Animation")]
    public Animator animator;
    
    // Animation state names (match your Animator Controller)
    private const string ANIM_CREATE = "Create";
    private const string ANIM_IDLE = "Idle";
    private const string ANIM_DESTROY = "Destroy";

    private bool isActive = false;
    private float timeOffset; // So orbs don't all sync up
    private Action onDestroyComplete;

    void FixedUpdate()
    {
        if (!isActive || player == null) return;

        float t = (Time.time + timeOffset) * speed;

        float x = Mathf.Cos(t) * horizontalRadius;
        float z = Mathf.Sin(t) * depthRadius;
        float y = Mathf.Sin(t) * verticalSway;

        transform.position = player.position
                           + bellyOffset
                           + new Vector3(x, y, z);
    }

    /// <summary>
    /// Initialize the orb with randomized parameters and play creation animation.
    /// </summary>
    /// <param name="playerTransform">The player to orbit around</param>
    public void Initialize(Transform playerTransform)
    {
        player = playerTransform;
        
        // Randomize parameters
        horizontalRadius = UnityEngine.Random.Range(horizontalRadiusRange.Min, horizontalRadiusRange.Max);
        verticalSway = UnityEngine.Random.Range(verticalSwayRange.Min, verticalSwayRange.Max);
        
        // Randomize speed AND direction (positive or negative)
        float randomSpeed = UnityEngine.Random.Range(speedRange.Min, speedRange.Max);
        speed = UnityEngine.Random.value > 0.5f ? randomSpeed : -randomSpeed;
        
        // Random time offset so orbs don't orbit in sync
        timeOffset = UnityEngine.Random.Range(0f, 10f);
        
        // Activate
        isActive = true;
        gameObject.SetActive(true);
        
        // Play creation animation
        if (animator != null)
        {
            animator.Play(ANIM_CREATE, 0, 0f);
        }
    }

    /// <summary>
    /// Called by Animation Event at the end of Create animation.
    /// Transitions to Idle state.
    /// </summary>
    public void OnCreateAnimationComplete()
    {
        if (animator != null)
        {
            animator.Play(ANIM_IDLE);
        }
    }

    /// <summary>
    /// Trigger the destroy animation. When complete, calls the callback.
    /// </summary>
    /// <param name="onComplete">Callback when destruction is complete (return to pool)</param>
    public void TriggerDestroy(Action onComplete = null)
    {
        onDestroyComplete = onComplete;
        
        if (animator != null)
        {
            animator.Play(ANIM_DESTROY, 0, 0f);
        }
        else
        {
            // No animator, just complete immediately
            OnDestroyAnimationComplete();
        }
    }

    /// <summary>
    /// Called by Animation Event at the end of Destroy animation.
    /// Deactivates the orb and invokes callback.
    /// </summary>
    public void OnDestroyAnimationComplete()
    {
        isActive = false;
        gameObject.SetActive(false);
        
        onDestroyComplete?.Invoke();
        onDestroyComplete = null;
    }

    /// <summary>
    /// Force deactivate without animation (for edge cases like scene transitions).
    /// </summary>
    public void ForceDeactivate()
    {
        isActive = false;
        gameObject.SetActive(false);
        onDestroyComplete = null;
    }

    /// <summary>
    /// Check if this orb is currently active.
    /// </summary>
    public bool IsActive => isActive;
}

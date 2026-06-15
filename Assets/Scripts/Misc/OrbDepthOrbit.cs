using UnityEngine;
using MyBox;
using System;

public class OrbDepthOrbit : MonoBehaviour
{
    [Header("Target")]
    public Transform orbitCenter;

    [Header("Orbit Parameters")]
    public float horizontalRadius = 0.35f;
    public float depthRadius = 0.2f;
    public float verticalSway = 0.05f;
    public float speed = 2f;

    [Header("Randomization Ranges")]
    [MinMaxRange(1.4f, 1.8f)]
    public RangedFloat horizontalRadiusRange = new RangedFloat(0.25f, 0.45f);
    
    [MinMaxRange(-0.85f, 0.85f)]
    public RangedFloat verticalSwayRange = new RangedFloat(0.03f, 0.1f);

    [MinMaxRange(2f, 3f)]
    public RangedFloat depthRadiusRange = new RangedFloat(0.03f, 0.1f);
    
    [MinMaxRange(3f, 5f)]
    public RangedFloat speedRange = new RangedFloat(1.5f, 3f);

    [Header("Trail")]
    public float trailTimeIdle = 0.3f;
    public float trailTimeMoving = 0.06f;
    [Tooltip("Minimum player speed (units/sec) to count as moving")]
    public float moveSpeedThreshold = 1f;

    [Header("Animation")]
    public Animator animator;
    
    private const string ANIM_CREATE = "Create";
    private const string ANIM_IDLE = "Idle";
    private const string ANIM_DESTROY = "Destroy";

    private bool isActive = false;
    private float timeOffset;
    private Action onDestroyComplete;
    private TrailRenderer trailRenderer;
    private Vector3 lastCenterPos;

    void FixedUpdate()
    {
        if (!isActive || orbitCenter == null) return;

        UpdateOrbPosition();

        if (trailRenderer == null) return;

        // Snap trail time based on player speed
        float sqrSpeed = (orbitCenter.position - lastCenterPos).sqrMagnitude
                         / (Time.fixedDeltaTime * Time.fixedDeltaTime);
        bool isMoving = sqrSpeed > moveSpeedThreshold * moveSpeedThreshold;
        trailRenderer.time = isMoving ? trailTimeMoving : trailTimeIdle;
        lastCenterPos = orbitCenter.position;
    }

    void LateUpdate()
    {
        if (!isActive || orbitCenter == null) return;

        // Re-apply position after Update (where player flip happens) so the orb
        // is always in the correct spot before the frame renders.
        UpdateOrbPosition();
    }

    private void UpdateOrbPosition()
    {
        float t = (Time.time + timeOffset) * speed;
        float x = Mathf.Cos(t) * horizontalRadius;
        float z = Mathf.Sin(t) * depthRadius;
        float y = Mathf.Sin(t) * verticalSway;

        transform.position = orbitCenter.position + new Vector3(x, y, z);
    }

    /// <summary>
    /// Initialize the orb with randomized parameters and play creation animation.
    /// </summary>
    /// <param name="center">Transform to orbit around (e.g. orbParent under the player)</param>
    public void Initialize(Transform center)
    {
        orbitCenter = center;

        horizontalRadius = UnityEngine.Random.Range(horizontalRadiusRange.Min, horizontalRadiusRange.Max);
        verticalSway = UnityEngine.Random.Range(verticalSwayRange.Min, verticalSwayRange.Max);
        depthRadius = UnityEngine.Random.Range(depthRadiusRange.Min, depthRadiusRange.Max);

        float randomSpeed = UnityEngine.Random.Range(speedRange.Min, speedRange.Max);
        speed = UnityEngine.Random.value > 0.5f ? randomSpeed : -randomSpeed;

        timeOffset = UnityEngine.Random.Range(0f, 10f);

        // Set position before activating so the trail doesn't smear from old pooled position
        float t = (Time.time + timeOffset) * speed;
        transform.position = center.position + new Vector3(
            Mathf.Cos(t) * horizontalRadius,
            Mathf.Sin(t) * verticalSway,
            Mathf.Sin(t) * depthRadius
        );

        trailRenderer = GetComponent<TrailRenderer>();
        lastCenterPos = center.position;

        isActive = true;
        gameObject.SetActive(true);

        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }

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

using System;
using UnityEngine;
using MyBox;

[Serializable]
public class ColliderDataHolder
{
    public Vector2 position; // Local position of the collider
    public Vector2 size;     // Size of the collider
    public float rotation;   // Rotation of the collider
}

[Serializable]
public class HitFrameData
{
    public ColliderDataHolder[] colliders; // Array of colliders for this frame
    public bool useSpecialValues;          // Toggle for special values

    [ConditionalField(nameof(useSpecialValues), false, true)] public int damage;
    [ConditionalField(nameof(useSpecialValues), false, true)] public float screenShakeForce; 
}

public class AttackData : MonoBehaviour
{
    public HitFrameData[] hitFrames; // Array of frames for this attack
}
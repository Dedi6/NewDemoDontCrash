using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AttackData))]
public class AttackDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        AttackData attackData = (AttackData)target;

        if (GUILayout.Button("Arrange Attack Data"))
        {
            ArrangeAttackData(attackData);
        }
    }

    private void ArrangeAttackData(AttackData attackData)
    {
        // Clear existing data
        attackData.hitFrames = new HitFrameData[0];

        // Get all direct child GameObjects (frames)
        int frameCount = attackData.transform.childCount;
        attackData.hitFrames = new HitFrameData[frameCount];

        for (int i = 0; i < frameCount; i++)
        {
            Transform frame = attackData.transform.GetChild(i);

            // Get all CapsuleCollider2D components in this frame and its children
            CapsuleCollider2D[] colliders = frame.GetComponentsInChildren<CapsuleCollider2D>();

            // Initialize the HitFrameData for this frame
            HitFrameData hitFrame = new HitFrameData
            {
                colliders = new ColliderDataHolder[colliders.Length]
            };

            // Populate the ColliderDataHolder array
            for (int j = 0; j < colliders.Length; j++)
            {
                CapsuleCollider2D collider = colliders[j];

                // Calculate the world position of the collider, including its offset
                Vector2 colliderWorldPosition = collider.transform.TransformPoint(collider.offset);

                // Convert the world position to the local space of the AttackData parent
                Vector2 localPosition = attackData.transform.InverseTransformPoint(colliderWorldPosition);

                // Calculate the rotation relative to the AttackData parent
                Quaternion globalRotation = collider.transform.rotation;
                Quaternion localRotation = Quaternion.Inverse(attackData.transform.rotation) * globalRotation;

                hitFrame.colliders[j] = new ColliderDataHolder
                {
                    position = localPosition, // Local position relative to AttackData parent (including offset)
                    size = collider.size,     // Size
                    rotation = localRotation.eulerAngles.z // Rotation relative to AttackData parent
                };
            }

            // Add the HitFrameData to the array
            attackData.hitFrames[i] = hitFrame;
        }

        Debug.Log("Attack Data Arranged Successfully!");
    }
}
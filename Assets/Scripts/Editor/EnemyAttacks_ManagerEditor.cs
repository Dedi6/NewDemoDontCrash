using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyAttacks_Manager))]
public class EnemyAttacks_ManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        base.OnInspectorGUI();

        EnemyAttacks_Manager manager = (EnemyAttacks_Manager)target;

        // Add a button to configure constant hitboxes
        if (GUILayout.Button("Configure Constant Hitboxes"))
        {
            ConfigureConstantHitboxes(manager);
        }
    }

  

    private void ConfigureConstantHitboxes(EnemyAttacks_Manager manager)
    {
        // Loop through all attacks and configure constant hitboxes
        foreach (var attack in manager.attacks)
        {
            if (attack.isConstantHitbox && attack.constantHitbox == null)
            {
                // Create a new GameObject for the constant hitbox
                GameObject hitboxObject = new GameObject("ConstantHitbox");
                hitboxObject.transform.SetParent(manager.transform);
                hitboxObject.transform.localPosition = Vector3.zero;

                // Add a CapsuleCollider2D and configure it
                CapsuleCollider2D collider = hitboxObject.AddComponent<CapsuleCollider2D>();
                collider.isTrigger = true;
                attack.constantHitbox = collider;

                Debug.Log($"Configured constant hitbox for {attack.attackName}");
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class EnemyAttacks_Manager : MonoBehaviour
{
    public EnemyAttack[] attacks; // Array of attacks for this enemy

    private EnemyAttack currentAttack; // The currently active attack

    [System.Serializable]
    public class EnemyAttack
    {
        public string attackName; // Unique identifier for the attack
        public AttackData attackData; // Hitbox data for animation-based attacks
        public bool isConstantHitbox; // Is this a constant hitbox attack?
        [ConditionalField(nameof(isConstantHitbox), false, true)]
        public CapsuleCollider2D constantHitbox; // Reference to the constant hitbox collider
    }

    // Call this method to start an attack
    public void StartAttack(string attackName)
    {
        // Find the attack by name
        currentAttack = System.Array.Find(attacks, attack => attack.attackName == attackName);

        if (currentAttack == null)
        {
            Debug.LogWarning("Attack not found: " + attackName);
            return;
        }

        // Enable the constant hitbox if this is a constant hitbox attack
        if (currentAttack.isConstantHitbox && currentAttack.constantHitbox != null)
        {
            currentAttack.constantHitbox.enabled = true;
            currentAttack.constantHitbox.isTrigger = true; // Ensure it's a trigger collider
        }

        // Trigger the attack animation (you can replace this with your animation logic)
        GetComponent<Animator>().SetTrigger(currentAttack.attackName);
    }

    // Called when the attack animation completes
    public void EndAttack()
    {
        if (currentAttack == null)
        {
            return;
        }

        // Disable the constant hitbox if this is a constant hitbox attack
        if (currentAttack.isConstantHitbox && currentAttack.constantHitbox != null)
        {
            currentAttack.constantHitbox.enabled = false;
        }

        // Reset the current attack
        currentAttack = null;
    }

    // Handle trigger events for constant hitbox attacks
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (currentAttack != null && currentAttack.isConstantHitbox)
        {
            if (other.CompareTag("Player")) // Check if the collider is the player
            {
                // Apply damage to the player
                other.GetComponent<MovementPlatformer>().GotHitByAnEnemy(currentAttack.attackData.hitFrames[0].damage);
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class EnemyAttacks_Manager : MonoBehaviour
{
    public EnemyAttack[] attacks; // Array of attacks for this enemy

    private EnemyAttack currentAttack; // The currently active attack

    private int currentFrame = 0;      // Tracks the current frame of the attack

    private LayerMask hitcheck_Layermasks = (1 << 11) | (1 << 25); 

    [System.Serializable]
    public class EnemyAttack
    {
        public string attackName; // Unique identifier for the attack
        public AttackData attackData; // Hitbox data for animation-based attacks
        public bool isConstantHitbox; // Is this a constant hitbox attack?
        [ConditionalField(nameof(isConstantHitbox), false, true)]
        public CapsuleCollider2D constantHitbox; // Reference to the constant hitbox collider
        public float parryTime;
    }

    // Call this method to start an attack
    public void StartAttack(string attackName)
    {
        // Find the attack by name
        currentAttack = System.Array.Find(attacks, attack => attack.attackName == attackName);

        if (currentAttack == null)
        {
            Debug.Log("Attack not found: " + attackName);
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


    // Handle trigger events for constant hitbox attacks
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (currentAttack != null && currentAttack.isConstantHitbox)
        {
            if (other.gameObject.layer == hitcheck_Layermasks) // Check if the collider is the player
            {
                // Apply damage to the player
                //other.GetComponent<MovementPlatformer>().GotHitByAnEnemy(currentAttack.attackData.hitFrames[0].damage);
                PlayerKnockBackAndDamage(other.GetComponent<MovementPlatformer>());
            }
        }
    }


    public void PlayerKnockBackAndDamage(MovementPlatformer player)
    {
        if (player.transform.position.x > transform.position.x)
            player.GetComponent<MovementPlatformer>().KnockBackPlayer(25f, 1f, 0.5f, true);
        else
            player.GetComponent<MovementPlatformer>().KnockBackPlayer(25f, 1f, 0.5f, false);
        player.GetComponent<MovementPlatformer>().GotHitByAnEnemy(4);
    }

    // Called by animation events to check for hits
    public void CheckHit()
    {
        if (currentAttack == null || currentAttack.isConstantHitbox)
        {
            return; // Skip if no attack is active or this is a constant hitbox attack
        }

        if (currentFrame >= currentAttack.attackData.hitFrames.Length)
        {
            Debug.LogWarning("Frame index out of range.");
            return;
        }

        // Get the HitFrameData for the current frame
        HitFrameData hitFrame = currentAttack.attackData.hitFrames[currentFrame];

        // Perform overlap checks for each collider in the frame
        foreach (var colliderData in hitFrame.colliders)
        {
            // Calculate the world position and rotation of the collider
            Vector2 worldPosition = transform.TransformPoint(colliderData.position);
            Quaternion worldRotation = transform.rotation * Quaternion.Euler(0, 0, colliderData.rotation);

            // Check for overlaps with player colliders
            Collider2D[] hitPlayers = Physics2D.OverlapCapsuleAll(
                worldPosition,
                colliderData.size,
                CapsuleDirection2D.Vertical,
                worldRotation.eulerAngles.z,
                hitcheck_Layermasks // Adjust the layer mask as needed
            );


            /* // Apply damage to all hit players
             foreach (Collider2D player in hitPlayers)
             {

                 if (player.gameObject.layer == 1 << 25) // parry
                 {
                 Debug.Log(player.gameObject.name);  
                     Debug.Log("parry" + player.GetComponent<Player_Pull_Handler>().Get_ParryState());
                     return;
                 }

                 PlayerKnockBackAndDamage(player.GetComponent<MovementPlatformer>());
             }*/


            // If the attack hits the player
            if (hitPlayers.Length > 0)
            {
                // Check if the player is parrying and the attack is parriable
                if (GameMaster.instance.Get_ParryState() != Player_Pull_Handler.Parry_State.Waiting)  //currentAttack.isParriable)
                {
                    GameMaster.Succesful_Parry();
                    TryGetComponent<IParriable>(out IParriable parried_Enemy);
                    {
                        parried_Enemy.Got_Parried(currentAttack.parryTime);
                    }
                   // SuccessfulParry();
                    return; // Skip damage application if the attack is parried
                }

                // Apply damage to all hit players
                foreach (Collider2D player in hitPlayers)
                {
                    PlayerKnockBackAndDamage(player.GetComponent<MovementPlatformer>());
                }
            }

        }

        // Increment the frame counter for the next call
        currentFrame++;
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
        currentFrame = 0;
    }

    /*private void OnDrawGizmos()
    {
        if (currentAttack != null && !currentAttack.isConstantHitbox)
        {
            Gizmos.color = Color.red;
            foreach (var hitFrame in currentAttack.attackData.hitFrames)
            {
                foreach (var colliderData in hitFrame.colliders)
                {
                    Vector2 worldPosition = transform.TransformPoint(colliderData.position);
                    Quaternion worldRotation = transform.rotation * Quaternion.Euler(0, 0, colliderData.rotation);

                    Gizmos.matrix = Matrix4x4.TRS(worldPosition, worldRotation, Vector3.one);
                    Gizmos.DrawWireCube(Vector3.zero, colliderData.size);
                }
            }
        }
    }*/

    private void SuccessfulParry()
    {
        Debug.Log("Parry successful!");
        // Grant Chakra points
        // Trigger parry animation
        // Apply i-frames or counterattack
    }
}
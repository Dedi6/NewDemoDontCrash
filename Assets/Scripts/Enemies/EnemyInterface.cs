using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISFXResetable
{
    void ResetSFXCues();
    void SetStateDead();
}

public interface IKnockbackable
{
    void DisableOtherMovement();
}

public interface IPhaseable<T>
{
    void HandlePhases(T hp);
}

public interface IRespawnResetable
{
    void PlayerHasRespawned();
}



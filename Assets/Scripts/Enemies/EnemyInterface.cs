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

public interface IParriable
{
    void Got_Parried(float parryTime);
}



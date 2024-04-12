using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Set_StepAudio : MonoBehaviour
{
    [SerializeField]
    private TileTerrain.TerrainList terrainType;

  /*  [SerializeField]
    private AudioManager.SoundList walkingSound, landingSound;*/
    

    public TileTerrain.TerrainList GetTerrainType()
    {
        return terrainType;
    }
}

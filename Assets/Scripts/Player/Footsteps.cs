using UnityEngine;
using UnityEngine.Tilemaps;

public class Footsteps : MonoBehaviour
{

    public Tilemap currentTileMap;
    public Transform footstepPosition;
    private float footstepTime = 0.1f;
    public float stepsPerT;
    AudioManager.SoundList currentSound;
    private AudioManager audioManager;

    void Start()
    {
        audioManager = AudioManager.instance;
    }
    void Update()
    {
        float isMoving = GetComponentInParent<MovementPlatformer>().moveInput;
        bool isGrounded = GetComponentInParent<MovementPlatformer>().isGrounded;
        if (isGrounded && isMoving != 0 && (footstepTime + stepsPerT < Time.time))
        {
            checkGroundType();
            footstepTime = Time.time;
        }

    }

    private void checkGroundType()
    {
        TileTerrain currentTerrain = currentTileMap.GetTile(currentTileMap.WorldToCell(footstepPosition.position)) as TileTerrain;
        if (currentTerrain != null)
        {
            switch (currentTerrain.TerrainType)
            {
                case TileTerrain.TerrainList.Grass:
                    //gm.GetComponent<AudioManager>().PlaySound(AudioManager.SoundList.StepGrass);
                    currentSound = AudioManager.SoundList.StepGrass;
                    break;
                case TileTerrain.TerrainList.Stone:
                    currentSound = AudioManager.SoundList.StepStone;
                    break;
                case TileTerrain.TerrainList.Wood:
                    break;
            }
            audioManager.PlaySound(currentSound);
        }
        else
            audioManager.PlaySound(currentSound);
    }

    public void PlayerLanded()
    {
        TileTerrain currentTerrain = currentTileMap.GetTile(currentTileMap.WorldToCell(footstepPosition.position)) as TileTerrain;
        if (currentTerrain != null)
        {
            switch (currentTerrain.TerrainType)
            {
                case TileTerrain.TerrainList.Grass:
                 //   gm.GetComponent<AudioManager>().PlaySound(AudioManager.SoundList.LandGrass);
                    currentSound = AudioManager.SoundList.LandGrass;
                    break;
                case TileTerrain.TerrainList.Stone:
               //     gm.GetComponent<AudioManager>().PlaySound(AudioManager.SoundList.LandStone);
                    currentSound = AudioManager.SoundList.LandStone;
                    break;
                case TileTerrain.TerrainList.Wood:
                    //  gm.GetComponent<AudioManager>().PlaySound(AudioManager.SoundList.LandGrass);
                    break;
            }
            audioManager.PlaySound(currentSound);
        }
        else
            audioManager.PlaySound(currentSound);

    }

}

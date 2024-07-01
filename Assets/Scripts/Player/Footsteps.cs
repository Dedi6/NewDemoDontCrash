using UnityEngine;
using UnityEngine.Tilemaps;

public class Footsteps : MonoBehaviour
{

    public Tilemap currentTileMap;
    public Transform footstepPosition;
    private float footstepTime = 0.1f;
    public float stepsPerT;
    private AudioManager.SoundList currentSound_Walking, currentSound_Landing;
    private AudioManager audioManager;

    [SerializeField]
    private LayerMask groundLayerMask;


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
        TileTerrain.TerrainList current_Type = TileTerrain.TerrainList.Stone;

        Collider2D platform = Physics2D.OverlapCircle(footstepPosition.position, 0.1f, groundLayerMask);
        if (platform == null)
        {
            audioManager.PlaySound(currentSound_Walking);
            return;
        }

        if(!platform.gameObject.Equals(currentTileMap.gameObject))
        {
            if(platform.TryGetComponent(out Set_StepAudio audioData))
            {
                current_Type = audioData.GetTerrainType();
            }
        }
        else // is on tilemap
        {
            TileTerrain currentTerrain = currentTileMap.GetTile(currentTileMap.WorldToCell(footstepPosition.position)) as TileTerrain;
            if (currentTerrain != null)
                current_Type = currentTerrain.TerrainType;
            else
            {
                audioManager.PlaySound(currentSound_Walking);
                return;
            }
        }

        switch (current_Type)
        {
            case TileTerrain.TerrainList.Grass:
                currentSound_Walking = AudioManager.SoundList.StepGrass;
                break;
            case TileTerrain.TerrainList.Stone:
                currentSound_Walking = AudioManager.SoundList.StepStone;
                break;
            case TileTerrain.TerrainList.Wood:
                break;
        }

        audioManager.PlaySound(currentSound_Walking);
    }

    public void PlayerLanded()
    {
        TileTerrain.TerrainList current_Type = TileTerrain.TerrainList.Stone;

        Collider2D platform = Physics2D.OverlapCircle(footstepPosition.position, 0.1f, groundLayerMask);
        if (platform == null)
        {
            audioManager.PlaySound(currentSound_Landing);
            return;
        }

        if (!platform.gameObject.Equals(currentTileMap.gameObject))
        {
            if (platform.TryGetComponent(out Set_StepAudio audioData))
            {
                current_Type = audioData.GetTerrainType();
            }
        }
        else // is on tilemap
        {
            TileTerrain currentTerrain = currentTileMap.GetTile(currentTileMap.WorldToCell(footstepPosition.position)) as TileTerrain;
            if (currentTerrain != null)
                current_Type = currentTerrain.TerrainType;
            else
            {
                audioManager.PlaySound(currentSound_Landing);
                return;
            }
        }

        switch (current_Type)
        {
            case TileTerrain.TerrainList.Grass:
                currentSound_Landing = AudioManager.SoundList.LandGrass;
                break;
            case TileTerrain.TerrainList.Stone:
                currentSound_Landing = AudioManager.SoundList.LandStone;
                break;
            case TileTerrain.TerrainList.Wood:
                break;
        }

        audioManager.PlaySound(currentSound_Landing);
    }

}


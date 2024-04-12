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
            return;

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

    void TomarkinTest()
    {
        TileTerrain currentTerrain = currentTileMap.GetTile(currentTileMap.WorldToCell(footstepPosition.position)) as TileTerrain;
        if (currentTerrain != null)
        {
            switch (currentTerrain.TerrainType)
            {
                case TileTerrain.TerrainList.Grass:
                    //   gm.GetComponent<AudioManager>().PlaySound(AudioManager.SoundList.LandGrass);
                    currentSound_Walking = AudioManager.SoundList.StepGrass;
                    break;
                case TileTerrain.TerrainList.Stone:
                    //     gm.GetComponent<AudioManager>().PlaySound(AudioManager.SoundList.LandStone);
                    currentSound_Walking = AudioManager.SoundList.StepStone;
                    break;
                case TileTerrain.TerrainList.Wood:
                    //  gm.GetComponent<AudioManager>().PlaySound(AudioManager.SoundList.LandGrass);
                    break;
            }
            audioManager.PlaySound(currentSound_Walking);
        }
        else
            audioManager.PlaySound(currentSound_Walking);
    }

    public void SetCurrentSound(AudioManager.SoundList newSound_Walking, AudioManager.SoundList newSound_Landing) // use this for the fix later to set every steppable object manually
    {
        currentSound_Walking = newSound_Walking;
    }


    public void PlayerLanded()
    {
        TileTerrain.TerrainList current_Type = TileTerrain.TerrainList.Stone;

        Collider2D platform = Physics2D.OverlapCircle(footstepPosition.position, 0.1f, groundLayerMask);
        if (platform == null)
            return;

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


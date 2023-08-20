using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cooking_Minigame : MonoBehaviour
{
    [SerializeField]
    private float speed;
    [SerializeField]
    private Vector2 markerRange;  // y here is just for ease of using, it's still on the X axis
    [SerializeField]
    private Image marker, letterHolder;
    [SerializeField]
    private Sprite[] lettersSprites;
    [SerializeField]
    private Animator panAnimator;
    private float yBar;
    private RectTransform rectTransform;
    private int successCounter, currentLetter, sideSwitcher = 1; // if it's positive, than the bar will go right
    
    private Cooking_Recipe currentRecipe;
    private float successCounterMax;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        yBar = rectTransform.anchoredPosition.y; 
    }

    void Start()
    {
        ChangeMarkerPosition();
    }

    void Update()
    {
        MoveBar();

        CheckForInputs();
    }

    void MoveBar()
    {
        Vector2 position = rectTransform.anchoredPosition;
        position.x = position.x + (sideSwitcher * speed * Time.deltaTime);
        position.y = yBar;
        rectTransform.anchoredPosition = position;
    }

    void CheckForInputs()
    {
        if (Input.GetKeyDown(KeyCode.W))
            ChecksAfterInput(0); // 0 is W
        if (Input.GetKeyDown(KeyCode.A))
            ChecksAfterInput(1); // 1 is a
        if (Input.GetKeyDown(KeyCode.S))
            ChecksAfterInput(2); // 2 is s
        if (Input.GetKeyDown(KeyCode.D))
            ChecksAfterInput(3); // 3 is d
    }

    void ChecksAfterInput(int letterPressed)
    {
        if (currentLetter == letterPressed)
            StartCoroutine(HandleMarker());
        else
            FailedToHit();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Minigame_Triggers")) // tag needs to be the same name as the triggers.
            SwitchDirection();

        if (collision.CompareTag("CookingGame_Marker")) // tag needs to be the same name as the Marker.
            HitMarker();
    }

    private void SwitchDirection()
    {
        sideSwitcher *= -1;
    }

    private IEnumerator HandleMarker()
    {
        int currentCounter = successCounter;
        marker.transform.GetChild(0).gameObject.SetActive(true);
        marker.GetComponent<BoxCollider2D>().enabled = true;

        yield return new WaitForSeconds(0.1f);

        if (successCounter == currentCounter)
            FailedToHit();
        else if (successCounter == successCounterMax)
            CompletedMinigame();

        marker.GetComponent<BoxCollider2D>().enabled = false;
        marker.transform.GetChild(0).gameObject.SetActive(false);
        ChangeMarkerPosition();
    }

    private void ChangeMarkerPosition()
    {
        GetRandomKey();
        float rndXPos = Random.Range(markerRange.x, markerRange.y);
        Vector2 newPos = new Vector2(rndXPos, marker.rectTransform.anchoredPosition.y);
        marker.rectTransform.anchoredPosition = newPos;
    }

    private void HitMarker()
    {
        successCounter++;
        if(successCounter != successCounterMax)
            panAnimator.SetTrigger("NextStage");

        Debug.Log("MarkerHit SFX");
        AudioManager.instance.PlaySound(AudioManager.SoundList.ClankTriggered);
    }


    private void FailedToHit()
    {
        ResetMinigame();
        GameMaster.instance.ShakeCameraTopDown(0.1f, 5f);
        AudioManager.instance.PlaySound(AudioManager.SoundList.PlayerHit);
        Debug.Log("player missed SFX");
    }

    public void ResetMinigame()
    {
        successCounter = 0;
        panAnimator.ResetTrigger("NextStage");
        panAnimator.SetTrigger("Failed");
    }

    private void CompletedMinigame()
    {
        RemoveIngredientsFromInventory();
        InventoryController.instance.Add_ItemToInventory(currentRecipe.recipeFor_Item, 1);
        ResetMinigame();
        Debug.Log("Completed SFX and or animation");
    }

    private void GetRandomKey()
    {
        int rndKey = Random.Range(0, 4); // 0W - 1A - 2S - 3D
        currentLetter = rndKey;
        letterHolder.sprite = lettersSprites[rndKey];
    }

    public void SetSpeed(float newSpeed, Cooking_Recipe recipeToCook)
    {
        speed = newSpeed;
        currentRecipe = recipeToCook;
        successCounterMax = recipeToCook.numberOfSteps;
    }

    private void RemoveIngredientsFromInventory()
    {
        for (int i = 0; i < currentRecipe.listOfIngredients.Length; i++)
        {
            Inventory_Item.Item itemToRemove = currentRecipe.listOfIngredients[i].ingredient;
            int amountToRemove = currentRecipe.listOfIngredients[i].amount;
            InventoryController.instance.Remove_Items(itemToRemove, amountToRemove);
        }
    }

}

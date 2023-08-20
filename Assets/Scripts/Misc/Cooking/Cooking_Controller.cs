using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class Cooking_Controller : MonoBehaviour
{

    [SerializeField]
    private GameObject miniGameArea, recipeSelector, recipePage, cookButton, minigameScript;

    [SerializeField]
    private Image[] buttonsArray;
    [SerializeField]
    private Image[] ingredientDisplayArray;

    private Cooking_Recipe[] currentRecipeArray;

    private bool isPlayerNear, isRecipeSelectorOpen, isRecipeOpen;
    private GameObject player, selectedButton;
    private float minigameSpeed;
    private Cooking_Recipe currentRecipe;
    private bool[] ingredient_CheckArray;

    private Vector2 originalPos;    // for recipe display
    private EventSystem eventSystem;

    void Start()
    {
        player = GameMaster.instance.playerInstance;
        originalPos = recipePage.transform.position;
        eventSystem = EventSystem.current;
    }

    void Update()
    {
        if (!isPlayerNear)
            return;

        if (Input.GetKeyDown(KeyCode.E) && !miniGameArea.activeSelf)
        {
            if (!isRecipeSelectorOpen)
                OpenRecipeSelector();
            else
            {
                if (isRecipeOpen)  // transition to minigame
                    TransitionToMinigame();
                else
                    OpenRecipePage();
            }
        }


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (miniGameArea.activeSelf)    // close minigame and open recipe
            {
                OpenRecipeSelector();
                minigameScript.GetComponent<Cooking_Minigame>().ResetMinigame();
                miniGameArea.SetActive(false);
            }
            else
            {
                if (isRecipeOpen)
                    CloseRecipePage();
                else
                    ExitCookingStation();
            }
        }
    }


    void ExitCookingStation()
    {
        Debug.Log("Exit cooking station SFX");
        player.GetComponent<TopDownMovement>().EndIgnoreInput();
        StartCoroutine(HandleFolding(false));
        isRecipeSelectorOpen = false;
    }

    void OpenRecipeSelector()
    {
        Debug.Log("Recipe selector Opened SFX");

        player.GetComponent<TopDownMovement>().StartIgnoreInput();
        StartCoroutine(HandleFolding(true));
        isRecipeSelectorOpen = true;
        recipeSelector.SetActive(true);
    }

    void OpenRecipePage()
    {
        Debug.Log("Recipe page open SFX");
        selectedButton = eventSystem.currentSelectedGameObject;
        SetRecipeDisplay(GetButton(selectedButton));
        isRecipeOpen = true;
        recipePage.SetActive(true);
        recipePage.transform.position = originalPos + Random.insideUnitCircle * 100; //randomize page position
        eventSystem.SetSelectedGameObject(cookButton);
    }

    void CloseRecipePage()
    {
        Debug.Log("recipe page closed SFX");
        isRecipeOpen = false;
        recipePage.SetActive(false);
        if (selectedButton != null)
            eventSystem.SetSelectedGameObject(selectedButton);
    }

    public void TransitionToMinigame()
    {
        if (!Does_HaveIngredient())  // check if have enough 
        {
            Debug.Log("Doesn't have enough room in inventory   -   can add SFX");
            //sfx for fail
            return;
        }

        Debug.Log("SFX or bg music for cooking?");
        miniGameArea.SetActive(true);
        recipePage.SetActive(false);
        recipeSelector.SetActive(false);
        isRecipeOpen = false;
        isRecipeSelectorOpen = false;
        minigameScript.GetComponent<Cooking_Minigame>().SetSpeed(minigameSpeed, currentRecipe);
    }


    private IEnumerator HandleFolding(bool shouldOpen)
    {
        RectTransform _rectTransform = recipeSelector.GetComponent<RectTransform>();
        if (shouldOpen)
        {
            for (float i = 0; i < 1; i += Time.deltaTime * 6f)
            {
                _rectTransform.localScale = new Vector3(1, i, 1); // we can change the 'i' position to the x value to open horizontally.
                yield return null;
            }
            _rectTransform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            for (float i = 1; i > 0; i -= Time.deltaTime * 6f)
            {
                _rectTransform.localScale = new Vector3(1, i, 1); // we can change the 'i' position to the x value to open horizontally.
                yield return null;
            }
            recipeSelector.SetActive(false);
            _rectTransform.localScale = new Vector3(1, 0, 1);
            isRecipeSelectorOpen = false;
        }
    }

    public void SetButtons(Cooking_Recipe[] recipeArray)
    {

        for (int i = 0; i < buttonsArray.Length; i++)
        {
            Debug.Log(buttonsArray[i].gameObject);
            buttonsArray[i].GetComponentInParent<TextMeshProUGUI>(true).text = recipeArray[i].recipeFor_Item.itemName;
            buttonsArray[i].sprite = recipeArray[i].recipeFor_Item.artwork;
        }

        currentRecipeArray = recipeArray;
    }

    private void SetRecipeDisplay(Cooking_Recipe recipe)
    {
        int ingredientList_Length = recipe.listOfIngredients.Length;
        ingredient_CheckArray = new bool[ingredientList_Length];

        for (int i = 0; i < ingredientList_Length; i++)  // cycle through ingrendient list
        {
            ingredientDisplayArray[i].gameObject.SetActive(true);

            Inventory_Item currentItem = InventoryController.instance.GetItem_Base(recipe.listOfIngredients[i].ingredient);
            ingredientDisplayArray[i].transform.GetChild(0).GetComponent<Image>().sprite = currentItem.artwork;
            ingredientDisplayArray[i].GetComponentInChildren<TextMeshProUGUI>().text = currentItem.itemName + " " + "x" + recipe.listOfIngredients[i].amount;

            bool doesItemExist = InventoryController.instance.Is_ItemInInventoryOrFridge(currentItem.item, recipe.listOfIngredients[i].amount);
            Sprite itemCheckSprite = doesItemExist ? PrefabManager.instance.GetSprite(PrefabManager.ListOfSprites.v_Sign) : PrefabManager.instance.GetSprite(PrefabManager.ListOfSprites.x_Sign);
            ingredientDisplayArray[i].GetComponent<Image>().sprite = itemCheckSprite;
            ingredient_CheckArray[i] = doesItemExist;
        }

        recipePage.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = recipe.recipeFor_Item.itemName;
        minigameSpeed = recipe.minigameSpeed;
        currentRecipe = recipe;
    }

    private bool Does_HaveIngredient()
    {
        int counter = 0;
        int arrayLength = ingredient_CheckArray.Length;

        for (int i = 0; i < arrayLength; i++)
        {
            if (ingredient_CheckArray[i] == true)
                counter++;
        }

        if (InventoryController.instance.Is_PlayerInventory_Full())
            return false;

        return counter == arrayLength;
    }

    private Cooking_Recipe GetButton(GameObject button)
    {
        for (int i = 0; i < buttonsArray.Length; i++)
        {
            if (ReferenceEquals(button, buttonsArray[i].GetComponentInParent<Button>().gameObject))
                return currentRecipeArray[i];
        }

        return currentRecipeArray[0];
    }

    public void SetPlayerNearBool(bool isNear)
    {
        isPlayerNear = isNear;
    }

    public void InitializeFromButton(int buttonPosition)
    {
        OpenRecipePage();
    }
}

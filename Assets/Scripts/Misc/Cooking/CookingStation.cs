using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class CookingStation : MonoBehaviour
{

    [SerializeField]
    private GameObject miniGame_Holder, miniGameArea, recipeSelector, recipePage, cookButton, minigameScript;

    [SerializeField]
    private Image[] buttonsArray;
    [SerializeField]
    private Image[] ingredientDisplayArray;
    [SerializeField]
    private Cooking_Recipe[] recipeArray;

    private bool isPlayerNear, isRecipeSelectorOpen, isRecipeOpen;
    private GameObject player, selectedButton;
    private float minigameSpeed;

    private Vector2 originalPos;    // for recipe display
    EventSystem eventSystem;

    void Start()
    {
        player = GameMaster.instance.playerInstance;
        originalPos = recipePage.transform.position;
        eventSystem = EventSystem.current;
    }

    private void OnEnable()
    {
        SetButtons();
    }

    void Update()
    {
        if (!isPlayerNear)
            return;

        if(Input.GetKeyDown(KeyCode.E) && !miniGameArea.activeSelf)
        {
            if(!isRecipeSelectorOpen)
                OpenRecipeSelector();
            else
            {
                if(isRecipeOpen)  // transition to minigame
                    TransitionToMinigame();
                else
                    OpenRecipePage();
            }
        }

        if (!miniGame_Holder.activeSelf)
            return;

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(miniGameArea.activeSelf)    // close minigame and open recipe
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
        player.GetComponent<TopDownMovement>().EndIgnoreInput();
        StartCoroutine(HandleFolding(false));
        isRecipeSelectorOpen = false;
    }

    void OpenRecipeSelector()
    {
        player.GetComponent<TopDownMovement>().StartIgnoreInput();
        miniGame_Holder.SetActive(true);
        StartCoroutine(HandleFolding(true));
        isRecipeSelectorOpen = true;
        recipeSelector.SetActive(true);
    }

    void OpenRecipePage()
    {
        selectedButton = eventSystem.currentSelectedGameObject;
        SetRecipeDisplay(GetButton(selectedButton));
        isRecipeOpen = true;
        recipePage.SetActive(true);
        recipePage.transform.position = originalPos + Random.insideUnitCircle * 100; //randomize page position
        eventSystem.SetSelectedGameObject(cookButton);
    }

    void CloseRecipePage()
    {
        isRecipeOpen = false;
        recipePage.SetActive(false);
        if (selectedButton != null)
            eventSystem.SetSelectedGameObject(selectedButton);
    }

    public void TransitionToMinigame()
    {
        miniGameArea.SetActive(true);
        recipePage.SetActive(false);
        recipeSelector.SetActive(false);
        isRecipeOpen = false;
        isRecipeSelectorOpen = false;
        minigameScript.GetComponent<Cooking_Minigame>().SetSpeed(minigameSpeed);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) 
        {
            // here you can add an animation for interact key or anything that happens when the player is need
            isPlayerNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // player leaving the cooking station area
            isPlayerNear = false;
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
            miniGame_Holder.SetActive(false);
            isRecipeSelectorOpen = false;
        }
    }

    private void SetButtons()
    {
        for (int i = 0; i < buttonsArray.Length; i++)
        {
            buttonsArray[i].GetComponentInParent<TextMeshProUGUI>(true).text = recipeArray[i].name;
            buttonsArray[i].sprite = recipeArray[i].artwork;
        }
    }

    private void SetRecipeDisplay(Cooking_Recipe recipe)
    {
        for (int i = 0; i < recipe.listOfIngredients.Length; i++)
        {
            ingredientDisplayArray[i].gameObject.SetActive(true);
            string ingredientName = recipe.listOfIngredients[i].ingredient.ToString();
            PrefabManager.ListOfSprites spriteEnum = (PrefabManager.ListOfSprites)System.Enum.Parse(typeof(PrefabManager.ListOfSprites), ingredientName);
            ingredientDisplayArray[i].transform.GetChild(0).GetComponent<Image>().sprite = PrefabManager.instance.GetSprite(spriteEnum);
            ingredientDisplayArray[i].GetComponentInChildren<TextMeshProUGUI>().text = ingredientName + " " + "x" + recipe.listOfIngredients[i].amount;
        }

        recipePage.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = recipe.name;
        minigameSpeed = recipe.minigameSpeed;
    }

    private Cooking_Recipe GetButton(GameObject button)
    {
        for (int i = 0; i < buttonsArray.Length; i++)
        {
            if (ReferenceEquals(button, buttonsArray[i].GetComponentInParent<Button>().gameObject))
                return recipeArray[i];
        }

        return recipeArray[0];
    }

    public void InitializeFromButton(int buttonPosition)
    {
        OpenRecipePage();
        SetRecipeDisplay(recipeArray[buttonPosition]);
    }
}

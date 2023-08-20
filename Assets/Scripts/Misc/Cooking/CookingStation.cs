using UnityEngine;

public class CookingStation : MonoBehaviour
{

    [SerializeField]
    private Cooking_Recipe[] recipeArray;
    [SerializeField]
    private Cooking_Controller _cooking_Controller;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) 
        {
            // here you can add an animation for interact key or anything that happens when the player is need
            _cooking_Controller.gameObject.SetActive(true);
            _cooking_Controller.SetPlayerNearBool(true);
            _cooking_Controller.SetButtons(recipeArray);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // player leaving the cooking station area
        {
            _cooking_Controller.SetPlayerNearBool(false);
            _cooking_Controller.gameObject.SetActive(false);
        }
    }

}

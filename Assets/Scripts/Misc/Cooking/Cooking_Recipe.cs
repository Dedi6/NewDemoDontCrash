using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Recipe")]
public class Cooking_Recipe : ScriptableObject
{
   // public new string name;

  //  public Sprite artwork;

  //  [SearchableEnum]
  //  public Recipe recipeFor;

    public Inventory_Item recipeFor_Item;
    public int numberOfSteps;
    public Ingredient[] listOfIngredients;
   // public Ingredient2[] listOfIngredients2;

    public int minigameSpeed;

    [System.Serializable]
    public class Ingredient2
    {
        [SearchableEnum] public IngredientList ingredient;
        public int amount = 1;
    }

    [System.Serializable]
    public class Ingredient
    {
        [SearchableEnum] public Inventory_Item.Item ingredient;
        public int amount = 1;
    }

    public enum Recipe
    {
        Gimbap_Tuna,
        Gimbap_Beef,
        Gimbap_Tofu,
        Gimbap_Pork,
    }

    public enum IngredientList
    {
        Rice,
        Seaweed,
        Veggies, 
        Tuna,
        Beef,
        Tofu,
        Pork,
    }

}

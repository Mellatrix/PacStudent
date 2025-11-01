using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MinigameManager : MonoBehaviour
{
    public Dictionary<Recipe.Ingredient, int> collectedIngredients = new Dictionary<Recipe.Ingredient, int>();
    public List<Recipe> recipes = new List<Recipe>();

    void Start()
    {
        
    }
}

[System.Serializable]
public class Recipe
{
    public string recipeName;
    public Dictionary<Ingredient, int> ingredients;
    public UnityEvent onRecipeComplete;

    public Recipe(string name, Dictionary<Ingredient, int> _ingredients, UnityEvent action)
    {
        recipeName = name;
        ingredients = _ingredients;
        onRecipeComplete = action;
    }

    public bool IsCompleted(Dictionary<Ingredient, int> collected)
    {
        foreach (var ingredient in ingredients)
        {
            if (!collected.ContainsKey(ingredient.Key) || collected[ingredient.Key] < ingredient.Value)
                return false;
        }
        return true;
    }

    public enum Ingredient
    {
        shroom,
        rat,
        honey
    }
}


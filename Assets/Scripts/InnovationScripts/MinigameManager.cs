using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MinigameManager : MonoBehaviour
{
    Dictionary<Recipe.Ingredient, int> collectedIngredients = new Dictionary<Recipe.Ingredient, int>();
    List<Recipe> recipes = new List<Recipe>();
    
    Queue<IEnumerator> minigameQue = new Queue<IEnumerator>();
    
    public Transform[] tracerPrefabs;
    
    bool isMinigameActive = false;

    [SerializeField]
    private TextMeshProUGUI[] shroomText, honeyText, ratText;
    [SerializeField]
    Image[] shroomFill, shroomTime, honeyFill, honeyTime, ratFill, ratTime;

    public TextMeshProUGUI text;
    
    void Start()
    {
        recipes.Add(new Recipe("Mushroomy!", 
            new Dictionary<Recipe.Ingredient, int>{ { Recipe.Ingredient.shroom , 15}},
            ()=> Mushroomy()));
        
        recipes.Add(new Recipe("So Honey",
            new Dictionary<Recipe.Ingredient, int>{ { Recipe.Ingredient.honey , 2}},
            ()=> SoHoney()));;
        
        recipes.Add(new Recipe("Ratthew",
            new Dictionary<Recipe.Ingredient, int>{{ Recipe.Ingredient.rat, 1}},
            ()=> Ratthew()));;
        
        UpdateUI();
    }
    
    public void CollectIngredient(Recipe.Ingredient ingredient)
    {
        if (!collectedIngredients.ContainsKey(ingredient))
            collectedIngredients[ingredient] = 0;
        
        collectedIngredients[ingredient]++;
        UpdateUI();
        Recipe recipe;
        if (IsRecipeComplete(out recipe))
        {
            Debug.Log(recipe.recipeName);
            switch (recipe.recipeName)
            {
                case "Mushroomy!":
                    StartCoroutine(ShowTimer(shroomTime,0.2f));
                    if (!hasShownShroomTip)
                    {
                        text.text = "Mushroomy! tasty (+score)";
                        StartCoroutine(ShowText());
                        hasShownShroomTip = true;
                    }
                    break;
                case "So Honey":
                    StartCoroutine(ShowTimer(honeyTime,15f));
                    if (!hasShownHoneyTip)
                    {
                        text.text = "So Honey make everything tasty (+multiplier)";
                        StartCoroutine(ShowText());
                        hasShownHoneyTip = true;
                    }
                    break;
                case "Ratthew":
                    StartCoroutine(ShowTimer(ratTime,10f));
                    if (!hasShownRatTip)
                    {
                        text.text = "Ratthew go zoom (+speed)";
                        StartCoroutine(ShowText());
                        hasShownRatTip = true;
                    }
                    break;
            }
        }
    }

    IEnumerator ShowTimer(Image[] timeFills, float time)
    {
        float t = time;
        while (t > 0)
        {
            t -= Time.deltaTime;
            foreach (var fill in timeFills)
            {
                fill.fillAmount = t / time;
            }
            yield return null;
        }

        foreach (var fill in timeFills)
        {
            fill.fillAmount = 0;
        }
    }

    private bool hasShownShroomTip = false;
    bool hasShownHoneyTip = false;
    bool  hasShownRatTip = false;
    IEnumerator ShowText()
    {
        text.transform.parent.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        text.transform.parent.gameObject.SetActive(false);
    }

    void UpdateUI()
    {
        collectedIngredients.TryGetValue(Recipe.Ingredient.shroom, out int shroomCount);
        for (int i = 0; i < 2; i++)
        {
            shroomText[i].text = "x"+ shroomCount.ToString("00");
            shroomFill[i].fillAmount = shroomCount / 15f;
        }
        collectedIngredients.TryGetValue(Recipe.Ingredient.honey, out int honeyCount);
        for (int i = 0; i < 2; i++)
        {
            honeyText[i].text = "x"+ honeyCount.ToString("00");
            honeyFill[i].fillAmount = honeyCount / 2f;
        }
        collectedIngredients.TryGetValue(Recipe.Ingredient.rat, out int ratCount);
        for (int i = 0; i < 2; i++)
        {
            ratText[i].text = "x"+ ratCount.ToString("00");
            ratFill[i].fillAmount = ratCount / 2f;
        }
    }

    bool IsRecipeComplete(out Recipe completedRecipe)
    {
        completedRecipe = null;
        foreach (Recipe recipe in recipes)
        {
            if (recipe.IsCompleted(collectedIngredients))
            {
                recipe.onRecipeComplete.Invoke();
                
                foreach (var req in recipe.ingredients)
                    collectedIngredients[req.Key] -= req.Value;
                
                completedRecipe = recipe;
                return true;
            }
        }
        
        return false;
    }

    public bool GameCompleted = false;
    public float minigameScore;
    IEnumerator MiniGame(Transform tracer, Action action)
    {
        GameCompleted = false;
        Transform _tracer = SpawnTracerObj(tracer);
        _tracer.GetComponent<TracerHandler>().SetMinigameManager(this);
        Transform child;
        for (int i = 0; i < _tracer.childCount; i++)
        {
            child = _tracer.GetChild(i);
            // animate tracer
            StartCoroutine(AnimateTracerOrb(child));
            child.gameObject.SetActive(true);
            StartCoroutine(DeactivateTracerOrb(child.gameObject));
            yield return new WaitForSeconds(0.1f);
        }
        
        yield return new WaitUntil(() => GameCompleted);
        if (minigameScore > 0.5f)
            action();
    }

    IEnumerator DeactivateTracerOrb(GameObject orb)
    {
        yield return new WaitForSeconds(1f);
        if (orb != null)
        {
            orb.gameObject.name = "Done";
            orb.SetActive(false);
        }
    }

    IEnumerator AnimateTracerOrb(Transform tracerOrb)
    {
        float startScale = tracerOrb.transform.localScale.x;
        float endScale = startScale * 1.2f;
        startScale *= 0.7f;
        tracerOrb.transform.localScale = Vector3.one * startScale;
        
        while (tracerOrb != null)
        {
            tracerOrb.localScale = Vector3.Lerp(tracerOrb.localScale, Vector3.one * endScale, 0.2f);
            yield return null;
        }
    }

    Transform SpawnTracerObj(Transform tracer)
    {
        GameObject _tracer = Instantiate(tracer.gameObject, Vector3.zero, Quaternion.identity, Camera.main.transform);
        for (int i = 0; i < _tracer.transform.childCount; i++)
        {
            _tracer.transform.GetChild(i).gameObject.SetActive(false);
        }
        
        return _tracer.transform;
    }
    
    void QueueMiniGame(Transform tracer, Action action)
    {
        //Debug.Log("Queueing MiniGame");
        IEnumerator newMiniGame = MiniGame(tracer, action);
        minigameQue.Enqueue(newMiniGame);
        if (!isMinigameActive)
            StartCoroutine(DequeueMiniGame());
    }

    IEnumerator DequeueMiniGame()
    {
        isMinigameActive = true;
        while (minigameQue.Count > 0)
        {
            yield return StartCoroutine(minigameQue.Dequeue());
        }

        isMinigameActive = false;
    }

    void Mushroomy()
    {
        // queue game
        QueueMiniGame(tracerPrefabs[0], ()=>AddScore(50));
    }

    void SoHoney()
    {
        //StartCoroutine(ScoreMultiplier());
        QueueMiniGame(tracerPrefabs[1], ()=>StartCoroutine(ScoreMultiplier()));
    }

    void Ratthew()
    {
        QueueMiniGame(tracerPrefabs[2], ()=>StartCoroutine(FeedingFrenzy()));
    }

    void AddScore(int score)
    {
        GameManager.instance.AddScore(score);
        //Debug.Log("YAY");
    }

    IEnumerator FeedingFrenzy()
    {
        PacStudentController player = GameManager.instance.Player;
        player.speedMultiplier *= 1.5f;
        player.speedMultiplier = Mathf.Clamp(player.speedMultiplier, 1, 2.5f);
        yield return new WaitForSeconds(10f);
        player.speedMultiplier /= 1.5f;
        player.speedMultiplier = Mathf.Clamp(player.speedMultiplier, 1, 2.5f);
    }

    IEnumerator ScoreMultiplier()
    {
        GameManager.instance.Multiplier++;
        yield return new WaitForSeconds(15f);
        GameManager.instance.Multiplier--;
    }
}

[System.Serializable]
public class Recipe
{
    public string recipeName;
    public Dictionary<Ingredient, int> ingredients;
    public Action onRecipeComplete;

    public Recipe(string name, Dictionary<Ingredient, int> _ingredients, Action action)
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


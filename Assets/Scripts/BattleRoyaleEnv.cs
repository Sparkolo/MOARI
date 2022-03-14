using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class BattleRoyaleEnv : MonoBehaviour
{

    [Header("Max Environment Steps")] public int MaxEnvironmentSteps = 4000;

    public BattleRoyaleManager manager;

    private int _resetTimer = 0;
    private Bounds _bounds;
    [SerializeField] int maxFoodsPlant = 30;
    [SerializeField] int maxFoodsMeat = 10;
    [SerializeField] GameObject foodPlantPrefab;
    [SerializeField] GameObject foodMeatPrefab;
    float meatToPlantRatio;

    public List<Food> foodsPlant = new List<Food>();
    public List<Food> foodsMeat = new List<Food>();
    public List<CellAgentInference> agentsList = new List<CellAgentInference>();

    [SerializeField] private int veganCount, totalVeganCount;
    [SerializeField] private int carnivoreCount, totalCarnivoreCount;

    private int sumCurrentAgeVegan, sumCurrentAgeCarnivore;

    private bool isFirstReset = true;

    private void Start()
    {
        _bounds = GetComponent<Collider2D>().bounds;
        float foodColliderRadiusPlant = foodPlantPrefab.GetComponent<CircleCollider2D>().radius * foodPlantPrefab.transform.localScale.x;
        float foodColliderRadiusMeat = foodMeatPrefab.GetComponent<CircleCollider2D>().radius * foodMeatPrefab.transform.localScale.x;
        float foodColliderRadius = Mathf.Max(foodColliderRadiusPlant, foodColliderRadiusMeat);
        _bounds.min += new Vector3(foodColliderRadius, foodColliderRadius, 0);
        _bounds.max -= new Vector3(foodColliderRadius, foodColliderRadius, 0);
        meatToPlantRatio = (float)maxFoodsMeat / (float)(maxFoodsPlant + maxFoodsMeat);

        veganCount = 0;
        carnivoreCount = 0;
        agentsList = new List<CellAgentInference>(GetComponentsInChildren<CellAgentInference>());
        foreach (CellAgentInference agent in agentsList)
        {
            if (agent.type == CellAgent.AgentType.Vegan)
            {
                veganCount++;
            }
            else if (agent.type == CellAgent.AgentType.Carnivore)
            {
                carnivoreCount++;
            }
        }
        totalVeganCount = veganCount;
        totalCarnivoreCount = carnivoreCount;

        sumCurrentAgeVegan = 0;
        sumCurrentAgeCarnivore = 0;

        isFirstReset = true;

        Init();
    }

    public void Init()
    {
        //Debug.Log("Initializing");

        while (foodsPlant.Count > maxFoodsPlant)
        {
            Destroy(foodsPlant[foodsPlant.Count - 1].gameObject);
        }
        while (foodsMeat.Count > maxFoodsMeat)
        {
            Destroy(foodsMeat[foodsMeat.Count - 1].gameObject);
        }

        while (foodsPlant.Count < maxFoodsPlant)
            AddFood(Food.FoodType.Plant);
        while (foodsMeat.Count < maxFoodsMeat)
            AddFood(Food.FoodType.Meat);

        Reset();
    }

    public void Reset()
    {
        //Debug.Log("Resetting");
        sumCurrentAgeVegan += _resetTimer * veganCount;
        sumCurrentAgeCarnivore += _resetTimer * carnivoreCount;

        float vegPerc = (float)veganCount / (float)totalVeganCount;
        float vegAvgAge = (float)sumCurrentAgeVegan / (float)totalVeganCount;

        float carnPerc = (float)carnivoreCount / (float)totalCarnivoreCount;
        float carnAvgAge = (float)sumCurrentAgeCarnivore / (float)totalCarnivoreCount;
        
        if (!isFirstReset)
            manager.AddSimResult(vegPerc, vegAvgAge, carnPerc, carnAvgAge);
        else
            isFirstReset = false;

        _resetTimer = 0;
        sumCurrentAgeVegan = 0;
        sumCurrentAgeCarnivore = 0;

        foreach (Food f in foodsPlant)
            ConsumeFood(f);
        foreach (Food f in foodsMeat)
            ConsumeFood(f);

        foreach (CellAgentInference agent in agentsList)
        {
            agent.EndEpisode();
            agent.gameObject.SetActive(true);
        }

        veganCount = totalVeganCount;
        carnivoreCount = totalCarnivoreCount;
    }

    private void FixedUpdate()
    {
        _resetTimer++;
        if ((_resetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0) || veganCount + carnivoreCount == 0)
        {
            Reset();
        }
    }


    public void CellDeath(CellAgent.AgentType type)
    {
        if (type == CellAgent.AgentType.Vegan)
        {
            sumCurrentAgeVegan += _resetTimer;
            veganCount--;
        }
        else if (type == CellAgent.AgentType.Carnivore)
        {
            sumCurrentAgeCarnivore += _resetTimer;
            carnivoreCount--;
        }
    }

    public void AddFood(Food.FoodType type = Food.FoodType.Plant)
    {
        GameObject foodToSpawn = type == Food.FoodType.Plant ? foodPlantPrefab : foodMeatPrefab;
        GameObject newFood = Instantiate(foodToSpawn, Vector2.zero, Quaternion.identity);
        newFood.transform.parent = transform;
        Food foodComponent = newFood.GetComponent<Food>();
        foodComponent.env3 = this;
        (type == Food.FoodType.Plant ? foodsPlant : foodsMeat).Add(foodComponent);
    }

    public void AddFood(Vector2 pos, Food.FoodType type = Food.FoodType.Meat)
    {
        GameObject foodToSpawn = type == Food.FoodType.Plant ? foodPlantPrefab : foodMeatPrefab;
        GameObject newFood = Instantiate(foodToSpawn, pos, Quaternion.identity);
        newFood.transform.parent = transform;
        Food foodComponent = newFood.GetComponent<Food>();
        foodComponent.env3 = this;
        (type == Food.FoodType.Plant ? foodsPlant : foodsMeat).Add(foodComponent);
    }

    public void RepositionFood(Food food)
    {
        Vector2 newPos = new Vector2(Random.Range(_bounds.min.x, _bounds.max.x), Random.Range(_bounds.min.y, _bounds.max.y));
        food.transform.position = newPos;
    }

    public void ConsumeFood(Food food)
    {
        if ((food.type == Food.FoodType.Plant ? foodsPlant : foodsMeat).Count > (food.type == Food.FoodType.Plant ? maxFoodsPlant : maxFoodsMeat))
        {
            Destroy(food.gameObject);
        }
        else
        {
            RepositionFood(food);
        }
    }
}

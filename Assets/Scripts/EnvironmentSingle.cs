using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class EnvironmentSingle : MonoBehaviour
{
    [Header("Max Environment Steps")] public int MaxEnvironmentSteps = 25000;

    private float _resetTimer = 0.0f;
    private float _timePassed = 0.0f;
    private Bounds _bounds;
    [SerializeField] int maxFoodsPlant = 30;
    [SerializeField] int maxFoodsMeat = 10;
    [SerializeField] GameObject foodPlantPrefab;
    [SerializeField] GameObject foodMeatPrefab;

    public List<Food> foodsPlant = new List<Food>();
    public List<Food> foodsMeat = new List<Food>();
    public List<CellAgentSingle> agentsList = new List<CellAgentSingle>();
    public List<CellRand> cellsList = new List<CellRand>();

    private CellAgent.AgentType agentType;
    private int agentsCount, totalAgentsCount;
    private int randCount, totalRandCount;

    private void Start()
    {
        _bounds = GetComponent<Collider2D>().bounds;
        float foodColliderRadiusPlant = foodPlantPrefab.GetComponent<CircleCollider2D>().radius * foodPlantPrefab.transform.localScale.x;
        float foodColliderRadiusMeat = foodMeatPrefab.GetComponent<CircleCollider2D>().radius * foodMeatPrefab.transform.localScale.x;
        float foodColliderRadius = Mathf.Max(foodColliderRadiusPlant, foodColliderRadiusMeat);
        _bounds.min += new Vector3(foodColliderRadius, foodColliderRadius, 0);
        _bounds.max -= new Vector3(foodColliderRadius, foodColliderRadius, 0);

        agentsCount = 0;
        randCount = 0;
        agentType = agentsList[0].type;

        totalAgentsCount = agentsList.Count;
        totalRandCount = cellsList.Count;

        Init();
    }

    public void Init()
    {
        Debug.Log("Initializing");

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
        Debug.Log("Resetting");
        _resetTimer = 0.0f;

        foreach (Food f in foodsPlant)
            ConsumeFood(f);
        foreach (Food f in foodsMeat)
            ConsumeFood(f);

        foreach (CellAgentSingle agent in agentsList)
        {
            agent.EndEpisode();
            agent.gameObject.SetActive(true);
        }

        foreach (CellRand cell in cellsList)
            cell.gameObject.SetActive(true);

        agentsCount = totalAgentsCount;
        randCount = totalRandCount;
    }

    private void FixedUpdate()
    {
        _resetTimer++;
        if (_resetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0 || agentsCount == 0)
        {
            Reset();
        }
    }


    public void CellDeath(CellAgent.AgentType type)
    {
        if (type == agentType)
            agentsCount--;
        else
            randCount--;
    }

    public void AddFood(Food.FoodType type = Food.FoodType.Plant)
    {
        GameObject foodToSpawn = type == Food.FoodType.Plant ? foodPlantPrefab : foodMeatPrefab;
        GameObject newFood = Instantiate(foodToSpawn, Vector2.zero, Quaternion.identity);
        newFood.transform.parent = transform;
        Food foodComponent = newFood.GetComponent<Food>();
        foodComponent.env2 = this;
        (type == Food.FoodType.Plant ? foodsPlant : foodsMeat).Add(foodComponent);
    }

    public void AddFood(Vector2 pos, Food.FoodType type = Food.FoodType.Meat)
    {
        GameObject foodToSpawn = type == Food.FoodType.Plant ? foodPlantPrefab : foodMeatPrefab;
        GameObject newFood = Instantiate(foodToSpawn, pos, Quaternion.identity);
        newFood.transform.parent = transform;
        Food foodComponent = newFood.GetComponent<Food>();
        foodComponent.env2 = this;
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
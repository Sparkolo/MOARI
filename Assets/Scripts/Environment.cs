using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class Environment : MonoBehaviour
{
    [Header("Max Environment Steps")] public int MaxEnvironmentSteps = 25000;

    private float _resetTimer = 0.0f;
    private float _timePassed = 0.0f;
    private Bounds _bounds;
    [SerializeField] float spawnRate = 0.5f;
    [SerializeField] int maxFoodsPlant = 30;
    [SerializeField] int maxFoodsMeat = 10;
    [SerializeField] GameObject foodPlantPrefab;
    [SerializeField] GameObject foodMeatPrefab;
    float meatToPlantRatio;

    public List<Food> foodsPlant = new List<Food>();
    public List<Food> foodsMeat = new List<Food>();
    public List<CellAgent> agentsList = new List<CellAgent>();

    private SimpleMultiAgentGroup veganAgentsGroup;
    private SimpleMultiAgentGroup carnivoreAgentsGroup;

    private int veganCount, totalVeganCount;
    private int carnivoreCount, totalCarnivoreCount;

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
        veganAgentsGroup = new SimpleMultiAgentGroup();
        carnivoreAgentsGroup = new SimpleMultiAgentGroup();
        foreach (CellAgent agent in agentsList)
        {
            if (agent.type == CellAgent.AgentType.Vegan)
            {
                veganAgentsGroup.RegisterAgent(agent);
                veganCount++;
            }
            else if (agent.type == CellAgent.AgentType.Carnivore)
            {
                carnivoreAgentsGroup.RegisterAgent(agent);
                carnivoreCount++;
            }
        }
        totalVeganCount = veganCount;
        totalCarnivoreCount = carnivoreCount;

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


        foreach (CellAgent agent in agentsList)
        {
            agent.gameObject.SetActive(true);
            if (agent.type == CellAgent.AgentType.Vegan)
                veganAgentsGroup.RegisterAgent(agent);
            else if (agent.type == CellAgent.AgentType.Carnivore)
                carnivoreAgentsGroup.RegisterAgent(agent);
        }

        veganCount = totalVeganCount;
        carnivoreCount = totalCarnivoreCount;
    }

    private void EndRewards()
    {
        float veganReward = veganCount > 0 ? 1f - (float)(totalVeganCount - veganCount) / (float)totalVeganCount : -1.0f;
        float carnivoreReward = carnivoreCount > 0 ? 1f - (float)(totalCarnivoreCount - carnivoreCount) / (float)totalCarnivoreCount : -1.0f;

        veganAgentsGroup.AddGroupReward(veganReward);
        carnivoreAgentsGroup.AddGroupReward(carnivoreReward);
    }

    private void FixedUpdate()
    {
        _resetTimer++;
        if (_resetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            EndRewards();
            veganAgentsGroup.GroupEpisodeInterrupted();
            carnivoreAgentsGroup.GroupEpisodeInterrupted();
            Reset();
        }
        else if (veganCount + carnivoreCount == 0)
        {
            EndRewards();
            veganAgentsGroup.EndGroupEpisode();
            carnivoreAgentsGroup.EndGroupEpisode();
            Reset();
        }
    }


    public void CellDeath(CellAgent.AgentType type)
    {
        if (type == CellAgent.AgentType.Vegan)
        {
            veganAgentsGroup.AddGroupReward(-0.01f * ((MaxEnvironmentSteps / 2 - _resetTimer) / MaxEnvironmentSteps));
            veganCount--;
        }
        else if (type == CellAgent.AgentType.Carnivore)
        {
            carnivoreAgentsGroup.AddGroupReward(-0.01f * ((MaxEnvironmentSteps / 2 - _resetTimer) / MaxEnvironmentSteps));
            carnivoreCount--;
        }
    }

    public void AddFood(Food.FoodType type = Food.FoodType.Plant)
    {
        GameObject foodToSpawn = type == Food.FoodType.Plant ? foodPlantPrefab : foodMeatPrefab;
        GameObject newFood = Instantiate(foodToSpawn, Vector2.zero, Quaternion.identity);
        newFood.transform.parent = transform;
        Food foodComponent = newFood.GetComponent<Food>();
        foodComponent.env = this;
        (type == Food.FoodType.Plant ? foodsPlant : foodsMeat).Add(foodComponent);
    }

    public void AddFood(Vector2 pos, Food.FoodType type = Food.FoodType.Meat)
    {
        GameObject foodToSpawn = type == Food.FoodType.Plant ? foodPlantPrefab : foodMeatPrefab;
        GameObject newFood = Instantiate(foodToSpawn, pos, Quaternion.identity);
        newFood.transform.parent = transform;
        Food foodComponent = newFood.GetComponent<Food>();
        foodComponent.env = this;
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
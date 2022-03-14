using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    public enum FoodType
    {
        Plant = 0,
        Meat = 1
    }

    public Environment env;
    public EnvironmentSingle env2;
    public BattleRoyaleEnv env3;
    private const float PlantValue = 0.3f; // energy value of plants
    private const float MeatValue = 0.3f; // energy value of meat

    public FoodType type = FoodType.Plant;

    public float Consume()
    {
        if (env != null)
            env.ConsumeFood(this);
        else if (env2 != null)
            env2.ConsumeFood(this);
        else if (env3 != null)
            env3.ConsumeFood(this);
        return type == FoodType.Plant ? PlantValue : MeatValue;
    }

    public void OnDestroy()
    {
        if (env != null)
            (type == Food.FoodType.Plant ? env.foodsPlant : env.foodsMeat).Remove(this);
        else if (env2 != null)
            (type == Food.FoodType.Plant ? env2.foodsPlant : env2.foodsMeat).Remove(this);
        else if (env3 != null)
            (type == Food.FoodType.Plant ? env3.foodsPlant : env3.foodsMeat).Remove(this);
    }
}

using UnityEditor;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CellRand : MonoBehaviour
{
    [SerializeField] EnvironmentSingle env;
    // Base agent values
    float baseEnergy = 100f;
    float energyLossPerSecond = 2.5f;
    float baseMoveSpeed = 5f;
    float baseRotSpeed = 180f;
    float baseSize = 1f;
    float baseVisionRadius = 5f;
    Color veganColor = Color.green;
    Color carnivoreColor = Color.red;
    // Cell Modifiers
    public CellAgent.AgentType type;
    [SerializeField] [Range(.5f, 1.5f)] float moveSpeedModifier = 1f;
    [SerializeField] [Range(.5f, 1.5f)] float rotSpeedModifier = 1f;
    [SerializeField] [Range(.5f, 1.5f)] float sizeModifier = 1f;
    [SerializeField] [Range(0.5f, 1.5f)] float visionRadiusModifier = 1f;
    // Current Cell Values
    [SerializeField, ReadOnly] float curEnergy;
    [SerializeField, ReadOnly] float maxEnergy;
    [SerializeField, ReadOnly] float moveSpeed;
    [SerializeField, ReadOnly] float rotSpeed;
    [SerializeField, ReadOnly] float size;
    [SerializeField, ReadOnly] float visionRadius;

    RayPerceptionSensorComponent2D[] rayPercComponents;

    SpriteRenderer sr;
    Rigidbody2D rb;

    private void Start()
    {
        maxEnergy = baseEnergy * sizeModifier;
        moveSpeed = baseMoveSpeed * moveSpeedModifier;
        rotSpeed = baseRotSpeed * rotSpeedModifier;
        size = baseSize * sizeModifier;
        visionRadius = baseVisionRadius * visionRadiusModifier;

        sr = GetComponent<SpriteRenderer>();
        switch (type)
        {
            case CellAgent.AgentType.Vegan:
                sr.color = veganColor;
                break;
            case CellAgent.AgentType.Carnivore:
                sr.color = carnivoreColor;
                break;
            default:
                break;
        }

        rb = GetComponent<Rigidbody2D>();

        curEnergy = maxEnergy;
        transform.localPosition = new Vector3(UnityEngine.Random.Range(-0.45f, 0.45f), UnityEngine.Random.Range(-0.45f, 0.45f), 0f);
        transform.localRotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0.0f, 360.0f));
    }
    void OnEnable()
    {
        curEnergy = maxEnergy;
        transform.localPosition = new Vector3(UnityEngine.Random.Range(-0.45f, 0.45f), UnityEngine.Random.Range(-0.45f, 0.45f), 0f);
        transform.localRotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0.0f, 360.0f));
    }

    private void FixedUpdate()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, visionRadius);
        Vector2 nearestFoodDirection = new Vector2(50000f, 50000f);
        Vector2 nearestWallDirection = new Vector2(50000f, 50000f);
        foreach (Collider2D other in hitColliders)
        {
            if (other.gameObject != this.gameObject)
            {
                string cTag = other.gameObject.tag;
                bool canEatVegan = type == CellAgent.AgentType.Vegan && cTag == "FoodVegan";
                bool canEatCarnivore = type == CellAgent.AgentType.Carnivore && (cTag == "FoodCarnivore" || cTag == "AgentVegan");

                if (canEatVegan || canEatCarnivore)
                {
                    Vector2 foodDirection = other.gameObject.transform.position - transform.position;
                    if (foodDirection.magnitude < nearestFoodDirection.magnitude)
                        nearestFoodDirection = foodDirection;
                }

                if(cTag == "Wall")
                {
                    Vector2 wallDirection = other.ClosestPoint(transform.position) - (Vector2)transform.position;
                    if (wallDirection.magnitude < nearestWallDirection.magnitude)
                        nearestWallDirection = wallDirection;
                }
            }
        }

        float moveForward = Random.Range(0f,1f);
        float rotate = Random.Range(-1f,1f);

        if (nearestFoodDirection.x < 40000f)
        {
            moveForward = Random.Range(0.5f, 1f);
            float angleOfFood = Vector2.SignedAngle(transform.right, nearestFoodDirection);
            if (Mathf.Abs(angleOfFood) < 90f)
                rotate = angleOfFood > 0f ? -1f : 1f;
        }
        else if (nearestWallDirection.x < 40000f)
        {
            moveForward = Random.Range(0.5f, 1f);
            float angleOfWall = Vector2.SignedAngle(transform.right, nearestWallDirection);
            if (Mathf.Abs(angleOfWall) < 90f)
                rotate = angleOfWall > 0f ? 1f : -1f;
        }

        rb.MovePosition(transform.position + transform.right * moveForward * moveSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(rb.rotation - rotate * rotSpeed * Time.fixedDeltaTime);

        curEnergy -= (1f + rb.velocity.magnitude / 5f) * sizeModifier * energyLossPerSecond * Time.fixedDeltaTime;
        if (curEnergy <= 0f)
            Die();
    }

    void Die()
    {
        if (type == CellAgent.AgentType.Vegan)
            env.AddFood(transform.position);
        env.CellDeath(type);
        gameObject.SetActive(false);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        string cTag = collision.collider.tag;

        bool isFood = cTag == "FoodVegan" || cTag == "FoodCarnivore";
        bool isAgentVeg = cTag == "AgentVegan";
        bool isAgentCarn = cTag == "AgentCarnivore";

        if (type == CellAgent.AgentType.Vegan)
        {
            if (isFood)
            {
                if (collision.collider.TryGetComponent<Food>(out Food food))
                {
                    float foodTypeMultiplier = food.type == Food.FoodType.Plant ? 1f : -1f;
                    float foodValue = foodTypeMultiplier * food.Consume();
                    curEnergy += foodValue * 35;
                    curEnergy = Mathf.Clamp(curEnergy, 0f, maxEnergy);
                }
            }
            if (isAgentCarn)
            {
                Die();
            }
        }
        else if (type == CellAgent.AgentType.Carnivore)
        {
            if (collision.collider.TryGetComponent<Food>(out Food food))
            {
                float foodTypeMultiplier = food.type == Food.FoodType.Meat ? 1f : -1f;
                float foodValue = foodTypeMultiplier * food.Consume();
                curEnergy += foodValue * 35;
                curEnergy = Mathf.Clamp(curEnergy, 0f, maxEnergy);
            }
        }
    }
}
using UnityEditor;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class ReadOnlyAttribute : PropertyAttribute
{ }

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property,
                                            GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position,
                               SerializedProperty property,
                               GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}

public class CellAgent : Agent
{
    public enum AgentType
    {
        Vegan = 0,
        Carnivore = 1
    }

    [SerializeField] Environment env;
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
    public AgentType type;
    [SerializeField] [Range(.5f, 1.5f)] float moveSpeedModifier = 1f;
    [SerializeField] [Range(.5f, 1.5f)] float rotSpeedModifier = 1f;
    [SerializeField] [Range(.5f, 1.5f)] float sizeModifier = 1f;
    [SerializeField] [Range(0.5f, 1.5f)] float visionRadiusModifier = 1f;
    // Current Cell Values
    [SerializeField, ReadOnly] int age;
    [SerializeField, ReadOnly] float curEnergy;
    [SerializeField, ReadOnly] float maxEnergy;
    [SerializeField, ReadOnly] float moveSpeed;
    [SerializeField, ReadOnly] float rotSpeed;
    [SerializeField, ReadOnly] float size;
    [SerializeField, ReadOnly] float visionRadius;

    RayPerceptionSensorComponent2D[] rayPercComponents;

    SpriteRenderer sr;
    Rigidbody2D rb;

    Vector2 lastPosition = Vector2.zero;

    public override void Initialize()
    {
        age = 0;
        maxEnergy =  baseEnergy * sizeModifier;
        moveSpeed =  baseMoveSpeed * moveSpeedModifier;
        rotSpeed =  baseRotSpeed * rotSpeedModifier;
        size =  baseSize * sizeModifier;
        visionRadius =  baseVisionRadius * visionRadiusModifier;

        rayPercComponents = GetComponentsInChildren<RayPerceptionSensorComponent2D>();
        foreach (RayPerceptionSensorComponent2D rpc in rayPercComponents)
        {
            rpc.RayLength = rpc.name.Contains("Fwd") ? 2f * visionRadius : visionRadius;
        }

        sr = GetComponent<SpriteRenderer>();
        switch(type)
        {
            case AgentType.Vegan:
                sr.color =  veganColor;
                break;
            case AgentType.Carnivore:
                sr.color =  carnivoreColor;
                break;
            default:
                break;
        }

        rb = GetComponent<Rigidbody2D>();

        if (!Academy.Instance.IsCommunicatorOn)
        {
            this.MaxStep = 0;
        }
    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
        age = 0;
        curEnergy = maxEnergy;
        transform.localPosition = new Vector3(UnityEngine.Random.Range(-0.45f, 0.45f), UnityEngine.Random.Range(-0.45f, 0.45f), 0f);
        lastPosition = rb.position;
        transform.localRotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0.0f, 360.0f));
    }

    private void FixedUpdate()
    {
        float curVelocity = ((rb.position - lastPosition) / Time.fixedDeltaTime).magnitude;
        lastPosition = rb.position;
        age++;
        if (curVelocity > 2.5f)
            AddReward(0.0001f);
        else if (curVelocity < 0.1f)
            AddReward(-0.0001f);
        curEnergy -= (1f + rb.velocity.magnitude / 5f) * sizeModifier *  energyLossPerSecond * Time.fixedDeltaTime;
        if (curEnergy <= 0f)
            Die();
    }

    public override void OnActionReceived(ActionBuffers vectorAction)
    {
        float moveForward = Mathf.Clamp01(vectorAction.ContinuousActions[0]);
        float rotate = vectorAction.ContinuousActions[1];

        rb.MovePosition(transform.position + transform.right * moveForward * moveSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(rb.rotation - rotate * rotSpeed * Time.fixedDeltaTime);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Vertical");
        continuousActions[1] = Input.GetAxisRaw("Horizontal");
    }

    void Die()
    {
        AddReward(-1.0f);
        if(type == AgentType.Vegan)
            env.AddFood(transform.position);
        env.CellDeath(type);
        gameObject.SetActive(false);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        string cTag = collision.collider.tag;
        if (cTag == "Wall")
            AddReward(-0.1f);

        bool isFood = cTag == "FoodVegan" || cTag == "FoodCarnivore";
        bool isAgentVeg = cTag == "AgentVegan";
        bool isAgentCarn = cTag == "AgentCarnivore";

        if(type == AgentType.Vegan)
        {
            if(isFood)
            {
                if (collision.collider.TryGetComponent<Food>(out Food food))
                {
                    float foodTypeMultiplier = food.type == Food.FoodType.Plant ? 1f : -1f;
                    float foodValue = foodTypeMultiplier * food.Consume();
                    AddReward(foodValue);
                    curEnergy += foodValue * 35;
                    curEnergy = Mathf.Clamp(curEnergy, 0f, maxEnergy);
                }
            }
            if(isAgentCarn)
            {
                AddReward(-1.0f);
                Die();
            }
        }
        else if(type == AgentType.Carnivore)
        {
            if (collision.collider.TryGetComponent<Food>(out Food food))
            {
                float foodTypeMultiplier = food.type == Food.FoodType.Meat ? 1f : -1f;
                float foodValue = foodTypeMultiplier * food.Consume();
                AddReward(foodValue);
                curEnergy += foodValue * 35;
                curEnergy = Mathf.Clamp(curEnergy, 0f, maxEnergy);
            }
            if(isAgentVeg)
            {
                AddReward(0.1f);
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.tag == "Wall")
            AddReward(-0.01f);
    }
}
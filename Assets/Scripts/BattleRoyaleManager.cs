using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class BattleRoyaleManager : MonoBehaviour
{
    public enum BrainType
    {
        POCA = 0,
        PPO = 1,
        SAC = 2
    }

    public class VeganContent
    {
        public List<float> survivorPercentageVegan;
        public List<float> survivorAvgAgeVegan;
    }

    public class CarnivoreContent
    {
        public List<float> survivorPercentageCarnivore;
        public List<float> survivorAvgAgeCarnivore;
    }

    public class TrainingContent
    {
        public string brainTypeVegan;
        public VeganContent veganContent;
        public string brainTypeCarnivore;
        public CarnivoreContent carnivoreContent;
    }
    private class SaveObject
    {
        public string trainingType;
        public TrainingContent trainingContent;
    }

    public TournamentManager tournamentManager;

    public int numberOfSimulations = 100;
    [ReadOnly, SerializeField] private int currentSimulation = 0;

    [SerializeField] BrainType veganType;
    [SerializeField] BrainType carnivoreType;

    private List<float> survivorsPercentageVegan = new List<float>();
    private List<float> survivorsAvgAgeVegan = new List<float>();

    private List<float> survivorsPercentageCarnivore = new List<float>();
    private List<float> survivorsAvgAgeCarnivore = new List<float>();

    // Start is called before the first frame update
    void Start()
    {
        currentSimulation = 0;
    }

    private void EndSimulation()
    {
        SaveObject saveObj = new SaveObject
        {
            trainingType = "veg_" + veganType.ToString() + "__carn_" + carnivoreType.ToString(),
            trainingContent = new TrainingContent
            {
                brainTypeVegan = veganType.ToString(),
                veganContent = new VeganContent
                {
                    survivorPercentageVegan = this.survivorsPercentageVegan,
                    survivorAvgAgeVegan = this.survivorsAvgAgeVegan
                },
                brainTypeCarnivore = carnivoreType.ToString(),
                carnivoreContent = new CarnivoreContent
                {
                    survivorPercentageCarnivore = this.survivorsPercentageCarnivore,
                    survivorAvgAgeCarnivore = this.survivorsAvgAgeCarnivore
                }
            }
        };

        JsonSerializerSettings settings = new JsonSerializerSettings();
        settings.Formatting = Formatting.Indented;

        string jsonContent = JsonConvert.SerializeObject(saveObj, settings);
        string fileName = "veg" + veganType.ToString() + "carn" + carnivoreType.ToString() + ".json";
        File.WriteAllText(Application.dataPath + "/Comparisons/" + fileName, jsonContent);

        tournamentManager.NextSimulation();
    }

    public void AddSimResult(float vegPerc, float vegAvgAge, float carnPerc, float carnAvgAge)
    {
        Debug.Log(veganType + "VS" + carnivoreType + currentSimulation + ">>> veg%: " + vegPerc + ", vegAvgAge: " + vegAvgAge + ", carn%: " + carnPerc + ", carnAvgAge: " + carnAvgAge);
        survivorsPercentageVegan.Add(vegPerc);
        survivorsAvgAgeVegan.Add(vegAvgAge);

        survivorsPercentageCarnivore.Add(carnPerc);
        survivorsAvgAgeCarnivore.Add(carnAvgAge);

        currentSimulation++;
        if (currentSimulation >= numberOfSimulations)
            EndSimulation();
    }
}
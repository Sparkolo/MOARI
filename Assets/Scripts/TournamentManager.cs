using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TournamentManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> battleRoyaleManagers = new List<GameObject>();
    [ReadOnly, SerializeField] private int currentIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        currentIndex = 0;
        foreach (GameObject go in battleRoyaleManagers)
            go.SetActive(false);
        battleRoyaleManagers[currentIndex].SetActive(true);
    }

    public void NextSimulation()
    {
        currentIndex++;
        if(currentIndex < battleRoyaleManagers.Count)
        {
            battleRoyaleManagers[currentIndex - 1].SetActive(false);
            battleRoyaleManagers[currentIndex].SetActive(true);
        }
        else
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
    }
}

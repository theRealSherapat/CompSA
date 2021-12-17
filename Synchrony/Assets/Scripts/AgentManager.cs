using System.Collections.Generic;
using UnityEngine;
using static DavidsUtils;

public class AgentManager : MonoBehaviour {
    // ------- START OF Variable Declarations -------

    // General Meta-variables
    public float runDuration = 10f;
    public float adjustedTimeScale = 1.0f;

    // Spawning variables:
    public int collectiveSize = 3;
    public GameObject[] squigglePrefabs;
    public float spawnRadius = 10.0f; // units in radius from origo to the outermost Dr. Squiggle spawn-point

    // CSV-Serialization variables:
    public string frequencyCSVPath = System.IO.Directory.GetCurrentDirectory() + "\\" + "SavedData" + "\\" + "Frequencies" + "\\" + "freqs_over_time.csv";
    public string phaseCSVPath = System.IO.Directory.GetCurrentDirectory() + "\\" + "SavedData" + "\\" + "Phases" + "\\" + "phases_over_time.csv";


    // Spawning variables:
    private float agentWidth = Mathf.Sqrt(Mathf.Pow(4.0f, 2) + Mathf.Pow(4.0f, 2)); // diameter from tentacle to tentacle (furthest from each other)
    private List<Vector2> spawnedPositions = new List<Vector2>();
    private List<SquiggleScript> spawnedAgentScripts = new List<SquiggleScript>();

    // ------- END OF Variable Declarations -------





    // ------- START OF MonoBehaviour Functions/Methods -------

    void Start() {
        // Speeding up or down the simulation if that is wanted
        Time.timeScale = adjustedTimeScale;

        // Spawning all agents randomly (but pretty naively as of now)
        SpawnAgents();

        // Creating all .CSV-files I want to update throughout the simulation 
        CreateAllCSVFiles();

        Invoke("QuitMyGame", runDuration);
    }

    void FixedUpdate() {
        // Updating all my CSV-files with a constant interval (here at the rate at which FixedUpdate() is called, hence at 50Hz)
        UpdateCSVFiles();
    }

    // ------- END OF MonoBehaviour Functions/Methods -------






    // ------- START OF Spawning-Functions/-Methods -------

    private void SpawnAgents() {
        for (int i = 0; i < collectiveSize; i++) {
            // Finding a position in a circle free to spawn (taking into account not wanting to collide with each other)
            Vector2 randomCirclePoint = FindFreeSpawnPosition();
            spawnedPositions.Add(randomCirclePoint);

            // Spawning an agent from squigglePrefabs on the free position
            GameObject newAgent = Instantiate(squigglePrefabs[Random.Range(0, squigglePrefabs.Length)],
                                                                new Vector3(randomCirclePoint.x, -0.96f, randomCirclePoint.y),
                                                                Quaternion.identity);

            newAgent.GetComponent<SquiggleScript>().SetAgentID(i + 1); // Setting AgentIDs so that agents have IDs {1, 2, 3, ..., N}, where N is the number of agents in the scene.
            spawnedAgentScripts.Add(newAgent.GetComponent<SquiggleScript>());

            // IF-TIME Debug-TODO: Figure out local vs. global Dr.Squiggle-rotations:
                // Rotating agents to face each other
                // Finding the angle in y-rotation from the agents's z-axis and origo
                //Vector2 targetDir = Vector2.zero - randomCirclePoint;
                //float angle = Vector2.Angle(Vector2.up, targetDir);
                //newAgent.transform.eulerAngles = Vector3.up * (angle + Random.Range(-3f, 3f));
        }
    }

    private Vector2 FindFreeSpawnPosition() {
        // IF-TIME TODO: Find the free spawn position in a much smarter manner (like with artificial potential fields, or Gauss-Neuton or the likes).

        Vector2 currentGuess = Random.insideUnitCircle * spawnRadius;
        bool foundFreeSpawnPoint = false;

        while (!foundFreeSpawnPoint) {
            foundFreeSpawnPoint = true;

            currentGuess = (Random.insideUnitCircle) * spawnRadius;

            foreach (Vector2 spawnedPosition in spawnedPositions) {
                if (Vector2.Distance(spawnedPosition, currentGuess) < agentWidth) {
                    foundFreeSpawnPoint = false;
                }
            }
        }

        return currentGuess;
    }

    // ------- END OF Spawning-Functions/-Methods -------







    // ------- START OF CSV-Serialization Functions/Methods -------

    private void CreateAllCSVFiles() {
        // Creating a .CSV-header consisting of the agents's IDs
        List<int> agentIDHeader = new List<int>();
        foreach (SquiggleScript squiggScr in spawnedAgentScripts) {
            agentIDHeader.Add(squiggScr.GetAgentID());
        }

        // 1) Creating one .CSV-file for the agents's frequencies over time
        CreateCSVWithHeader(frequencyCSVPath, agentIDHeader);

        // 2) Creating one .CSV-file for the agents's phases over time
        CreateCSVWithHeader(phaseCSVPath, agentIDHeader);
    }

    private void UpdateCSVFiles() {
        // 1) Updating the Frequency-Over-Time-.CSV-file
        List<float> frequencyIntervalEntries = new List<float>();
        foreach (SquiggleScript squiggScr in spawnedAgentScripts) {
            frequencyIntervalEntries.Add(squiggScr.GetFrequency());
        }
        FloatUpdateCSV(frequencyCSVPath, frequencyIntervalEntries);

        // 2) Updating the Phase-Over-Time-.CSV-file
        List<float> phaseIntervalEntries = new List<float>();
        foreach (SquiggleScript squiggScr in spawnedAgentScripts) {
            phaseIntervalEntries.Add(squiggScr.GetPhase());
        }
        FloatUpdateCSV(phaseCSVPath, phaseIntervalEntries);
    }

    // ------- END OF CSV-Serialization Functions/Methods -------







    // ------- START OF Helping-/Utility Functions/Methods -------

    private void QuitMyGame() {
        UnityEditor.EditorApplication.isPlaying = false;
    }

    // ------- END OF Helping-/Utility Functions/Methods -------
}

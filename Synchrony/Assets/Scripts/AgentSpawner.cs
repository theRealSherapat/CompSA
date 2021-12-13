//using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DavidsUtils;

public class AgentSpawner : MonoBehaviour {
    public GameObject[] squigglePrefabs;
    public int collectiveSize = 3;
    public float spawnRadius = 10.0f; // units in radius from origo to the outermost Dr. Squiggle spawn-point
    public float samplingRate = 50f; // Hz for phase- and freq.-data collection
    public float startupTime = 0.3f;

    public string frequencyCSVPath = System.IO.Directory.GetCurrentDirectory() + "\\" + "SavedData" + "\\" + "Frequencies" + "\\" + "freqs_over_time.csv";
    public string phaseCSVPath = System.IO.Directory.GetCurrentDirectory() + "\\" + "SavedData" + "\\" + "Phases" + "\\" + "phases_over_time.csv";

    private float agentWidth = Mathf.Sqrt(Mathf.Pow(4.0f, 2) + Mathf.Pow(4.0f, 2)); // diameter from tentacle to tentacle (furthest from each other
    private List<Vector2> spawnedPositions = new List<Vector2>();

    private List<SquiggleScript> spawnedAgentScripts = new List<SquiggleScript>();

    void Start() {
        // Spawning all agents randomly (but pretty naively as of now)
        SpawnAllAgents();

        // Creating all .CSV-files I want to update throughout the simulation 
        CreateAllCSVFiles();
    }

    void FixedUpdate() {
        // Updating relevant .CSV-files at 2Hz
        //Debug.Log("Condition-part1 (relating to sampling rate): " + (Mathf.Approximately(Mathf.Repeat(Time.time, 1.0f/samplingRate), 0f) || Mathf.Approximately(Mathf.Repeat(Time.time, 1.0f/samplingRate), 1.0f/samplingRate)) + ", fordi Mathf.Repeat(Time.time, 1.0f/samplingRate): " + Mathf.Repeat(Time.time, 1.0f / samplingRate) + ", returverdien av diff approx= 0: " + Mathf.Approximately(Mathf.Repeat(Time.time, 1.0f / samplingRate), 0f) + ", og returverdien av diff approx= 0.02: " + Mathf.Approximately(Mathf.Repeat(Time.time, 1.0f / samplingRate), 1.0f / samplingRate));
        //if ((Time.time % (1.0f/samplingRate) == 0f) && (Time.time > startupTime)) UpdateAllCSVFilesWithAConstantInterval(); // skipping the first time the condition would be true (due to non-initialized phases and frequencies)

        //Debug.Log("Time.time: " + Time.time + ", 1/samplingsRate: " + 1.0f/samplingRate + ", Mathf.Repeat(Time.time, 1.0f / samplingRate): " + Mathf.Repeat(Time.time, 1.0f / samplingRate));

        UpdateAllCSVFilesWithAConstantInterval();
    }

    private void CreateAllCSVFiles() {
        List<int> agentIDHeader = new List<int>();
        foreach (SquiggleScript squiggScr in spawnedAgentScripts) {
            agentIDHeader.Add(squiggScr.GetAgentID());
        }

        // 1) Creating one .CSV-file for the agents's frequencies over time
        CreateCSVWithHeader(frequencyCSVPath, agentIDHeader);

        // 2) Creating one .CSV-file for the agents's phases over time
        CreateCSVWithHeader(phaseCSVPath, agentIDHeader);
    }

    private void UpdateAllCSVFilesWithAConstantInterval() {
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

    private void SpawnAllAgents() {
        for (int i = 0; i < collectiveSize; i++) {
            // Finding a position in a circle free to spawn (taking into account not wanting to collide with each other)
            Vector2 randomCirclePoint = FindFreeSpawnPosition();
            spawnedPositions.Add(randomCirclePoint);

            // Spawning an agent from squigglePrefabs on the free position
            GameObject newAgent = Instantiate(squigglePrefabs[Random.Range(0, squigglePrefabs.Length)], 
                                                                new Vector3(randomCirclePoint.x, -0.96f, randomCirclePoint.y), 
                                                                Quaternion.identity);

            newAgent.GetComponent<SquiggleScript>().SetAgentID(i+1); // Setting AgentIDs so that agents have IDs {1, 2, 3, ..., N}, where N is the number of agents in the scene.
            spawnedAgentScripts.Add(newAgent.GetComponent<SquiggleScript>());

            // Debug-TODO: Figure out local vs. global Dr.Squiggle-rotations:
                // Rotating agents to face each other
                // Finding the angle in y-rotation from the agents's z-axis and origo
                //Vector2 targetDir = Vector2.zero - randomCirclePoint;
                //float angle = Vector2.Angle(Vector2.up, targetDir);
                //newAgent.transform.eulerAngles = Vector3.up * (angle + Random.Range(-3f, 3f));
        }
    }

    private Vector2 FindFreeSpawnPosition() {
        // TODO: Find the free spawn position much smarter manner (like with artificial potential fields, or Gauss-Neuton or the likes).

        Vector2 currentGuess = Random.insideUnitCircle* spawnRadius;
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
}

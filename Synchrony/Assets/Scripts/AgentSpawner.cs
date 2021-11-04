//using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// OUTDATED STRUCT
    //struct AgentColorMix {
    //    Color agentBase;
    //    Color body;
    //    Color goggles;
    //    Color iris;
    //    Color pupil;
    //    Color fireColor;

    //    public AgentColorMix(Color agB, Color bdy, Color ggls, Color irs, Color ppil, Color frClr) {
    //        agentBase = agB;
    //        body = bdy;
    //        goggles = ggls;
    //        iris = irs;
    //        pupil = ppil;
    //        fireColor = frClr;
    //    }
    //}

public class AgentSpawner : MonoBehaviour {
    public GameObject[] squigglePrefabs;
    public int collectiveSize = 6;
    public float spawnRadius = 8.0f; // units in radius from origo to the outermost Dr. Squiggle spawn-point

    private float agentWidth = Mathf.Sqrt(Mathf.Pow(4.0f, 2) + Mathf.Pow(4.0f, 2)); // diameter from tentacle to tentacle (furthest from each other
    private List<Vector2> spawnedPositions = new List<Vector2>();

    // OUTDATED TRY {
        //AgentColorMix cozyOrange = new AgentColorMix(new Color(0f, 0.6431373f, 0.6301261f), new Color(0.8113208f, 0.3672103f, 0.05663935f), 
        //                                                           new Color(0.2095941f, 0.2504433f, 0.7075472f), new Color(0.678627f, 0.4288359f, 0.7169812f), 
        //                                                           new Color(0.8584906f, 0.4192884f, 0.01133851f), new Color(1f, 0.05673758f, 0f));


        //public static AgentColorMix[] agentColorChoices = {cozyOrange}; // UTVID DENNE MED KOSELIGE FARGE-KOMBINASJONER. ELLER BARE INSTANTIATE FORSKJELLIGE DR SQUIGGLES MED FORSKJELLIGE FARGER (LETTERE) => HA EN ARRAY MED REFERANSER TIL SQUIGGLES-PREFABS (SOM MED PATHSA I HITMAN STEALTH).
    // }

    void Start() {
        SpawnAllAgents();
    }

    private void SpawnAllAgents() {
        for (int i = 0; i < collectiveSize; i++) {
            // Finding a position in a circle free to spawn (taking into account not wanting to collide with each other)
            Vector2 randomCirclePoint = FindFreeSpawnPosition();
            spawnedPositions.Add(randomCirclePoint);

            // Spawning an agent from squigglePrefabs on the free position
            GameObject newAgent = Instantiate(squigglePrefabs[Random.Range(0, squigglePrefabs.Length)], new Vector3(randomCirclePoint.x, -0.96f, randomCirclePoint.y), Quaternion.identity);

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

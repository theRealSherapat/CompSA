using System.Collections.Generic;
using UnityEngine;
using static DavidsUtils;

public class AgentManager : MonoBehaviour {
    // ------- START OF Variable Declarations -------

    // General Meta-variables:
    public float runDurationLimit = 300f; // 5 minutes (given in seconds)
    public float adjustedTimeScale = 1.0f;

    // Spawning variables:
    public int collectiveSize = 3;
    public GameObject[] squigglePrefabs;
    public float spawnRadius = 10.0f; // units in radius from origo to the outermost Dr. Squiggle spawn-point

    // Performance-measure / Synchronization-time variables
    public float t_f = 0.08f; // the duration of the time-window the nodes are allowed to fire during
    public int k = 8; // the number of times in a row the 't_q'-/t_q-window (where no fire-events can be heard) must be equally long

    // CSV-Serialization variables:
    public string frequencyCSVPath = System.IO.Directory.GetCurrentDirectory() + "\\" + "SavedData" + "\\" + "Frequencies" + "\\" + "freqs_over_time.csv";
    public string phaseCSVPath = System.IO.Directory.GetCurrentDirectory() + "\\" + "SavedData" + "\\" + "Phases" + "\\" + "phases_over_time.csv";
    public string t_f_is_nowPath = System.IO.Directory.GetCurrentDirectory() + "\\" + "SavedData" + "\\" + "t_f_is_now.csv";
    public string nodeFiringDataPath = System.IO.Directory.GetCurrentDirectory() + "\\" + "SavedData" + "\\" + "node_firing_data.csv";


    // Spawning variables:
    private float agentWidth = Mathf.Sqrt(Mathf.Pow(4.0f, 2) + Mathf.Pow(4.0f, 2)); // diameter from tentacle to tentacle (furthest from each other)
    private List<Vector2> spawnedPositions = new List<Vector2>();
    private List<SquiggleScript> spawnedAgentScripts = new List<SquiggleScript>();

    // Performance-measure (Synchronization-time) variables
    private bool hSynchConditionsAreMet = false; // A boolean that should be true no sooner than when all the conditions for the achievement of Harmonic Synchrony are fulfilled.
    private bool t_f_is_now = false; // A short "up-time" when all nodes are allowed to fire during. The duration itself is constant, but when in simulation-time it will be "up", depends largely on the t_q-variable.
    private float last_t_q_definer = 0f; // The oldest defining firing-event-time to be used to definine a new t_q-value.
    private bool define_t_q_at_next_firing = false; // A boolean signal/flag that signalizes that the next fire event will be defining (together with the "now set" or "old" last_t_q_definer) in terms of a new t_q-value/-window.
    private float t_q = 0f; // The varying (but hopefully eventually converging) time-window/-duration lasting for how long no fire-events can be heard after the last time_fire-window ended.
    private static int number_in_a_row_of_no_firings_within_t_q = 0;
    private bool firstFiringTriggered;

    // ------- END OF Variable Declarations -------





    // ------- START OF MonoBehaviour Functions/Methods -------

    void Start() {
        // Speeding up or down the simulation if that is wanted
        Time.timeScale = adjustedTimeScale;

        // Spawning all agents randomly (but pretty naively as of now)
        SpawnAgents();

                                        // HUSK: OGSÅ TENKE PÅ Å LOADE INN EN ALLEREDE-EKSISTERENDE .CSV-FIL MED DATAPUNKTER FOR HSYNCHTIMEs OG DENS KOVARIATER.
        // Creating all .CSV-files I want to update throughout the simulation 
        CreateAllCSVFiles();
    }

    void Update() {
        //                                                                                                    <---------- WORK ON!!!!!!!

        CheckHSynchConditions();
        
        if (hSynchConditionsAreMet) {
            
            // Saving a successful data-point
            //SaveDataPointToCSVFile(Time.timeSinceLevelLoad);
            
            // BARE FOR TESTING
            Debug.Log("Synchrony achieved! The synch-time was: " + Time.timeSinceLevelLoad);
            
            QuitMyGame();
        } else if (!hSynchConditionsAreMet && (Time.timeSinceLevelLoad >= runDurationLimit)) { // Max-time limit for the run (so that it won't go on forever)
            // Saving an un-successful data-point
            //SaveDataPointToCSVFile("fail");

            // BARE FOR TESTING
            Debug.Log("Synchrony not acheived. Exiting Simulation.");

            QuitMyGame();
        }
    }

    void FixedUpdate() {
        // Updating all my CSV-files with a constant interval (here at the rate at which FixedUpdate() is called, hence at 50Hz)
        UpdateCSVFiles();
    }

    // ------- END OF MonoBehaviour Functions/Methods -------





    // ------- START OF Performance-measure Termination-evaluation Functions/Methods -------

    private void IJustHeardSomeoneFire() {
        // When this method gets called, it means the AgentManager has picked upon a Dr. Squiggle's Event-Action — meaning it "heard" a Dr. Squiggle's ``fire''-signal.

        // Fanger Accurately opp Action-Eventsa til Dr. Squiggles'a.
        // FØLG TRINNENE FRA 1) TIL 7) I Fig. 6. I Nymoen-paperet.

        // Calling for the initialization (technically resetting) of the t_q-/t_q-window
        if (last_t_q_definer == 0f) {
            last_t_q_definer = Time.time;

            //TriggerFirstFiringTime();                                                                 // KAN MEST SANNSYNLIG KUTTE UT

            define_t_q_at_next_firing = true;
        }
        // Resetting the t_q-/t_q-window if this is not the first observed Fire-event and the reset is flagged and wanted
        else if (define_t_q_at_next_firing) {
            if (!firstFiringTriggered) { // First mover if this is the first time t_q gets defined?
                firstFiringTriggered = true;
                TriggerFiringTime();
            }

            t_q = Time.time - last_t_q_definer - 3/2*t_f;                   // KANSKJE IKKE TREKKE FRA EN HALV t_f TIL (SOM JEG TOLKET Fig. 6 SOM FØRST)?

            define_t_q_at_next_firing = false;
        }
        // Calling for a reset for the t_q-/t_q-window since observed fire-event was received during !t_f a.k.a. t_q, and we did not want to reset t_q this time, nor calling for the initialization of t_q
        else if (!t_f_is_now) {
            number_in_a_row_of_no_firings_within_t_q = 0; // resetting the streak, because synch bad

            last_t_q_definer = Time.time;

            define_t_q_at_next_firing = true;
        }

        // MÅ PASSE PÅ AT TriggerFiringTime() BLIR KALT ETTER HVERT t_q-/t_q-window.

        // NECESSARY CONDITIONS FOR SYNCHRONIZATION:
            // DOES FIRING ALWAYS OCCUR WITHIN THE time_fire-WINDOW, AND NEVER DURING THE time-quiet-WINDOW ANYMORE? GOOOD!

            // HAVE ALL NODES FIRED AT LEAST ONCE DURING THE EVALUATION PERIOD? GOOOD!

            // HAVE THERE BEEN NO FIRINGS WITHIN THE t_q-WINDOWS k TIMES? GOOOD!

            // IF SO — CONGRATS! YOU CAN SET synchronizationIsAchieved = true;
    }

    private void CheckHSynchConditions() {
        // SJEKK AT HVER NODE HAR FYRT MINST EN GANG ILØPET AV EVALUERINGS-PERIODEN.

        if (number_in_a_row_of_no_firings_within_t_q == k) hSynchConditionsAreMet = true;
    }

    private void TriggerFiringTime() {
        t_f_is_now = true;
        number_in_a_row_of_no_firings_within_t_q ++;
        Debug.Log("Counter towards k: " + number_in_a_row_of_no_firings_within_t_q);
        Invoke("TriggerQuietPeriod", t_f);
    }

    private void TriggerQuietPeriod() {
        t_f_is_now = false;
        Invoke("TriggerFiringTime", t_q);
    }

    // KAN MEST SANNSYNLIG KUTTE UT:
        //private void TriggerFirstFiringTime() {
        //    // t_q is yet not defined (i.e. t_q = 0f)
        //    t_f_is_now = true;
        //    Invoke("FalsifyFiringTime", t_f);
        //}

        //private void FalsifyFiringTime() {
        //    // t_q is yet not defined (i.e. t_q = 0f)
        //    t_f_is_now = false;
        //}


    // ------- END OF Performance-measure Termination-evaluation Functions/-Methods -------





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

            SquiggleScript extractedSquiggleScript = newAgent.GetComponent<SquiggleScript>();

            extractedSquiggleScript.SetAgentID(i + 1); // Setting AgentIDs so that agents have IDs {1, 2, 3, ..., N}, where N is the number of agents in the scene.
            extractedSquiggleScript.OnSquiggleFire += IJustHeardSomeoneFire;
            spawnedAgentScripts.Add(extractedSquiggleScript);

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

        // 3) Creating one .CSV-file for the t_f_is_now-variable, telling (in time) when it is legal for nodes to fire
        List<int> t_f_is_nowHeader = new List<int>();
        CreateCSVWithHeader(t_f_is_nowPath, t_f_is_nowHeader);

        // 4) Creating one .CSV-file for the data needed to create the "Node-firing-plot", as in Nymoen's Fig. 6
        CreateCSVWithHeader(nodeFiringDataPath, agentIDHeader);
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

        // 3) Updating the t_f_is_now digital signal saved to the .CSV-file
        float t_f_is_nowFloat = System.Convert.ToSingle(t_f_is_now);
        List<float> t_f_is_nowList = new List<float>();
        t_f_is_nowList.Add(t_f_is_nowFloat);
        FloatUpdateCSV(t_f_is_nowPath, t_f_is_nowList);

        // 4) Updating the node-firing data                                               SHOULD BE UPDATED WHEN FIRING EVENTS ARE HEARD, NOT CONTINUOUSLY
        // MÅ FINNE UT HVILKEN NODE SOM FYRTE AKKURAT NÅ, OG SÅ KONSTRUERE EN .CSV-ENTRY/-LINJE UTIFRA DET.
    }

    // ------- END OF CSV-Serialization Functions/Methods -------







    // ------- START OF Helping-/Utility Functions/Methods -------

    private void QuitMyGame() {
        UnityEditor.EditorApplication.isPlaying = false;
    }

    // ------- END OF Helping-/Utility Functions/Methods -------
}

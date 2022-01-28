using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static DavidsUtils;

public class AgentManager : MonoBehaviour {
    // ------- START OF Variable Declarations -------

    // Simulation Hyperparameters/HSYNCH-Covariates
    public int collectiveSize = 3;

    // General Meta-variables:
    public int simulationRuns = 1;
    public float runDurationLimit = 300f; // 5 minutes (given in seconds)
    public float adjustedTimeScale = 1.0f;

    // Spawning variables:
    public GameObject[] squigglePrefabs;
    public float spawnRadius = 10.0f; // units in radius from origo to the outermost Dr. Squiggle spawn-point

    // Performance-measure / Synchronization-time variables
    public float t_f = 0.08f; // the duration of the time-window the nodes are allowed to fire during
    public int k = 9; // the number of times in a row the 't_q'-/t_q-window (where no fire-events can be heard) must be equally long. KAN SETTE TILBAKE k=8 HVIS JEG FORSIKRER MEG OM AT INKREMENT-REGELEN MIN IKKE ER FOR LIBERAL.


    // CSV-Serialization variables:
    public string phaseCSVPathStart = System.IO.Directory.GetCurrentDirectory() + "\\" + "SavedData" + "\\" + "Phases" + "\\" + "phases_over_time";
    public string frequencyCSVPathStart = System.IO.Directory.GetCurrentDirectory() + "\\" + "SavedData" + "\\" + "Frequencies" + "\\" + "freqs_over_time";
    public string nodeFiringDataPathStart = System.IO.Directory.GetCurrentDirectory() + "\\" + "SavedData" + "\\" + "NodeFiringPlotMaterial" + "\\" + "node_firing_data";

    public string datasetPath = System.IO.Directory.GetCurrentDirectory() + "\\" + "SavedData" + "\\" + "synchronyDataset.csv";
    
    
    
    private static int atSimRun = 0;

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
    private int number_in_a_row_of_no_firings_within_t_q = 0;
    private bool firstFiringTriggered;
    private bool[] agentiHasFiredAtLeastOnce;

    // Node-firing plot variables
    private List<int> agentWithAgentIDsJustFired = new List<int>(); // Initializing (used for creation of node-firing-plot) a list for all the agents with agent-ids that just fired. 

    // ------- END OF Variable Declarations -------





    // ------- START OF MonoBehaviour Functions/Methods -------

    void Start() {
        // Speeding up or down the simulation if that is wanted
        Time.timeScale = adjustedTimeScale;

        // Spawning all agents randomly (but pretty naively as of now)
        SpawnAgents();

        // Initializing and allocating as many boolean values (used for the performance measure) as there are agents, which are to be put "high"/true if agent with agentID fired at least once.
        agentiHasFiredAtLeastOnce = new bool[spawnedAgentScripts.Count];

        // Creating all .CSV-files I want to update throughout the simulation 
        CreateAllCSVFiles();
    }

    void Update() {
        // POTENTIAL PERFORMANCE-GAIN:
            // - Make the simulation quit immediately the even-beat-counter hits k, instead of at the next Update()-call.
            // - Call EndSimulationIfHSynchConditionsAreReached() in FixedUpdate() instead of Update().

        CheckHSynchConditions();

        EndSimulationRunIfHSynchConditionsAreReached();
    }

    void FixedUpdate() {
        // Updating all my CSV-files with a constant interval (here at the rate at which FixedUpdate() is called, hence at 50Hz)
        UpdateAllCSVFiles();
    }

    // ------- END OF MonoBehaviour Functions/Methods -------





    // ------- START OF Performance-measure Termination-evaluation Functions/Methods -------
    
    private void CheckHSynchConditions() {
        // Checks if all nodes have fired at least once, as well as beat k times evenly (i.e. with an equal t_q-window but in a very short time-window t_f) in a row — and raising the signal/flag for achieved harmonic synchrony as a consequence (if these conditions are true).

        bool notAllHaveFiredOnceYet = agentiHasFiredAtLeastOnce.AsQueryable().Any(val => val == false);
        bool agentsHaveBeatenInAnEvenRhythmKTimes = number_in_a_row_of_no_firings_within_t_q == k;

        if (!notAllHaveFiredOnceYet && agentsHaveBeatenInAnEvenRhythmKTimes) hSynchConditionsAreMet = true;
    }

    private void EndSimulationRunIfHSynchConditionsAreReached() {
        // If the simulation has either succeeded, or failed — save datapoint and move on
        // Condition 2: Max-time limit for the run (so that it won't go on forever)
        if (hSynchConditionsAreMet || (!hSynchConditionsAreMet && (Time.timeSinceLevelLoad >= runDurationLimit))) {

            // Signifying that I am done with one simulator-run
            atSimRun++;

            // Saving a successful or unsuccessful data-point
            SaveDatapointToDataset(Time.timeSinceLevelLoad);

            if (atSimRun != simulationRuns) {
                ResetSimulationVariables();
                LoadMySceneAgain();
            } else {
                QuitMyGame();
            }
        }
    }

    public void IJustHeardSomeoneFire(int firingAgentId) {
        // When this method gets called, it means the AgentManager has picked upon a Dr. Squiggle's method-call — meaning it "heard" a Dr. Squiggle's ``fire''-signal.

        agentWithAgentIDsJustFired.Add(firingAgentId);

        agentiHasFiredAtLeastOnce[firingAgentId - 1] = true; // Flagging that corresponding agent with AgentID has fired at least once during the simulation.

        AdjustHsynchCriteriasIfNeeded();
    }
    
    private void AdjustHsynchCriteriasIfNeeded() {
        // Calling for the initialization (technically resetting) of the t_q-/t_q-window
        if (last_t_q_definer == 0f) { // bra logikk?
            if (!firstFiringTriggered) {
                TriggerFirstFiringTime();
                firstFiringTriggered = true;
            }

            last_t_q_definer = Time.timeSinceLevelLoad;


            define_t_q_at_next_firing = true;
        }
        // Resetting the t_q-/t_q-window if this is not the first observed Fire-event and the reset is flagged and wanted
        else if (define_t_q_at_next_firing) {
            float new_t_q = Time.timeSinceLevelLoad - last_t_q_definer - t_f; // Optionally subtract 3/2*t_f instead of just t_f.

            if ((new_t_q+t_f) > 0.5f) { // weird condition because I don't want too close fire-events to define the t_q-window (i.e. not too small fire-time-differences)
                t_q = new_t_q;
                define_t_q_at_next_firing = false;

                RestartInvokeCycle();
            }
            
            
        }
        // Calling for a reset for the t_q-/t_q-window since observed fire-event was received during !t_f a.k.a. t_q, and we did not want to reset t_q this time, nor calling for the initialization of t_q
        else if (!t_f_is_now) {
            number_in_a_row_of_no_firings_within_t_q = 0; // resetting the streak, because synch bad
            Debug.Log("Even-beat-counter towards k=" + k + ":    " + number_in_a_row_of_no_firings_within_t_q);

            last_t_q_definer = Time.timeSinceLevelLoad;

            define_t_q_at_next_firing = true;
        }
    }

    private void SaveDatapointToDataset(float runDuration) {
        // Saving the Performance Measure (HSYNCHTIME) and the current Simulator-covariates/-hyperparameters (assumed to be manually written in the existing .CSV already).

        // Initializing empty float-List soon-to-contain the performance measure and the covariates/explanators
        List<float> performanceAndCovariateValues = new List<float>();

        // Adding the performance measure
        performanceAndCovariateValues.Add(runDuration);
        
        // Adding Covariate 1
        float hSynchConditionsAreMetFloat = System.Convert.ToSingle(hSynchConditionsAreMet);
        performanceAndCovariateValues.Add(hSynchConditionsAreMetFloat);
        // Telling the programmer in the console whether the simulation run was a success or not
        if (hSynchConditionsAreMet) Debug.Log("Congratulations! Harmonic synchrony in your musical multi-robot collective at simRun " + atSimRun + " was achieved in " + runDuration + " seconds!");
        else                        Debug.Log("That's too bad... Harmonic synchrony, according to the performance-measure defined by K. Nymoen et al., was not achieved at simRun " + atSimRun + " within the run time-limit of " + runDurationLimit + " seconds..");


        // Adding Covariate 2
        float useNymoenFloat = System.Convert.ToSingle(spawnedAgentScripts[0].useNymoen);
        performanceAndCovariateValues.Add(useNymoenFloat);

        // Add Covariate N
        
        // Saving one datapoint, a.k.a. writing one .CSV-row (Performance-measure, Covariates) to the .CSV-file at the datasetPath
        FloatUpdateCSV(datasetPath, performanceAndCovariateValues);
    }

    private void TriggerFiringTime() {
        t_f_is_now = true;
        number_in_a_row_of_no_firings_within_t_q ++;
        Debug.Log("Even-beat-counter towards k=" + k + ":    " + number_in_a_row_of_no_firings_within_t_q);
        Invoke("TriggerQuietPeriod", t_f);
    }

    private void TriggerHalfFiringTime() {
        t_f_is_now = true;
        Invoke("TriggerQuietPeriod", t_f/2f);
    }

    private void TriggerQuietPeriod() {
        t_f_is_now = false;
        Invoke("TriggerFiringTime", t_q);
    }

    private void TriggerFirstFiringTime() {
        // Remember: t_q is yet not defined (i.e. t_q = 0f)
        t_f_is_now = true;
        Invoke("FalsifyFiringTime", t_f);
    }

    private void FalsifyFiringTime() {
        // Remember: t_q is yet not defined (i.e. t_q = 0f)
        t_f_is_now = false;
    }

    private void RestartInvokeCycle() {
        CancelInvoke(); // All t_q-/t_f-Invokes are ended/executed.
        TriggerHalfFiringTime(); // A new cycle of t_f-/t_q-Invokes commences.
    }


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
        // TODO: Find the free spawn position in a much smarter manner (like with artificial potential fields, or Gauss-Neuton or the likes).

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
        // Obtaining the .CSV-header consisting of the agents's IDs
        List<string> agentIDHeader = GetAgentHeader();

        // Creating one .CSV-file for the agents's phases over time
        CreatePhaseCSV(agentIDHeader);
        
        // Creating one .CSV-file for the agents's frequencies over time
        CreateFrequencyCSV(agentIDHeader);

        // Creating one .CSV-file for the node_firing_data (including t_f_is_now) needed to create the "Node-firing-plot" as in Nymoen's Fig. 6
        CreateNodeFiringCSV(agentIDHeader);

        // FOR AUTOMATIC (CURRENTLY IT IS MANUAL) SYNCHRONY-/PERFORMANCE-PLOT .CSV-FILE:
        //CreateSynchDatasetCSV();
    }

    private List<string> GetAgentHeader() {
        // Creating a .CSV-header consisting of the agents's IDs

        List<string> agentIDHeader = new List<string>();
        foreach (SquiggleScript squiggScr in spawnedAgentScripts) {
            agentIDHeader.Add("agent" + squiggScr.GetAgentID().ToString());
        }

        return agentIDHeader;
    }

    private void CreatePhaseCSV(List<string> agentHeader) {
        CreateCSVWithStringHeader(phaseCSVPathStart + "_atSimRun" + atSimRun + ".csv", agentHeader);
    }

    private void CreateFrequencyCSV(List<string> agentHeader) {
        CreateCSVWithStringHeader(frequencyCSVPathStart + "_atSimRun" + atSimRun + ".csv", agentHeader);
    }

    private void CreateNodeFiringCSV(List<string> agentHeader) {
        List<string> nodeFiringWithTfHeader = new List<string>(agentHeader);
        nodeFiringWithTfHeader.Insert(0, "t_f_is_now");
        CreateCSVWithStringHeader(nodeFiringDataPathStart + "_atSimRun" + atSimRun + ".csv", nodeFiringWithTfHeader);
    }

    //private void CreateSynchDatasetCSV() {
        // Creating a .CSV-file for the MSc Synchrony-dataset, which are to contain the HSYNCHTIMEs with their covariates.
        //List<string> performanceAndCovariatesHeader = new List<string>(); // The covariates we want to record the performance-measure/outcome/response-variable for
        //performanceAndCovariatesHeader.Add("HSYNCHTIME");
        //performanceAndCovariatesHeader.Add("SUCCESS");    // Binary covariate (no=0 or yes=1)
        //performanceAndCovariatesHeader.Add("PHASEADJ");
        //CreateCSVWithStringHeader(datasetPath, performanceAndCovariatesHeader);
    //}



    private void UpdateAllCSVFiles() {
        // 1) Updating the Frequencies-Over-Time-.CSV-file
        UpdateFrequencyCSV();

        // 2) Updating the Phases-Over-Time-.CSV-file
        UpdatePhaseCSV();

        // 3) Updating the .CSV-file for the t_f_is_now digital signal which is telling when it is legal for nodes to fire, together with 0s indicating no firing
        UpdateNodeFiringCSV();
    }

    private void UpdatePhaseCSV() {
        List<float> phaseIntervalEntries = new List<float>();
        foreach (SquiggleScript squiggScr in spawnedAgentScripts) {
            phaseIntervalEntries.Add(squiggScr.GetPhase());
        }
        FloatUpdateCSV(phaseCSVPathStart + "_atSimRun" + atSimRun + ".csv", phaseIntervalEntries);
    }

    private void UpdateFrequencyCSV() {
        List<float> frequencyIntervalEntries = new List<float>();
        foreach (SquiggleScript squiggScr in spawnedAgentScripts) {
            frequencyIntervalEntries.Add(squiggScr.GetFrequency());
        }
        FloatUpdateCSV(frequencyCSVPathStart + "_atSimRun" + atSimRun + ".csv", frequencyIntervalEntries);
    }

    private void UpdateNodeFiringCSV() {
        float[] nodeFiringDataArray = new float[spawnedAgentScripts.Count + 1];

        // Marking the digital signal t_f_is_now
        float t_f_is_nowFloat = System.Convert.ToSingle(t_f_is_now);
        nodeFiringDataArray[0] = t_f_is_nowFloat;

        // Marking the correct agents having fired if they just fired recently for the CSV-row.
        foreach (int agentId in agentWithAgentIDsJustFired) {
            nodeFiringDataArray[agentId] = 1f;
        }

        agentWithAgentIDsJustFired.Clear();

        // Converting the nodeFiringData-array to a list
        List<float> nodeFiringDataList = new List<float>(nodeFiringDataArray);

        // Updating the CSV with one time-row
        FloatUpdateCSV(nodeFiringDataPathStart + "_atSimRun" + atSimRun + ".csv", nodeFiringDataList);
    }

    // ------- END OF CSV-Serialization Functions/Methods -------







    // ------- START OF Helping-/Utility Functions/Methods -------

    private void ResetSimulationVariables() {
        // Basically reset all the initial/default values (to what they were by default) as in the variable-declaration-section all over again. Maybe it can be done smarter in the code, only writing the lines once?

        // Basically declare all default-values not assigned in Start().

        hSynchConditionsAreMet = false;
        agentiHasFiredAtLeastOnce = new bool[spawnedAgentScripts.Count];
        last_t_q_definer = 0f;
        define_t_q_at_next_firing = false;
        CancelInvoke(); // All t_q-/t_f-Invokes are ended/executed.
        t_f_is_now = false;
        t_q = 0f;
        number_in_a_row_of_no_firings_within_t_q = 0;
        firstFiringTriggered = false;
}

    private void QuitMyGame() {
        UnityEditor.EditorApplication.isPlaying = false;
    }

    // ------- END OF Helping-/Utility Functions/Methods -------
}

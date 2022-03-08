using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using static SynchronyUtils;

public class AgentManager : MonoBehaviour {
    
    // 'VARIABLES':

    // 'Recorded collective-/environment-hyperparameters':
    [Tooltip("The number of agents to be spawned and synchronized.")]
    public int collectiveSize = 6;
    [Tooltip("The duration (%) of the refractory period in terms of a percentage of the agents's oscillator-periods.")]
    public float t_ref_perc_of_period = 0.1f;       // ISH LENGDEN I TID P� digitalQuickTone er 0.4s. Nymoen BRUKTE 50ms I SIN IMPLEMENTASJON. JEG PR�VDE OGS� 0.6f. possiblePool = {0.09f, 0.4f, 0.6f}.
    [Tooltip("Minimum and maximum initialization-frequencies (Hz).")]
    public Vector2 minMaxInitialFreqs = new Vector2(0.5f, 4f);

    [Tooltip("The number of times in a row the t_q-window (where no fire-events can be heard) must be equally long.")]
    public int k = 8;
    [Tooltip("The duration (s) of the short time-window the nodes are allowed to fire within.")]
    public float t_f = 0.08f;
    [Tooltip("Whether to use Avg. when defining new t_q-windows, or not (implying using Median, as these two estimators are the only two implemented).")]
    public bool useTQAverage = false;

    [Tooltip("The simulation-speed compared to real-time?")]
    public float adjustedTimeScale = 1.0f;

    // 'Non-recorded general meta-/environment-hyperparemeters':
    [Tooltip("The number of simulation-runs per Unity Game-run.")]
    public int simulationRuns = 1;
    [Tooltip("The simulation-timelimit in seconds (i.e. the max simulation-time a simulation-run is allowed to run for before being regarded as a failed synchronization-run).")]
    public float runDurationLimit = 300f;
    [Tooltip("Whether to get logs about Synchrony-successes (in terms of the 'towards-k'-counter) or not.")]
    public bool debugSuccessOn = true;
    [Tooltip("Whether to get logs about t_f-/t_q-windows and corresponding timestamps or not.")]
    public bool debugTqTfOn = false;
    [Tooltip("Whether to give the human observer a sound on every agent-pulse/-firing or not.")]
    public bool useSound = true;
    [Tooltip("Whether to give the human observer a visual and Lerped color-signal on every agent-pulse/-firing or not.")]
    public bool useVisuals = true;

    // Spawning:
    [Tooltip("All the DrSquiggle-prefabs we want to spawn and be synchronized.")]
    public GameObject[] squigglePrefabs;


    
    // 'Private variables necessary to make the cogs go around':

    // General Meta:
    private static int atSimRun = 0;

    // Spawning:
    private float spawnRadius; // The radius from origo within which the agents are allowed to spawn without colliding.
    private float agentWidth = Mathf.Sqrt(Mathf.Pow(4.0f, 2) + Mathf.Pow(4.0f, 2)); // diameter from tentacle to tentacle (furthest from each other)
    private List<Vector2> spawnedPositions = new List<Vector2>();
    private List<SquiggleScript> spawnedAgentScripts = new List<SquiggleScript>();

    // CSV-Serialization:
    private string phasesFolderPath = Directory.GetCurrentDirectory() + "\\" + "SavedData" + "\\" + "Phases" + "\\";
    private string frequenciesFolderPath = Directory.GetCurrentDirectory() + "\\" + "SavedData" + "\\" + "Frequencies" + "\\";
    private string nodeFiringPlotMaterialFolderPath = Directory.GetCurrentDirectory() + "\\" + "SavedData" + "\\" + "PerformanceMeasurePlotMaterial" + "\\";
    private string datasetPath = Directory.GetCurrentDirectory() + "\\" + "SavedData" + "\\" + "synchronyDataset.csv";

    // Node-firing plot:
    private List<int> agentWithAgentIDsJustFired = new List<int>(); // Initializing (used for creation of node-firing-plot) a list for all the agents with agent-ids that just fired. 

    // 'Synch.-Perf.-measure related':
    private bool t_f_is_now = false; // A short "up-time"-flag when all nodes are allowed to fire during. The duration of how long this flag is positive itself is constant (t_f), but when in simulation-time it will be "up", depends largely on the t_q-variable as well as when the t_q-windows are triggered.
	private float t_q = 0f; // The time-window in which the agents are not allowed to fire pulses within.
	
	private bool first_firing_is_perceived = false;
	
	private float reset_t_q_flag_raiser = 0f; // The "syndebukk"-agent's firing-time (simulation time), who fired illegally and caused the initiation of a 'reset t_q'-process.
    private bool reset_t_q_flag = false; // A flag. Is it positive, a 'reset t_q'-process is initiated, if negative, all is good and the agents are on their way to achieve harmonic synchrony.
	
	private int defining_times_acquired = 0; // Will be 0 if the early defining time has not yet been found, 1 if the early defining time has been found, and reset to 0 if the late defining time has been found.
	
	private List<float> early_t_q_defining_times = new List<float>(); // A list of early t_q-definers (the firing-times of agents who fired within t_f simulation-seconds after the agent who fired after more than t_f seconds after reset_t_q_flag_raiser).
    private bool first_early_reset_defining_time_added = false;
	private float early_t_q_definer = 0f; // The first t_q-definer (either an Avg.- or Median-estimate) being used to define a new t_q-window.
	
	private List<float> late_reset_defining_times = new List<float>(); // A list of late t_q-definers (the firing-times of agents who fired within t_f simulation-seconds after the agent who fired after more than t_f seconds after early_t_q_definer).
    private bool first_late_t_q_defining_time_added = false;
	
	private bool[] agentiHasFiredAtLeastOnce; // An array with as many boolean values as there are agents, which are to be put "high"/true if agent with agentID fired at least once during the simulation-run.
    private bool hSynchConditionsAreMet = false; // A boolean that should be true no sooner than when all the conditions (as defined the bulletpoints in section 5.5 in my MSc thesis, or the bulletpoints in section V.A. in Nymoen's "firefly"-paper) for the achievement of Harmonic Synchrony are fulfilled.
    private int equal_t_q_streak_counter = 0; // 'towards-k'-counter to become equal to 'k'.

	private int last_t_f_firers_counter = 0; // A counter for how many fire-events were heard throughout the last firing-period t_f, used as a safety-mechanism to detect firing-periods within which no agents fire � so that we don't increment the 'towards-k'-counter after those firing-periods.




    // 'MonoBehaviour':

    void Start() {
        InitializeHelpingVariables();

        // Creating all .CSV-files I want to update throughout the simulation 
        CreateAllCSVFiles();

        // Spawning all agents randomly (but pretty naively as of now)
        SpawnAgents();
    }

    void Update() {
        // Ending simulation-run if we deem it either a synchronization -success or -failure.
        EndSimulationRunIfTerminationCriteriaAreReached();
    }

    void FixedUpdate() {
        // Updating all my CSV-files with a constant frequency (at the rate at which FixedUpdate() is called)?
        UpdateAllCSVFiles();
    }




    // 'PERFORMANCE-MEASURE':

    private void CheckHSynchConditions() {
        // Checks 'Condition 2' and 'Condition 3' respectively: namely, '2': if the agents have beat evenly (with a constant t_q-value) k times in a row, as well as if all nodes have fired at least once throughout the evaluation-/testing-period (simulation-run).
		
		// The whole synchronization-measure implemented is assumed to facilitate and ensure the keeping of 'Condition 1', namely that only legal firing is happening (during a very short and white time-window t_f), not illegal firing (during the longer and gray t_q-window). Thus this 'Condition 1' is not explicitly checked here, but assumed.

		// If all these conditions are true, the function is thus raising the (Congratulations-vibed) hSynchConditionsAreMet-signal/-flag high, for having achieved harmonic synchrony. If all conditions are true and the hSynchConditionsAreMet-flag is raised: Rejoice! Synchrony is obtained!

        bool agentsHaveBeatenAtAnEvenRhythmKTimes = equal_t_q_streak_counter == k;
        bool allAgentsHaveFiredAtLeastOnce = !(agentiHasFiredAtLeastOnce.AsQueryable().Any(val => val == false));

        if (agentsHaveBeatenAtAnEvenRhythmKTimes && allAgentsHaveFiredAtLeastOnce) hSynchConditionsAreMet = true; // IF TRUE: REJOICE!
    }
	
	private void EndSimulationRunIfTerminationCriteriaAreReached() {
        // If the simulation has either succeeded, or failed: save the corresponding datapoint and move on.
        if (SimulationRunEitherSucceededOrFailed()) {
            // Saving a successful or unsuccessful data-point
            SaveDatapointToDataset(Time.timeSinceLevelLoad);

            // Signifying that I am done with one simulator-run
            atSimRun++;

            if (atSimRun != simulationRuns) {
                ResetSimulationVariables();
                LoadMySceneAgain();
            } else {
                QuitMyGame();
            }
        }
    }
	
	private bool SimulationRunEitherSucceededOrFailed() {
		// If the first condition is true, harmonic synchrony has been achieved (the synchrony-conditions are fulfilled), and the simulation-run has succeeded. 
		// If the latter condition is true, the simulation-run has exceeded the maximum time-limit allowed, and the simulation-run has failed.
		return hSynchConditionsAreMet || (Time.timeSinceLevelLoad >= runDurationLimit);
	}
	
	private void ResetSimulationVariables() {
		// Here we are resetting/reassigning the Performance-/Synchronization-measure variables to their default-values that they are set up to have before starting a simulation run. This way, we are "cleaning up" the current/previous simulation-run and setting up for another new simulation-run within the same Unity "Game-play-run".
		
		t_f_is_now = false;
		t_q = 0f;
		CancelInvoke(); // All t_q-/t_f-Invokes are ended/executed.
		
		first_firing_is_perceived = false;
		
		reset_t_q_flag_raiser = 0f;

		early_t_q_definer = 0f;
		
		agentiHasFiredAtLeastOnce = new bool[collectiveSize];
		hSynchConditionsAreMet = false;
		equal_t_q_streak_counter = 0;
	}
	
	public void IJustHeardAFireEvent(int firingAgentId) {
		// When this method gets called, it means the AgentManager has picked upon a Dr. Squiggle's method-call � meaning it "heard" a Dr. Squiggle's ``fire''-signal.

                                                                                                                                                                    // BARE FOR TESTING:
        if (debugTqTfOn) Debug.Log("(SimTime: - " + Time.timeSinceLevelLoad + " -) Node " + firingAgentId + " fired while 't_f_is_now'=" + BoolToString(t_f_is_now) + ".");

        // For saving of Node-firing-plot-material:
        agentWithAgentIDsJustFired.Add(firingAgentId);
		
		// For enforcing 'Condition 3' about all agents having fired at least once during the simulation-run/evaluation:
		agentiHasFiredAtLeastOnce[firingAgentId - 1] = true; // Flagging that agent corresponding with AgentID has fired at least once during the simulation.
		
		
			// THE REAL PSEUDO-LOGIC:
		if (!ItIsLegalToFireNow() && !reset_t_q_flag) { // the 3)-"reset t_q"-process is started if not started already
			if (debugSuccessOn) Debug.Log("Illegal firing! Even-beat-counter towards k=" + k + ":    " + equal_t_q_streak_counter);

			StartTheResetTQProcess();
		}
		
		if (!FirstFiringHasBeenPerceived()) { // the 1)-step is performed
			PerformTheFirstSynchMeasureStep();
		}
		
		if (EarlyTQDefinerIsInTheMaking()) { // estimating the first "holdepunkt", median_1, for defining the new t_q-estimate (given by B-sketch)
			AddEarlyResetDefiningTime(Time.timeSinceLevelLoad);
            if (debugTqTfOn) Debug.Log("(SimTime: - " + Time.timeSinceLevelLoad + " -) Reset-defining firing-time added to early median-list.");

            if (!FirstEarlyResetDefiningTimeIsAdded()) {
				TriggerDefineEarlyMedian();
			}
		}
		
		if (LateTQDefinerIsInTheMaking()) { // estimating the second "holdepunkt", median_2, for defining the new t_q-estimate (given by B-sketch)
			AddLateResetDefiningTime(Time.timeSinceLevelLoad);
            if (debugTqTfOn) Debug.Log("(SimTime: - " + Time.timeSinceLevelLoad + " -) Reset-defining firing-time added to late median-list.");

            if (!FirstLateTQDefiningTimeIsAdded()) {
				TriggerDefineNewTQ();
			}
		}

		// Safety-mechanism prohibiting the TowardsKCounter, equal_t_q_streak_counter, to hit 'k' when no nodes are firing within the t_f-periods/-windows
		if (FiringWasPerceivedDuringTF()) {
			last_t_f_firers_counter++;
		}
	}
	
	private bool FiringWasPerceivedDuringTF() {
		return t_f_is_now;
	}
	
	private void TriggerDefineNewTQ() {
		first_late_t_q_defining_time_added = true;
		t_f_is_now = true; // Setting off a t_f_is_now-period.
		Invoke("DefineNewTQ", t_f/2f);
        if (debugTqTfOn) Debug.Log("(SimTime: - " + Time.timeSinceLevelLoad + " -) 't_f-START' and setting new 't_q' in half a 't_f'-period = " + t_f/2.0 + " SimSecs.");

    }
	
	private void DefineNewTQ() {
        float new_t_q_estimate;
        if (useTQAverage) {
		    new_t_q_estimate = ListAverage(late_reset_defining_times) - early_t_q_definer - t_f;
        } else {
            new_t_q_estimate = ListMedian(late_reset_defining_times) - early_t_q_definer - t_f;
        }
		defining_times_acquired = 0; // Resetting median-counter despite its true-value of 2 (for the next possible TQ-resetting).
		// the 4)-"t_q-reset"-processclosing is executed:
		t_q = new_t_q_estimate;
        if (debugTqTfOn) Debug.Log("(SimTime: - " + Time.timeSinceLevelLoad + " -) New 't_q' set: " + t_q + ".");

        RestartTFTQWindows();
		// Variabel-opprydning:
		late_reset_defining_times.Clear();
		first_late_t_q_defining_time_added = false;
	}
	
	private void RestartTFTQWindows() {
		CancelInvoke(); // Cancelling all currently ongoing Invoke-calls.
        if (debugTqTfOn) Debug.Log("(SimTime: - " + Time.timeSinceLevelLoad + " -) Invoke-Cancellations and 't_q-START' with 't_q'=" + t_q + ".");
		TriggerTQPeriod();

    }
	
	private void TriggerTQPeriod() {
		t_f_is_now = false;

		IncrementTowardsKCounterIfWanted();
		reset_t_q_flag = false;
		ResetLastTFFirersCounter();

		if (debugSuccessOn) Debug.Log("Even-beat-counter towards k=" + k + ":    " + equal_t_q_streak_counter);

		Invoke("TriggerTFPeriod", t_q);
        if (debugTqTfOn) Debug.Log("(SimTime: - " + Time.timeSinceLevelLoad + " -) 't_f-EXIT' & 't_q-START'. Calling for a 't_q-EXIT' & 't_f-START' in 't_q'=" + t_q + " SimSecs.");
    }

	private void ResetLastTFFirersCounter() {
		last_t_f_firers_counter = 0;
    }

	private void IncrementTowardsKCounterIfWanted() {
        // Ignoring the first counter-increment, so that the ending of the simulation-run actually follows 'Condition 2', relating to k.
        // Ignoring, for the sake of 'Condition 2' and the TowardsKCounter equal_t_q_streak_counter, "successfully" finished t_f-windows if no nodes fired during it.
        
        // Incrementing the counter otherwise.
        // Immediately checking the requirements/conditions for harmonic synchrony after, to see whether we now have achieved the system target goal.

        if (!reset_t_q_flag && last_t_f_firers_counter != 0) {
            equal_t_q_streak_counter++;

            CheckHSynchConditions();
        }
	}


	private void TriggerTFPeriod() {
		t_f_is_now = true;
		Invoke("TriggerTQPeriod", t_f);
        if (debugTqTfOn) Debug.Log("(SimTime: - " + Time.timeSinceLevelLoad + " -) 't_q-EXIT' & 't_f-START'. Called for 't_f-EXIT' & 't_q-START' in 't_f'=" + t_f + " SimSecs.");

    }

	private void TriggerDefineEarlyMedian() {
		first_early_reset_defining_time_added = true;
		t_f_is_now = true;
		Invoke("DefineEarlyMedian", t_f);

        if (debugTqTfOn) Debug.Log("(SimTime: - " + Time.timeSinceLevelLoad + " -) 't_f-START'. Defining 'early_median' in 't_f'=" + t_f + " SimSecs.");

    }
	
	private void DefineEarlyMedian() {
        if (useTQAverage) {
            early_t_q_definer = ListAverage(early_t_q_defining_times);
        } else {
            early_t_q_definer = ListMedian(early_t_q_defining_times);
        }

        if (debugTqTfOn) Debug.Log("(SimTime: - " + Time.timeSinceLevelLoad + " -) 'early_median' defined: " + early_t_q_definer + ".");
        
        defining_times_acquired ++;
		TFLower();
		// Variabel-opprydning:
		early_t_q_defining_times.Clear();
		first_early_reset_defining_time_added = false;
	}
	

	private bool FirstLateTQDefiningTimeIsAdded() {
		return first_late_t_q_defining_time_added;
	}
	
	private bool FirstEarlyResetDefiningTimeIsAdded() {
		return first_early_reset_defining_time_added;
	}
	
	
	private void AddLateResetDefiningTime(float simulationTimeInSeconds) {
		late_reset_defining_times.Add(simulationTimeInSeconds);

        // Hvis du er sjuk og vil debugge ekstremt hardt:
        //Debug.Log("Late Reset Defining Times: ");
        //DebugLogMyFloatList(late_reset_defining_times);
    }

	private void AddEarlyResetDefiningTime(float simulationTimeInSeconds) {
		early_t_q_defining_times.Add(simulationTimeInSeconds);

        // Hvis du er sjuk og vil debugge ekstremt hardt:
        //Debug.Log("Early Reset Defining Times: ");
		//DebugLogMyFloatList(early_t_q_defining_times);
	}
	
	
	private bool LateTQDefinerIsInTheMaking() {
		// Returning true if the 3)-"reset t_q"-process is started, the candidate late median definer is far enough away from the early median time (to avoid negative numbers), the early median has been found but the late and second median still hasn't been used to define the new t_q-estimate through the DefineNewTQ()-function. Returns false otherwise.
		
		return (reset_t_q_flag && (Time.timeSinceLevelLoad > (early_t_q_definer + t_f)) && (defining_times_acquired == 1));
	}
	
	private bool EarlyTQDefinerIsInTheMaking() {
		// Returning true if the 3)-"reset t_q"-process is started, the candidate early median definer is far enough away from the reset-triggering firing-time (to avoid negative numbers), and the early and first median still haven't been acquired through the DefineEarlyMedian()-function. Returns false otherwise.
		
		return (reset_t_q_flag && (Time.timeSinceLevelLoad > (reset_t_q_flag_raiser + t_f)) && (defining_times_acquired == 0));
	}
	
	
	private void PerformTheFirstSynchMeasureStep() {
		first_firing_is_perceived = true;
		TriggerFirstTFRise();
	}
	
	private void TriggerFirstTFRise() {
		t_f_is_now = true;
		Invoke("TFLower", t_f);

        if (debugTqTfOn) Debug.Log("(SimTime: - " + Time.timeSinceLevelLoad + " -) 't_f-START', supposedly the first. Also called for 't_f-EXIT' in 't_f'=" + t_f + " SimSecs.");
    }
	
	private void TFLower() {
		t_f_is_now = false;

        if (debugTqTfOn) Debug.Log("(SimTime: - " + Time.timeSinceLevelLoad + " -) 't_f-EXIT'.");
    }
	
	
	private bool FirstFiringHasBeenPerceived() {
		return first_firing_is_perceived;
	}
	
    
	private void StartTheResetTQProcess() {
        CancelInvoke(); // Cancelling all currently ongoing Invoke-calls. 	PASS P� S� DU IKKE CANCELLER DefineEarlyMedian- ELLER DefineNewTQ-Invokesa MED ET UHELL HER.
        reset_t_q_flag_raiser 		= Time.timeSinceLevelLoad;
		reset_t_q_flag 				= true;
		equal_t_q_streak_counter 	= 0;

        if (debugTqTfOn) Debug.Log("(SimTime: - " + Time.timeSinceLevelLoad + " -) Illegal firing. 't_q-RESET' initiated.");
    }
	
	
	private bool ItIsLegalToFireNow() {
		return t_f_is_now;
	}




    // 'SPAWNING':

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




    // 'HELPING':

    private void InitializeHelpingVariables() {
        // Speeding up or down the simulation if that is wanted?
        Time.timeScale = adjustedTimeScale;

        spawnRadius = collectiveSize * 1.5f; // Simply an empirical model of the necessary space the agents need to spawn. Or just a guess I guess.

        agentiHasFiredAtLeastOnce = new bool[collectiveSize];
    }




    // '.CSV -CREATE & -UPDATE':

    private void CreateAllCSVFiles() {
        // First of all making sure all the necessary folders the .CSVs are gonna reside in exist.
        //CreateFoldersIfNonExistant();

        // Obtaining the .CSV-header consisting of the agents's IDs.
        List<string> agentIDHeader = GetAgentHeader();

        // Creating one .CSV-file for the agents's phases over time.
        CreatePhaseCSV(agentIDHeader);
        
        // Creating one .CSV-file for the agents's frequencies over time.
        CreateFrequencyCSV(agentIDHeader);

        // Creating one .CSV-file for the node_firing_data (including t_f_is_now) needed to create the "Node-firing-plot" as in Nymoen's Fig. 6.
        CreateNodeFiringCSV(agentIDHeader);

        // Automatically creating a Synchrony Dataset-.CSV if it does not exist.
        if (!File.Exists(datasetPath)) CreateSynchDatasetCSV();
    }

    //private void CreateFoldersIfNonExistant() {
    //    // Creating the directories/folders where the saved .CSV-data will reside:

    //    Directory.CreateDirectory(phasesFolderPath);
    //    Directory.CreateDirectory(frequenciesFolderPath);
    //    Directory.CreateDirectory(nodeFiringPlotMaterialFolderPath);
    //}

    private List<string> GetAgentHeader() {
        // Creating a .CSV-header consisting of the agents's IDs

        List<string> agentIDHeader = new List<string>();
        foreach (SquiggleScript squiggScr in spawnedAgentScripts) {
            agentIDHeader.Add("agent" + squiggScr.GetAgentID().ToString());
        }

        return agentIDHeader;
    }

    private void CreatePhaseCSV(List<string> agentHeader) {
        CreateCSVWithStringHeader(phasesFolderPath + "phases_over_time_atSimRun" + atSimRun + ".csv", agentHeader);
    }

    private void CreateFrequencyCSV(List<string> agentHeader) {
        CreateCSVWithStringHeader(frequenciesFolderPath + "freqs_over_time_atSimRun" + atSimRun + ".csv", agentHeader);
    }

    private void CreateNodeFiringCSV(List<string> agentHeader) {
        List<string> nodeFiringWithTfHeader = new List<string>(agentHeader);
        nodeFiringWithTfHeader.Insert(0, "t_f_is_now");
        CreateCSVWithStringHeader(nodeFiringPlotMaterialFolderPath + "node_firing_data_atSimRun" + atSimRun + ".csv", nodeFiringWithTfHeader);
    }

    private void CreateSynchDatasetCSV() {
        // Creating a .CSV - file for the MSc Synchrony - dataset, which are to contain the measurements with their covariates.
        List<string> performanceAndCovariatesHeader = new List<string>(); // The covariates we want to record the performance-measure/outcome/response-variable for
        performanceAndCovariatesHeader.Add("SIMTIME");
        performanceAndCovariatesHeader.Add("SUCCESS");    // Binary covariate (no=0 or yes=1)
        performanceAndCovariatesHeader.Add("COLLSIZE");
        performanceAndCovariatesHeader.Add("TREFPERC");
        performanceAndCovariatesHeader.Add("MINFREQ");
        performanceAndCovariatesHeader.Add("MAXFREQ");
        performanceAndCovariatesHeader.Add("K");
        performanceAndCovariatesHeader.Add("T_F");
        performanceAndCovariatesHeader.Add("TQDEFINER");
        performanceAndCovariatesHeader.Add("ADJTIMESCALE");
        performanceAndCovariatesHeader.Add("ALPHA");
        performanceAndCovariatesHeader.Add("PHASEADJ");
        performanceAndCovariatesHeader.Add("BETA");
        performanceAndCovariatesHeader.Add("FREQADJ");
        performanceAndCovariatesHeader.Add("M");
        CreateCSVWithStringHeader(datasetPath, performanceAndCovariatesHeader);
    }


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
        FloatUpdateCSV(phasesFolderPath + "phases_over_time_atSimRun" + atSimRun + ".csv", phaseIntervalEntries);
    }

    private void UpdateFrequencyCSV() {
        List<float> frequencyIntervalEntries = new List<float>();
        foreach (SquiggleScript squiggScr in spawnedAgentScripts) {
            frequencyIntervalEntries.Add(squiggScr.GetFrequency());
        }
        FloatUpdateCSV(frequenciesFolderPath + "freqs_over_time_atSimRun" + atSimRun + ".csv", frequencyIntervalEntries);
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
        FloatUpdateCSV(nodeFiringPlotMaterialFolderPath + "node_firing_data_atSimRun" + atSimRun + ".csv", nodeFiringDataList);
    }

    private void SaveDatapointToDataset(float runDuration) {
        // Saving a datapoint to a .CSV-dataset containing data: Measurements Simulation time (s) (perf.-measure) and Simulation success, as well as Simulation hyper-parameters/covariates/explanators/predictors. (.CSV-header currently assumed to be manually written in the existing .CSV already)

        // Initializing empty list soon-to-contain the data described above:
        List<float> performanceAndCovariateValues = new List<float>();

        // Adding Response/Outcome Measurements that I don't know before simulating:

        float SIMTIME = runDuration;
        performanceAndCovariateValues.Add(SIMTIME);

        float SUCCESS = System.Convert.ToSingle(hSynchConditionsAreMet);
        performanceAndCovariateValues.Add(SUCCESS);
        // (JUST FOR DEBUGGING) Telling the programmer in the console whether the simulation run was a success or not:
        if (hSynchConditionsAreMet) Debug.Log("CONGRATULATIONS! Harmonic synchrony in your musical multi-robot collective at simRun " + atSimRun + " was achieved in " + runDuration + " seconds!");
        else Debug.Log("TOO BAD... Harmonic synchrony, according to the performance-measure defined by K. Nymoen et al., was not achieved at simRun " + atSimRun + " within the run time-limit of " + runDurationLimit + " seconds..");


        // Adding Collective/Environment Hyper-parameters/Covariates that I know before simulating:

        float COLLSIZE = collectiveSize;
        performanceAndCovariateValues.Add(COLLSIZE);

        float TREFPERC = System.Convert.ToSingle(t_ref_perc_of_period);
        performanceAndCovariateValues.Add(TREFPERC);

        float MINFREQ = System.Convert.ToSingle(minMaxInitialFreqs.x);
        performanceAndCovariateValues.Add(MINFREQ);

        float MAXFREQ = System.Convert.ToSingle(minMaxInitialFreqs.y);
        performanceAndCovariateValues.Add(MAXFREQ);

        float K = System.Convert.ToSingle(k);
        performanceAndCovariateValues.Add(K);

        float T_F = t_f;
        performanceAndCovariateValues.Add(T_F);

        float TQDEFINER = System.Convert.ToSingle(useTQAverage);
        performanceAndCovariateValues.Add(TQDEFINER);

        float ADJTIMESCALE = adjustedTimeScale;
        performanceAndCovariateValues.Add(ADJTIMESCALE);


        // Adding Individual/Agent Hyper-parameters/Covariates (NB! Due to current design of datapoint-saving, these have to be equal for all agents) that I know before simulating:

        float ALPHA = System.Convert.ToSingle(spawnedAgentScripts[0].alpha); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE (due to [0])
        performanceAndCovariateValues.Add(ALPHA);

        float PHASEADJ = System.Convert.ToSingle(spawnedAgentScripts[0].useNymoenPhaseAdj); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE (due to [0])
        performanceAndCovariateValues.Add(PHASEADJ);

        float BETA = System.Convert.ToSingle(spawnedAgentScripts[0].beta); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE (due to [0])
        performanceAndCovariateValues.Add(BETA);
         
        float FREQADJ = System.Convert.ToSingle(spawnedAgentScripts[0].useNymoenFreqAdj); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE (due to [0])
        performanceAndCovariateValues.Add(FREQADJ);

        float M = System.Convert.ToSingle(spawnedAgentScripts[0].m); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE (due to [0])
        performanceAndCovariateValues.Add(M);


        // POSSIBLE TODOs:
            // - Make a prettier Inspector for grouping relevant variables with each other (cf. Unity bookmark in Brave).
            // - Make a separate 'simulation-run dataset' with rows for each agent and columns for each Individual/Agent Hyper-parameters (so that the NB! on the reMarkable hyper-parameter note is taken care of) � if it is needed or wanted or useful.


        // Saving one datapoint, a.k.a. writing one .CSV-row (Measurements, Covariates) to the .CSV-file at the datasetPath:
        FloatUpdateCSV(datasetPath, performanceAndCovariateValues);
    }
}

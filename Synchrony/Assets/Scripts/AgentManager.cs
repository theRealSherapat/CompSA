using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using static SynchronyUtils;
using static SquiggleScript;
using System.Collections;

public class AgentManager : MonoBehaviour {

    // 'VARIABLES':

    // 'Recorded collective-/environment-hyperparameters':
    [Tooltip("The number of agents to be spawned and synchronized.")]
    public int collectiveSize = 6;
    [Tooltip("Whether we are trying to recreate a Nymoen experiment as closely as his one, or using our own system's variables.")]
    public bool recreatingNymoenResults;
    [Tooltip("The duration (%) of the refractory period in terms of a percentage of the agents's oscillator-periods.")]
    public float t_ref_perc_of_period = 0.05f;       // ISH LENGDEN I TID PÅ digitalQuickTone er 0.4s. Nymoen BRUKTE 50ms I SIN IMPLEMENTASJON. JEG PRØVDE OGSÅ 0.6f. possiblePool = {0.09f, 0.4f, 0.6f}.
    [Tooltip("Minimum and maximum initialization-frequencies (Hz).")]
    public Vector2 minMaxInitialFreqs = new Vector2(0.5f, 4f);

    [Tooltip("The number of times in a row the t_q-window (where no fire-events can be heard) must be equally long.")]
    public int k = 8;
    [Tooltip("The duration (s) of the short time-window the nodes are allowed to fire within.")]
    public float t_f = 0.08f;
    public enum TQDefinerEnum // Min customme enumeration
    {
        Median,
        Average
    };
    [Tooltip("Whether to use Averages when defining new t_q-windows, or Medians.")]
    public TQDefinerEnum TQDefiner = TQDefinerEnum.Median;

    // For recreating certain simulation-runs:
    [Tooltip("Whether or not to use the deterministic user defined random seed, or just simply a randomly generated one.")]
    public bool useDeterministicSeed;
    [Tooltip("The random-seed for the pseudo-random number-generator in Unity.")]
    public int randomSeed;

    // 'Non-recorded general meta-/environment-hyperparemeters':
    public enum simulationModesEnum // Min customme enumeration
    {
        Experiment,
        Analysis
    };
    [Tooltip("Whether to simply save the performance measure and datapoint (in experiment mode), or also saving all the plotting materials (in analysis mode).")]
    public simulationModesEnum simulationMode = simulationModesEnum.Experiment;
    [Tooltip("How frequently (Hz) we want to sample our simulator-values, potentially affecting saving times significantly.")]
    public float dataSavingFrequency = 50.0f;
    [Tooltip("The simulation-timelimit in seconds (i.e. the max simulation-time a simulation-run is allowed to run for before being regarded as a failed synchronization-run).")]
    public float runDurationLimit = 300f;
    [Tooltip("Whether to give the human observer a sound on every agent-pulse/-firing or not.")]
    public bool useSound = true;
    [Tooltip("Volume knob (%).")]
    public float volumePercentage = 0.5f;
    [Tooltip("Whether to give the human observer a visual and Lerped color-signal on every agent-pulse/-firing or not.")]
    public bool useVisuals = true;

    // 'Knobs for the nerds':
    [Tooltip("Whether to get logs about Synchrony-successes (in terms of the 'towards-k'-counter) or not.")]
    public bool debugSuccessOn = false;
    [Tooltip("Whether to get logs about t_f-/t_q-windows and corresponding timestamps or not.")]
    public bool debugTqTfOn = false;
    [Tooltip("How many 'longest possible' periods we are to let robots with low frequencies struggle before we double their frequency.")]
    public int allowRobotsToStruggleForPeriods = 3;

    // Spawning:
    [Tooltip("All the DrSquiggle-prefabs we want to spawn and be synchronized.")]
    public GameObject[] squigglePrefabs;


    
    // 'Private variables necessary to make the cogs go around':

    // General Meta:
    private string wantedHyperparamsPath = Directory.GetCurrentDirectory() + "\\" + "wantedHyperparametersForSimulationRun.csv";
    private static int atSimRun = 0;
    private System.Random randGen;

    // Spawning:
    private float spawnRadius; // The radius from origo within which the agents are allowed to spawn without colliding.
    private float agentWidth = Mathf.Sqrt(Mathf.Pow(4.0f, 2) + Mathf.Pow(4.0f, 2)); // diameter from tentacle to tentacle (furthest from each other)
    private List<Vector2> spawnedPositions = new List<Vector2>();
    private List<SquiggleScript> spawnedSquiggleScripts = new List<SquiggleScript>();
    private List<List<float>> distancesMatrix = new List<List<float>>();
    private float wantedAlpha;
    private phaseSyncEnum wantedPhaseAdjustmentMethod;
    private float wantedBeta;
    private frequencySyncEnum wantedFrequencyAdjustmentMethod;
    private int wantedM;

    // CSV-Serialization:
    private int dataSavingFrequencyY;
    private string phasesFolderPath = Directory.GetCurrentDirectory() + "\\" + "SavedData" + "\\" + "Phases" + "\\";
    private string frequenciesFolderPath = Directory.GetCurrentDirectory() + "\\" + "SavedData" + "\\" + "Frequencies" + "\\";
    private string nodeFiringPlotMaterialFolderPath = Directory.GetCurrentDirectory() + "\\" + "SavedData" + "\\" + "PerformanceMeasurePlotMaterial" + "\\";
    private string synchronyEvolutionsFolderPath = Directory.GetCurrentDirectory() + "\\" + "SavedData" + "\\" + "SynchronyEvolutions" + "\\";
    private string datasetPath = Directory.GetCurrentDirectory() + "\\" + "SavedData" + "\\" + "synchronyDataset.csv";

    // Node-firing plot:
    private List<int> agentWithAgentIDsJustFired = new List<int>(); // Initializing (used for creation of node-firing-plot) a list for all the agents with agent-ids that just fired. 

    // 'Synch.-Perf.-measure related':
    private bool t_f_is_now = false; // A short "up-time"-flag when all nodes are allowed to fire during. The duration of how long this flag is positive itself is constant (t_f), but when in simulation-time it will be "up", depends largely on the t_q-variable as well as when the t_q-windows are triggered.
        // 'Node-firing-plot-materials related':
    private List<float> t_f_is_nows = new List<float>();
    private List<List<float>> agents_fired_matrix = new List<List<float>>();

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
    private int towards_k_counter = 0; // 'towards-k'-counter to become equal to 'k'.
    private List<float> towards_k_counters = new List<float>(); // all the logged 'towards-k-counter'-variable-values throughout the simulation-run.

	private int last_t_f_firers_counter = 0; // A counter for how many fire-events were heard throughout the last firing-period t_f, used as a safety-mechanism to detect firing-periods within which no agents fire — so that we don't increment the 'towards-k'-counter after those firing-periods.




    // 'MonoBehaviour':

    void Awake() {
        // (AV EN ELLER ANNEN GRUNN ER JEG AVHENGIG AV Å BRUKE DENNE NÅ):
        // Loading the user or Python-script given hyperparameters wanted for the simulation run being set up.
        LoadCSVCovariatesIntoSimulation();

        InitializeRandomGenerator();

        // Spawning all agents randomly (but pretty naively as of now)
        SpawnAgents();

        // Assigning the distancesMatrix that the Squiggles will use for filling up each others's firesignal subscriber lists.
        distancesMatrix = GetRobotDistances();
        
        InitializeVariables();
    }


    void FixedUpdate() { // having to do with physics, time-critical functionality (depends on fixedTime e.g.)
        // Ending simulation-run if we deem it either a synchronization -success or -failure.
        EndSimulationRunIfTerminationCriteriaAreReached();
        
        foreach (SquiggleScript squig in spawnedSquiggleScripts) {
            // Detecting a phase-climax, or increasing the phase according to the frequency (which will also be doubled if an unstable amount of time has gone without the agent climaxing).
            squig.DetectPhaseAnomaliesOrIncreaseIt();
            // Logging simulation-run-values for plotting and data-serialization purposes.
            squig.SavePlottingDataIfOnRightFrame(); // DETERMINISM PROBLEM DUE TO LONG SAVING EXECUTION TIME?
        }

        // Logging simulation-run-values for plotting and data-serialization purposes.
        SavePlottingDataIfOnRightFrame();
    }




    // 'PERFORMANCE-MEASURE' (detecting harmonic synchrony):

    private void CheckHSynchConditions() {
        // Checks 'Condition 2' and 'Condition 3' respectively: namely, '2': if the agents have beat evenly (with a constant t_q-value) k times in a row, as well as if all nodes have fired at least once throughout the evaluation-/testing-period (simulation-run).
		
		// The whole synchronization-measure implemented is assumed to facilitate and ensure the keeping of 'Condition 1', namely that only legal firing is happening (during a very short and white time-window t_f), not illegal firing (during the longer and gray t_q-window). Thus this 'Condition 1' is not explicitly checked here, but assumed.

		// If all these conditions are true, the function is thus raising the (Congratulations-vibed) hSynchConditionsAreMet-signal/-flag high, for having achieved harmonic synchrony. If all conditions are true and the hSynchConditionsAreMet-flag is raised: Rejoice! Synchrony is obtained!

        bool agentsHaveBeatenAtAnEvenRhythmKTimes = towards_k_counter == k;
        bool allAgentsHaveFiredAtLeastOnce = !(agentiHasFiredAtLeastOnce.AsQueryable().Any(val => val == false));

        if (agentsHaveBeatenAtAnEvenRhythmKTimes && allAgentsHaveFiredAtLeastOnce) hSynchConditionsAreMet = true; // IF TRUE: REJOICE!
    }
	
	private void EndSimulationRunIfTerminationCriteriaAreReached() {
        // If the simulation has either succeeded, or failed: save the corresponding datapoint and move on.
        if (SimulationRunEitherSucceededOrFailed()) {
            // Saving a successful or unsuccessful data-point
            SaveDatapointToSynchDatasetCSV(Time.timeSinceLevelLoad);

            // Signifying that I am done with one simulator-run
            atSimRun++;

            // Ensuring we have updated the synchrony-evolution-plot, as this is most time-critical in the complete end of the simulation.                                          MULIGENS TENK PÅ
            //UpdateSynchronyEvolutionCSV();

            QuitMyGame();

            //if (atSimRun != simulationRuns) {
            //    //ResetSimulationVariables();
            //    LoadMySceneAgain();
            //} else {
            //    QuitMyGame();
            //}
        }
    }
    
	private bool SimulationRunEitherSucceededOrFailed() {
		// If the first condition is true, harmonic synchrony has been achieved (the synchrony-conditions are fulfilled), and the simulation-run has succeeded. 
		// If the latter condition is true, the simulation-run has exceeded the maximum time-limit allowed, and the simulation-run has failed.
		return hSynchConditionsAreMet || (Time.timeSinceLevelLoad >= runDurationLimit);
	}
	
	public void IJustHeardAFireEvent(int firingAgentId) {
		// When this method gets called, it means the AgentManager has picked upon a Dr. Squiggle's method-call — meaning it "heard" a Dr. Squiggle's ``fire''-signal.

                                                                                                                                                                    // BARE FOR TESTING:
        if (debugTqTfOn) Debug.Log("(SimTime: - " + Time.timeSinceLevelLoad + " -) Node " + firingAgentId + " fired while 't_f_is_now'=" + BoolToString(t_f_is_now) + ".");

        // For saving of Node-firing-plot-material:
        agentWithAgentIDsJustFired.Add(firingAgentId);
		
		// For enforcing 'Condition 3' about all agents having fired at least once during the simulation-run/evaluation:
		agentiHasFiredAtLeastOnce[firingAgentId - 1] = true; // Flagging that agent corresponding with AgentID has fired at least once during the simulation.
		
		
			// THE REAL PSEUDO-LOGIC:
		if (!ItIsLegalToFireNow() && !reset_t_q_flag) { // the 3)-"reset t_q"-process is started if not started already
			if (debugSuccessOn) Debug.Log("Illegal firing! Even-beat-counter towards k=" + k + ":    " + towards_k_counter);

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

		// Safety-mechanism prohibiting the TowardsKCounter, towards_k_counter, to hit 'k' when no nodes are firing within the t_f-periods/-windows
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
        StartCoroutine(TriggerPeriodAfter(2, t_f/2f));
        if (debugTqTfOn) Debug.Log("(SimTime: - " + Time.timeSinceLevelLoad + " -) 't_f-START' and setting new 't_q' in half a 't_f'-period = " + t_f/2.0 + " SimSecs.");

    }
	
	private void DefineNewTQ() {
        float new_t_q_estimate;
        
        if ((int)TQDefiner == 1) {
		    new_t_q_estimate = ListAverage(late_reset_defining_times) - early_t_q_definer - t_f;
        } else {
            new_t_q_estimate = ListMedian(late_reset_defining_times) - early_t_q_definer - t_f;
        }
        // Performing a safety measure so that no errors where t_q -> 0 leads to any false positives (like it did mid phase sync hyperparam. tuning experiment) e.g.
        float smallest_legal_t_q_value = 1.0f / spawnedSquiggleScripts[0].GetHighestRobotFrequency() - t_f;
        if (new_t_q_estimate < smallest_legal_t_q_value) {
            new_t_q_estimate = smallest_legal_t_q_value;
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
        StopAllCoroutines(); // Stopping all currently ongoing or stored Coroutines.
        if (debugTqTfOn) Debug.Log("(SimTime: - " + Time.timeSinceLevelLoad + " -) Coroutine-Stopping and 't_q-START' with 't_q'=" + t_q + ".");
		TriggerTQPeriod();
    }
	
	private void TriggerTQPeriod() {
		t_f_is_now = false;

		IncrementTowardsKCounterIfWantedAndResetTowardsKCounterIfNeeded();
		reset_t_q_flag = false;
		ResetLastTFFirersCounter();

		if (debugSuccessOn) Debug.Log("Even-beat-counter towards k=" + k + ":    " + towards_k_counter);

        StartCoroutine(TriggerPeriodAfter(3, t_q));
        if (debugTqTfOn) Debug.Log("(SimTime: - " + Time.timeSinceLevelLoad + " -) 't_f-EXIT' & 't_q-START'. Calling for a 't_q-EXIT' & 't_f-START' in 't_q'=" + t_q + " SimSecs.");
    }

	private void ResetLastTFFirersCounter() {
		last_t_f_firers_counter = 0;
    }

	private void IncrementTowardsKCounterIfWantedAndResetTowardsKCounterIfNeeded() {
        // Ignoring the first counter-increment, so that the ending of the simulation-run actually follows 'Condition 2', relating to k.
        // Ignoring, for the sake of 'Condition 2' and the TowardsKCounter towards_k_counter, "successfully" finished t_f-windows if no nodes fired during it.
        
        // Incrementing the counter otherwise.
        // Immediately checking the requirements/conditions for harmonic synchrony after, to see whether we now have achieved the system target goal.
        if (last_t_f_firers_counter == 0) {
            towards_k_counter = 0;
        } else if (!reset_t_q_flag) {
            towards_k_counter++;

            CheckHSynchConditions();
        }
	}


	private void TriggerTFPeriod() {
		t_f_is_now = true;
        StartCoroutine(TriggerPeriodAfter(4, t_f));
        if (debugTqTfOn) Debug.Log("(SimTime: - " + Time.timeSinceLevelLoad + " -) 't_q-EXIT' & 't_f-START'. Called for 't_f-EXIT' & 't_q-START' in 't_f'=" + t_f + " SimSecs.");

    }

	private void TriggerDefineEarlyMedian() {
		first_early_reset_defining_time_added = true;
		t_f_is_now = true;
        StartCoroutine(TriggerPeriodAfter(1, t_f));

        if (debugTqTfOn) Debug.Log("(SimTime: - " + Time.timeSinceLevelLoad + " -) 't_f-START'. Defining 'early_median' in 't_f'=" + t_f + " SimSecs.");

    }
	
	private void DefineEarlyMedian() {
        if ((int)TQDefiner==1) {
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
        StartCoroutine(TriggerPeriodAfter(0, t_f));

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
        StopAllCoroutines(); // Stopping all currently ongoing Coroutines. PASS PÅ SÅ DU IKKE CANCELLER DefineEarlyMedian- ELLER DefineNewTQ-Coroutines MED ET UHELL HER.
        reset_t_q_flag_raiser 		= Time.timeSinceLevelLoad;
		reset_t_q_flag 				= true;
		towards_k_counter           = 0;

        if (debugTqTfOn) Debug.Log("(SimTime: - " + Time.timeSinceLevelLoad + " -) Illegal firing. 't_q-RESET' initiated.");
    }
	
	
	private bool ItIsLegalToFireNow() {
		return t_f_is_now;
	}


    IEnumerator TriggerPeriodAfter(int periodType, float inFixedDeltaTimeSeconds) {
        float timeTracker = 0.0f;
        while (timeTracker < (inFixedDeltaTimeSeconds - 0.0001f)) {
            timeTracker += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        // Quick'n'dirty way of calling the right type of period after 4.0f seconds of fixed time (or so it is supposed to be):
        if (periodType == 0) { // Triggering TFLower (since its Coroutine with a second-parameter appears firstly in the Synch-/perf.-measure's reM.-pseudocode).
            TFLower();
        } else if (periodType == 1) { // Triggering DefineEarlyMedian (since its Coroutine with a second-parameter appears secondly in the Synch-/perf.-measure's reM.-pseudocode).
            DefineEarlyMedian();
        } else if (periodType == 2) { // Triggering DefineNewTQ (since its Coroutine with a second-parameter appears thirdly in the Synch-/perf.-measure's reM.-pseudocode).
            DefineNewTQ();
        } else if (periodType == 3) { // Triggering TriggerTFPeriod (--||--).
            TriggerTFPeriod();
        } else { // Triggering TriggerTQPeriod (--||--).
            TriggerTQPeriod();
        }
    }




    // 'SPAWNING':

    private void SpawnAgents() {
        float slingringsmonn = 4.0f;
        spawnRadius = (collectiveSize/6.0f)*agentWidth + slingringsmonn; // Simply an empirical model of the necessary space the agents need to spawn. Or just a guess I guess.

        for (int i = 0; i < collectiveSize; i++) {
            // Finding a position in a circle free to spawn (taking into account not wanting to collide with each other) (x, z)
            Vector2 randomCirclePoint = FindFreeSpawnPosition();
            spawnedPositions.Add(randomCirclePoint);

            int randomSquigglePrefabIndex = randGen.Next(0, squigglePrefabs.Length);

            // BARE FOR TESTING:
            Debug.Log(squigglePrefabs[randomSquigglePrefabIndex].name + " with AgentID " + (i + 1) + " will spawn at position: " + randomCirclePoint);

            // Spawning an agent from squigglePrefabs on the free position
            GameObject newAgent = Instantiate(squigglePrefabs[randomSquigglePrefabIndex],
                                                                new Vector3(randomCirclePoint.x, -0.96f, randomCirclePoint.y),
                                                                Quaternion.identity);

            SquiggleScript extractedSquiggleScript = newAgent.GetComponent<SquiggleScript>();

            // Assigning robot-variables:
            extractedSquiggleScript.SetAgentID(i + 1); // Setting AgentIDs so that agents have IDs {1, 2, 3, ..., N}, where N is the number of agents in the scene.

            extractedSquiggleScript.alpha = wantedAlpha;
            extractedSquiggleScript.phaseAdjustment = wantedPhaseAdjustmentMethod;
            extractedSquiggleScript.beta = wantedBeta;
            extractedSquiggleScript.frequencyAdjustment = wantedFrequencyAdjustmentMethod;
            extractedSquiggleScript.m = wantedM;

            spawnedSquiggleScripts.Add(extractedSquiggleScript);

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

        Vector2 currentGuess = GetPredictableRandomUnitCircleCoord() * spawnRadius;
        bool foundFreeSpawnPoint = false;

        while (!foundFreeSpawnPoint) {
            foundFreeSpawnPoint = true;

            currentGuess = GetPredictableRandomUnitCircleCoord() * spawnRadius;

            foreach (Vector2 spawnedPosition in spawnedPositions) {
                if (Vector2.Distance(spawnedPosition, currentGuess) < agentWidth) {
                    foundFreeSpawnPoint = false;
                }
            }
        }

        return currentGuess;
    }

    private Vector2 GetPredictableRandomUnitCircleCoord() {
        float x = (randGen.Next(0, 2) * 2 - 1) * (float)randGen.NextDouble();
        float y = (randGen.Next(0, 2) * 2 - 1) * (float)randGen.NextDouble();
        Vector2 withinUnitCircle = new Vector2(x, y);
        if (withinUnitCircle.magnitude > 1.0f) {
            withinUnitCircle = withinUnitCircle.normalized;
        }

        return withinUnitCircle;
    }

    private List<List<float>> GetRobotDistances() {
        List<List<float>> distanceMat = new List<List<float>>();

        foreach (Vector2 spawnedPositionFrom in spawnedPositions) {
            List<float> newDistanceRow = new List<float>();

            foreach (Vector2 spawnedPostionTo in spawnedPositions) {
                newDistanceRow.Add(Vector2.Distance(spawnedPositionFrom, spawnedPostionTo));
            }

            distanceMat.Add(newDistanceRow);
        }


        return distanceMat;
    }




    // 'HELPING':

    // Get-functions:
    public System.Random GetRandomNumberGenerator() {
        return randGen;
    }

    public int GetRandomGeneratorSeed() {
        return randomSeed;
    }
    
    public int GetDataSavingParameter() {
        return dataSavingFrequencyY;
    }

    public void InitializeRandomGenerator() {
        // Generating a random random-seed if we don't want to use the Inspector random seed.
        if (!useDeterministicSeed) randomSeed = Random.Range(1, 100000);
        // Initializing the AgentManager's random-generator with either a random seed given by the user, or simply a randomly generated one.
        randGen = new System.Random(randomSeed);
    }

    private void InitializeVariables() {
        agentiHasFiredAtLeastOnce = new bool[collectiveSize];
        dataSavingFrequencyY = Mathf.RoundToInt(Mathf.Clamp((100.0f/dataSavingFrequency), 1.0f, 100.0f));

        InitializeAgentsFiredMatrix();

        GetCameraIntoDecentFOVPosition();

        ScaleGroundAccordingToSpawnRadius();
    }

    private void LoadCSVCovariatesIntoSimulation() {
        // 'Reading in a character separated file (.CSV)':
        List<float> covariatesToUse = new List<float>();

        using (var reader = new StreamReader(wantedHyperparamsPath)) {

            // Skipping the first boring (text) line containing the HCI-understandable hyperparameter values (to be read by hoomans).
            reader.ReadLine();

            // Extracting the interesting covariates to a C# list.
            var line = reader.ReadLine();
            var values = line.Split(';');

            foreach (string covariate in values) {
                float convertedCovariate = System.Convert.ToSingle(covariate);
                covariatesToUse.Add(convertedCovariate);
            }
        }

        AssignSimulatorHyperparameters(covariatesToUse);
    }

    private void AssignSimulatorHyperparameters(List<float> covariatesToAssign) {
        // Here the wanted simulator covariates / hyperparameters are assigned to the Unity synchronization simulator's internal variables.
        // NB! Remember that the order of hyperparameters (in the .CSV file compared to here) has to be manually ensured that is correct.                   POENTIAL SOURCE OF ERROR

        // Assigning collective / environment hyperparameters:

        collectiveSize = (int)covariatesToAssign[0];

        recreatingNymoenResults = System.Convert.ToBoolean(covariatesToAssign[1]); // '0' means no, so then we're using a dynamical oscillatorperiod-based t_ref and not a constant 50ms t_ref. 
        t_ref_perc_of_period = covariatesToAssign[2]; // '[0.0,1.0]' Percentage of how much of its period an oscillator should be inactive after firing a ``fire'' signal.
        
        minMaxInitialFreqs = new Vector2(covariatesToAssign[3], covariatesToAssign[4]); // Lower and upper boundary for oscillator frequency initializations.

        k = (int)covariatesToAssign[5]; // How many times the agents have to ``beat evenly'' (has to be defined and explained clearer) before (amongst some few other requirements having to be fulfilled) deeming the musical robot collective 'harmonically synchronized.'
        t_f = covariatesToAssign[6]; // The duration of the short time windows the robot collective are allowed to fire within at a time.
        TQDefiner = (TQDefinerEnum)(int)covariatesToAssign[7]; // '0' means Median, '1' means Average. Whether we use averages or medians in the calculation of new t_q values.

        useDeterministicSeed = System.Convert.ToBoolean(covariatesToAssign[8]); // '0' means randomize simulation run (in terms of random seeds), '1' means use given seed deterministically.
        randomSeed = (int)covariatesToAssign[9]; // Seed given if we want to use it deterministically to try and recreate a specific simulation run.

        simulationMode = (simulationModesEnum)(int)covariatesToAssign[10]; // '0' means 'Experiment' and '1' 'Analysis.'
        dataSavingFrequency = covariatesToAssign[11]; // Sampling rate of plotting data given in Hz.

        runDurationLimit = covariatesToAssign[12]; // Maximum allowed simulation time (sim s) for the musical robot collective to have achieved harmonic synchrony before it is deemed as a failure.

        allowRobotsToStruggleForPeriods = (int)covariatesToAssign[13]; // For how long robots who never reach phase climax should be left hanging without being frequency-doubled.

        // runHeterogenousRobots = System.Convert.ToBoolean(covariatesToAssign[13KOMMA5]); // '0' means 'no', '1' means 'yes'. Whether we want to run an experiment with heterogenous robots or not.


        // Assigning individual / robot hyperparameters (CAN AT THE MOMENT ONLY BE HOMOGENOUS):

        wantedAlpha = covariatesToAssign[14]; // Homogenous phase coupling constant.
        wantedPhaseAdjustmentMethod = (phaseSyncEnum)(int)covariatesToAssign[15]; // Homogenous phase adjustment method wanting to be used.

        wantedBeta = covariatesToAssign[16]; // Homogenous frequency coupling constant.
        wantedFrequencyAdjustmentMethod = (frequencySyncEnum)(int)covariatesToAssign[17]; // Homogenous frequency adjustment method wanting to be used.
        wantedM = (int)covariatesToAssign[18]; // Homogenous error memory length (length of the error buffer list that captures how much out of synch the robot were during the last m fire events).
    }

    private void ScaleGroundAccordingToSpawnRadius() {
        float spawnDiameter = 2.0f * spawnRadius;
        GameObject.Find("Ground").transform.localScale = new Vector3(spawnDiameter, spawnDiameter, spawnDiameter);
    }

    private void GetCameraIntoDecentFOVPosition() {
        float t = GetParameterizedT(collectiveSize);

        GameObject.Find("Main Camera").transform.position = GetParameterizedR(t);
    }

    private float GetParameterizedT(int collectiveSize) {
        float collectiveSizeFloat = System.Convert.ToSingle(collectiveSize);
        return (collectiveSizeFloat-3.0f)/27.0f;
    }

    private Vector3 GetParameterizedR(float t) {
        float x = 11.02f + 22.28f * t;
        float y = 9.32f + 5.91f * t;
        float z = 16.45f + 21.15f * t;

        return new Vector3(x, y, z);
    }

    private void InitializeAgentsFiredMatrix() {
        // Initializing agents_fired_matrix so that it isn't an empty nested list, but one one can index and access.

        for (int i = 0; i < spawnedSquiggleScripts.Count; i++) {
            agents_fired_matrix.Add(new List<float>(100));
        }
    }

    private void UpdateAgentFiredMatrix() {
        for (int i = 0; i < spawnedSquiggleScripts.Count; i++) { // for alle agentId'er
            if (agentWithAgentIDsJustFired.Contains(i + 1)) agents_fired_matrix[i].Add(1f); // adding a positive/high digital binary signal signalling agent with agId just fired.
            else agents_fired_matrix[i].Add(0f); // adding a negative/low digital binary signal signalling agent with agId did not just fire.
        }

        agentWithAgentIDsJustFired.Clear(); // clearing out the "nodes-that-just-fired"-list.
    }




    // '.CSV -CREATING & -UPDATING & -SAVING':

    private List<string> GetAgentHeader() {
        // Creating a .CSV-header consisting of the agents's IDs

        List<string> agentIDHeader = new List<string>();
        foreach (SquiggleScript squiggScr in spawnedSquiggleScripts) {
            agentIDHeader.Add("agent" + squiggScr.GetAgentID().ToString());
        }

        return agentIDHeader;
    }

    private void CreateSynchDatasetCSV() {
        // Creating a .CSV - file for the MSc Synchrony - dataset, which are to contain the measurements with their covariates.
        List<string> performanceAndCovariatesHeader = new List<string>(); // The covariates we want to record the performance-measure/outcome/response-variable for
        performanceAndCovariatesHeader.Add("SIMTIME");
        performanceAndCovariatesHeader.Add("SUCCESS");    // Binary covariate (no=0 or yes=1)
        performanceAndCovariatesHeader.Add("COLLSIZE");
        if (recreatingNymoenResults) performanceAndCovariatesHeader.Add("TREF");
        else performanceAndCovariatesHeader.Add("TREFPERC");
        performanceAndCovariatesHeader.Add("MINFREQ");
        performanceAndCovariatesHeader.Add("MAXFREQ");
        performanceAndCovariatesHeader.Add("K");
        performanceAndCovariatesHeader.Add("T_F");
        performanceAndCovariatesHeader.Add("TQDEFINER");
        performanceAndCovariatesHeader.Add("ALPHA");
        performanceAndCovariatesHeader.Add("PHASEADJ");
        performanceAndCovariatesHeader.Add("BETA");
        performanceAndCovariatesHeader.Add("FREQADJ");
        performanceAndCovariatesHeader.Add("M");
        performanceAndCovariatesHeader.Add("RANDOMSEED");
        CreateCSVWithStringHeader(datasetPath, performanceAndCovariatesHeader);
    }

    private void SaveDatapointToSynchDatasetCSV(float runDuration) {
        // Saving a datapoint to a .CSV-dataset containing data: Measurements Simulation time (s) (perf.-measure) and Simulation success, as well as Simulation hyper-parameters/covariates/explanators/predictors.

        // Automatically creating a Synchrony Dataset-.CSV if it does not exist.
        if (!File.Exists(datasetPath)) CreateSynchDatasetCSV();

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

        if (recreatingNymoenResults) {
            float TREF = System.Convert.ToSingle(spawnedSquiggleScripts[0].GetTRef());
            performanceAndCovariateValues.Add(TREF);
        } else {
            float TREFPERC = System.Convert.ToSingle(t_ref_perc_of_period);
            performanceAndCovariateValues.Add(TREFPERC);
        }
        

        float MINFREQ = System.Convert.ToSingle(minMaxInitialFreqs.x);
        performanceAndCovariateValues.Add(MINFREQ);

        float MAXFREQ = System.Convert.ToSingle(minMaxInitialFreqs.y);
        performanceAndCovariateValues.Add(MAXFREQ);

        float K = System.Convert.ToSingle(k);
        performanceAndCovariateValues.Add(K);

        float T_F = t_f;
        performanceAndCovariateValues.Add(T_F);

        float TQDEFINER = System.Convert.ToSingle((int)TQDefiner);
        performanceAndCovariateValues.Add(TQDEFINER);


        // Adding Individual/Agent Hyper-parameters/Covariates (NB! Due to current design of datapoint-saving, these have to be equal for all agents) that I know before simulating:

        float ALPHA = System.Convert.ToSingle(spawnedSquiggleScripts[0].alpha); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE (due to [0])
        performanceAndCovariateValues.Add(ALPHA);

        float PHASEADJ = System.Convert.ToSingle((int)spawnedSquiggleScripts[0].phaseAdjustment); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE (due to [0])
        performanceAndCovariateValues.Add(PHASEADJ);

        float BETA = System.Convert.ToSingle(spawnedSquiggleScripts[0].beta); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE (due to [0])
        performanceAndCovariateValues.Add(BETA);
         
        float FREQADJ = System.Convert.ToSingle((int)spawnedSquiggleScripts[0].frequencyAdjustment); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE (due to [0])
        performanceAndCovariateValues.Add(FREQADJ);

        float M = System.Convert.ToSingle(spawnedSquiggleScripts[0].m); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE (due to [0])
        performanceAndCovariateValues.Add(M);

        // Remember: this is not a "real" covariate.
        float RANDOMSEED = System.Convert.ToSingle(randomSeed);
        performanceAndCovariateValues.Add(RANDOMSEED);


        // POSSIBLE TODOs:
        // - Make a prettier Inspector for grouping relevant variables with each other (cf. Unity bookmark in Brave).
        // - Make a separate 'simulation-run dataset' with rows for each agent and columns for each Individual/Agent Hyper-parameters (so that the NB! on the reMarkable hyper-parameter note is taken care of) — if it is needed or wanted or useful.


        // Saving one datapoint, a.k.a. writing one .CSV-row (Measurements, Covariates) to the .CSV-file at the datasetPath:
        FloatUpdateCSV(datasetPath, performanceAndCovariateValues);

        if ((int)simulationMode == 1) SaveAllLoggedValuesToCSVs();
    }

    private void SavePlottingDataIfOnRightFrame() {
        if ((Mathf.RoundToInt(Time.fixedTime / Time.fixedDeltaTime) % dataSavingFrequencyY) == 0) { // Executing if the frame matches up with the wanted data-saving-frequency
            // For the synchrony evolution plot.
            towards_k_counters.Add(System.Convert.ToSingle(towards_k_counter));
            // For the synchrony detection plot.
            t_f_is_nows.Add(System.Convert.ToSingle(t_f_is_now));
            UpdateAgentFiredMatrix();
        }
    }

    private void SaveAllLoggedValuesToCSVs() {
        // Saving all the logged values (like phases and frequencies) throughout the simulation-run in the genious way Tommy suggested:
        SavePhasesToCSV();
        SaveFrequenciesToCSV();
        SaveTowardsKCountersToCSV();
        SavePerformanceMeasurePlotMaterialsToCSV();
    }
    private void SavePhasesToCSV() {
        List<List<float>> allPhaseColumns = new List<List<float>>();
        foreach (SquiggleScript squiggScr in spawnedSquiggleScripts) {
            allPhaseColumns.Add(squiggScr.GetPhases());
        }
        LoggedNestedValuesToCSV(phasesFolderPath + "phases_over_time_atSimRun" + atSimRun + ".csv", GetAgentHeader(), allPhaseColumns);
    }
    private void SaveFrequenciesToCSV() {
        List<List<float>> allFrequencyColumns = new List<List<float>>();
        foreach (SquiggleScript squiggScr in spawnedSquiggleScripts) {
            allFrequencyColumns.Add(squiggScr.GetFrequencies());
        }
        LoggedNestedValuesToCSV(frequenciesFolderPath + "freqs_over_time_atSimRun" + atSimRun + ".csv", GetAgentHeader(), allFrequencyColumns);
    }
    private void SavePerformanceMeasurePlotMaterialsToCSV() {
        List<string> perfMeasureHeader = GetAgentHeader();
        perfMeasureHeader.Insert(0, "t_f_is_now");

        List<List<float>> t_f_and_agents_fired_matrix = new List<List<float>>(agents_fired_matrix);
        t_f_and_agents_fired_matrix.Insert(0, t_f_is_nows);

        LoggedNestedValuesToCSV(nodeFiringPlotMaterialFolderPath + "node_firing_data_atSimRun" + atSimRun + ".csv", perfMeasureHeader, t_f_and_agents_fired_matrix);
    }
    private void SaveTowardsKCountersToCSV() {
        // Adding one last towards_k_counter-value right before saving/termination since it often gets left out (the last step to e.g. k=8).
        towards_k_counters.Add(towards_k_counter);

        LoggedColumnToCSV(synchronyEvolutionsFolderPath + "synch_evolution_data_atSimRun" + atSimRun + ".csv", "towards_k_counter", towards_k_counters);
    }
}

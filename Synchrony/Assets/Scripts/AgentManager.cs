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

	// Node-firing plot variables:
    private List<int> agentWithAgentIDsJustFired = new List<int>(); // Initializing (used for creation of node-firing-plot) a list for all the agents with agent-ids that just fired. 
    //private List<float> synchedFiringTimeErrorSpreads = new List<float>();
    //private float synchedFiringTimeErrorSpread = 0.0101f; // the time from the first firing to the last when agents are synched(but have some small errors due to calculations)
	
	
	// OLD ATTEMPT:
			// Performance-measure (Synchronization-time) variables
			/* private bool hSynchConditionsAreMet = false; // A boolean that should be true no sooner than when all the conditions for the achievement of Harmonic Synchrony are fulfilled.
			private bool t_f_is_now = false; // A short "up-time" when all nodes are allowed to fire during. The duration itself is constant, but when in simulation-time it will be "up", depends largely on the t_q-variable.
			private float oldest_t_q_definer_time = 0f; // The oldest defining firing-event-time to be used to definine a new t_q-value.
			private bool define_t_q_at_next_firings = false; // A boolean signal/flag that signalizes that the next fire event will be defining (together with the "now set" or "old" oldest_t_q_definer_time) in terms of a new t_q-value/-window.
			private float t_q = 0f; // The varying (but hopefully eventually converging) time-window/-duration lasting for how long no fire-events can be heard after the last time_fire-window ended.
			private int number_in_a_row_of_no_firings_within_t_q = 0;
			private bool firstFiringTriggered;
			private bool[] agentiHasFiredAtLeastOnce;
			private float firstDefineTime;
			private int timesHeardFiringsInARowShortly;
			private List<float> definingFiringsTimes = new List<float>(); */
	
	
	// Performance-/Synchronization-measure variables:
    // Defined hyperparameters and constant throughout simulation-run:
	public float t_f_duration = 0.08f; // the duration of the time-window the nodes are allowed to fire during
    public int k = 8; // the number of times in a row the 't_q'-/t_q-window (where no fire-events can be heard) must be equally long
	
	// Helping variables implementing the synch-measure and enabling the wanted functionality for testing/evaluating a synch-simulation run:
	private bool t_f_is_now = false; // A short "up-time" when all nodes are allowed to fire during. The duration itself is constant, but when in simulation-time it will be "up", depends largely on the t_q-variable.
	private float t_q = 0f;
	
	private bool first_firing_is_perceived = false;
	
	private float reset_t_q_flag_raiser = 0f;
	private bool reset_t_q_flag = false;
	
	private int medians_acquired = 0;
	
	private List<float> early_reset_defining_times = new List<float>();
	private bool first_early_reset_defining_time_added = false;
	private float early_median_time = 0f;
	
	private List<float> late_reset_defining_times = new List<float>();
	private bool first_late_reset_defining_time_added = false;
	
	private bool[] agentiHasFiredAtLeastOnce;
	private bool hSynchConditionsAreMet = false; // A boolean that should be true no sooner than when all the conditions for the achievement of Harmonic Synchrony are fulfilled.
	private int equal_t_q_streak_counter = 0; // to become equal to 'k'.
	

    // ------- END OF Variable Declarations -------





    // ------- START OF MonoBehaviour Functions/Methods -------

    void Start() {
        // Speeding up or down the simulation if that is wanted
        Time.timeScale = adjustedTimeScale;

		// Initializing an array with as many boolean values as there are agents, which are to be put "high"/true if agent with agentID fired at least once during the simulation-run.
		agentiHasFiredAtLeastOnce = new bool[collectiveSize];

		// Spawning all agents randomly (but pretty naively as of now)
		SpawnAgents();

        // Creating all .CSV-files I want to update throughout the simulation 
        CreateAllCSVFiles();
    }

    void Update() {
        // POTENTIAL PERFORMANCE-GAIN:
            // - Make the simulation quit immediately the even-beat-counter hits k, instead of at the next Update()-call.
            // - Call EndSimulationIfHSynchConditionsAreReached() in FixedUpdate() instead of Update().

        CheckHSynchConditions();

        EndSimulationRunIfTerminationCriteriaAreReached();
    }

    void FixedUpdate() {
        // Updating all my CSV-files with a constant interval (here at the rate at which FixedUpdate() is called, hence at 50Hz)
        UpdateAllCSVFiles();
    }

    // ------- END OF MonoBehaviour Functions/Methods -------





    // ------- START OF Performance-measure Termination-evaluation Functions/Methods -------
	
	
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
	
	private void SaveDatapointToDataset(float runDuration) {
        // Saving the Performance Measure (HSYNCHTIME or Simulation-time in secs) and the current Simulator-covariates/-hyperparameters (currently assumed to be manually written in the existing .CSV already).

        // Initializing empty float-List soon-to-contain the performance measure and the covariates/explanators
        List<float> performanceAndCovariateValues = new List<float>();

        // Adding the performance measure HSYNCHTIME
        float HSYNCHTIME = runDuration;
        performanceAndCovariateValues.Add(HSYNCHTIME);

        // Adding Covariate 1
        float SUCCESS = System.Convert.ToSingle(hSynchConditionsAreMet);
        performanceAndCovariateValues.Add(SUCCESS);
        // (JUST FOR DEBUGGING) Telling the programmer in the console whether the simulation run was a success or not:
        if (hSynchConditionsAreMet) Debug.Log("Congratulations! Harmonic synchrony in your musical multi-robot collective at simRun " + atSimRun + " was achieved in " + runDuration + " seconds!");
        else                        Debug.Log("That's too bad... Harmonic synchrony, according to the performance-measure defined by K. Nymoen et al., was not achieved at simRun " + atSimRun + " within the run time-limit of " + runDurationLimit + " seconds..");

        // Adding Covariate 2
        float COLLSIZE = collectiveSize;
        performanceAndCovariateValues.Add(COLLSIZE);

        // Adding Covariate 3
        float PHASEADJ = System.Convert.ToSingle(spawnedAgentScripts[0].useNymoenPhaseAdj); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE
        performanceAndCovariateValues.Add(PHASEADJ);

        // Adding Covariate 4
        float FREQADJ = System.Convert.ToSingle(spawnedAgentScripts[0].useNymoenFreqAdj); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE
        performanceAndCovariateValues.Add(FREQADJ);

        // Adding Covariate 5
        float ALPHA = System.Convert.ToSingle(spawnedAgentScripts[0].alpha); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE
        performanceAndCovariateValues.Add(ALPHA);

        // Adding Covariate 6
        float BETA = System.Convert.ToSingle(spawnedAgentScripts[0].beta); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE
        performanceAndCovariateValues.Add(BETA);

        // Adding Covariate 7
        float TREFPERC = System.Convert.ToSingle(spawnedAgentScripts[0].t_ref_perc_of_period); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE
        performanceAndCovariateValues.Add(TREFPERC);

        // Adding Covariate 8
        float M = System.Convert.ToSingle(spawnedAgentScripts[0].m); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE
        performanceAndCovariateValues.Add(M);

        // Adding Covariate 9
        float MINFREQ = System.Convert.ToSingle(spawnedAgentScripts[0].minMaxInitialFreqs.x); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE
        performanceAndCovariateValues.Add(MINFREQ);

        // Adding Covariate 10
        float MAXFREQ = System.Convert.ToSingle(spawnedAgentScripts[0].minMaxInitialFreqs.y); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE
        performanceAndCovariateValues.Add(MAXFREQ);

        // Adding (Environment-) Covariate 11
        float ADJTIMESCALE = adjustedTimeScale;
        performanceAndCovariateValues.Add(ADJTIMESCALE);

        // POSSIBLE TODO:
        // Splitting Covariates into collective-covariates and individual-covariates.

        // Add Covariate N

        // Saving one datapoint, a.k.a. writing one .CSV-row (Performance-measure, Covariates) to the .CSV-file at the datasetPath
        FloatUpdateCSV(datasetPath, performanceAndCovariateValues);
    }
	
	private void ResetSimulationVariables() {
		// Here we are resetting/reassigning the Performance-/Synchronization-measure variables to their default-values that they are set up to have before starting a simulation run. This way, we are "cleaning up" the current/previous simulation-run and setting up for another new simulation-run within the same Unity "Game-play-run".
		
		t_f_is_now = false;
		t_q = 0f;
		CancelInvoke(); // All t_q-/t_f-Invokes are ended/executed.
		
		first_firing_is_perceived = false;
		
		reset_t_q_flag_raiser = 0f;
		// reset_t_q_flag = false; 													BURDE IKKE VÆRE NØDVENDIG.
		
		// medians_acquired = 0; 													BURDE IKKE VÆRE NØDVENDIG.
		
		// early_reset_defining_times.Clear(); 										BURDE IKKE VÆRE NØDVENDIG.
		// first_early_reset_defining_time_added = false; 							BURDE IKKE VÆRE NØDVENDIG.
		early_median_time = 0f;
		
		// late_reset_defining_times.Clear(); 										BURDE IKKE VÆRE NØDVENDIG.
		//first_late_reset_defining_time_added = false;								BURDE IKKE VÆRE NØDVENDIG.
		
		agentiHasFiredAtLeastOnce = new bool[collectiveSize];
		hSynchConditionsAreMet = false;
		equal_t_q_streak_counter = 0;
	}
	
	
	
	// CONTAINING THE REAL PSEUDO-LOGIC:
	public void IJustHeardAFireEvent(int firingAgentId) {
		// When this method gets called, it means the AgentManager has picked upon a Dr. Squiggle's method-call — meaning it "heard" a Dr. Squiggle's ``fire''-signal.
		
		// For saving of Node-firing-plot-material:
		agentWithAgentIDsJustFired.Add(firingAgentId);
		
		// For enforcing 'Condition 3' about all agents having fired at least once during the simulation-run/evaluation:
		agentiHasFiredAtLeastOnce[firingAgentId - 1] = true; // Flagging that agent corresponding with AgentID has fired at least once during the simulation.
		
		
			// THE REAL PSEUDO-LOGIC:
		if (!ItIsLegalToFireNow() && !reset_t_q_flag) { // the 3)-"reset t_q"-process is started if not started already
																																		// BARE FOR TESTING
			Debug.Log("Illegal firing! Even-beat-counter towards k=" + k + ":    " + equal_t_q_streak_counter);

			StartTheResetTQProcess();
		}
		
		if (!FirstFiringHasBeenPerceived()) { // the 1)-step is performed
			PerformTheFirstSynchMeasureStep();
		}
		
		if (EarlyDefiningMedianIsInTheMaking()) { // estimating the first "holdepunkt", median_1, for defining the new t_q-estimate (given by B-sketch)
			AddEarlyResetDefiningTime(Time.timeSinceLevelLoad);
			
			if (!FirstEarlyResetDefiningTimeIsAdded()) {
				TriggerDefineEarlyMedian();
			}
		}
		
		if (LateDefiningMedianIsInTheMaking()) { // estimating the second "holdepunkt", median_2, for defining the new t_q-estimate (given by B-sketch)
			AddLateResetDefiningTime(Time.timeSinceLevelLoad);
			
			if (!FirstLateResetDefiningTimeIsAdded()) {
				TriggerDefineNewTQ();
			}
		}
	}
	
	
	private void TriggerDefineNewTQ() {
		first_late_reset_defining_time_added = true;
		t_f_is_now = true; // Setting off a t_f_is_now-period. 					 	// KANSKJE PLASSER NEDERST I FUNKSJONEN
		Invoke("DefineNewTQ", t_f_duration/2f);
	}
	
	private void DefineNewTQ() {
		float new_t_q_estimate = ListMedian(late_reset_defining_times) - early_median_time - t_f_duration;
		medians_acquired = 0; // Resetting median-counter despite its true-value of 2 (for the next possible TQ-resetting).
		// the 4)-"t_q-reset"-processclosing is executed:

																																					// BARE FOR TESTING
		//Debug.Log("late_median_time: " + ListMedian(late_reset_defining_times) + ", early_median_time: " + early_median_time + ". new_t_q_estimate = late_median_time - early_median_time - t_f_duration: " + new_t_q_estimate + ".");
		//Debug.Log("Old t_q-window was: " + t_q);

		t_q = new_t_q_estimate;

																																					// BARE FOR TESTING
		//Debug.Log("New t_q-window is now: " + t_q);


		RestartTFTQWindows();
		// Variabel-opprydning:
		reset_t_q_flag = false;
		late_reset_defining_times.Clear();
		first_late_reset_defining_time_added = false;
	}
	
	private void RestartTFTQWindows() {
		CancelInvoke(); // Cancelling all currently ongoing Invoke-calls.
		TriggerTQPeriod();
	}
	
	private void TriggerTQPeriod() {
		t_f_is_now = false;
		Invoke("TriggerTFPeriod", t_q);
	}
	
	private void TriggerTFPeriod() {
		t_f_is_now = true;
																																				// BARE FOR TESTING
		if (reset_t_q_flag) Debug.Log("Hæ? Er ikke dette umulig? Jeg tror du kanskje har feil/hull i logikken din...");
		equal_t_q_streak_counter ++;
		Debug.Log("Even-beat-counter towards k=" + k + ":    " + equal_t_q_streak_counter);
		Invoke("TriggerTQPeriod", t_f_duration);
	}

	private void TriggerDefineEarlyMedian() {
		first_early_reset_defining_time_added = true;
		t_f_is_now = true; 															// KANSKJE PLASSER NEDERST I FUNKSJONEN
		Invoke("DefineEarlyMedian", t_f_duration);
	}
	
	private void DefineEarlyMedian() {
		early_median_time = ListMedian(early_reset_defining_times);
		medians_acquired ++;
		TFLower();
		// Variabel-opprydning:
		early_reset_defining_times.Clear();
		first_early_reset_defining_time_added = false;
	}
	
	
	private bool FirstLateResetDefiningTimeIsAdded() {
		return first_late_reset_defining_time_added;
	}
	
	private bool FirstEarlyResetDefiningTimeIsAdded() {
		return first_early_reset_defining_time_added;
	}
	
	
	private void AddLateResetDefiningTime(float simulationTimeInSeconds) {
		late_reset_defining_times.Add(simulationTimeInSeconds);
																										// BARE FOR TESTING
		//Debug.Log("Late Reset Defining Times: ");
		//DebugLogMyFloatList(late_reset_defining_times);
	}
	
	private void AddEarlyResetDefiningTime(float simulationTimeInSeconds) {
		early_reset_defining_times.Add(simulationTimeInSeconds);
																										// BARE FOR TESTING
		//Debug.Log("Early Reset Defining Times: ");
		//DebugLogMyFloatList(early_reset_defining_times);
	}
	
	
	private bool LateDefiningMedianIsInTheMaking() {
		// Returning true if the 3)-"reset t_q"-process is started, the candidate late median definer is far enough away from the early median time (to avoid negative numbers), the early median has been found but the late and second median still hasn't been used to define the new t_q-estimate through the DefineNewTQ()-function. Returns false otherwise.
		
		return (reset_t_q_flag && (Time.timeSinceLevelLoad > (early_median_time + t_f_duration)) && (medians_acquired == 1));
	}
	
	private bool EarlyDefiningMedianIsInTheMaking() {
		// Returning true if the 3)-"reset t_q"-process is started, the candidate early median definer is far enough away from the reset-triggering firing-time (to avoid negative numbers), and the early and first median still haven't been acquired through the DefineEarlyMedian()-function. Returns false otherwise.
		
		return (reset_t_q_flag && (Time.timeSinceLevelLoad > (reset_t_q_flag_raiser + t_f_duration)) && (medians_acquired == 0));
	}
	
	
	private void PerformTheFirstSynchMeasureStep() {
		first_firing_is_perceived = true;
		TriggerFirstTFRise();
	}
	
	private void TriggerFirstTFRise() {
		t_f_is_now = true;
		Invoke("TFLower", t_f_duration);
	}
	
	private void TFLower() {
		t_f_is_now = false;
	}
	
	
	private bool FirstFiringHasBeenPerceived() {
		return first_firing_is_perceived;
	}
	
    
	private void StartTheResetTQProcess() {
        CancelInvoke(); // Cancelling all currently ongoing Invoke-calls. 	PASS PÅ SÅ DU IKKE CANCELLER DefineEarlyMedian- ELLER DefineNewTQ-Invokesa MED ET UHELL HER.
        reset_t_q_flag_raiser 		= Time.timeSinceLevelLoad;
		reset_t_q_flag 				= true;
		equal_t_q_streak_counter 	= 0;
	}
	
	
	private bool ItIsLegalToFireNow() {
		return t_f_is_now;
	}
	
	
		// OLD ATTEMPT:
			/* private void CheckHSynchConditions() {
				// Checks 'Condition 2' and 'Condition 3' respectively: namely, '2': if the agents have beat evenly (with a constant t_q-value) k times in a row, as well as if all nodes have fired at least once throughout the evaluation-/testing-period (simulation-run).
				
				// The whole synchronization-measure implemented is assumed to facilitate and ensure the keeping of 'Condition 1', namely that only legal firing is happening (during a very short and white time-window t_f), not illegal firing (during the longer and gray t_q-window). Thus this 'Condition 1' is not explicitly checked here, but assumed.

				// If all these conditions are true, the function is thus raising the (Congratulations-vibed) hSynchConditionsAreMet-signal/-flag high, for having achieved harmonic synchrony. If all conditions are true and the hSynchConditionsAreMet-flag is raised: Rejoice! Synchrony is obtained!

				bool agentsHaveBeatenInAnEvenRhythmKTimes = number_in_a_row_of_no_firings_within_t_q == k;
				bool notAllHaveFiredOnceYet = agentiHasFiredAtLeastOnce.AsQueryable().Any(val => val == false);

				if (agentsHaveBeatenInAnEvenRhythmKTimes && !notAllHaveFiredOnceYet) hSynchConditionsAreMet = true;
			}

			private void EndSimulationRunIfTerminationCriteriaAreReached() {
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

				agentWithAgentIDsJustFired.Add(firingAgentId); // For saving of Node-firing-plot-material.

				//if (t_f_is_now) t_f_firing_times.Add(Time.timeSinceLevelLoad); // In order to calculate and update the FiringTimesErrorSpread.

				agentiHasFiredAtLeastOnce[firingAgentId - 1] = true; // Flagging that corresponding agent with AgentID has fired at least once during the simulation.

				AdjustHsynchCriteriasIfNeeded();
			}
			
			private void AdjustHsynchCriteriasIfNeeded() {
				// Calling for the initialization (technically resetting) of the t_q-/t_q-window
				if (oldest_t_q_definer_time == 0f) {                                                    // BRA LOGIKK? HVA OM SIMULERINGEN RESETTES?
					if (!firstFiringTriggered) {
						TriggerFirstFiringTime();
						firstFiringTriggered = true;
					}

					oldest_t_q_definer_time = Time.timeSinceLevelLoad;

					define_t_q_at_next_firings = true;
				}
				// Resetting the t_q-/t_q-window if this is not the first observed Fire-event and the reset is flagged and wanted
				else if (define_t_q_at_next_firings) {
					//float new_t_q = Time.timeSinceLevelLoad - oldest_t_q_definer_time - 1.2f*t_f; // Optionally subtract 1.25*t_f instead for Nymoen-Phase-Adj. of just t_f.  - synchedFiringTimeErrorSpread / 2f

					firstDefineTime = timesHeardFiringsInARowShortly == 0 ? Time.timeSinceLevelLoad : firstDefineTime;
					timesHeardFiringsInARowShortly ++;
					definingFiringsTimes.Add(Time.timeSinceLevelLoad);

					if (!IAmInADefiningFiringsPeriod(Time.timeSinceLevelLoad)) {
						RestartTheQuietTimewindow();
					}
				}
				// Calling for a reset for the t_q-/t_q-window since observed fire-event was received during !t_f a.k.a. t_q, and we did not want to reset t_q this time, nor calling for the initialization of t_q
				else if (!t_f_is_now) {
					number_in_a_row_of_no_firings_within_t_q = 0; // resetting the streak, because synch bad
					Debug.Log("Even-beat-counter towards k=" + k + ":    " + number_in_a_row_of_no_firings_within_t_q);

					oldest_t_q_definer_time = Time.timeSinceLevelLoad;

					define_t_q_at_next_firings = true;
				}
			}

			private void RestartTheQuietTimewindow() {
				float t_firing_median = ListMedian(definingFiringsTimes);

				float limit = 0.9f;

				if ((t_firing_median-oldest_t_q_definer_time) > limit) { //                  HUSK Å FJERNE ELLER ENDRE DENNE FOR FREQ.-ADJ.
					Debug.Log("t_firing_median: " + t_firing_median + ", oldest_t_q_definer_time: " + oldest_t_q_definer_time + ". Subtracted: " + (t_firing_median - oldest_t_q_definer_time) + ", which has to be > " + limit + ".");
					Debug.Log("Old QuietTimewindow was: " + t_q);

					t_q = t_firing_median - oldest_t_q_definer_time - t_f;

					Debug.Log("New QuietTimewindow is now: " + t_q);

					timesHeardFiringsInARowShortly = 0;
					definingFiringsTimes.Clear();
					define_t_q_at_next_firings = false;
					RestartInvokeCycle();
				}

				

				// OLD ATTEMPT:
					//if ((t_firing_median - oldest_t_q_definer_time) > 0.5f) { // weird condition because I don't want too close fire-events to define the t_q-window (i.e. not too small fire-time-differences) + synchedFiringTimeErrorSpread / 2f
					//    t_q = new_t_q;
					//    define_t_q_at_next_firings = false
					//        RestartInvokeCycle();
					//}
			}

			private bool IAmInADefiningFiringsPeriod(float firingTime) {
				return (firingTime - firstDefineTime) < t_f ? true : false;
			}

			private void SaveDatapointToDataset(float runDuration) {
				// Saving the Performance Measure (HSYNCHTIME) and the current Simulator-covariates/-hyperparameters (assumed to be manually written in the existing .CSV already).

				// Initializing empty float-List soon-to-contain the performance measure and the covariates/explanators
				List<float> performanceAndCovariateValues = new List<float>();

				// Adding the performance measure HSYNCHTIME
				float HSYNCHTIME = runDuration;
				performanceAndCovariateValues.Add(HSYNCHTIME);

				// Adding Covariate 1
				float SUCCESS = System.Convert.ToSingle(hSynchConditionsAreMet);
				performanceAndCovariateValues.Add(SUCCESS);
				// Telling the programmer in the console whether the simulation run was a success or not
				if (hSynchConditionsAreMet) Debug.Log("Congratulations! Harmonic synchrony in your musical multi-robot collective at simRun " + atSimRun + " was achieved in " + runDuration + " seconds!");
				else                        Debug.Log("That's too bad... Harmonic synchrony, according to the performance-measure defined by K. Nymoen et al., was not achieved at simRun " + atSimRun + " within the run time-limit of " + runDurationLimit + " seconds..");

				// Adding Covariate 2
				float COLLSIZE = collectiveSize;
				performanceAndCovariateValues.Add(COLLSIZE);

				// Adding Covariate 3
				float PHASEADJ = System.Convert.ToSingle(spawnedAgentScripts[0].useNymoenPhaseAdj); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE
				performanceAndCovariateValues.Add(PHASEADJ);

				// Adding Covariate 4
				float FREQADJ = System.Convert.ToSingle(spawnedAgentScripts[0].useNymoenFreqAdj); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE
				performanceAndCovariateValues.Add(FREQADJ);

				// Adding Covariate 5
				float ALPHA = System.Convert.ToSingle(spawnedAgentScripts[0].alpha); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE
				performanceAndCovariateValues.Add(ALPHA);

				// Adding Covariate 6
				float BETA = System.Convert.ToSingle(spawnedAgentScripts[0].beta); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE
				performanceAndCovariateValues.Add(BETA);

				// Adding Covariate 7
				float TREFPERC = System.Convert.ToSingle(spawnedAgentScripts[0].t_ref_perc_of_period); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE
				performanceAndCovariateValues.Add(TREFPERC);

				// Adding Covariate 8
				float M = System.Convert.ToSingle(spawnedAgentScripts[0].m); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE
				performanceAndCovariateValues.Add(M);

				// Adding Covariate 9
				float MINFREQ = System.Convert.ToSingle(spawnedAgentScripts[0].minMaxInitialFreqs.x); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE
				performanceAndCovariateValues.Add(MINFREQ);

				// Adding Covariate 10
				float MAXFREQ = System.Convert.ToSingle(spawnedAgentScripts[0].minMaxInitialFreqs.y); // BUILDING ON THE ASSUMPTION THAT ALL AGENTS HAVE THE SAME VALUE
				performanceAndCovariateValues.Add(MAXFREQ);

				// Adding (Environment-) Covariate 11
				float ADJTIMESCALE = adjustedTimeScale;
				performanceAndCovariateValues.Add(ADJTIMESCALE);

				// POSSIBLE TODO:
				// Splitting Covariates into collective-covariates and individual-covariates.

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
				//Debug.Log("Triggered half firing time, and will trigger the QuietPeriod in t_f/2=" + t_f / 2f + " + synchedErrorSpread/2=" + synchedFiringTimeErrorSpread / 2f + " = " + (t_f / 2f + synchedFiringTimeErrorSpread / 2f));
				Invoke("TriggerQuietPeriod", t_f/2f); // + synchedFiringTimeErrorSpread/2f)
			}

			private void TriggerQuietPeriod() {
				t_f_is_now = false;
				UpdateSynchedFiringTimeErrorSpread();
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

			private void UpdateSynchedFiringTimeErrorSpread() { */
				//synchedFiringTimeErrorSpreads.Add(t_f_firing_times.LastOrDefault() - t_f_firing_times.FirstOrDefault());
				//Debug.Log((t_f_firing_times.LastOrDefault()-t_f_firing_times.FirstOrDefault()) + " = errSpreadLast - errSpreadFirst was added to the list.");
				//synchedFiringTimeErrorSpread = ListAverage(synchedFiringTimeErrorSpreads);
				//synchedFiringTimeErrorSpread = t_f_firing_times.LastOrDefault() - t_f_firing_times.FirstOrDefault();
				//t_f_firing_times.Clear();
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

    // ------- END OF Helping-/Utility Functions/Methods -------
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static SynchronyUtils;

public class SquiggleScript : MonoBehaviour {

    // 'VARIABLES':

    // Recorded individual-/agent-hyperparameters:

    // Phase-adjustment:
    [Tooltip("Pulse coupling constant, denoting coupling strength between nodes, deciding how much robots adjust phases after detecting a pulse from a neighbour. The larger the constant, the larger (in absolute value) the phase-update?")]
    public float alpha = 0.1f;
    public enum phaseSyncEnum {
        MirolloStrogatz,
        Nymoen
    };
    [Tooltip("Which phase-adjustment / -update / -synchronization function or method we want our robots to synchronize their phases according to.")]
    public phaseSyncEnum adj_phase = phaseSyncEnum.MirolloStrogatz;

    // Frequency-adjustment:
    [Tooltip("Frequency coupling constant, deciding how much robots adjust frequencies after detecting pulse onsets from neighbours. The larger the constant, the larger (in absolute value) the frequency-update?")]
    public float beta = 0.4f;
    public enum frequencySyncEnum { 
        None,
        Nymoen
    };
    [Tooltip("Which frequency-adjustment / -update / -synchronization function or method we want our robots to synchronize their frequencies according to. If 'None' is selected, the robots in question will have 1Hz fixed frequency.")]
    public frequencySyncEnum adj_omega = frequencySyncEnum.None;
    [Tooltip("Degree of error-memory, i.e. the length of a list of the last m error-scores. The larger the length m, the more error-scores we calculate the self-assessed synch-score s(n) based upon.")]
    public int m = 5;

    // SA scopes:
    public int k_s; // K nearest neighbours in robot's self awareness scope
    public float d_s; // Radius of self awareness scope

    // Audio:
    public AudioClip[] audioClips;


    // 'Private variables necessary to make the cogs go around':

    // For self awareness scopes:
    private List<SquiggleScript> myFireSignalSubscribers = new List<SquiggleScript>();

    // Core for (oscillator-) synchronization:
    private System.Random randGen;
    private float phase;                        // The agents's progression through its oscillator-period. A number between 0 (at the beginning of its cycle) and 1 (at the end of its cycle).
    private List<float> simRunPhases = new List<float>(); // A list for all the phases throughout a simulation-run for agent (sampled at the FixedUpdate-frequency).
    private float frequency;                    // The agents's oscillator-frequency (Hz). How fast the agent traverses through its cycle — or in other words the rate of the phase's change.
    private List<float> simRunFrequencies = new List<float>();
    private float t_ref;                        // The refractory period (s) being the period within which agents are "recovering" in, i.e. not adjusting themselves, after firing/sending a pulse.
    private List<float> inPhaseErrorBuffer = new List<float>(); // The list of m error-scores (our error-memory).
    private List<float> HBuffer = new List<float>(); // An accumulating and self-clearing list of H-values (which I calculated from the bottom up in the compiled reMarkable-note of mine).

    // Special rules (to stabilize/optimize/implement correctly synchronization):
    private bool inRefractoryPeriod = false;    // if this flag is true, neither phase- nor frequency-adjustment happens.
    private bool firedLastClimax;               // A flag which is high after having fired at the previous phase-climax (so that the agent won't fire on the next one, but the one after).
    private float timeNotClimaxed;
    float longestLegalPeriod;
    //private float unstableFrequencyPeriod = 1f / 0.5f;

    // Identity / Existential:
    private int agentID;                        // The numerical ID of the agent (from 1 to myCreator.collectiveSize).
    private AgentManager myCreator;             // A reference to the Squiggle-/Agent-Manager that spawned this agent.
    private List<SquiggleScript> otherSquiggles = new List<SquiggleScript>(); // A list of all the agent's neighbour-squiggles.

    // Visual / Color:
    private Renderer corpsOfAgentRenderer;      // A reference to the agent body Renderer (making the object visible or invisible, as well as giving access to the object's material).
    private float colorLerpUntilPhase = 0.15f;  // Duration (a % of the oscillator-period) during which the body-color of the agent is "cooling down" after having fired/sent a pulse.
    private Transform iris;                     // A reference to the iris of the agent's eye.
    private Transform pupil;                    // A reference to the pupil of the agent's eye.
    private Color bodyColor;                    // The initial/default color of the agent's body.
    private Color fireColor = Color.yellow;     // The desired fire-color of the agent's body.

    // Audible / Sound:
    private AudioSource audioSource;            // A reference to the AudioSource-component on the agent that is told to play the fire sound.
    private static float highestRobotFrequency;
    private static float smallestRobotFrequency;




    // 'MonoBehaviour':

    void Start() {
        // 'simulation setup':
        AssignHelpingVariables();

        // Fill up wanted neighbouring subscriber-lists with yourself ('this').
        List<SquiggleScript> neighboursToListenTo = FindNeighboursToListenTo();
        SubscribeToNeighboursWithinSAScope(neighboursToListenTo);

        // Setting up for a human listener being able to see the ``fire''-events from the Dr. Squiggles.
        AssignVisualVariables();

        // Setting up for a human listener being able to hear the ``fire''-events from the Dr. Squiggles.
        AssignAudioVariables();


            // 'synchronization setup':

        InitializeAgentPhase();

        InitializeAgentFrequency();

        UpdateTheRefractoryPeriod();
    }

    private void Update() { // having to do with graphics and rendering (depends on FPS e.g.) BUT A PROBLEM SINCE I DID NOT USE THIS AFTER SWITCHING TO COROUTINES WHEN THINGS SEEMED TO WORK NICELY?
        // Updating the agent-body's color so that it cools down from just having fired.
        CoolDownAgentColorIfItJustFired();
    }


    private void SubscribeToNeighboursWithinSAScope(List<SquiggleScript> neighboursToListenTo) {
        foreach (SquiggleScript neighbourRobot in neighboursToListenTo) {
            neighbourRobot.SubscribeToAgentFiresignalSubscriberList(this);
        }
    }

    private List<SquiggleScript> FindNeighboursToListenTo() {
        List<SquiggleScript> neighboursToListenTo = new List<SquiggleScript>();

        if (SAScopeIsKNearest()) {
            // Finding my k_s nearest neighbours:
            neighboursToListenTo = FindKNearestNeighbours();
        }
        else if (SAScopeIsRadial()) {
            neighboursToListenTo = FindNeighboursWithinRadiusD();
        }
        else {
            Debug.Log("Invalid 'k_s' or 'd_s' arguments given (xor logic isn't followed).");
            return null;
        }

        return neighboursToListenTo;
    }

    private List<SquiggleScript> FindNeighboursWithinRadiusD() {
        List<int> robotIDsWithinSAScope = FindNeighbourWithinRadiusDIDs();

        List<SquiggleScript> robotsWithinSAScope = ConvertSquiggIDsToActualGameObjects(robotIDsWithinSAScope);


        return robotsWithinSAScope;
    }

    private List<int> FindNeighbourWithinRadiusDIDs() {
        // Just retrieving already existing info used for the calculations (from the AgentManager):
        List<List<float>> distMatrix = myCreator.GetRobotDistances();
        List<float> fromAgentIDRowList = distMatrix[agentID - 1];

        // Filling up a dictionary with key-value (agentID-distance) pairs to be sorted and returned the agentIDs for (in the sorted order) later:
        Dictionary<int, float> agentIDDistanceDict = new Dictionary<int, float>();
        int enumAgentIDCounter = 1;
        foreach (float distFromAgentToAgent in fromAgentIDRowList) {
            agentIDDistanceDict.Add(enumAgentIDCounter++, distFromAgentToAgent);
        }

        // Converting dictionary of key-value pairs into a list of the same key-value pairs.
        List<KeyValuePair<int, float>> agentIDDistanceKeyValuePairsList = agentIDDistanceDict.ToList();

        // Sorting the list of key-value pairs based on the value (i.e. the distances).
        agentIDDistanceKeyValuePairsList.Sort((x, y) => x.Value.CompareTo(y.Value));

        // Skipping the first element since the smallest distance always is itself (JUST TO REMOVE THE 0).
        agentIDDistanceKeyValuePairsList = agentIDDistanceKeyValuePairsList.Skip(1).ToList();

        List<int> agentIDsWithinRadiusD = ExtractKeysWithValuesLessThanRadiusD(agentIDDistanceKeyValuePairsList);


        return agentIDsWithinRadiusD;
    }

    private List<int> ExtractKeysWithValuesLessThanRadiusD(List<KeyValuePair<int, float>> agentIDDistanceKeyValuePairsList) {
        List<int> agentIDsWithDistanceLessThanD = new List<int>();

        foreach (KeyValuePair<int, float> keyValuePair in agentIDDistanceKeyValuePairsList) {
            if (keyValuePair.Value <= d_s) {
                agentIDsWithDistanceLessThanD.Add(keyValuePair.Key);
            }
        }


        return agentIDsWithDistanceLessThanD;
    }



    private List<SquiggleScript> FindKNearestNeighbours() {
        // Ser igjennom rad 'agentID' (aka. den (agentID-1)'te List'en) og returnerer 

        List<int> robotIDsWithinSAScope = FindKNearestNeighbourIDs();

        List<SquiggleScript> robotsWithinSAScope = ConvertSquiggIDsToActualGameObjects(robotIDsWithinSAScope);


        return robotsWithinSAScope;
    }

    private List<SquiggleScript> ConvertSquiggIDsToActualGameObjects(List<int> robotIDsWithinSAScope) {
        List<SquiggleScript> allSpawnedSquiggles = myCreator.GetSpawnedSquiggleScripts();

        List<SquiggleScript> squiggListo = new List<SquiggleScript>();

        foreach (int robotID in robotIDsWithinSAScope) {
            squiggListo.Add(allSpawnedSquiggles[robotID-1]);
        }


        return squiggListo;
    }

    private List<int> FindKNearestNeighbourIDs() {
        // Just retrieving already existing info used for the calculations (from the AgentManager):
        List<List<float>> distMatrix = myCreator.GetRobotDistances();
        List<float> fromAgentIDRowList = distMatrix[agentID-1];

        // Filling up a dictionary with key-value (agentID-distance) pairs to be sorted and returned the agentIDs for (in the sorted order) later:
        Dictionary<int, float> agentIDDistanceDict = new Dictionary<int, float>();
        int enumAgentIDCounter = 1;
        foreach (float distFromAgentToAgent in fromAgentIDRowList) {
            agentIDDistanceDict.Add(enumAgentIDCounter++, distFromAgentToAgent);
        }

        // Converting dictionary of key-value pairs into a list of the same key-value pairs.
        List<KeyValuePair<int, float>> agentIDDistanceKeyValuePairsList = agentIDDistanceDict.ToList();

        // Sorting the list of key-value pairs based on the value (i.e. the distances).
        agentIDDistanceKeyValuePairsList.Sort((x, y) => x.Value.CompareTo(y.Value));

        // Skipping the first element since the smallest distance always is itself.
        agentIDDistanceKeyValuePairsList = agentIDDistanceKeyValuePairsList.Skip(1).ToList();

        // Extracting only the keys from the sorted list of key-value pairs.
        List<int> agentIDsList = ExtractKeysFromKeyValuePairList(agentIDDistanceKeyValuePairsList);

        // Only extracting the k_s first agentIDs (corresponding to the ones with the smallest distance from the robot with agentID='fromAgentID').
        List<int> kNearestNeighbourAgentIDs = agentIDsList.Take(k_s).ToList();


        return kNearestNeighbourAgentIDs;
    }

    private List<int> ExtractKeysFromKeyValuePairList(List<KeyValuePair<int, float>> keyValuePairsList) {
        List<int> integerKeysList = new List<int>();

        foreach (KeyValuePair<int, float> keyValuePair in keyValuePairsList) {
            integerKeysList.Add(keyValuePair.Key);
        }

        return integerKeysList;
    }


    private bool SAScopeIsKNearest() {
        return (k_s != 0 && d_s == 0.0f);
    }

    private bool SAScopeIsRadial() {
        return (k_s == 0 && d_s != 0.0f);
    }




    // 'CORE & ESSENTIAL' (incl. phase- & frequency-updating):

    public void DetectPhaseAnomaliesOrIncreaseIt() {
        if (phase == 1f) {  // Detecting (wanted) anomaly 1: the phase has climaxed (i.e. phase = 1.0).
            OnPhaseClimax();
        } else {
            timeNotClimaxed += Time.fixedDeltaTime;

            if (timeNotClimaxed >= longestLegalPeriod * myCreator.allowRobotsToStruggleForPeriods) {    // Detected (not wanted) anomaly 2: the phase didn't climax within l=5 periods, and needs a frequency-boost.
                frequency = 2f * frequency; // Giving the agent a frequency-boost since it never climaxes but only gets dragged down by others.
                timeNotClimaxed = 0.0f;
            }

            if (!(Time.timeSinceLevelLoad == 0f)) { // Don't want to update the already-initialized phase-value at Simulationtime=0.
                phase = Mathf.Clamp(phase + frequency * Time.fixedDeltaTime, 0f, 1f); // Increasing agent's phase according to its frequency = d(phi)/dt.
            }
        }
    }

    private void OnPhaseClimax() {
        FireIfWanted();

        UpdateFrequencyIfAdjustingFrequency();

        ResetPhaseClimaxValues();
    }


    private void FireIfWanted() {
        if (!firedLastClimax) { // Node should only fire every other phase-climax.
            FireNode();
            firedLastClimax = true;
        } else {
            firedLastClimax = false;
        }
    }

    private void FireNode() {
        // Notifying creator I just fired so He can create a nice-looking Node-firing plot (performance-measure plot).
        NotifyMyCreator();

        // Visually and audibly signalizing a human Unity-observer I just fired.
        NotifyTheHuman();

        // Turning off the "receiver" for t_ref seconds, not adjusting myself during that time.
        TriggerRefractoryPeriod();

        // Transmitting fire-/adjustment-signal: Calling on all the agents to adjust their phases and calculate frequency-H-values.
        NotifyTheAgents();
    }

    private void NotifyMyCreator() {
        // Notifying my creator so that it can record my fire-event for the creation of the Node-firing-plot
        myCreator.IJustHeardAFireEvent(agentID);
    }

    private void NotifyTheHuman() {
        // Showing the human "visually" that a fire-event just happened (by changing the agent-color and blinking with the agent-eyes)
        if (myCreator.useVisuals) {
            corpsOfAgentRenderer.material.color = fireColor;
            BlinkWithEyes();
        }

        // Showing the human "audially" that a fire-event just happened (by playing an audio clip)
        if (myCreator.useSound) audioSource.Play();
    }

    private void TriggerRefractoryPeriod() {
        inRefractoryPeriod = true;
        StartCoroutine(ToggleRefractoryModeOffAfter(t_ref));
    }

    IEnumerator ToggleRefractoryModeOffAfter(float inFixedDeltaTimeSeconds) {
        float timeTracker = 0.0f;
        while (timeTracker < (inFixedDeltaTimeSeconds - 0.0001f)) {
            timeTracker += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        inRefractoryPeriod = false;
    }

    private void NotifyTheAgents() {
        // Calls out to neighbouring agents whose SA scopes cover me and notifies them of its fire-event.

        foreach (SquiggleScript oscillator in myFireSignalSubscribers) { // "giving away a signal" all other robots which subscribes to me (i.e. I am within their SA scope) can hear
            oscillator.OnHeardFireEvent();
        }
    }


    private void UpdateFrequencyIfAdjustingFrequency() {
        if ((int)adj_omega == 1) {
            RFAAdjustFrequency(); // Adjusting frequency at each phase-climax, according to K. Nymoen's Freq.-Adj.-method and the Reachback Firefly Algorithm for frequency-update-contributions (not phase-update contributions).

            UpdateTheRefractoryPeriod(); // Given an updated oscillator-frequency (hence also oscillator-period), we update t_ref to be the right percentage of the new oscillator-period.
        }
    }

    private void RFAAdjustFrequency() {
        // Adjusting frequency according to the reachback firefly algorithm (RFA) with the values calculated since the start of previous oscillator-cycle until now.

            // 'calculating F_n':

        float cycleHSum = 0f;
        float y = (float)HBuffer.Count;
        foreach (float H in HBuffer) {
            cycleHSum += H;
        }

        float F_n = 0f;
        if (y != 0f) {
            F_n = beta * cycleHSum / y;
        }

        // Clearing the buffer of H(n)-values and making it ready for the next cycle.
        HBuffer.Clear();

            // 'updating frequency':

        float newFrequency = frequency * Mathf.Pow(2, F_n); // BØR MAKS VÆRE EN DOBLING AV DEN GAMLE FREKVENSEN

        if (newFrequency > highestRobotFrequency) highestRobotFrequency = newFrequency;
        else if (newFrequency < smallestRobotFrequency) smallestRobotFrequency = newFrequency;

        frequency = newFrequency;

        audioSource.clip = audioClips[GetAudioIndex(frequency)];
    }

    private void UpdateTheRefractoryPeriod() {
        float oscillator_period = 1.0f / frequency;
        if (myCreator.recreatingNymoenResults)  t_ref = 0.05f;
        else                                    t_ref = myCreator.t_ref_dyn * oscillator_period;
    }

    private void ResetPhaseClimaxValues() {
        phase = 0f;                // Resetting the agent's own phase after phase-climax.
        timeNotClimaxed = 0f;      // Stop feeling sorry for me, I can manage on my own and don't need no frequency-boost.
    }


        // 'after receiving a signal':
    public void OnHeardFireEvent() {
        // Gets called when an "audio"-fire-event-signal is "detected" by the agent.

        if (!inRefractoryPeriod) AdjustPhase(); // for immediate Phase-adjustment when hearing a fire-event
        AddNthFireEventsHToList(); // for the later Frequency-adjustment at each phase-climax
    }

    private void AdjustPhase() {
        if ((int)adj_phase == 0) { // using Mirollo-Strogatz's "standard" phase update function
            phase = Mathf.Clamp(phase * (1 + alpha), 0f, 1f);
        } else if ((int)adj_phase == 1) { // using Kristian et al.'s Bi-Directional phase sync function
            float wave = Mathf.Sin(2 * Mathf.PI * phase);
            phase = Mathf.Clamp(phase - alpha * wave * Mathf.Abs(wave), 0f, 1f);
        }
    }

    private void AddNthFireEventsHToList() {
        // Calculating and saving the n-th fire-event's corresponding H(n)-value capturing how much and in which direction the frequency of an agent should be adjusted at the agent's phase-climax, judged at the time of the agent hearing fire-event n.

        // Recording the n'th error-score, capturing whether the node itself is in synch with the node it hears the "fire"-event from or not.
        float epsilon_n;
        if (!inRefractoryPeriod) {
            epsilon_n = Mathf.Pow(Mathf.Sin(Mathf.PI * phase), 2);
        }
        else {
            epsilon_n = 0f; // Special rule Kristian et al. implemented to achieve harmonic synchrony with unequal phases and frequencies.
        }

        inPhaseErrorBuffer = ShiftFloatListRightToLeftWith(inPhaseErrorBuffer, epsilon_n);

        // Calculating the median of the inPhaseErrorBuffer, being the self-assessed synch-score s(n).
        float s_n = ListMedian(inPhaseErrorBuffer);

        // Calculating the measure capturing the amplitude and sign of the frequency-modification of the n-th "fire"-event received.
        float rho_n = -Mathf.Sin(2 * Mathf.PI * phase); // negative for phase < 1/2, and positive for phase > 1/2, and element in [-1, 1]

        float H_n = rho_n * s_n;
        HBuffer.Add(H_n); // H-value appended to the end of the list H(n).
    }




    // 'HELPING':

        // 'visual':
    private void CoolDownAgentColorIfItJustFired() {
        if (myCreator.useVisuals && firedLastClimax) CoolDownAgentBodyColor();
    }

    private void CoolDownAgentBodyColor() {
        // Sets the agent body's corps to a lerped color (ranging from the fire-color, to its standard corps-color).

        float t = Mathf.Clamp(phase / colorLerpUntilPhase, 0, 1); // a percentage going from 0 when phase=0, to 1 when phase=colorLerpUntilPhase
        t = Mathf.Sin(t * Mathf.PI * 0.5f); // Lerping like a pro
        corpsOfAgentRenderer.material.color = Color.Lerp(fireColor, bodyColor, t);
    }

    private void BlinkWithEyes() {
        Vector3 pupilScaleChange = new Vector3(0.273f, -0.1126f, 0);
        pupil.localScale += pupilScaleChange;
        Vector3 yellowEyeScaleChange = new Vector3(0, 0, -0.4465f);
        iris.localScale += yellowEyeScaleChange;

        Invoke("OpenEyes", 0.1f * (1.0f / frequency));
    }

    private void OpenEyes() {
        Vector3 pupilScaleChange = new Vector3(-0.273f, 0.1126f, 0);
        pupil.localScale += pupilScaleChange;
        Vector3 yellowEyeScaleChange = new Vector3(0, 0, 0.4465f);
        iris.localScale += yellowEyeScaleChange;
    }

        // 'variable-assigning':
    private void AssignVisualVariables() {
        corpsOfAgentRenderer = transform.GetChild(1).transform.GetChild(1).GetComponent<Renderer>();
        bodyColor = corpsOfAgentRenderer.material.color;
        iris = transform.GetChild(0).transform.GetChild(3);
        pupil = transform.GetChild(0).transform.GetChild(4);
        //tentacleColor = transform.GetChild(2).transform.GetChild(0).GetComponent<Renderer>().material.color;
    }

    private void AssignAudioVariables() {
        audioSource = GetComponent<AudioSource>();
        float utgangspunkt = 1f / (2 * (otherSquiggles.Count + 1)); // Remove the factor of 2 in the denominator to increase volume
        audioSource.volume = myCreator.volumePercentage * utgangspunkt;
    }
    
    private void AssignHelpingVariables() {
        myCreator = FindObjectOfType<AgentManager>();
        highestRobotFrequency = myCreator.minMaxInitialFreqs.y;
        smallestRobotFrequency = myCreator.minMaxInitialFreqs.x;
        longestLegalPeriod = 1.0f / smallestRobotFrequency;

        int randomSeed = myCreator.GetRandomGeneratorSeed() + agentID;
        randGen = new System.Random(randomSeed);

        // Acquiring a neighbour-list for each agent so that they can call on them when they themselves are firing
        FillUpNeighbourSquigglesList();

        // Initializing the error-memory used in Nymoen's frequency-adjustment method.
        InitializeInPhaseErrorBuffer();
    }

    private void FillUpNeighbourSquigglesList() {
        // Filling up a Unity List with all other neighbouring Squiggles-GameObjects in the scene.

        List<GameObject> allSquiggleObjs = new List<GameObject>();
        this.gameObject.tag = "temp";
        allSquiggleObjs.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        this.gameObject.tag = "Player";
        foreach (GameObject squigObj in allSquiggleObjs) { otherSquiggles.Add(squigObj.GetComponent<SquiggleScript>()); }
    }

    private void InitializeInPhaseErrorBuffer() {
        for (int i = 0; i < m; i++) inPhaseErrorBuffer.Add(1f); // BØR DENNE INITIALISERES MED 0f ISTEDENFOR 1f HVIS STØRRE FREKVENSOPPDATERINGS-BIDRAG FØRER TIL STØRRE (I ABSOLUTTVERDI) FREKVENS-OPPDATERINGER?
    }


    private void InitializeAgentPhase() {
        phase = (float)randGen.NextDouble(); // Initializing the agent's phase randomly within the range of (0.0, 1.0).
    }

    private void InitializeAgentFrequency() {
        if ((int)adj_omega == 0) { // adj_omega = None
            frequency = 1f; // Setting agent's frequency to default frequency of 1Hz.
        } else if ((int)adj_omega == 1) { // adj_omega = Nymoen
            frequency = (float)randGen.NextDouble() * (myCreator.minMaxInitialFreqs.y - myCreator.minMaxInitialFreqs.x) + myCreator.minMaxInitialFreqs.x; // Initializing frequency (Hz) in range [0.5Hz, 8Hz] was found useful by Nymoen et al.
        }

        audioSource.clip = audioClips[GetAudioIndex(frequency)];
    }

    private int GetAudioIndex(float freq) {
        float freqDistributionPercentagePosition;
        if ((highestRobotFrequency - smallestRobotFrequency) != 0f) freqDistributionPercentagePosition = (freq - smallestRobotFrequency) / (highestRobotFrequency - smallestRobotFrequency);
        else return 0;

        if (freqDistributionPercentagePosition < 0.25f) return 0;
        else if (freqDistributionPercentagePosition < 0.5f) return 1;
        else if (freqDistributionPercentagePosition < 075f) return 2;
        else return 3;
    }

        // 'get-/set-functions':

    public List<float> GetFrequencies() {
        return simRunFrequencies;
    }

    public List<float> GetPhases() {
        return simRunPhases;
    }
    
    public int GetAgentID() {
        return agentID;
    }

    public float GetTRef() {
        return t_ref;
    }

    public float GetHighestRobotFrequency() {
        return highestRobotFrequency;
    }

    public void SetAgentID(int idNumber) {
        agentID = idNumber;
    }

    public void SubscribeToAgentFiresignalSubscriberList(SquiggleScript robotWhoseSAScopeCoversMe) {
        myFireSignalSubscribers.Add(robotWhoseSAScopeCoversMe);
    }

    public void SavePlottingDataIfOnRightFrame() {
        if ((Mathf.RoundToInt(Time.fixedTime / Time.fixedDeltaTime) % myCreator.GetDataSavingParameter()) == 0) { // Executing if the frame matches up with the wanted data-saving-frequency
            // For the phase plot.
            simRunPhases.Add(phase);
            // For the frequency plot.
            simRunFrequencies.Add(frequency);
        }
    }




    // 'TESTING':

    private void UpdateFrequencyImmediately() {
        float epsilon_n;
        if (!inRefractoryPeriod) {
            epsilon_n = Mathf.Pow(Mathf.Sin(Mathf.PI * phase), 2);
        } else {
            epsilon_n = 0f;
        }

        inPhaseErrorBuffer = ShiftFloatListRightToLeftWith(inPhaseErrorBuffer, epsilon_n);

        // Calculating the median of the inPhaseErrorBuffer, being the self-assessed synch-score
        float s_n = ListMedian(inPhaseErrorBuffer);

        // Calculating the measure capturing the amplitude and sign of the frequency-modification of the n-th "fire"-event received
        float rho_n = -Mathf.Sin(2 * Mathf.PI * phase); // negative for phase < 1/2, and positive for phase > 1/2, and element in [-1, 1]

        float H_n = rho_n * s_n;

        float F_n = beta * H_n;

        float newFrequency = frequency * Mathf.Pow(2, F_n);
        frequency = newFrequency;
    }

    public void ShowYourSubscriberList() {
        Debug.Log("Hello, this is robot " + this.name + " with agentID " + agentID + ". " + myFireSignalSubscribers.Count + " neighbour robots are subscribed to my fire signals.");
        //foreach (SquiggleScript sub in myFireSignalSubscribers) {
        //    Debug.Log("Hello, this is robot " + this.name + " with agentID " + agentID + ". Robot " + sub.name + " with agentID " + sub.GetAgentID() + " is listening to my fire signals.");
        //}
    }
}
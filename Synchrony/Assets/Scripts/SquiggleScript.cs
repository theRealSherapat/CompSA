using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using static DavidsUtils;

public class SquiggleScript : MonoBehaviour {
    // ------- START OF Variable Declarations -------

    // Phase-adjustment variables:
    public float alpha = 0.05f; // pulse coupling constant, denoting coupling strength between nodes
    public bool useNymoen = true;

    // Frequency-adjustment variables:
    public float beta = 0.8f; // frequency coupling constant
    public int m = 5; // "running median-filter" length
    public Vector2 minMaxInitialFreqs = new Vector2(0.2f, 4f);
    
    // Meta environment-variables:
    public bool useSound = true;
    public bool useVisuals = false;
    public float adjustedTimeScale = 1.0f;


    // Core (oscillator) synchronization-variables:
    private float phase; // a number between 0 and 1
    private float frequency; // Hz
    private List<float> inPhaseErrorBuffer = new List<float>();
    private List<float> HBuffer = new List<float>(); // a list of H(n)-values (which I calculated from the bottom up in the compiled reMarkable-note of mine)

    // Helping-variables:
    private List<SquiggleScript> otherSquiggles = new List<SquiggleScript>();
    private AudioSource audioSource; // reference to Audio Source component on the Musical Node that is told to play the fire sound
    private int agentID;
    private bool firedLastClimax = true;

    // Visual color-variables:
    private Renderer corpsOfAgentRenderer;
    private float colorLerpUntilPhase = 0.15f;
    private Transform yellowEye;
    private Transform pupil;
    private Color bodyColor;
    private Color fireColor = Color.yellow;

    // FOR REFRACTORY PERIOD:
        //private bool inRefractoryPeriod = false; // if this is true, no (neither phase- or frequency-) adjustment should happen (?) - <--------------- DIDN'T SOLVE THE ISSUE INITIALLY
        //private float t_ref = 0.09f; // ISH LENGDEN I TID P� digitalQuickTone er 0.4s. Nymoen BRUKTE 50ms I SIN IMPLEMENTASJON.

    // ------- END OF Variable Declarations -------





    // ------- START OF MonoBehaviour Functions/Methods -------

    void Start() {
        // Setting up for a human listener being able to see the ``fire''-events from the Dr. Squiggles
        AssignVisualVariables();

        // Setting up for a human listener being able to hear the ``fire''-events from the Dr. Squiggles
        AssignAudioVariables();

        AssignHelpingVariables();

        // Initializing the agent's phase randomly
        phase = Random.Range(0.0f, 1.0f);

        // Setting up for Frequency-Adjustment
        InitializeInPhaseErrorBuffer();
        frequency = Random.Range(minMaxInitialFreqs.x, minMaxInitialFreqs.y); // Initializing frequency in range Random.Range(0.5f, 8f) was found useful by Nymoen et al.                                                                                                       (BRUK '1f;' ISTEDET HVIS DU BARE FIL FASE-JUSTERE)
    }

    void Update() {
        // Allowing phase-restarts for quick demonstrations. Stop doing this if errors occur because of it.
        if (Input.GetKeyDown(KeyCode.Space)) {
            LoadMySceneAgain();
        }
    }

    void FixedUpdate() {
        // Eventually updating/lerping the agent-body's color
        if (useVisuals) SetAgentCorpsColor();

        // TODO: MAKE THE REQUIREMENT/CONDITION STRICTER (USE Mathf.Repeat() OR SOMETHING?)
        if (phase > 1) OnPhaseClimax();

        // Increasing agent's phase according to its frequency
        phase += frequency * Time.fixedDeltaTime;
    }

    // ------- END OF MonoBehaviour Functions/Methods -------







    // ------- START OF Core-/Essential Functions/Methods -------

    private void OnPhaseClimax() {
        if (!firedLastClimax) { // this should run only every other/second time it gets checked
            FireNode(); // node firing at every other phase-climax
            firedLastClimax = true;
        } else {
            firedLastClimax = false;
        }

        RFAAdjustFrequency(); // adjust frequency at phase-climax regardless of if the node fired or not last climax         (KOMMENTER UT HVIS DU BARE FIL FASE-JUSTERE)
    }

    void FireNode() {
        NotifyTheHuman();

        // FOR REFRACTORY PERIOD:
            //Invoke("ToggleOffRefractoryMode", 0.6f);

        NotifyTheAgents();

        phase = 0; // resetting phase

        // FOR REFRACTORY PERIOD:
            //inRefractoryPeriod = true;
            //Invoke("NotifyTheAgents", t_ref); // ISH LENGDEN I TID P� digitalQuickTone er 0.4s. Nymoen BRUKTE 50ms I SIN IMPLEMENTASJON.
    }

    private void NotifyTheHuman() {
        // Showing the human "visually" that a fire-event just happened (by changing the agent-color and blinking with the agent-eyes)
        if (useVisuals) {
            corpsOfAgentRenderer.material.color = fireColor;
            BlinkWithEyes();
        }

        // Showing the human "audially" that a fire-event just happened (by playing an audio clip)
        if (useSound) audioSource.Play();
    }

    private void NotifyTheAgents() {
        // Calls out to all neighbouring agents and notifies them of its fire-event.

        foreach (SquiggleScript oscillator in otherSquiggles) { // "giving away a signal" all other nodes can hear

            // FOR REFRACTORY PERIOD:
            //oscillator.AdjustPhase(); // Invoke this after a physical-realistic-constrained time-period?

            oscillator.OnHeardFireEvent();
        }

        // FOR REFRACTORY PERIOD:
            //inRefractoryPeriod = false;
    }

    public void OnHeardFireEvent() {
        // Gets called when an "audio"-fire-event-signal is "detected" by the agent.

        AdjustPhase(); // for immediate Phase-adjustment when hearing a fire-event
        AddNthFireEventsHToList(); // for the later Frequency-adjustment at each phase-climax
    }

    void AdjustPhase() {
        if (!useNymoen) {
            phase *= (1 + alpha); // using Phase Update Function (1); "standard" Mirollo-Strogatz
        } else {
            float wave = Mathf.Sin(2 * Mathf.PI * phase);
            phase -= alpha * wave * Mathf.Abs(wave); // using Phase Update Function (2); Nymoen et al.'s Bi-Directional
        }
    }

    private void AddNthFireEventsHToList() {
        // Calculating and saving the n-th fire-event's corresponding H(n)-value capturing how much and in which direction frequency of an agent should be adjusted at phase-climax, judged at the time of hearing fire-event n.

        // Recording the n'th error-score, capturing whether the node itself is in synch with the node it hears the "fire"-event from or not
        float epsilon_n = Mathf.Pow(Mathf.Sin(Mathf.PI * phase), 2);

        // FOR REFRACTORY PERIOD:
        //if (!inRefractoryPeriod) {
        //    epsilon_n = Mathf.Pow(Mathf.Sin(Mathf.PI * phase), 2);
        //}
        //else {
        //    epsilon_n = 0f;
        //}

        inPhaseErrorBuffer = ShiftFloatListRightToLeftWith(inPhaseErrorBuffer, epsilon_n);

        // Calculating the median of the inPhaseErrorBuffer, being the self-assessed synch-score
        float s_n = ListMedian(inPhaseErrorBuffer);

        // Calculating the measure capturing the amplitude and sign of the frequency-modification of the n-th "fire"-event received
        float rho_n = -Mathf.Sin(2 * Mathf.PI * phase); // negative for phase < 1/2, and positive for phase > 1/2, and element in [-1, 1]

        float H_n = rho_n * s_n;
        HBuffer.Add(H_n); // H(n)-values appended to the end of the list
    }

    private void RFAAdjustFrequency() {
        // Adjusting frequency according to the reachback firefly algorithm (RFA) with the values calculated since the start of previous oscillator cycle until now.

        // OLD COMMENTS AND THOUGHTS:
            // IMPLEMENTER FORMELEN �VERST I �UiO/MSc/Logs/Simulations/Frequency adjustment�-notatet p� reMarkable'n.
            // N� har jeg verdiene s(n), og kan lett finne rho(n), og da alts� H(n).
            // Da mangler jeg � ha en beta, en y, og � summe med alle disse verdiene � og til slutt sette den resulterende frekvens-verdien som min nye/n�v�rende/oppdaterte frekvens.

            // F_n = beta * sum_0^{y-1}(H(n-x)/y);,       der beta er frequency coupling constant, y er antall h�rte/mottatte "fire-events",
            //                                           H(n) = rho(n) * s(n), og rho(n)=-sin(2*PI*phase)

        float averageCycleH = 0;
        int HBufferLength = HBuffer.Count;
        foreach (float H in HBuffer) {
            averageCycleH += H/(float)HBufferLength;
        }
        float F_n = beta * averageCycleH;

        // Clearing the buffer of H(n)-values and making it ready for the next cycle
        HBuffer.Clear();

        float newFrequency = frequency * Mathf.Pow(2, F_n);
        frequency = newFrequency;
    }

    // ------- END OF Core-/Essential Functions/Methods -------






    // ------- START OF Helping-/Utility Functions/Methods -------

    void SetAgentCorpsColor() {
        // Sets the agent body's corps to a lerped color (ranging from the fire-color, to its standard corps-color).

        float t = Mathf.Clamp(phase / colorLerpUntilPhase, 0, 1); // a percentage going from 0 when phase=0, to 1 when phase=colorLerpUntilPhase
        t = Mathf.Sin(t * Mathf.PI * 0.5f); // Lerping like a pro
        corpsOfAgentRenderer.material.color = Color.Lerp(fireColor, bodyColor, t);
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
        for (int i = 0; i < m; i++) inPhaseErrorBuffer.Add(1f);
    }

    private void BlinkWithEyes() {
        Vector3 pupilScaleChange = new Vector3(0.273f, -0.1126f, 0);
        pupil.localScale += pupilScaleChange;
        Vector3 yellowEyeScaleChange = new Vector3(0, 0, 0.4465f);
        yellowEye.localScale -= yellowEyeScaleChange;

        Invoke("OpenEyes", 0.1f * (1.0f / frequency));
    }

    private void OpenEyes() {
        Vector3 pupilScaleChange = new Vector3(-0.273f, 0.1126f, 0);
        pupil.localScale += pupilScaleChange;
        Vector3 yellowEyeScaleChange = new Vector3(0, 0, 0.4465f);
        yellowEye.localScale += yellowEyeScaleChange;
    }

    private void AssignVisualVariables() {
        corpsOfAgentRenderer = transform.GetChild(1).transform.GetChild(1).GetComponent<Renderer>();
        bodyColor = corpsOfAgentRenderer.material.color;
        yellowEye = transform.GetChild(0).transform.GetChild(3);
        pupil = transform.GetChild(0).transform.GetChild(4);
        //tentacleColor = transform.GetChild(2).transform.GetChild(0).GetComponent<Renderer>().material.color;
    }
    
    private void AssignAudioVariables() {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 1f / (otherSquiggles.Count + 1);
    }

    private void AssignHelpingVariables() {
        // Speeding up or down the simulation if that is wanted
        Time.timeScale = adjustedTimeScale;

        // Acquiring a neighbour-list for each agent so that they can call on them when they themselves are firing
        FillUpNeighbourSquigglesList();
    }

    private void LoadMySceneAgain() {
        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }

    public float GetFrequency() {
        return frequency;
    }

    public float GetPhase() {
        return phase;
    }

    public void SetAgentID(int idNumber) {
        agentID = idNumber;
    }

    public int GetAgentID() {
        return agentID;
    }

    // ------- END OF Helping-/Utility Functions/Methods -------
}
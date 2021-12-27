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
    public Vector2 minMaxInitialFreqs = new Vector2(0.5f, 8f);

    // Meta environment-variables:
    public float t_ref_perc_of_period = 0.1f; // ISH LENGDEN I TID PÅ digitalQuickTone er 0.4s. Nymoen BRUKTE 50ms I SIN IMPLEMENTASJON. JEG PRØVDE OGSÅ 0.6f. possiblePool = {0.09f, 0.4f, 0.6f}.
    public bool useSound = true;
    public bool useVisuals = false;


    // Core (oscillator) synchronization-variables:
    private float phase; // a number between 0 and 1
    private float frequency; // Hz
    private float t_ref; // seconds
    private List<float> inPhaseErrorBuffer = new List<float>();
    private List<float> HBuffer = new List<float>(); // a list of H(n)-values (which I calculated from the bottom up in the compiled reMarkable-note of mine)

    // Helping-variables:
    private List<SquiggleScript> otherSquiggles = new List<SquiggleScript>();
    private AudioSource audioSource; // reference to Audio Source component on the Musical Node that is told to play the fire sound
    private int agentID;
    private bool firedLastClimax;

    // Visual color-variables:
    private Renderer corpsOfAgentRenderer;
    private float colorLerpUntilPhase = 0.15f;
    private Transform yellowEye;
    private Transform pupil;
    private Color bodyColor;
    private Color fireColor = Color.yellow;
    private bool inRefractoryPeriod = false; // if this is true, neither phase- or frequency-adjustment should happen.
    private float unstableFrequencyPeriod = 1f/0.5f;
    private float timeNotClimaxed;

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
        frequency = Random.Range(minMaxInitialFreqs.x, minMaxInitialFreqs.y); // Initializing frequency in range Random.Range(0.5f, 8f) was found useful by Nymoen et al.                                                                                                       (BRUK '1f;' ISTEDET HVIS DU BARE VIL FASE-JUSTERE)

        t_ref = t_ref_perc_of_period * 1.0f / frequency;
    }

    void Update() {
        // Allowing phase-restarts for quick demonstrations. Stop doing this if errors occur because of it.
        if (Input.GetKeyDown(KeyCode.Space)) {
            LoadMySceneAgain();
        }

                                                                                                                            //Debug.Log("errorBuffer: \n");
                                                                                                                            //DebugLogMyFloatList(inPhaseErrorBuffer);
    }

    void FixedUpdate() {
        // Eventually updating/lerping the agent-body's color
        if (useVisuals) SetAgentCorpsColor();

        if (phase == 1f) {
            OnPhaseClimax();
            phase = 0f;
            timeNotClimaxed = 0f;
        }
        else if (timeNotClimaxed >= unstableFrequencyPeriod*5) {
            frequency = 2f * frequency;
        }
        else {
            // Increasing agent's phase according to its frequency if it did just hear a ``fire''-event
            phase = Mathf.Clamp(phase + frequency * Time.fixedDeltaTime, 0f, 1f);
            timeNotClimaxed += Time.fixedDeltaTime;
        }
    }

    // ------- END OF MonoBehaviour Functions/Methods -------





    // ------- START OF Testing Functions/Methods -------

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

    // ------- END OF Testing Functions/Methods -------






    // ------- START OF Core-/Essential Functions/Methods -------

    private void OnPhaseClimax() {
        if (!firedLastClimax) { // this should run only every other/second time it gets checked
            FireNode(); // node firing at every other phase-climax
            firedLastClimax = true;
        } else {
            firedLastClimax = false;
        }

        RFAAdjustFrequency(); // adjust frequency at phase-climax regardless of if the node fired or not last climax         (KOMMENTER UT HVIS DU BARE FIL FASE-JUSTERE)

        t_ref = 0.1f * 1.0f / frequency; // updating the refractory period to 10% of the new period
    }

    void FireNode() {
        NotifyTheHuman();

        // FOR REFRACTORY PERIOD:
        inRefractoryPeriod = true;
        Invoke("ToggleOffRefractoryMode", t_ref);

        // Calling on all the agents to adjust their phases and calculate frequency-H-values
        NotifyTheAgents();
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

    private void ToggleOffRefractoryMode() {
        inRefractoryPeriod = false;
    }

    private void NotifyTheAgents() {
        // Calls out to all neighbouring agents and notifies them of its fire-event.

        foreach (SquiggleScript oscillator in otherSquiggles) { // "giving away a signal" all other nodes can hear
            oscillator.OnHeardFireEvent();
        }
    }

    public void OnHeardFireEvent() {
        // Gets called when an "audio"-fire-event-signal is "detected" by the agent.

        if (!inRefractoryPeriod) AdjustPhase(); // for immediate Phase-adjustment when hearing a fire-event
        AddNthFireEventsHToList(); // for the later Frequency-adjustment at each phase-climax
    }

    void AdjustPhase() {
        if (!useNymoen) {
            phase = Mathf.Clamp(phase *(1 + alpha), 0f, 1f); // using Phase Update Function (1); "standard" Mirollo-Strogatz
        } else {
            float wave = Mathf.Sin(2 * Mathf.PI * phase);
            phase = Mathf.Clamp(phase - alpha * wave * Mathf.Abs(wave), 0f, 1f); // using Phase Update Function (2); Nymoen et al.'s Bi-Directional
        }
    }

    private void AddNthFireEventsHToList() {
        // Calculating and saving the n-th fire-event's corresponding H(n)-value capturing how much and in which direction frequency of an agent should be adjusted at phase-climax, judged at the time of hearing fire-event n.

        // Recording the n'th error-score, capturing whether the node itself is in synch with the node it hears the "fire"-event from or not
        float epsilon_n;
        if (!inRefractoryPeriod) {
            epsilon_n = Mathf.Pow(Mathf.Sin(Mathf.PI * phase), 2);
        }
        else {
            epsilon_n = 0f;
        }

        inPhaseErrorBuffer = ShiftFloatListRightToLeftWith(inPhaseErrorBuffer, epsilon_n);

        // Calculating the median of the inPhaseErrorBuffer, being the self-assessed synch-score
        float s_n = ListMedian(inPhaseErrorBuffer);

                                                                                                            // BARE FOR TESTING:
                                                                                                            // Debug.Log(gameObject.name + "'s self-assessed synch-score s(n): " + s_n);


        // Calculating the measure capturing the amplitude and sign of the frequency-modification of the n-th "fire"-event received
        float rho_n = -Mathf.Sin(2 * Mathf.PI * phase); // negative for phase < 1/2, and positive for phase > 1/2, and element in [-1, 1]

        float H_n = rho_n * s_n;
        HBuffer.Add(H_n); // H-values appended to the end of the list H(n)
    }

    private void RFAAdjustFrequency() {
        // Adjusting frequency according to the reachback firefly algorithm (RFA) with the values calculated since the start of previous oscillator cycle until now.

        // OLD COMMENTS AND THOUGHTS:
            // IMPLEMENTER FORMELEN ØVERST I “UiO/MSc/Logs/Simulations/Frequency adjustment”-notatet på reMarkable'n.
            // Nå har jeg verdiene s(n), og kan lett finne rho(n), og da altså H(n).
            // Da mangler jeg å ha en beta, en y, og å summe med alle disse verdiene — og til slutt sette den resulterende frekvens-verdien som min nye/nåværende/oppdaterte frekvens.

            // F_n = beta * sum_0^{y-1}(H(n-x)/y);,       der beta er frequency coupling constant, y er antall hørte/mottatte "fire-events",
            //                                           H(n) = rho(n) * s(n), og rho(n)=-sin(2*PI*phase)

        float averageCycleH = 0f;
        float HBufferLength = (float)HBuffer.Count;
        foreach (float H in HBuffer) {
            averageCycleH += H;
        }

                                                                // BARE FOR TESTING:
                                                                //Debug.Log("beta: " + beta + ", averageCycleH: " + averageCycleH + ", HBufferLength: " + HBufferLength);

        float F_n = 0f;
        if (HBufferLength != 0f) {
            F_n = beta * averageCycleH / HBufferLength;
        }

        // Clearing the buffer of H(n)-values and making it ready for the next cycle
        HBuffer.Clear();

        float newFrequency = frequency * Mathf.Pow(2, F_n);

                                                    // BARE FOR TESTING:
                                                    //Debug.Log("oldFrequency: " + frequency + ", F_n: " + F_n + ", \n newFrequency (old_freq * 2^F_n): " + newFrequency);

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
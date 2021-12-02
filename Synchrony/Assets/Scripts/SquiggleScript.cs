using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using static DavidsUtils;

public class SquiggleScript : MonoBehaviour {
    // TODO: Clean up in variables.
    public float adjustedTimeScale = 1.0f;
    public float alpha = 0.05f; // pulse coupling constant, denoting coupling strength between nodes
    public float colorLerpUntilPhase = 0.15f;
    public bool useNymoen = true;
    public bool useSound = true;
    public bool useVisuals = false;

    // FOR FREQUENCY-ADJUSTMENT:
    public int m = 5; // running median-filter length
    public float beta = 0.8f; // frequency coupling constant


    private List<SquiggleScript> otherSquiggles = new List<SquiggleScript>();
    private AudioSource source; // reference to Audio Source component on the Musical Node that is told to play the fire sound
    private float phase; // a number between 0 and 1
    private float frequency; // Hz
    private Renderer corpsOfAgentRenderer;
    private Transform yellowEye;
    private Transform pupil;
    private Color bodyColor;
    private Color fireColor = Color.yellow;

    // FOR FREQUENCY-ADJUSTMENT:
    private List<float> errorBuffer = new List<float>();
    private List<float> HBuffer = new List<float>();
    private bool cycleTraversedOnce = false;
    //private bool inRefractoryPeriod = false; // if this is true, no (neither phase- or frequency-) adjustment should happen (?) - <--------------- DIDN'T SOLVE THE ISSUE
    //private float t_ref = 0.09f; // ISH LENGDEN I TID PÅ digitalQuickTone er 0.4s. Nymoen BRUKTE 50ms I SIN IMPLEMENTASJON.

    void Start() {
        // TODO: Clean up in variables.
        // Initializing variables
        corpsOfAgentRenderer = transform.GetChild(1).transform.GetChild(1).GetComponent<Renderer>();
        yellowEye = transform.GetChild(0).transform.GetChild(3);
        pupil = transform.GetChild(0).transform.GetChild(4);
        bodyColor = corpsOfAgentRenderer.material.color;
        //tentacleColor = transform.GetChild(2).transform.GetChild(0).GetComponent<Renderer>().material.color;

        // TODO: Clean up in function-calls.
        source = GetComponent<AudioSource>();
        Time.timeScale = adjustedTimeScale;

        FillUpOtherSquigglesList();

        FineTuneTheSquiggles();

        phase = Random.Range(0.0f, 1.0f);

        // FOR FREQUENCY-ADJUSTMENT:
        InitializeErrorBuffer();
        frequency = Random.Range(0.1f, 1.5f); // Initializing frequency in range Random.Range(0.5f, 8f) was found useful by Nymoen et al.
    }

    // TODO: Clean up in functions.
    void Update() {
        // Allowing phase-restarts for quick demonstrationS (MEN BØR DENNE VÆRE I Squiggles.cs OG IKKE AgentSpawner.cs DA?)
        if (Input.GetKeyDown(KeyCode.Space)) {
            string currentScene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentScene); // Stop doing this if errors occur because of it.
        } 
        // FOR FREQUENCY-ADJUSTMENT:
        else if (Input.GetKeyDown(KeyCode.P)) {
            float s_n = ListMedian(errorBuffer);
            print("Median av errorBuffer til " + gameObject.name + ": " + s_n);
        }
        else if (Input.GetKeyDown(KeyCode.H)) {
            DebugLogMyFloatList(HBuffer);
        }
    }

    void FixedUpdate() {
        if (useVisuals) SetLerpedColor();

        if (phase > 1) {
            if (cycleTraversedOnce) { // only run this clause every other check
                FireNode();
                cycleTraversedOnce = false;
            }

            RFAAdjustFrequency();

            cycleTraversedOnce = true;
        }

        phase += frequency * Time.fixedDeltaTime;
    }

    void FireNode() {
        //BlinkWithEyes(); PRØVER Å KOMMENTERE UT FOR Å SE OM BOXCOLLIDER-WARNINGSA FORSVINNER

        // Signalizing a "fire"-event to the observer watching the Unity simulation through audio
        if (useSound) source.Play();

        //Invoke("ToggleOffRefractoryMode", 0.6f); // 

        // Signalizing a "fire"-event visually to the observer watching the Unity simulation
        if (useVisuals) corpsOfAgentRenderer.material.color = fireColor;

        CallOnOtherAgents();

        phase = 0; // resetting phase

        //inRefractoryPeriod = true;
        //Invoke("CallOnOtherAgents", t_ref); // ISH LENGDEN I TID PÅ digitalQuickTone er 0.4s. Nymoen BRUKTE 50ms I SIN IMPLEMENTASJON.
    }

    void AdjustPhase() {
        if (!useNymoen) {
            phase *= (1 + alpha); // using Phase Update Function (1); "standard" Mirollo-Strogatz
        } else {
            float wave = Mathf.Sin(2 * Mathf.PI * phase);
            phase -= alpha * wave * Mathf.Abs(wave); // using Phase Update Function (2); Nymoen et al.'s Bi-Directional
        }
    }

    private void RFAAdjustFrequency() {
        // Adjusting frequency according to the reachback firefly algorithm (RFA) with the values calculated since the start of previous oscillator cycle until now.

        // OLD COMMENTS:
        // IMPLEMENTER FORMELEN ØVERST I “UiO/MSc/Logs/Simulations/Frequency adjustment”-notatet på reMarkable'n.
        // Nå har jeg verdiene s(n), og kan lett finne rho(n), og da altså H(n).
        // Da mangler jeg å ha en beta, en y, og å summe med alle disse verdiene — og til slutt sette den resulterende frekvens-verdien som min nye/nåværende/oppdaterte frekvens.

        // F_n = beta * sum_0^{y-1}(H(n-x)/y);,       der beta er frequency coupling constant, y er antall hørte/mottatte "fire-events",
        //                                           H(n) = rho(n) * s(n), og rho(n)=-sin(2*PI*phase)

        float averageCycleH = 0;
        int HBufferLength = HBuffer.Count;
        foreach (float H in HBuffer) {
            averageCycleH += H/(float)HBufferLength;
        }
        float F_n = beta * averageCycleH;

        // Clear the buffer of H-values and make it ready for the next cycle
        HBuffer.Clear();

        float newFrequency = frequency * Mathf.Pow(2, F_n);
        frequency = newFrequency;
    }

    private void CallOnOtherAgents() {
        foreach (SquiggleScript oscillator in otherSquiggles) { // "giving away a signal" all other nodes can hear
            //oscillator.AdjustPhase(); // Invoke this after a physical-realistic-constrained time-period?
            oscillator.JustHeardFireEvent();
        }

        //inRefractoryPeriod = false;
    }

    public void JustHeardFireEvent() {
        AdjustPhase(); // for immediate Phase-adjustment when hearing a fire-event
        AddNthFireEventsHToList(); // for the later Frequency-adjustment at each phase-climax
    }

    private void AddNthFireEventsHToList() {
        // KALKULERER MEG OPP TIL EN LISTE MED H(n)-VERDIER (SOM JEG GJORDE NEDENFRA OG OPP I DET KOMPILERTE reMarkable-NOTATET MITT) (since the agent is "hearing" a fire-signal)

        // Recording the n'th error-score, capturing whether the node itself is in synch with the node it hears the "fire"-event from or not
        float epsilon_n = Mathf.Pow(Mathf.Sin(Mathf.PI * phase), 2);
        //if (!inRefractoryPeriod) {
        //    epsilon_n = Mathf.Pow(Mathf.Sin(Mathf.PI * phase), 2);
        //}
        //else {
        //    epsilon_n = 0f;
        //}
        errorBuffer = ShiftFloatListRightToLeftWith(errorBuffer, epsilon_n);

        // Calculating the median of the errorBuffer, being the self-assessed synch-score and public self-awareness component
        float s_n = ListMedian(errorBuffer);

        // Calculating the measure capturing the amplitude and sign of the frequency-modification of the n-th "fire"-event received
        float rho_n = -Mathf.Sin(2 * Mathf.PI * phase); // negative for phase < 1/2, and positive for phase > 1/2, and element in [-1, 1]

        float H_n = rho_n * s_n;
        HBuffer.Add(H_n); // H(n)-values appended to the end of the list
        Debug.Log("HBuffer with .Count=" + HBuffer.Count + " added with value <" + H_n + ">.");
    }

    private void FineTuneTheSquiggles() {
        source.volume = 1f / (otherSquiggles.Count + 1);
        //transform.position = new Vector3(transform.position.x, -0.96f, transform.position.z);
    }

    private void FillUpOtherSquigglesList() {
        // Filling up a Unity List with all other Squiggles-GameObjects in the scene apart from ones own
        List<GameObject> allSquiggleObjs = new List<GameObject>();
        this.gameObject.tag = "temp";
        allSquiggleObjs.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        this.gameObject.tag = "Player";
        foreach (GameObject squigObj in allSquiggleObjs) { otherSquiggles.Add(squigObj.GetComponent<SquiggleScript>()); }
    }

    void SetLerpedColor() {
        float t = Mathf.Clamp(phase / colorLerpUntilPhase, 0, 1); // a percentage going from 0 when phase=0, to 1 when phase=colorLerpUntilPhase
        t = Mathf.Sin(t * Mathf.PI * 0.5f); // Lerping like a pro
        corpsOfAgentRenderer.material.color = Color.Lerp(fireColor, bodyColor, t);
    }

    void BlinkWithEyes() {
        Vector3 pupilScaleChange = new Vector3(0.273f, -0.1126f, 0);
        pupil.localScale += pupilScaleChange;
        Vector3 yellowEyeScaleChange = new Vector3(0, 0, 0.4465f);
        yellowEye.localScale -= yellowEyeScaleChange;

        Invoke("OpenEyes", 0.1f * (1.0f / frequency));
    }

    void OpenEyes() {
        Vector3 pupilScaleChange = new Vector3(-0.273f, 0.1126f, 0);
        pupil.localScale += pupilScaleChange;
        Vector3 yellowEyeScaleChange = new Vector3(0, 0, 0.4465f);
        yellowEye.localScale += yellowEyeScaleChange;
    }

    // FOR FREQUENCY-ADJUSTMENT:
    void InitializeErrorBuffer() {
        for (int i = 0; i < m; i++) errorBuffer.Add(1f);
    }
}
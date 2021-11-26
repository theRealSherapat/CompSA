using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using static DavidsUtils;

public class Squiggles : MonoBehaviour {
    // TODO: Clean up in variables.
    public float adjustedTimeScale = 1.0f;
    public float frequency = 0.5f; // Hz                                                    // <--------------------- FORTSETT HER! DU HAR FUNNET SELF-ASSESSED SYNCHRONY SCORE
    public float alpha = 0.05f; // pulse coupling constant, denoting coupling strength between nodes
    public float colorLerpUntilPhase = 0.25f;
    public bool useNymoen = true;
    public bool useSound = true;
    public bool useVisuals = false;
    public int m = 5; // running median-filter length

    private List<Squiggles> otherSquiggles = new List<Squiggles>();
    private AudioSource source; // reference to Audio Source component on the Musical Node that is told to play the fire sound
    private float phase;
    private Renderer corpsOfAgentRenderer;
    private Transform yellowEye;
    private Transform pupil;
    private Color bodyColor;
    private Color fireColor = Color.yellow;

    // FOR FREQUENCY-ADJUSTMENT:
    private List<float> errorBuffer = new List<float>();

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

        // FOR FREQUENCY-ADJUSTMENT:
        InitializeErrorBuffer();
        // frequency = Random.Range(0.5f, 8f);                                            // <--------------------- FORTSETT HER! DU HAR FUNNET SELF-ASSESSED SYNCHRONY SCORE

        phase = Random.Range(0.0f, 1.0f);
    }

    // TODO: Clean up in functions.
    void Update() {
        // Allowing phase-restarts for quick demonstrationS (MEN BØR DENNE VÆRE I Squiggles.cs OG IKKE AgentSpawner.cs DA?)
        if (Input.GetKeyDown(KeyCode.Space)) {
            //phase = Random.Range(0.0f, 1.0f);
            string currentScene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentScene); // IKKE GJØRE DETTE HVIS DET FORTSETTER Å LAGE ERROR!
        } 
        // FOR FREQUENCY-ADJUSTMENT:
        else if (Input.GetKeyDown(KeyCode.P)) {
            float s_n = ListMedian(errorBuffer);                                           // <--------------------- FORTSETT HER! DU HAR FUNNET SELF-ASSESSED SYNCHRONY SCORE
            print("Median av errorBuffer til " + gameObject.name + ": " + s_n);
        }
    }

    void FixedUpdate() {
        if (useVisuals) SetLerpedColor();

        if (phase > 1) {
            FireNode();
            //AdjustOwnFrequency();                                                        // <--------------------- FORTSETT HER! DU HAR FUNNET SELF-ASSESSED SYNCHRONY SCORE
        }

        phase += frequency * Time.fixedDeltaTime;
    }

    void AdjustPhase() {
        // Recording the n'th error-score (since the agent is "hearing" a fire-signal)
        float errorScore = Mathf.Pow(Mathf.Sin(Mathf.PI * phase), 2);
        errorBuffer = ShiftFloatListRightWith(errorBuffer, errorScore);

        if (!useNymoen) {
            phase *= (1 + alpha); // using Phase Update Function (1); "standard" Mirollo-Strogatz
        } else {
            float wave = Mathf.Sin(2 * Mathf.PI * phase);
            phase -= alpha * wave * Mathf.Abs(wave); // using Phase Update Function (2); Nymoen et al.'s Bi-Directional
        }
    }

    private void AdjustOwnFrequency() {
        // IMPLEMENTER FORMELEN ØVERST I “UiO/MSc/Logs/Simulations/Frequency adjustment”-notatet på reMarkable'n.
        // Nå har jeg verdiene s(n), og kan lett finne rho(n), og da altså H(n).
        // Da mangler jeg å ha en beta, en y, og å summe med alle disse verdiene — og til slutt sette den resulterende frekvens-verdien som min nye/nåværende/oppdaterte frekvens.

        // F_n = beta * sum_0^{y-1}(H(n-x)/y);,       der beta er frequency coupling constant, y er antall hørte/mottatte "fire-events",
        //                                           H(n) = rho(n) * s(n), og rho(n)=-sin(2*PI*phase)

        // new_frequency = old_frequency * 2^F_n;
        // frequency = new_frequency;
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
        foreach (GameObject squigObj in allSquiggleObjs) { otherSquiggles.Add(squigObj.GetComponent<Squiggles>()); }
    }

    void SetLerpedColor() {
        float t = Mathf.Clamp(phase / colorLerpUntilPhase, 0, 1); // a percentage going from 0 when phase=0, to 1 when phase=colorLerpUntilPhase
        t = Mathf.Sin(t * Mathf.PI * 0.5f); // Lerping like a pro
        corpsOfAgentRenderer.material.color = Color.Lerp(fireColor, bodyColor, t);
    }

    void FireNode() {
        BlinkWithEyes();

        if (useSound) source.Play();
        
        TransmitSignalToEnvironment();

        if (useVisuals) corpsOfAgentRenderer.material.color = fireColor; // signalizing, visually, to the observer a firing-event
        
        phase = 0; // resetting phase
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

    void TransmitSignalToEnvironment() {
        foreach (Squiggles oscillator in otherSquiggles) { // "giving away a signal" all other nodes can hear
            oscillator.AdjustPhase(); // Invoke this after a physical-realistic-constrained time-period?
        }
    }

    // FOR FREQUENCY-ADJUSTMENT:
    void InitializeErrorBuffer() {
        for (int i = 0; i < m; i++) errorBuffer.Add(1f);
    }
}
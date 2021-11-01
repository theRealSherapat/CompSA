using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Squiggles : MonoBehaviour {
    public float adjustedTimeScale = 1.0f;
    public float frequency = 0.5f; // Hz
    public float alpha = 0.1f; // pulse coupling constant, denoting coupling strength between nodes
    public float colorLerpUntilPhase = 0.15f;
    public bool useNymoen;
    public bool useSound = true;
    public bool useVisuals = true;
    public Color fireColor = Color.yellow;

    private Color originalColor;
    private List<Squiggles> otherSquiggles = new List<Squiggles>();
    private AudioSource source; // reference to Audio Source component on the Musical Node that is told to play the fire sound
    private float phase;
    private Renderer corpsOfAgentRenderer;
    private Transform yellowEye;
    private Transform pupil;

    void Start() {
        // Initializing variables
        corpsOfAgentRenderer = transform.GetChild(1).transform.GetChild(1).GetComponent<Renderer>();
        originalColor = corpsOfAgentRenderer.material.color;
        source = GetComponent<AudioSource>();
        Time.timeScale = adjustedTimeScale;
        yellowEye = transform.GetChild(0).transform.GetChild(3);
        pupil = transform.GetChild(0).transform.GetChild(4);

        FillUpOtherSquigglesList();

        FineTuneTheSquiggles();

        phase = Random.Range(0.0f, 1.0f);
    }

    // BARE FOR Å KUNNE RESTARTE PROSESSEN LETT
    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            phase = Random.Range(0.0f, 1.0f);
        }
    }

    void FixedUpdate() {
        if (useVisuals) SetLerpedColor();

        if (phase > 1) FireNode();

        phase += frequency * Time.fixedDeltaTime;
    }

    private void FineTuneTheSquiggles() {
        source.volume = 1f / (otherSquiggles.Count + 1);
        transform.position = new Vector3(transform.position.x, -0.96f, transform.position.z);
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
        corpsOfAgentRenderer.material.color = Color.Lerp(fireColor, originalColor, t);
    }

    void FireNode() {
        BlinkWithEyes();

        if (useSound) source.Play();
        
        AdjustOtherNodePhases();

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

    void AdjustOtherNodePhases() {
        foreach (Squiggles oscillator in otherSquiggles) { // "giving away a signal" all other nodes can hear
            oscillator.AdjustPhase();
        }
    }

    void AdjustPhase() {
        if (!useNymoen) {
            phase *= (1 + alpha); // using Phase Update Function (1); "standard" Mirollo-Strogatz
        } else {
            float wave = Mathf.Sin(2 * Mathf.PI * phase);
            phase -= alpha * wave * Mathf.Abs(wave); // using Phase Update Function (2); Nymoen et al.'s Bi-Directional
        }
    }
}
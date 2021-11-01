using System.Collections.Generic;
using UnityEngine;

public class Oscillate : MonoBehaviour {
    public float adjustedTimeScale = 1.0f;
    public float frequency = 0.01f; // Hz
    public float alpha = 0.1f; // pulse coupling constant, denoting coupling strength between nodes
    public float colorLerpUntilPhase = 0.2f;
    public Color fireColor;
    public bool useNymoen;

    private Color originalColor;
    private List<Oscillate> otherOscillators = new List<Oscillate>();
    private AudioSource _source; // reference to Audio Source component on the Musical Node that is told to play the fire sound
    private float phase, colorPerc;
    
    void Start() {
        originalColor = gameObject.GetComponent<Renderer>().material.color;
        _source = GetComponent<AudioSource>();
        Time.timeScale = adjustedTimeScale; // speeding up the simulation to twice the speed
        phase = Random.Range(0.0f, 1.0f);

        // Filling up a Unity List with all other Oscillate-GameObjects in the scene apart from ones own
        this.gameObject.tag = "temp";
        var allOscillators = GameObject.FindGameObjectsWithTag("Player");
        this.gameObject.tag = "Player";

        foreach (GameObject oscObj in allOscillators) { otherOscillators.Add(oscObj.GetComponent<Oscillate>()); }
        _source.volume = 1f / otherOscillators.Count;
    }

    void FixedUpdate() {        
        colorPerc = Mathf.Clamp(phase / colorLerpUntilPhase, 0, 1); // a percentage going from 0 when phase=0, to 1 when phase=colorLerpUntilPhase
        float t = Mathf.Sin(colorPerc * Mathf.PI * 0.5f); // Lerping like a pro
        gameObject.GetComponent<Renderer>().material.color = Color.Lerp(fireColor, originalColor, t);

        if (phase > 1) FireNode();

        phase += frequency * Time.fixedDeltaTime;
    }

    void FireNode() {
        _source.Play();
        CallOnOtherNodes(otherOscillators);
        gameObject.GetComponent<Renderer>().material.color = fireColor; // signalizing, visually, to the observer a firing-event
        phase = 0; // resetting phase
    }

    void CallOnOtherNodes(List<Oscillate> oscillators) {
        foreach (Oscillate oscillator in oscillators) { // "giving away a signal" all other nodes can hear
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

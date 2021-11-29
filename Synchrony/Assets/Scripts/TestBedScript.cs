using System.Collections.Generic;
using UnityEngine;
using static DavidsUtils;

// ER ENTRIESA/VALUESA I ET C# ARRAY VERDIER ELLER REFERANSER?

public class TestBedScript : MonoBehaviour {
    private List<float> errorBuffer = new List<float>();

    void Start() {
        errorBuffer.Add(0.01f);
        errorBuffer.Add(0.02f);
        errorBuffer.Add(0.03f);
        errorBuffer.Add(0.04f);
    }

    void Update() {
        // Jeg shifter alle Liste-verdier ved 'Space'-tastetrykk, og skriver ut Lista ved et 'p'-tastetrykk. Sorterer Lista og skriver ut ved 's'.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShiftListWith(Random.Range(-20f, 20f));
        } 
        else if (Input.GetKeyDown(KeyCode.P)) 
        {
            DebugLogMyFloatList(errorBuffer);
        } 
        else if (Input.GetKeyDown(KeyCode.S)) 
        {
            List<float> sortertListe = GetMyDarnSortedList(errorBuffer);
            DebugLogMyFloatList(sortertListe);
        }
        else if (Input.GetKeyDown(KeyCode.M)) 
        {
            float beTheMedianPls = ListMedian(errorBuffer);
            print("Medianen er forhåpentligvis: " + beTheMedianPls);
        }
    }

    private List<float> GetMyDarnSortedList(List<float> listeAaSortere) {
        var klonetListe = new List<float>(listeAaSortere);
        klonetListe.Sort();

        return klonetListe;
    }

    void ShiftListWith(float thisInput) {
        errorBuffer.Add(thisInput);
        errorBuffer.RemoveAt(0);
    }
}

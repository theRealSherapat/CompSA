using System.Collections.Generic;
using UnityEngine;
using static DavidsUtils;

public class TestBedScript : MonoBehaviour {
    // ------- START OF Variable Declarations -------

    private List<float> errorBuffer = new List<float>();
    private int incrementVariable;

    List<float> squiggleIDer = new List<float> {2f,1f,3f,4f}; // N = 4
    int[] agentIDer;

    bool[] agentIHasFired;

    // ------- END OF Variable Declarations -------

    void Start() {

        // Tester konverteringen fra ARRAY -> LIST
        agentIDer = new int[10];

        agentIDer[3] = 1;
        agentIDer[5] = 1;
        agentIDer[7] = 1;
        agentIDer[1] = 1;
        agentIDer[0] = 1;

        DebugLogMyIntArray(agentIDer);

        //List<object> list = agentIDer.Cast<Object>().ToList();
        List<int> nyeListo = new List<int>(agentIDer);

        DebugLogMyIntList(nyeListo);


        // Tester list-shifteren:
        //errorBuffer.Add(0.01f);
        //errorBuffer.Add(0.02f);
        //errorBuffer.Add(0.03f);
        //errorBuffer.Add(0.04f);

        // Jeg tester hvordan boolske arrays fungerer i C#
        //agentIHasFired = new bool[errorBuffer.Count];

        //// Tester .CSV-lagringen:
        //string path = System.IO.Directory.GetCurrentDirectory() + "\\" + "SavedData" + "\\" + "minForsteDataFil.csv";
        //List<int> CSVHeaderEntries = new List<int> { 2, 1, 3, 4};
        //CreateCSVWithIntHeader(path, CSVHeaderEntries);
    }

    void Update() {
        // Jeg tester hvordan boolske arrays fungerer i C#
        //if (Input.GetKeyDown(KeyCode.P)) {
        //    for (int i = 0; i < agentIHasFired.Length; i++) {
        //        Debug.Log("agentIHasFired[" + i + "]: " + agentIHasFired[i]);
        //    }
        //} else if (Input.GetKeyDown(KeyCode.J)) {
        //    agentIHasFired[0] = true;
        //} else if (Input.GetKeyDown(KeyCode.D)) {
        //    agentIHasFired[1] = true;
        //} else if (Input.GetKeyDown(KeyCode.T)) {
        //    agentIHasFired[2] = true;
        //} else if (Input.GetKeyDown(KeyCode.S)) {
        //    agentIHasFired[3] = true;
        //}

        //if (Input.GetKeyDown(KeyCode.A)) {
        //    Debug.Log("Any-resultat: " + agentIHasFired.AsQueryable().Any(val => val == false));
        //}

        // Jeg shifter alle Liste-verdier ved 'Space'-tastetrykk, og skriver ut Lista ved et 'p'-tastetrykk. Sorterer Lista og skriver ut ved 's'.
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    ShiftListWith(Random.Range(-20f, 20f));
        //} else if (Input.GetKeyDown(KeyCode.P)) {
        //    // FOR Å TESTE LIST-SHIFTEREN:
        //    DebugLogMyFloatList(errorBuffer);

        //    // FOR Å TESTE CSV-LAGRINGEN:
        //    //Debug.Log(System.IO.Directory.GetCurrentDirectory() + "\\" + "minForsteDataFil.csv");
        //    //foreach (int squig in squiggleIDer) {
        //    //    Debug.Log(squig);
        //    //}
        //}
        //else if (Input.GetKeyDown(KeyCode.S)) 
        //{
        //    // FOR Å TESTE LIST-SHIFTEREN:
        //    //List<float> sortertListe = GetMyDarnSortedList(errorBuffer);
        //    //DebugLogMyFloatList(sortertListe);

        //    // FOR Å TESTE CSV-LAGRINGEN:
        //    //string path = System.IO.Directory.GetCurrentDirectory() + "\\" + "SavedData" + "\\" + "minForsteDataFil.csv";
        //    //FloatUpdateCSV(path, squiggleIDer);
        //} 
        //  else if (Input.GetKeyDown(KeyCode.M)) 
        //{
        //    float beTheMedianPls = ListMedian(errorBuffer);
        //    print("Medianen er forhåpentligvis: " + beTheMedianPls);
        //}
        //else if (Input.GetKeyDown(KeyCode.H)) {
        //    // Tester om Invoke's stopper sekvensen/koden, men det gjør den ikke. Linjene etter blir kjørt med en gang.
        //    Invoke("PrintHelloFromTheOtherSideIn2Sec", 5f);
        //    Debug.Log("Beat you to it suckah.");
        //}
        //else if (Input.GetKeyDown(KeyCode.I)) {
        //    // Checking C# integer increments
        //    incrementVariable++;
        //    Debug.Log(incrementVariable);
        //}
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

    void PrintHelloFromTheOtherSideIn2Sec() {
        Debug.Log("Hello From The Other Side");
    }
}

using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static SynchronyUtils;

public class TestBedScript : MonoBehaviour {

    // 'VARIABLES':

    private int incrementVariable;
    private int[] agentIDer;
    private bool[] agentIHasFired;
    private List<float> errorBuffer = new List<float>();
    private List<float> squiggleIDer = new List<float> {2f,1f,3f,4f};


    // 'MonoBehaviour':

    void Start() {
        // '"Array -> List"-conversion':
        //agentIDer = new int[10];
        //agentIDer[3] = 1;
        //agentIDer[5] = 1;
        //agentIDer[7] = 1;
        //agentIDer[1] = 1;
        //agentIDer[0] = 1;
        //DebugLogMyIntArray(agentIDer);
        //List<object> list = agentIDer.Cast<Object>().ToList();
        //List<int> nyeListo = new List<int>(agentIDer);
        //DebugLogMyIntList(nyeListo);


        // 'list-shifting':
        errorBuffer.Add(0.01f);
        errorBuffer.Add(0.02f);
        errorBuffer.Add(0.03f);
        errorBuffer.Add(0.04f);


        // 'boolean arrays in C#':
        //agentIHasFired = new bool[errorBuffer.Count];


        // '.csv-saving':
        //string path = System.IO.Directory.GetCurrentDirectory() + "\\" + "SavedData" + "\\" + "minForsteDataFil.csv";
        //List<int> CSVHeaderEntries = new List<int> { 2, 1, 3, 4};
        //CreateCSVWithIntHeader(path, CSVHeaderEntries);
    }

    void Update() {
        // 'KeyDown-if-clauses':

        if (Input.GetKeyDown(KeyCode.Space)) {
            // 'list-shifting':
            ShiftListWith(Random.Range(-20f, 20f));
        } else if (Input.GetKeyDown(KeyCode.P)) {
            // 'list-printing':
            DebugLogMyFloatList(errorBuffer);
        } else if (Input.GetKeyDown(KeyCode.S)) {
            // 'list-sorting':
            //List<float> sortertListe = GetMyDarnSortedList(errorBuffer);
            //DebugLogMyFloatList(sortertListe);
        } else if (Input.GetKeyDown(KeyCode.M)) {
            // 'Median-testing':
            float beTheMedianPls = ListMedian(errorBuffer);
            print("Medianen er forhåpentligvis: " + beTheMedianPls);
        } else if (Input.GetKeyDown(KeyCode.A)) {
            // 'Avg.-testing':
            float beTheAveragePls = ListAverage(errorBuffer);
            print("Gjennomsnittet er forhåpentligvis: " + beTheAveragePls);
        } else if (Input.GetKeyDown(KeyCode.H)) {
            // 'Invoke-testing':
            // Tester om Invoke's stopper sekvensen/koden, men det gjør den ikke. Linjene etter blir kjørt med en gang.
            Invoke("PrintHelloFromTheOtherSideIn2Sec", 5f);
            Debug.Log("Beat you to it suckah.");
        } else if (Input.GetKeyDown(KeyCode.I)) {
            // 'C# integer-increments':
            // Checking C# integer increments
            incrementVariable++;
            Debug.Log(incrementVariable);
        } else if (Input.GetKeyDown(KeyCode.A)) {
            // 'C# Any()-method':
            Debug.Log("Any-resultat: " + agentIHasFired.AsQueryable().Any(val => val == false));
        } else if (Input.GetKeyDown(KeyCode.C)) {
            // '.csv-saving':
            //string path = System.IO.Directory.GetCurrentDirectory() + "\\" + "SavedData" + "\\" + "minForsteDataFil.csv";
            //FloatUpdateCSV(path, squiggleIDer);

            //Debug.Log(System.IO.Directory.GetCurrentDirectory() + "\\" + "minForsteDataFil.csv");
            //foreach (int squig in squiggleIDer) {
            //    Debug.Log(squig);
            //}
        } // 'boolean arrays in C#':
        else if (Input.GetKeyDown(KeyCode.P)) {
            for (int i = 0; i < agentIHasFired.Length; i++) {
                Debug.Log("agentIHasFired[" + i + "]: " + agentIHasFired[i]);
            }
        } else if (Input.GetKeyDown(KeyCode.J)) {
            agentIHasFired[0] = true;
        } else if (Input.GetKeyDown(KeyCode.D)) {
            agentIHasFired[1] = true;
        } else if (Input.GetKeyDown(KeyCode.T)) {
            agentIHasFired[2] = true;
        } else if (Input.GetKeyDown(KeyCode.Q)) {
            agentIHasFired[3] = true;
        }
    }


    // 'HELPING':

        // 'lists':
    private List<float> GetMyDarnSortedList(List<float> listeAaSortere) {
        var klonetListe = new List<float>(listeAaSortere);
        klonetListe.Sort();

        return klonetListe;
    }

    void ShiftListWith(float thisInput) {
        errorBuffer.Add(thisInput);
        errorBuffer.RemoveAt(0);
    }

    float ListAverage(List<float> listeAaTaGjennomsnittAv) {
        return listeAaTaGjennomsnittAv.Average();
    }

        // 'printing':
    void PrintHelloFromTheOtherSideIn2Sec() {
        Debug.Log("Hello From The Other Side");
    }
}

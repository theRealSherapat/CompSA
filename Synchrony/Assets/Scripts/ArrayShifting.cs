using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ER ENTRIESA/VALUESA I ET C# ARRAY VERDIER ELLER REFERANSER?

public class ArrayShifting : MonoBehaviour {
    private List<float> errorBuffer = new List<float>();

    void Start() {
        errorBuffer.Add(0.01f);
        errorBuffer.Add(0.02f);
        errorBuffer.Add(0.03f);
    }

    void Update() {
        // SHIFTER ALLE Array-VERDIER VED Space-TASTETRYKK, OG SKRIVER UT Arrayet VED ET p-TASTETRYKK.
        if (Input.GetKeyDown(KeyCode.Space)) {
            ShiftListWith(Time.time);
        } else if (Input.GetKeyDown(KeyCode.P)) {
            print("START:");
            foreach (float fl in errorBuffer) {
                print("List-entry verdi: " + fl);
            }
            print("END.");

            // TEST-PRINTS:
                //Type type = a.GetValue(0).GetType();
                //print("En " + type.Name + " har array-entry verdi: " + a.GetValue(0));

                //print("10 + errorBuffer[0] = " + (10f + errorBuffer[0]));

                //print("errorBuffer.Median(): " + errorBuffer.Median()); FINN DENNE!
        }
    }
    void ShiftListWith(float thisInput) {
        errorBuffer.Add(thisInput);
        errorBuffer.RemoveAt(0);
    }

    // OLD FUNCTION:
        //void ShiftArrayWith(float newFloat) {
        //    // antar array-verdier er verdier, ikke referanser. feil antakelse?
        //    // old try
        //    //float[] shallowcopiedarray = (float[])a.clone();

        //    int arrayLength = a.Length;
        //    float nextFloat = (float)a.GetValue(0);
        //    a.SetValue(newFloat, 0);
        //    for (int i = 1; i < (arrayLength - 1); i++) {
        //        a.SetValue(nextFloat, i);
        //        nextFloat = (float)a.GetValue(i + 1);
        //    }
        //    a.SetValue(nextFloat, arrayLength - 1);

        //    // old try
        //    //a = shallowcopiedarray;
        //}
}

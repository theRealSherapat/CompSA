using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Text;

public static class SynchronyUtils {

    // '.CSV -CREATING & -UPDATING & -SAVING':

    public static void CreateCSVWithStringHeader(string path, List<string> headerEntries) {
        // Summary: creates a .CSV-file at arg1, <path>, with the top-line/header according to arg2, <headerEntries>.
        TextWriter tw = new StreamWriter(path, false, new UTF8Encoding(true));
        string firstLine = "";

        // Fyller inn firstLine med IDene til Dr. Squigglene f.eks.:
        for (int i = 0; i < headerEntries.Count; i++) {
            firstLine += headerEntries[i];

            if (i != headerEntries.Count - 1) firstLine += ";";
        }

        tw.WriteLine(firstLine);
        tw.Close();
    }

    public static void FloatUpdateCSV(string path, List<float> lineEntries) {
        TextWriter tw = new StreamWriter(path, true, new UTF8Encoding(true));
        string newLine = "";

        for (int i = 0; i < lineEntries.Count; i++) {
            newLine += $"{lineEntries[i]}";
            //newLine += string.Format("{0:N6}", lineEntries[i]);

            if (i != lineEntries.Count - 1) newLine += ";";
        }

        tw.WriteLine(newLine);
        tw.Close();
    }

    public static void LoggedColumnToCSV(string path, string header, List<float> allValuesColumn) {
        // Summary: creates a .CSV-file at arg1, <path>, with the top-line/header according to arg2, <headerEntries>.
        TextWriter tw = new StreamWriter(path, false, new UTF8Encoding(true));
        string csvLine = "";

        csvLine += header + "\r\n";

        for (int i = 0; i < allValuesColumn.Count - 1; i++) {
            csvLine += allValuesColumn[i] + "\r\n";
        }

        csvLine += allValuesColumn[allValuesColumn.Count-1];

        tw.WriteLine(csvLine);
        tw.Close();
    }

    public static void LoggedNestedValuesToCSV(string path, List<string> headerEntries, List<List<float>> allValueColumns) {
        // Summary: creates a .CSV-file at arg1, <path>, with the top-line/header according to arg2, <headerEntries>.
        TextWriter tw = new StreamWriter(path, false, new UTF8Encoding(true));
        string csvLine = "";
        for (int i = 0; i < headerEntries.Count; i++) {
            csvLine += headerEntries[i];

            if (i != headerEntries.Count - 1) csvLine += ";";
        }
        csvLine += "\r\n";

        for (int m = 0; m < allValueColumns[0].Count-1; m++) { // hardcoding the termination-criteria a bit to avoid the last CR/LF-symbols.
            for (int n = 0; n < allValueColumns.Count; n++) {
                //csvLine += string.Format("{0:N6}", allValueColumns[n][m]);
                csvLine += $"{allValueColumns[n][m]}";

                if (n != allValueColumns.Count - 1) csvLine += ";";
            }
            csvLine += "\r\n";
        }

        for (int n = 0; n < allValueColumns.Count; n++) {
            //csvLine += string.Format("{0:N6}", allValueColumns[n][allValueColumns[0].Count-1]);
            csvLine += $"{allValueColumns[n][allValueColumns[0].Count - 1]}";

            if (n != allValueColumns.Count - 1) csvLine += ";";
        }

        tw.WriteLine(csvLine);
        tw.Close();
    }

    



    // 'Unity Scene & Game':

    public static void QuitMyGame() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        UnityEngine.Application.Quit();
#endif
    }




    // 'LISTS':

    public static List<float> ShiftFloatListRightToLeftWith(List<float> thisList, float thisInput) {
        thisList.Add(thisInput);
        thisList.RemoveAt(0);

        return thisList;
    }

    public static float ListMedian(List<float> myList) {
        // Improvement possibilities:
        //
        // 1) Optimization in performance:
            // If performance is too slow, consider calculating the (Count-1)/2-th order statistic.
            // "Median is simply (Count-1)/2-order statistic." - https://newbedev.com/calculate-median-in-c

        // 2) Generalizability:
            // Make the function generic, and open to be used on all (or more at least) datatypes, and not just floats.

        var clonedList = new List<float>(myList);
        clonedList.Sort();

        List<float> sortedList = clonedList;

        float median;
        if (sortedList.Count % 2 == 1) { // we have an 'odd-length' list.
            int medianIndex = (sortedList.Count - 1) / 2;
            median = sortedList[medianIndex];
        } else { // we have an 'even-length' list.
            int lowerMedianIndex = Mathf.FloorToInt((sortedList.Count - 1) / 2.0f);
            int upperMedianIndex = Mathf.CeilToInt((sortedList.Count - 1) / 2.0f);
            median = (sortedList[lowerMedianIndex] + sortedList[upperMedianIndex]) / 2.0f; // using average median instead of lower middle median, which it seems like Nymoen also used in his Matlab-implementation.
        }

        return median;
    }

    public static float ListAverage(List<float> myList) {
        return myList.Count > 0 ? myList.Average() : 0.0f;
    }

    public static void DebugLogMyFloatList(List<float> listo) {
        Debug.Log("START:");
        for (int i = 0; i < listo.Count; i++) {                         // selvom 'for-in' loops i C# virker til å være ordered.
            Debug.Log("List-entry " + i + " sin verdi: " + listo[i]);
        }
        Debug.Log("END.");
    }

    public static void DebugLogMyIntList(List<int> listo) {
        Debug.Log("START:");
        for (int i = 0; i < listo.Count; i++) {                         // selvom 'for-in' loops i C# virker til å være ordered.
            Debug.Log("List-entry " + i + " sin verdi: " + listo[i]);
        }
        Debug.Log("END.");
    }




    // 'ARRAYS':

    public static void DebugLogMyIntArray(int[] arrayo) {
        Debug.Log("START:");
        for (int i = 0; i < arrayo.Length; i++) {                         // selvom 'for-in' loops i C# virker til å være ordered.
            Debug.Log("Array-entry " + i + " sin verdi: " + arrayo[i]);
        }
        Debug.Log("END.");
    }




    // 'DATATYPE CONVERSIONS':

    public static string BoolToString(bool value) {
        if (value) return "True";
        else return "False";
    }
}

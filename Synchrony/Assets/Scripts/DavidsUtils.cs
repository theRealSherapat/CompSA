using System.Collections.Generic;
using UnityEngine;

public static class DavidsUtils {
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
            median = (sortedList[lowerMedianIndex] + sortedList[upperMedianIndex]) / 2.0f; // using average median instead of lower middle median.
        }

        return median;
    }

    public static void DebugLogMyFloatList(List<float> listo) {
        Debug.Log("START:");
        for (int i = 0; i < listo.Count; i++) {                         // selvom 'for-in' loops i C# virker til å være ordered.
            Debug.Log("List-entry " + i + " sin verdi: " + listo[i]);
        }
        Debug.Log("END.");
    }
}

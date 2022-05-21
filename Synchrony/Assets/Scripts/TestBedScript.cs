using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static SynchronyUtils;
using System.Collections;
using System.IO;
using System.Globalization;

public class TestBedScript : MonoBehaviour {

    // 'VARIABLES':


        // DITTA FONHGERA:
    //TestBedScript megSelvo;


    Dictionary<int, float> agentIDdistanceDictionary = new Dictionary<int, float>();



    // 'MonoBehaviour':

    void Start() {
            // DITTA FONHGERA:
        //var k_or_d = "jeo";

        //if (k_or_d.GetType() == typeof(int)) {
        //    Debug.Log("Bruh.., vi har et heltall for hvor mange av de nærmeste naborobotene vi skal legge oss selv (eller vår egen AgentID) i en subscriber-liste.");
        //} else if (k_or_d.GetType() == typeof(float)) {
        //    Debug.Log("Bruhh..., vi har en radius d jo der alle robotene innenfor denne radiusen sine subscriber lister skal Addes med oss selv eller vår egen AgentID.");
        //} else {
        //    megSelvo = this;
        //    Debug.Log("meg selv:" + megSelvo.GetType());
        //}



        agentIDdistanceDictionary.Add(0, 115.4f);
        agentIDdistanceDictionary.Add(1, 15.4f);
        agentIDdistanceDictionary.Add(2, 113.4f);
        agentIDdistanceDictionary.Add(3, 1115.4f);
        agentIDdistanceDictionary.Add(4, 11113.4f);
        agentIDdistanceDictionary.Add(5, 11115.4f);
        agentIDdistanceDictionary.Add(6, 13.4f);
        agentIDdistanceDictionary.Add(7, 5.4f);
        agentIDdistanceDictionary.Add(8, 1111113.4f);



        // DITTA FONHGERA:
        List<KeyValuePair<int, float>> meinList = agentIDdistanceDictionary.ToList();

        Debug.Log("Det fjerde elementet i losto: " + meinList.ElementAt<KeyValuePair<int, float>>(4 - 1));

        //meinList.Sort((x, y) => x.Value.CompareTo(y.Value));


        //var firstTwoItems = meinList.Take(2).ToList();
    }
}

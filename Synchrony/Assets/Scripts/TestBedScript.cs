using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static SynchronyUtils;
using System.Collections;
using System.IO;
using System.Globalization;

public class TestBedScript : MonoBehaviour {

    // 'VARIABLES':

    TestBedScript megSelvo;




    // 'MonoBehaviour':

    void Start() {
        var k_or_d = "jeo";

        if (k_or_d.GetType() == typeof(int)) {
            Debug.Log("Bruh.., vi har et heltall for hvor mange av de nærmeste naborobotene vi skal legge oss selv (eller vår egen AgentID) i en subscriber-liste.");
        } else if (k_or_d.GetType() == typeof(float)) {
            Debug.Log("Bruhh..., vi har en radius d jo der alle robotene innenfor denne radiusen sine subscriber lister skal Addes med oss selv eller vår egen AgentID.");
        } else {
            megSelvo = this;
            Debug.Log("meg selv:" + megSelvo.GetType());
        }
    }
}

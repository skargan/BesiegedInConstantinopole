#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Called by the presentation capture; restores campaign and RNG after visibility fixtures.
public static class IntelligenceSpySummaryChecks
{
    public static int Run(CouncilUI council)
    {
        int checks=0;
        var saved=VariableSingleton.Instance.CaptureState();var rng=UnityEngine.Random.state;
        Action<bool,string> check=(ok,why)=>{if(!ok)throw new InvalidOperationException("SPY SUMMARY: "+why);checks++;};
        try {
            foreach(int agents in new[]{0,7,8,10,7})foreach(int enemies in new[]{0,4}){
                VariableSingleton.SetInt("iSpies",agents);VariableSingleton.SetInt("iTurkSpies",enemies);
                foreach(bool operations in new[]{false,true}){
                    council.Spies();
                    if(operations)GameObject.Find("Council screen").GetComponentsInChildren<Button>().Single(b=>b.name=="Intelligence operations tab").onClick.Invoke();
                    Canvas.ForceUpdateCanvases();
                    var network=GameObject.Find("Intelligence network");check(network!=null,"summary exists in both tabs");
                    var count=network.GetComponentsInChildren<TMP_Text>().Single(t=>t.name=="Enemy spies summary");
                    check(count.text=="ENEMY SPIES  "+(agents>=8?enemies.ToString("N0"):"?"),"count respects current infiltration, including zero and lost access");
                    foreach(var label in network.GetComponentsInChildren<TMP_Text>()){
                        label.ForceMeshUpdate();check(!label.isTextOverflowing,"header text fits: "+label.text);
                    }
                    check(VariableSingleton.GetIntVariable("iTurkSpies")==enemies&&VariableSingleton.GetIntVariable("iSpies")==agents,"viewing does not change agents");
                }
            }
        } finally {VariableSingleton.Instance.RestoreValidated(saved);UnityEngine.Random.state=rng;}
        return checks;
    }
}
#endif

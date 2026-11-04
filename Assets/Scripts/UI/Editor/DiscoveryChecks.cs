#if UNITY_EDITOR
using System;
using UnityEngine;
public static class DiscoveryChecks
{
    public static void Run(){
        var e=new TechnologyDiscoveries.Experiment();
        Require(!e.Advance(1,0),"cannot skip first step");
        for(int i=0;i<6;i++)Require(e.Advance(i,i*3),"sequential step "+i);
        Require(!e.Advance(6,20),"six steps only");
        e.Reset();Require(e.Step==0,"screen reset");e.Advance(0,10);e.Tick(69);Require(e.Step==1,"retain before idle timeout");
        e.Tick(70);Require(e.Step==0,"60 second idle reset");
        Require(Achievements.Names.Length==AchievementPlatform.Ids.Length,"catalogue identity alignment");
        Require(Achievements.Descriptions.Length==Achievements.Names.Length,"catalogue descriptions alignment");
        Require(!SaveGames.DiscoveryAchievementEligible,"uninitialised campaigns cannot earn discoveries");
        Debug.Log("DISCOVERY FOUNDATION CHECKS PASS: sequence, inactivity reset, catalogue, eligibility.");
    }
    static void Require(bool ok,string name){if(!ok)throw new Exception("Discovery check: "+name);}
}
#endif

#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
public static class AI32Exercise
{
    static int I(string k)=>DailyLifeEvents.I(k);
    static void Set(string k,int v)=>VariableSingleton.SetInt(k,v);
    public static int Run(){
        var baseline=VariableSingleton.Instance.CaptureState();int checks=0,assaults=0,days=0;var lines=new System.Collections.Generic.List<string>{"seed,personality,days,assaults,last_wall,city_lost"};
        try{for(int seed=0;seed<16;seed++){
            VariableSingleton.Instance.RestoreValidated(baseline);UnityEngine.Random.InitState(320100+seed);Set("iOttDirectorVersion",0);typeof(VariableSingleton).GetField("turn",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,16);OttomanDirector.Ensure();Set("iOttPersonality",(1+seed%3));Set("iEnemyDelay",0);int runAssaults=0,runDays=0;
            for(int day=0;day<90&&!VariableSingleton.Instance.EnduringWall.CityLost;day++){
                typeof(VariableSingleton).GetField("turn",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,16+day);DefencePosture.Advance();OttomanDirector.PrepareTurn();var plan=OttomanDirector.Plan;string preview=OttomanDirector.PlanReport;AI.Instance.TakeAIAction();int food=I("iFood");var pending=AI.Instance.PendingPhased;AI.Instance.TakeAIAction();if(I("iFood")!=food||AI.Instance.PendingPhased!=pending)throw new Exception("Repeated Ottoman screen consumed turn or replaced battle");checks++;
                if(pending!=null&&!pending.Committed){assaults++;runAssaults++;pending.Shelter(.65f);if(pending.ArrowScreens&&pending.Cannons>0&&pending.Ammo>0)pending.FireAtScreens();while(pending.Shots<3)pending.Volley(0);pending.Approach(VariableSingleton.GetFloatVariable("fMoat"),0);pending.AssignReserve(50);for(int round=0;round<3;round++)pending.Clash();pending.Resolve(pending.Guards>=pending.GuardsNeeded);AI.Instance.CommitPhased(pending);if(!pending.Committed)throw new Exception("Battle failed to commit");checks++;}
                if(string.IsNullOrEmpty(AI.Instance.LastReport)||AI.Instance.LastReport=="No major Ottoman operation was completed today.")throw new Exception("Silent Ottoman operation "+plan+" seed"+seed);checks++;days++;runDays++;
            }
            lines.Add((320100+seed)+","+((1+seed%3))+","+runDays+","+runAssaults+","+VariableSingleton.Instance.EnduringWall.GetCodeName()+","+VariableSingleton.Instance.EnduringWall.CityLost);
        }
        if(assaults<16)throw new Exception("Insufficient executed assault coverage: "+assaults);File.WriteAllLines(Path.Combine(SaveGames.TestRoot,"siege-exercises.csv"),lines);Debug.Log("AI32_EXECUTION_EXERCISES seeds16;days="+days+";assaults="+assaults+";checks="+checks);return checks;
        }finally{VariableSingleton.Instance.RestoreValidated(baseline);}
    }
}
#endif

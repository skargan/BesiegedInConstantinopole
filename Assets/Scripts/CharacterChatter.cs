using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Incidental speech only: never gates trading, quests, or full conversations.
// Saved per character, independent of scene instances and the campaign RNG.
public static class CharacterChatter
{
    public const int DailyLimit=3;
    public const double CooldownSeconds=3;
    public const string Raimondina="raimondina", GrainMerchant="grain_merchant";
    public const string RaimondinaFarewell="I have enjoyed this, Konstantin, but I have other business to attend to. Talk to you tomorrow.";
    public const string MerchantFarewell="Tomorrow, Majesty. If I spend the whole day talking, we shall both go hungry.";
    const string Prefix="iChatter_";
    static readonly System.Random cosmetic=new System.Random();
    static readonly DateTime Epoch=new DateTime(2020,1,1,0,0,0,DateTimeKind.Utc);
    static double Now=> (DateTime.UtcNow-Epoch).TotalSeconds;
    static string Key(string actor,string field)=>Prefix+actor+"_"+field;
    static int Read(string actor,string field)=>DailyLifeEvents.I(Key(actor,field));
    static void Put(string actor,string field,int value)=>VariableSingleton.SetInt(Key(actor,field),value);
    static void Ensure(string actor)
    {
        int day=TrainingPool.Day(VariableSingleton.GetDate());
        if(Read(actor,"Day")==day)return;
        Put(actor,"Day",day);Put(actor,"Count",0);Put(actor,"Shown",0);
        Put(actor,"UntilSeconds",0);Put(actor,"UntilMillis",0);
    }
    static double Until(string actor)=>Read(actor,"UntilSeconds")+Read(actor,"UntilMillis")/1000.0;
    public static int Count(string actor){Ensure(actor);return Read(actor,"Count");}
    public static bool Spent(string actor,double? now=null){Ensure(actor);return Read(actor,"Count")>=DailyLimit||(now??Now)<Until(actor);}
    public static string Current(string actor,string[] lines,string farewell)
    {
        Ensure(actor);int shown=Read(actor,"Shown");
        return shown<0?farewell:shown>0&&shown<=lines.Length?lines[shown-1]:"";
    }
    public static string Request(string actor,string[] lines,string farewell,double? now=null)
    {
        Ensure(actor);double clock=now??Now;
        if(clock<Until(actor)||Read(actor,"Shown")<0)return Current(actor,lines,farewell);
        if(Read(actor,"Count")>=DailyLimit){Put(actor,"Shown",-1);Checkpoint(now);return farewell;}
        if(lines==null||lines.Length==0)return "";
        int cycle=Math.Max(1,Read(actor,"Cycle"));var pool=new List<int>();
        for(int i=0;i<lines.Length;i++)if(Read(actor,"Seen"+i)!=cycle)pool.Add(i);
        if(pool.Count==0){cycle++;for(int i=0;i<lines.Length;i++)pool.Add(i);}
        // Avoid repeating the last entry across a completed cycle as well.
        int last=Read(actor,"Last")-1;if(pool.Count>1)pool.Remove(last);
        int selected=pool[cosmetic.Next(pool.Count)];
        Put(actor,"Cycle",cycle);Put(actor,"Seen"+selected,cycle);Put(actor,"Last",selected+1);
        Put(actor,"Shown",selected+1);Put(actor,"Count",Read(actor,"Count")+1);
        double until=clock+CooldownSeconds;int seconds=(int)Math.Floor(until);
        Put(actor,"UntilSeconds",seconds);Put(actor,"UntilMillis",(int)Math.Ceiling((until-seconds)*1000));
        Checkpoint(now);
        return lines[selected];
    }
    static void Checkpoint(double? testClock){if(!testClock.HasValue&&SaveGames.CanCheckpointCouncil)SaveGames.Checkpoint();}
    public static void ResetForRestore()
    {
        if(VariableSingleton.Instance==null)return;
        foreach(var value in VariableSingleton.Instance.CaptureState().ints)
            if(value.key.StartsWith(Prefix,StringComparison.Ordinal))VariableSingleton.SetInt(value.key,0);
    }
}

// Hover/focus requests speech; remaining on the target never advances it.
public sealed class CharacterChatterTrigger:MonoBehaviour,IPointerEnterHandler,ISelectHandler
{
    public string actor;
    public Action speak;
    public LocationHotspot hotspot;
    public void OnPointerEnter(PointerEventData e)=>speak?.Invoke();
    public void OnSelect(BaseEventData e)=>speak?.Invoke();
    void Update(){if(hotspot!=null)hotspot.SetSpent(CharacterChatter.Spent(actor));}
}

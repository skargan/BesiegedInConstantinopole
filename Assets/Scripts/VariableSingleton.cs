using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Yarn.Unity;
using System.Collections;
using System.Globalization;

[DefaultExecutionOrder(-1000)]
public class VariableSingleton : MonoBehaviour
{
    static private VariableSingleton _instance;
    static public VariableSingleton Instance => _instance;


    static int turn = 0;
    static public int Turn
    {
        get => turn;
        private set => turn = value;
    }
    public static string ArrivalNotice="";
    public List<TrainingPool.Order> TrainingOrders = new List<TrainingPool.Order>();
    static public int IncrementTurn()
    {
        var previousDate=GetDate();
        bool completedSiegeDay=HasSiegeStarted();
        EspionageSystem.AdvanceTurn();
        turn++;
        if (Instance.overrideTurn > -1) { turn = Instance.overrideTurn; Instance.overrideTurn = -1; }
        if(CompletedSiegeDays>0 && TechnologySystem.Has("RomanTaxation"))
        {
            int income=PopulationAccounting.Tax(); ChangeInt("iMoney",income);
            ArrivalNotice="Fiscal administration: +"+income.ToString("N0")+" gold. Treasury now "+GetIntVariable("iMoney").ToString("N0")+".\n\n";
        }
        else ArrivalNotice="";
        if(completedSiegeDay) { ArrivalNotice+=SupplyEconomy.CompleteDay(); NavalState.CompleteDay(); ArrivalNotice+=FleetOperations.CompleteDay(); }
        if(CompletedSiegeDays>0 && GetIntVariable("iIncome")>0) {int income=GetIntVariable("iIncome");ChangeInt("iMoney",income);ArrivalNotice+="Merchant levy: +"+income+" gold.\n\n";}
        if(completedSiegeDay)ArrivalNotice+=DefencePosture.Advance();
        ArrivalNotice+=CouncilActions.AdvanceAid(Math.Max(0,(GetDate()-previousDate).Days));
        ArrivalNotice+=TrainingPool.CompleteDueOrders(GetDate());
        ArrivalNotice+=TroopTraining.Complete(GetDate());
        ArrivalNotice+=PreparationCampaign.Advance();
        ArrivalNotice+=PoliticalSystem.AdvancePolitics();
        Achievements.CheckCampaign();
        return turn;
    }
    public int overrideTurn = -1;

    [SerializeField] int turnsToSiege;

    public static bool QuickStartRequested;
    public static string PreparationPresetPath => Path.Combine(SaveGames.Root,"preparation-preset-v1.json");
    public static bool HasPreparationPreset => File.Exists(PreparationPresetPath);
    public static int EffectiveSiegeTurn => PreparationCampaign.Active?PreparationCampaign.SiegeTurn:_instance.turnsToSiege;
    public static int CompletedSiegeDays => _instance == null ? 0 : Mathf.Max(0, turn - EffectiveSiegeTurn);
    [YarnFunction("GetPopulation")]
    public static string PopulationInPeople() => PopulationAccounting.People.ToString("N0", CultureInfo.InvariantCulture);

    public static bool HasSiegeStarted() => turn >= EffectiveSiegeTurn;

    public static int SiegeDay => _instance == null ? 0 : Mathf.Max(0, turn - EffectiveSiegeTurn + 1);

    static DateTime siegeStart = new(1453, 4, 6);
    static public DateTime GetDate()
    {
        if(PreparationCampaign.Active)return PreparationCampaign.Date(turn);
        if (turn < _instance.turnsToSiege)
        {
            return siegeStart - new TimeSpan((_instance.turnsToSiege - turn) * 7, 0, 0, 0);
        }
        else
        {
            return siegeStart + new TimeSpan(turn - EffectiveSiegeTurn, 0, 0, 0);
        }
    }

    [SerializeField] public RecruitmentData[] RecruitmentPricelist;

    [SerializeField] public WallData EnduringWall = new WallData("OuterWall");
    public void Abandon() => EnduringWall.Abandon();

    /*******************
     * YARN VARIABLES */

    static Dictionary<string, bool> bDict = new Dictionary<string, bool>();
    static Dictionary<string, int> iDict = new Dictionary<string, int>();
    static Dictionary<string, float> fDict = new Dictionary<string, float>();
        
    [YarnFunction("GetBool")]
    static public bool GetBoolVariable(string varName)
    {
        return bDict[varName];
    }


    [YarnFunction("GetInt")]
    static public int GetIntVariable(string varName)
    {
        return iDict[varName];
    }


    [YarnFunction("GetFloat")]
    static public float GetFloatVariable(string varName)
    {
        return fDict[varName];
    }

    [YarnFunction("GetPercent")]
    static public string GetPercentFloat(string varName)
    {
        return Mathf.RoundToInt(100 * fDict[varName]).ToString() + "%";
    }

    [YarnCommand("SetInt")]
    static  public void SetInt(string varName, int newValue) 
    {
        iDict[varName] = newValue;
        if(varName=="iGrandCannon" || varName=="iAdventurerArrived" || varName=="iShahStatus")Achievements.CheckCampaign();
    }


    [YarnCommand("Change")]
    static public void Change(string varName, string value = "1")
    {
        float Value = value.Equals("true", StringComparison.OrdinalIgnoreCase) ? 1 :
            value.Equals("false", StringComparison.OrdinalIgnoreCase) ? 0 :
            float.Parse(value, CultureInfo.InvariantCulture);
        switch (varName[0])
        {
            case 'b':
                {
                    ChangeBool(varName, Value!=0 ? true : false);
                    break;
                }

            case 'i':
                {
                    ChangeInt(varName, Mathf.RoundToInt(Value));
                    break;
                }

            case 'f':
                {
                    ChangeFloat(varName, Value);
                    break;
                }
        }
    }

    [YarnCommand("RndChange")]
    static public void RndChange(string varName, float min, float max)
    {
        if (varName[0] == 'i') ChangeInt(varName, (int)min, (int)max);
        else if (varName[0] == 'f') ChangeFloat(varName, min, max);
    }


    static public void ChangeBool(string varName, bool newValue)
    {
        int character=Array.IndexOf(CourtRoster.Keys,varName);
        if(newValue && character>=0 && iDict.TryGetValue("iAllegiance"+character,out int bloc) && bloc>0 && PoliticalSystem.Banned(bloc-1)) return;
        bDict[varName] = newValue;
        CourtRoster.Record(varName,newValue);
        Achievements.CheckCampaign();
    }

    static public void ChangeInt(string varName, int delta)
    {
        Achievements.TroopLoss(varName,Math.Min(iDict[varName],Math.Max(0,-delta)));
        if (iDict[varName] + delta < 0)
        {
            iDict[varName] = 0;
        }
        else
        {
            iDict[varName] += delta;
        }
    }

    static public void ChangeInt(string varName, int min, int max)
    {
        ChangeInt(varName, UnityEngine.Random.Range(Math.Min(min, max), Math.Max(min, max) + 1));
    }

    static public void ChangeFloat(string varName, float delta)
    {
        // Equipment quality is a lethality multiplier; other campaign floats are fractions.
        float value=fDict[varName]+delta;
        Achievements.WallChanged(varName,fDict[varName],Mathf.Clamp01(value));
        fDict[varName]=varName.EndsWith("Quality")?Mathf.Max(0,value):Mathf.Clamp01(value);
    }

    [YarnCommand("ChangeFraction")]
    static public void ChangePercentFloat(string varName, float delta) 
    {
        if (fDict[varName] + delta > 1)
        {
            Achievements.WallChanged(varName,fDict[varName],1);
            fDict[varName] = 1;
        }
        else ChangeFloat(varName, delta);
    }

    static public void ChangeFloat(string varName, float min, float max)
    {
        ChangeFloat(varName, UnityEngine.Random.Range(Math.Min(min, max), Math.Max(min, max)));
    }


    /*[SerializeField] List<float> 


    [YarnCommand("UpgradeArmy")]
    static public void UpgradeArmyQuality(string armyType, int grades = 1) 
    { 
        
    }*/

    /*************************
     *VARIABLE INITIALISATION*/

    public static int LastCheckpointTurn = -1;
    public static bool ContinueRequested;
    public static bool IsInitialized { get; private set; }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public static string TestSavePath;
#endif
    public static string SavePath
    {
        get
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!string.IsNullOrEmpty(TestSavePath)) return TestSavePath;
#endif
            return SaveGames.CurrentPath;
        }
    }

    private void InitializeVariables()
    {
        ArrivalNotice="";
        TrainingOrders.Clear();
        bDict.Clear();
        iDict.Clear();
        fDict.Clear();
        turn = 0;
        LastCheckpointTurn = -1;
        EnduringWall = new WallData("OuterWall");
        var data = Resources.Load<TextAsset>("Constantinopole_System_Variables");
        if (data == null) throw new InvalidOperationException("Starting campaign data is missing.");
        foreach (string row in data.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            string[] cells = row.Split(',');
            if (cells.Length != 2) throw new FormatException("Invalid campaign data: " + row);
            string key = cells[0].Trim();
            string value = cells[1].Trim();
            switch (key[0])
            {
                case 'b': bDict[key] = bool.Parse(value); break;
                case 'i': iDict[key] = int.Parse(value, CultureInfo.InvariantCulture); break;
                case 'f': fDict[key] = float.Parse(value, CultureInfo.InvariantCulture); break;
            }
        }
        PreparationCampaign.InitializeDefaults();
        DefencePosture.Initialise();
        IsInitialized = true;
        Achievements.BeginCampaign();
    }

    /*********************
     * STORY INDEX */

    [SerializeField] List<int> possibleStory = new List<int>() {0, 1, 2, 3};
    [SerializeField] List<int> possibleRandom = new List<int>();

    public bool StoryScheduled()
    {
        return PreparationCampaign.Active ? PreparationCampaign.FixedNode(turn)!=null : possibleStory.Contains(turn);
    }

    public int RandomEventID()
    {
        var candidates = (possibleRandom.Count == 0 ? Enumerable.Range(0,23).ToList() : possibleRandom).Concat(new[]{23}).Distinct().Where(RecurringEvents.Eligible).ToList();
        return candidates.Count==0 ? -1 : candidates[UnityEngine.Random.Range(0,candidates.Count)];
    }

    [YarnCommand("AddEvent")]
    public void AddPossibleEvent(string eventList, int eventId)
    {
        switch (eventList) 
        {
            case "story":
                if (!possibleStory.Contains(eventId))
                {
                    possibleStory.Add(eventId);
                }
                break;

            case "random":
                if (!possibleRandom.Contains(eventId))
                {
                    possibleRandom.Add(eventId);
                }
                break;
        }
    }

    [YarnCommand("RemoveEvent")]
    public void RemovePossibleEvent(string eventList, int eventId)
    {
        switch (eventList)
        {
            case "story":
                if (possibleStory.Contains(eventId))
                {
                    possibleStory.Remove(eventId);
                }
                break;

            case "random":
                if (possibleRandom.Contains(eventId))
                {
                    possibleRandom.Remove(eventId);
                }
                break;
        }
    }


    /****************
     * SAVE SERVICE */
    [Serializable] public class BoolValue { public string key; public bool value; }
    [Serializable] public class IntValue { public string key; public int value; }
    [Serializable] public class FloatValue { public string key; public float value; }
    [Serializable] public class CampaignSave
    {
        public int version = 2;
        public List<TrainingPool.Order> training = new List<TrainingPool.Order>();
        public int turn;
        public string wall;
        public bool cityLost;
        public UnityEngine.Random.State randomState;
        public List<int> story;
        public List<int> randomEvents;
        public List<BoolValue> bools = new List<BoolValue>();
        public List<IntValue> ints = new List<IntValue>();
        public List<FloatValue> floats = new List<FloatValue>();
    }

    public static bool HasSave(){try {return SaveGames.LatestPath()!=null;}catch(Exception){return false;}}

    // Checkpoints are made at the start of management, never halfway through an event.
    public CampaignSave CaptureState()
    {
        var data = new CampaignSave {
            turn = turn, wall = EnduringWall.GetCodeName(), cityLost = EnduringWall.CityLost,
            randomState = UnityEngine.Random.state,
            story = new List<int>(possibleStory), randomEvents = new List<int>(possibleRandom)
        };
        foreach(var order in TrainingOrders) data.training.Add(order.Copy());
        foreach (var pair in bDict) data.bools.Add(new BoolValue { key = pair.Key, value = pair.Value });
        foreach (var pair in iDict) data.ints.Add(new IntValue { key = pair.Key, value = pair.Value });
        foreach (var pair in fDict) data.floats.Add(new FloatValue { key = pair.Key, value = pair.Value });
        return data;
    }

    public string Save(){
        if(!SaveGames.Checkpoint())return "";
        string json=JsonUtility.ToJson(CaptureState(),true);
        if(!SaveGames.Ironman&&turn==EffectiveSiegeTurn)SaveGames.AtomicWrite(PreparationPresetPath,json);
        return json;
    }
    public void Load(){string path=SaveGames.RequestedFile??SaveGames.LatestPath();if(path!=null)SaveGames.Load(path,out var error);}
    public void RestoreValidated(CampaignSave data){RestoreState(data);}
    private void RestoreState(CampaignSave data)
    {
        DefencePosture.Initialise();
        PreparationCampaign.InitializeDefaults(legacy: true);
        // New systems also reset when loading an older checkpoint in the same session.
        foreach(string key in new[]{"bTechVarangianDrills","bTechDismountedChampions"}) bDict[key]=false;
        foreach(string key in new System.Collections.Generic.List<string>(iDict.Keys))
            if(key.StartsWith("iAchievement") || key.StartsWith("iHard") || key.StartsWith("iCombat") || key=="iEmperorAtWall" || key.StartsWith("iLife") || key.StartsWith("iLady") || (key.StartsWith("iPopulation") && key!="iPopulation") || key.StartsWith("iDrill") || key.StartsWith("iTerms") || key.StartsWith("iSurrender") || key.StartsWith("iSultanSeen") || key=="iSultanConversationDay") iDict[key]=0;
        iDict["iMerchantFavor"] = 50;
        iDict["iFencing"] = 25;
        turn = data.turn;
        TrainingOrders = new List<TrainingPool.Order>();
        if(data.training!=null) foreach(var order in data.training) TrainingOrders.Add(order.Copy());
        EnduringWall.Restore(data.wall, data.cityLost);
        foreach (var pair in data.bools) bDict[pair.key] = pair.value;
        if(!data.ints.Any(p=>p.key=="iTurkShips")) iDict["iTurkShips"]=bDict["bTurkFleet"]?NavalState.StartingShips:0;
        iDict["iCampAlert"] = 0; // Saves from before the alert system start at zero.
        foreach (var pair in data.ints) iDict[pair.key] = pair.value;
        iDict["iPopulation"] = checked((int)PopulationAccounting.Count(data));
        iDict.Remove("iPopulationRemainder");
        iDict.Remove("iPopulationFoodCarry");
        iDict["iCampAlert"] = Mathf.Clamp(iDict["iCampAlert"],0,100);
        foreach (var pair in data.floats) fDict[pair.key] = pair.key.EndsWith("Quality")?Mathf.Max(0,pair.value):Mathf.Clamp01(pair.value);
        possibleStory = data.story ?? possibleStory;
        possibleRandom = data.randomEvents ?? possibleRandom;
        UnityEngine.Random.state = data.randomState;
    }

    private void Awake()
    {
        _instance = this;
        SaveGames.BeginNew();
        InitializeVariables();
        if (QuickStartRequested && HasPreparationPreset)
            RestoreState(JsonUtility.FromJson<CampaignSave>(File.ReadAllText(PreparationPresetPath)));
        else if (ContinueRequested) Load();
        QuickStartRequested = false;
        ContinueRequested = false;
    }

    private void OnDestroy()
    {
        if (_instance == this) { _instance = null; IsInitialized = false; }
    }

    /*********
     * DEBUG */

    [ContextMenu("PrintVariables")]
    public void PrintVariables()
    {
        foreach (KeyValuePair<string, bool> b in bDict)
        {
            Debug.Log(b.Key + " : " + b.Value);
        }

        foreach (KeyValuePair<string, int> b in iDict)
        {
            Debug.Log(b.Key + " : " + b.Value);
        }

        foreach (KeyValuePair<string, float> b in fDict)
        {
            Debug.Log(b.Key + " : " + b.Value);
        }
    }

}

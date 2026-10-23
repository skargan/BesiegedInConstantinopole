using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

// Portable archive format wraps the existing campaign schema; UI never writes files itself.
public static class SaveGames {
    public const int Format=2;
    [Serializable] public class Archive {
        public int saveFormat=Format;
        public string build, campaignId, name, savedUtc, calendar, phase="council", node, title, report, notice;
        public bool ironman, terminal;
        public int sequence;
        public string payloadHash;
        public VariableSingleton.CampaignSave campaign;
    }
    public class Entry {public string path;public Archive archive;public string error;public bool Compatible=>error==null;}
    public static string TestRoot;
    public static string RequestedFile;
    public static bool RequestedIronman;
    public static bool NewRequested;
    public static bool Ironman {get;private set;}
    public static bool Terminal {get;private set;}
    public static string CampaignId {get;private set;}="";
    public static int Sequence {get;private set;}
    public static string LastError {get;private set;}="";
    public static string Phase {get;private set;}="council";
    public static Archive PendingResume;
    static bool restoring;
    static string folder;
    public static bool Automated {
        get {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return !string.IsNullOrEmpty(VariableSingleton.TestSavePath);
#else
            return false;
#endif
        }
    }
    public static string Root {
        get {
            if(!string.IsNullOrEmpty(TestRoot)){Directory.CreateDirectory(TestRoot);return Path.GetFullPath(TestRoot);}
            if(Automated){var path=Path.Combine(Path.GetDirectoryName(VariableSingleton.SavePath),"TestArchives");Directory.CreateDirectory(path);return path;}
            if(folder!=null)return folder;
            string local=Path.GetFullPath(Path.Combine(Application.dataPath,"..","Saves"));
            try {Directory.CreateDirectory(local);string probe=Path.Combine(local,".write-check");File.WriteAllText(probe,"");File.Delete(probe);folder=local;}
            catch(IOException){folder=Path.Combine(Application.persistentDataPath,"Saves");Directory.CreateDirectory(folder);}
            catch(UnauthorizedAccessException){folder=Path.Combine(Application.persistentDataPath,"Saves");Directory.CreateDirectory(folder);}
            return folder;
        }
    }
    static string SafeId(string value)=>new string((value??"").Where(c=>char.IsLetterOrDigit(c)||c=='-').Take(64).ToArray());
    public static string CurrentPath=>Path.Combine(Root,(Ironman?"ironman-":"autosave-")+SafeId(CampaignId)+".besieged.json");
    public static bool CanCheckpointCouncil=>VariableSingleton.IsInitialized&&!Terminal&&Phase=="council"&&PhasedAssaultUI.Current==null&&AssaultDuelUI.Current==null&&LadderDefenceUI.Current==null&&UnityEngine.Object.FindObjectOfType<ManagerUI>(true)!=null&&UnityEngine.Object.FindObjectOfType<EventUI>()==null&&UnityEngine.Object.FindObjectOfType<RestUI>()==null&&UnityEngine.Object.FindObjectOfType<AssaultUI>()==null;
    public static bool CanManualSave=>CanCheckpointCouncil&&!Ironman;
    public static bool AchievementEligible=>VariableSingleton.IsInitialized&&Ironman&&!Terminal&&!Automated&&(PhasedAssaultUI.Current==null||!PhasedAssaultUI.Current.Battle.Practice)&&!AssaultPractice.Active;
    public static void PrepareNew(bool ironman){RequestedFile=null;RequestedIronman=ironman;NewRequested=true;}
    public static void BeginNew(){Ironman=NewRequested&&RequestedIronman;Terminal=false;CampaignId=Guid.NewGuid().ToString("N");Sequence=0;Phase="council";PendingResume=null;NewRequested=false;LastError="";VariableSingleton.LastCheckpointTurn=-1;}
    public static string Hash(VariableSingleton.CampaignSave data){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(JsonUtility.ToJson(data)))).Replace("-","");}
    public static string Integrity(Archive a){string original=a.payloadHash;a.payloadHash=null;try {using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(JsonUtility.ToJson(a)))).Replace("-","");}finally{a.payloadHash=original;}}
    public static void Validate(Archive a){
        if(a==null||a.campaign==null)throw new InvalidDataException("This is not a Besieged campaign save.");
        if(a.saveFormat!=Format)throw new InvalidDataException("Save format "+a.saveFormat+" is not supported by this build (format "+Format+").");
        var c=a.campaign;if(c.version!=1&&c.version!=2)throw new InvalidDataException("Unsupported campaign schema "+c.version+".");
        if(c.ints!=null&&(PopulationAccounting.Count(c)<0||PopulationAccounting.Count(c)>int.MaxValue))throw new InvalidDataException("Invalid population count.");
        if(c.turn<0||c.turn>100000||c.ints==null||c.floats==null||c.bools==null||c.training==null)throw new InvalidDataException("Incomplete campaign data.");
        if(!new[]{"OuterWall","MiddleWall","InnerWall"}.Contains(c.wall))throw new InvalidDataException("Unknown defensive line: "+c.wall);
        var keys=new HashSet<string>();foreach(var v in c.ints)if(v==null||string.IsNullOrEmpty(v.key)||v.key.Length>128||!v.key.StartsWith("i")||!keys.Add(v.key))throw new InvalidDataException("Invalid or duplicate integer key.");
        foreach(string required in new[]{"iMoney","iInfantry","iArchers","iProvisions"})if(!keys.Contains(required))throw new InvalidDataException("Missing resource: "+required);
        keys.Clear();foreach(var v in c.bools)if(v==null||string.IsNullOrEmpty(v.key)||v.key.Length>128||!v.key.StartsWith("b")||!keys.Add(v.key))throw new InvalidDataException("Invalid or duplicate flag.");
        keys.Clear();foreach(var v in c.floats)if(v==null||string.IsNullOrEmpty(v.key)||v.key.Length>128||!v.key.StartsWith("f")||!keys.Add(v.key)||float.IsNaN(v.value)||float.IsInfinity(v.value))throw new InvalidDataException("Invalid or duplicate numeric value.");
        if(!keys.Contains("fHealth"))throw new InvalidDataException("Missing Emperor health.");
        foreach(var order in c.training)if(order==null||!TrainingPool.Units.Contains(order.unit)||order.quantity<0||order.readyDay<0||order.readyDay>3652058)throw new InvalidDataException("Missing training order.");
        if(!new[]{"council","action","event","advance","ended"}.Contains(a.phase))throw new InvalidDataException("Unknown resume phase.");
        if(a.phase=="event"&&string.IsNullOrWhiteSpace(a.node))throw new InvalidDataException("Missing event continuation.");
        if(a.sequence<0||string.IsNullOrWhiteSpace(a.campaignId)||SafeId(a.campaignId)!=a.campaignId)throw new InvalidDataException("Invalid campaign identity.");
        if(!string.Equals(a.payloadHash,Integrity(a),StringComparison.Ordinal))throw new InvalidDataException("Campaign checksum differs. File may be damaged or edited; keep the original for diagnosis.");
    }
    public static Archive Read(string path){
        var f=new FileInfo(path);if(!f.Exists||f.Length>16*1024*1024)throw new InvalidDataException("Missing or oversized save file.");
        string json=File.ReadAllText(path);Archive a;
        if(!json.Contains("\"saveFormat\"")){
            var legacy=JsonUtility.FromJson<VariableSingleton.CampaignSave>(json);
            a=new Archive{campaign=legacy,build="Legacy playtest (before Version 29)",campaignId="legacy-"+Path.GetFileNameWithoutExtension(path).Replace(".","-"),name="Legacy campaign",savedUtc=f.LastWriteTimeUtc.ToString("o"),ironman=false,phase="council"};a.payloadHash=Integrity(a);
        }else a=JsonUtility.FromJson<Archive>(json);
        Validate(a);return a;
    }
    public static void AtomicWrite(string path,string content){
        Directory.CreateDirectory(Path.GetDirectoryName(path));string temp=path+".tmp";
        using(var stream=new FileStream(temp,FileMode.Create,FileAccess.Write,FileShare.None))using(var writer=new StreamWriter(stream,new UTF8Encoding(false))){writer.Write(content);writer.Flush();stream.Flush(true);}
        if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);
    }
    public static Archive Snapshot(string name,string phase="council",string node=null,string title=null,string report=null){
        var data=VariableSingleton.Instance.CaptureState();var a=new Archive{build=CouncilUI.BuildLabel,campaignId=CampaignId,name=name,savedUtc=DateTime.UtcNow.ToString("o"),calendar=VariableSingleton.GetDate().ToString("d MMM yyyy"),ironman=Ironman,terminal=Terminal,sequence=Sequence+1,campaign=data,phase=phase,node=node,title=title,report=report,notice=VariableSingleton.ArrivalNotice};a.payloadHash=Integrity(a);Validate(a);return a;
    }
    static void Written(Archive a,string path){Sequence=a.sequence;Phase=a.phase;if(Automated)return;PlayerPrefs.SetString("Besieged.LastSave",path);if(a.ironman)PlayerPrefs.SetInt("Besieged.IronSequence."+a.campaignId,a.sequence);PlayerPrefs.Save();}
    public static bool Checkpoint(string phase="council",string node=null,string title=null,string report=null){
        if(restoring||AssaultPractice.Active||!VariableSingleton.IsInitialized||string.IsNullOrEmpty(CampaignId))return false;
        try {var a=Snapshot(Ironman?"Ironman campaign":"Automatic checkpoint",phase,node,title,report);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if(Automated){AtomicWrite(VariableSingleton.SavePath,JsonUtility.ToJson(a,true));Written(a,VariableSingleton.SavePath);VariableSingleton.LastCheckpointTurn=VariableSingleton.Turn;return true;}
#endif
            AtomicWrite(CurrentPath,JsonUtility.ToJson(a,true));Written(a,CurrentPath);VariableSingleton.LastCheckpointTurn=VariableSingleton.Turn;LastError="";AchievementPlatform.Sync();return true;
        }catch(Exception e)when(e is IOException||e is UnauthorizedAccessException||e is InvalidDataException||e is ArgumentException){LastError="Could not save: "+e.Message;Debug.LogError(LastError);return false;}
    }
    public static bool Named(string name,out string message){
        if(!CanManualSave){message=Ironman?"Ironman uses its rolling autosave. Manual slots are disabled.":"Return to the council before making a named save.";return false;}
        try {name=(name??"").Trim();if(name.Length==0)name="Council save";if(name.Length>60)name=name.Substring(0,60);var a=Snapshot(name);string path=Path.Combine(Root,"save-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")+"-"+Guid.NewGuid().ToString("N").Substring(0,6)+".besieged.json");AtomicWrite(path,JsonUtility.ToJson(a,true));Written(a,path);message="Saved: "+Path.GetFileName(path);return true;}catch(Exception e){message="Could not save: "+e.Message;return false;}
    }
    public static List<Entry> Entries(){
        var result=new List<Entry>();foreach(var path in Directory.GetFiles(Root,"*.besieged.json")){var entry=new Entry{path=path};try {entry.archive=Read(path);if(entry.archive.ironman&&entry.archive.sequence<PlayerPrefs.GetInt("Besieged.IronSequence."+entry.archive.campaignId,0))entry.error="Older Ironman checkpoint; rolling progress has moved on.";if(entry.archive.terminal&&entry.archive.ironman)entry.error="Finished Ironman campaign.";}catch(Exception e){entry.error=e.Message;}result.Add(entry);}return result.OrderByDescending(x=>File.GetLastWriteTimeUtc(x.path)).ToList();
    }
    public static string LatestPath(){
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if(Automated)return File.Exists(VariableSingleton.SavePath)?VariableSingleton.SavePath:null;
#endif
        string last=PlayerPrefs.GetString("Besieged.LastSave","");var entries=Entries();return entries.FirstOrDefault(e=>e.Compatible&&e.path==last)?.path??entries.FirstOrDefault(e=>e.Compatible)?.path;
    }
    public static bool Load(string path,out string error){
        try {var a=Read(path);if(a.ironman&&a.sequence<PlayerPrefs.GetInt("Besieged.IronSequence."+a.campaignId,0))throw new InvalidDataException("An older Ironman checkpoint cannot replace the rolling save.");if(a.terminal&&a.ironman)throw new InvalidDataException("This Ironman campaign has ended.");restoring=true;VariableSingleton.Instance.RestoreValidated(a.campaign);Ironman=a.ironman;Terminal=a.terminal;CampaignId=a.campaignId;Sequence=a.sequence;Phase=a.phase;VariableSingleton.ArrivalNotice=a.notice??"";VariableSingleton.LastCheckpointTurn=VariableSingleton.Turn;PendingResume=a.phase=="council"?null:a;RequestedFile=null;error=null;return true;}
        catch(Exception e){error="Cannot load this save: "+e.Message;LastError=error;return false;}finally{restoring=false;}
    }
    public static void ImportLegacy(){
        if(Automated)return;
        string old=Path.Combine(Application.persistentDataPath,"campaign-v1.json"),dest=Path.Combine(Root,"legacy-campaign.besieged.json");
        try {if(File.Exists(old)&&!File.Exists(dest)){Read(old);File.Copy(old,dest);}string prep=old+".preparation";if(File.Exists(prep)&&!File.Exists(VariableSingleton.PreparationPresetPath)){Read(prep);File.Copy(prep,VariableSingleton.PreparationPresetPath);}}
        catch(Exception e){Debug.LogWarning("Legacy checkpoint was not imported: "+e.Message);}
    }
    public static void End(string title,string report){Terminal=true;Checkpoint("ended",null,title,report);}
    public static void OpenFolder(){Application.OpenURL(new Uri(Root+Path.DirectorySeparatorChar).AbsoluteUri);}
}

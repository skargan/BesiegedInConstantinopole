from pathlib import Path
import json, hashlib, difflib
root=Path(__file__).resolve().parent
repo=root.parents[1]/'consolidation'
(root/'baseline').mkdir(exist_ok=True)
(root/'updated').mkdir(exist_ok=True)
edits={
 'Assets/Scripts/DecisionJournal.cs': [
 ('    public static void Record(string id,string title,string text,bool decision=false){\r\n', '    public static void Record(string id,string title,string text,bool decision=false){\r\n        if(!VariableSingleton.IsInitialized)return;\r\n        RecordAt(VariableSingleton.GetDate(),id,title,text,decision);\r\n    }\r\n    public static void RecordAt(DateTime date,string id,string title,string text,bool decision=false){\r\n'),
 ('date=VariableSingleton.GetDate().ToString("d MMM yyyy")', 'date=date.ToString("d MMM yyyy")')],
 'Assets/Scripts/CouncilDiplomacyUI.cs': [
 ('        int state=DailyLifeEvents.I("iAidState"+index);', '        int state=DailyLifeEvents.I("iAidState"+index);\r\n        if(CountryDiplomacy.Definitions[index].Id=="ragusa")return RagusaDiplomacy.FoodStatus+"\\n"+RagusaDiplomacy.ContractStatus+(state==1?"\\nEarlier state aid due in "+DailyLifeEvents.I("iAidDays"+index)+" days":"");')],
 'Assets/Scripts/CampaignPersonnelCalendar.cs': [
 ('reports.Append(CountryDiplomacy.Advance(date));','reports.Append(CountryDiplomacy.Advance(date));\n            reports.Append(RagusaDiplomacy.Advance(date));')],
 'Assets/Scripts/RecruitmentLogistics.cs': [
 (' - SpecialistPersonnel.LocalManpowerCommitted);',' - SpecialistPersonnel.LocalManpowerCommitted - RagusaDiplomacy.CommittedTroops);')],
 'Assets/Scripts/TrainingPool.cs': [
 ('public static string OrderName(Order order)=>order.kind==ShipRepair?', 'public static string OrderName(Order order)=>order.kind==RagusaDiplomacy.ExpeditionReturn?Name(order.unit)+" · Returning from Ragusan service":order.kind==ShipRepair?'),
 ('if(order.quantity==0)return false;', 'if(order.quantity==0)return false;\n        if(order.kind==RagusaDiplomacy.ExpeditionReturn)return RecruitmentLogistics.IsRegular(order.unit)&&order.engineersReserved==0&&order.manpowerReserved==0&&!order.instructorFree;')],
 'Assets/Scripts/ReliefStormSystem.cs': [
 ('        return result;','        if(RagusaDiplomacy.FoodAtSea)result.Add(RagusaDiplomacy.FoodStormTarget);\n        return result;'),
 ('        if(target>=0 && target<CouncilActions.Allies.Length', '        if(target==RagusaDiplomacy.FoodStormTarget)lines.Add(RagusaDiplomacy.ApplyFoodStorm(severity));\n        else if(target>=0 && target<CouncilActions.Allies.Length')],
 'Assets/Scripts/CountryDiplomacy.cs': [
 ('public static string RenewReason(int i,bool war)\r\n    {\r\n        Ensure();if(!Valid(i))return "Unknown court";', 'public static string RenewReason(int i,bool war)\r\n    {\r\n        Ensure();if(!Valid(i))return "Unknown court";\r\n        if(!war&&Definitions[i].Id=="ragusa")return "Ragusa uses monthly private provision arrangements, not renewed state aid appeals.";'),
 ('int state=DailyLifeEvents.I("iAidState"+i);\r\n        if(state==1)', 'int state=DailyLifeEvents.I("iAidState"+i);\r\n        if(Definitions[i].Id=="ragusa")return "Ragusa arranges private monthly provisions through its representatives.";\r\n        if(state==1)')]
}
manifest={'checkout':str(repo),'targetRelease':38,'files':{}}
for relative,replacements in edits.items():
 src=repo/relative;raw=src.read_bytes();before=raw.decode('utf-8-sig');after=before
 for old,new in replacements:
  assert after.count(old)==1,(relative,old)
  after=after.replace(old,new,1)
 name=src.name
 (root/'baseline'/name).write_bytes(raw)
 (root/'updated'/name).write_text(after,encoding='utf-8',newline='')
 diff=''.join(difflib.unified_diff(before.splitlines(True),after.splitlines(True),'a/'+relative,'b/'+relative))
 (root/(name+'.patch')).write_text(diff,encoding='utf-8',newline='')
 manifest['files'][relative]=hashlib.sha256(raw).hexdigest()
(root/'baseline.json').write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf-8')
journal=(root/'updated'/'DecisionJournal.cs').read_text(encoding='utf-8').split('    static GameObject root;')[0]+'}\n'
(root/'DecisionJournalMethods.generated.cs').write_text(journal.replace('using UnityEngine.UI;',''),encoding='utf-8')
print('Seven review-only integration patches staged; no live/consolidation edits.')

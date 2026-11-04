from pathlib import Path
import json, hashlib, difflib

root = Path(__file__).resolve().parent
repo = Path('C:/Users/Dainius/Documents/Codex/Besieged-release18')
source = repo / 'Assets/Scripts/DailyLifeEvents.cs'
raw = source.read_bytes()
before = raw.decode('utf-8-sig')
after = before.replace('"lifeLights"};', '"lifeLights","lifeTavern"};', 1)
after = after.replace('(n!="lifeBarrel"||I("iLifeBarrelDue")==0)', '(n!="lifeBarrel"||I("iLifeBarrelDue")==0)&&(n!="lifeTavern"||I("iLifeTavernDue")==0)', 1)
after = after.replace('return "lifeGunnerResult";return null;', 'return "lifeGunnerResult";if(I("iLifeTavernDue")>0&&Day>=I("iLifeTavernDue"))return "lifeTavernPledge";return null;', 1)
anchor = '    [YarnFunction("LifeInt")]'
commands = '''    [YarnCommand("LifeTavernPledge")] public static void TavernPledge(){if(I("iLifeTavernDue")==0)VariableSingleton.SetInt("iLifeTavernDue",Day+1);}
    // The due flag guards the complete reward, including replay of a restored narrative node.
    [YarnCommand("LifeTavernPledgeComplete")] public static void CompleteTavernPledge(){if(I("iLifeTavernDue")<=0||Day<I("iLifeTavernDue"))return;VariableSingleton.SetInt("iLifeTavernDue",0);VariableSingleton.ChangeInt("iInfantry",5);VariableSingleton.ChangeInt("iArchers",5);VariableSingleton.ChangePercentFloat("fTroopRep",.01f);}
    [YarnCommand("LifeTavernPay")] public static void TavernPay(int price)=>VariableSingleton.ChangeInt("iMoney",-System.Math.Min(System.Math.Max(0,I("iMoney")),System.Math.Max(0,price)));
'''
assert before.count(anchor) == 1
after = after.replace(anchor, commands + anchor)
assert before != after and 'return "lifeTavernPledge";' in after
(root/'baseline').mkdir(exist_ok=True)
(root/'updated').mkdir(exist_ok=True)
(root/'baseline'/'DailyLifeEvents.cs').write_bytes(raw)
(root/'updated'/'DailyLifeEvents.cs').write_text(after, encoding='utf-8', newline='')
patch = ''.join(difflib.unified_diff(before.splitlines(True), after.splitlines(True), 'a/Assets/Scripts/DailyLifeEvents.cs', 'b/Assets/Scripts/DailyLifeEvents.cs'))
(root/'DailyLifeEvents.cs.patch').write_text(patch,encoding='utf-8',newline='')
entries = [dict(node=n,path='EventArt/Covers/lifeTavern',source='User supplied codex-clipboard-291a0024-2029-41d5-92de-32cdd4de7b06.png',fit='contain',focusY=.5) for n in ['lifeTavern','lifeTavernPledge']]
(root/'EventArtwork.additions.json').write_text(json.dumps(entries,indent=2)+'\n',encoding='utf-8')
art_manifest = repo/'Assets/Resources/Content/EventArtwork.json'
data = json.loads(art_manifest.read_text(encoding='utf-8-sig'))
assert not any(e['node'] in ['lifeTavern','lifeTavernPledge'] for e in data['entries'])
baseline = {'release':'38 only; excluded from 37','checkout':str(repo),'files':{str(source.relative_to(repo)):hashlib.sha256(raw).hexdigest(),str(art_manifest.relative_to(repo)):hashlib.sha256(art_manifest.read_bytes()).hexdigest()}}
(root/'baseline.json').write_text(json.dumps(baseline,indent=2)+'\n',encoding='utf-8')
print('Staged DailyLifeEvents patch and two additive artwork entries; live files unchanged.')

from pathlib import Path
import difflib, hashlib, json
root=Path(__file__).resolve().parent
repo=root.parents[1]/'consolidation'
old=root.parents[1]/'candar-work'
for folder in ('baseline','updated'):(root/folder).mkdir(parents=True,exist_ok=True)
backend=(old/'Assets/Scripts/CandarDiplomacy.cs').read_text(encoding='utf-8-sig')
backend=backend.replace('// in existing campaign capture/reset/migration, including loading older saves.','// in existing campaign capture and reset. No separate state model is introduced.')
backend=backend.replace('op != OttomanDirector.Operation.AwaitOrders && op != OttomanDirector.Operation.Reinforce && op != OttomanDirector.Operation.Advance;', '''op != OttomanDirector.Operation.AwaitOrders && op != OttomanDirector.Operation.Reinforce && op != OttomanDirector.Operation.Advance &&
        op != OttomanDirector.Operation.NavalAssault && op != OttomanDirector.Operation.OverlandShips && op != OttomanDirector.Operation.ChainBattery;''')
backend=backend.replace('return I("Deferred")==1 && Day>=I("DeferredDue");', '''if(I("Deferred")!=1 || Day<I("DeferredDue")) return false;
        // A target may cease to exist while orders are held. Never force an illegal action.
        if(!Enum.IsDefined(typeof(OttomanDirector.Operation),op) || !Eligible(op) || !OttomanDirector.Legal(op)) { Put("Deferred",0);return false; }
        return true;''')
backend=backend.replace('        report=null;\n        if(I("Deferred")==1)', '''        report=null;
        // Naval pressure and non-tactical/emergency orders do not consume this purchase.
        if(!Eligible(op)) return false;
        if(I("Deferred")==1)''')
(root/'CandarDiplomacy.cs').write_text(backend,encoding='utf-8')
edits={
 'Assets/Scripts/OttomanDirector.cs': [('else if(Own("iEnemyDelay")>0)chosen=Operation.AwaitOrders;', 'else if(Own("iEnemyDelay")>0)chosen=Operation.AwaitOrders;\r\n        else if(CandarDiplomacy.DeferredOperation(out var deferred))chosen=deferred;')],
 'Assets/Scripts/AI/AI.cs': [('        string operationReport=OttomanSiegeOperations.Execute(this,planned);', '''        // Paid Candar postponement applies only after supply, existing delays and forced attacks.
        if(CandarDiplomacy.DelayOperation(planned,out var candarReport)){LastReport=candarReport;return;}
        string operationReport=OttomanSiegeOperations.Execute(this,planned);'''.replace('\n','\r\n'))]
}
manifest={'checkout':str(repo),'files':{},'backendSource':hashlib.sha256((old/'Assets/Scripts/CandarDiplomacy.cs').read_bytes()).hexdigest()}
for rel,replacements in edits.items():
 raw=(repo/rel).read_bytes();before=raw.decode('utf-8-sig');after=before
 for a,b in replacements:
  assert after.count(a)==1,(rel,a)
  after=after.replace(a,b,1)
 name=Path(rel).name
 (root/'baseline'/name).write_bytes(raw)
 (root/'updated'/name).write_text(after,encoding='utf-8',newline='')
 (root/(name+'.patch')).write_text(''.join(difflib.unified_diff(before.splitlines(True),after.splitlines(True),'a/'+rel,'b/'+rel)),encoding='utf-8',newline='')
 manifest['files'][rel]=hashlib.sha256(raw).hexdigest()
ai=(root/'updated/AI.cs').read_text(encoding='utf-8')
method=ai[ai.index('    public void TakeAIAction()'):ai.index('    /****************',ai.index('    public void TakeAIAction()'))]
(root/'AITakeAction.generated.cs').write_text('public partial class AI {\n'+method+'\n}\n',encoding='utf-8')
(root/'baseline.json').write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf-8')
print('Candar backend and two narrow scheduler patches staged; canonical files unchanged.')

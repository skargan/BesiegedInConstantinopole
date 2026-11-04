from pathlib import Path
import difflib, hashlib, json, shutil

stage=Path(__file__).resolve().parent
workspace=stage.parents[1]
approved=stage.parent/'content/approved-remarks.json'
data=json.loads(approved.read_text(encoding='utf-8-sig'))
assert len(data['lines'])==30 and len(set(data['lines']))==30
resource=stage/'Assets/Resources/Content/CandarRemarks.json'
resource.parent.mkdir(parents=True,exist_ok=True)
shutil.copyfile(approved,resource)
assert resource.read_bytes()==approved.read_bytes()

baseline=workspace/'consolidation/Assets/Scripts/CouncilDiplomacyUI.cs'
source=baseline.read_text(encoding='utf-8-sig')
old='CountryDiplomacy.Ensure();int total=CountryDiplomacy.Definitions.Length,perPage=8'
assert source.count(old)==1
updated=source.replace(old,'CountryDiplomacy.Ensure();int total=CountryDiplomacy.Definitions.Length+1,perPage=8')
needle='            int index=diplomacyPage*perPage+n;float left=(n%2)*.505f,top=1-(n/2)*.222f;\n'
assert updated.count(needle)==1
special='''            if(index==CountryDiplomacy.Definitions.Length) {
                var special=CandarPlainButton(stage,"Candar · İsmail Bey\\n"+(CandarDiplomacy.Unlocked?"Secret negotiations":"Hostile · a guarded door"),new Vector2(left,top-.207f),new Vector2(left+.495f,top),CandarCourt);
                special.gameObject.name="Diplomatic court candar";continue;
            }
'''
updated=updated.replace(needle,needle+special)
patch=''.join(difflib.unified_diff(source.splitlines(True),updated.splitlines(True),fromfile='a/Assets/Scripts/CouncilDiplomacyUI.cs',tofile='b/Assets/Scripts/CouncilDiplomacyUI.cs',n=3))
(stage/'CouncilDiplomacyUI.patch').write_text(patch,encoding='utf-8')
candidate=stage/'checks/updated/CouncilDiplomacyUI.cs'
candidate.parent.mkdir(parents=True,exist_ok=True)
candidate.write_text(updated,encoding='utf-8')
(stage/'baseline-sha256.json').write_text(json.dumps({str(baseline.relative_to(workspace)).replace('\\','/'):hashlib.sha256(baseline.read_bytes()).hexdigest(),str(approved.relative_to(workspace)).replace('\\','/'):hashlib.sha256(approved.read_bytes()).hexdigest()},indent=2)+'\n',encoding='utf-8')
print('Validated 30 approved remarks; runtime resource is a byte-for-byte copy. Staged special-tile patch.')

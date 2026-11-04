"""Rebuild the JsonUtility-compatible resource and a one-hook integration patch.
Reads sibling stages; writes only this UI stage.
"""
import hashlib
import json
from pathlib import Path

stage = Path(__file__).resolve().parent
workspace = stage.parents[1]
writing = stage.parent / 'writing'
read = lambda path: json.loads(path.read_text(encoding='utf-8-sig'))
people = read(writing / 'character-descriptions.json')
hover = read(writing / 'hover-remarks.json')
passages = read(writing / 'meeting-passages.json')
realm = read(writing / 'realm-profile.json')
art = read(stage.parent / 'art/art-manifest.json')
resource = {
    'realmTitle': realm['title'],
    'realmText': realm['text'],
    'characters': [dict(id=key, **value, hover=hover[key]) for key, value in people.items()],
    'passages': [dict(id=key, **value) for key, value in passages.items()],
    'scenes': [dict(foreground=a['foreground'], resource=a['suggestedResourcePath'],
        characters=[dict(id=p['id'], hitPriority=p['hitPriority'],
        polygon=[dict(x=x,y=y) for x,y in p['polygonUnityBottomLeft']])
        for p in a['characters']]) for a in art['assets']]
}
out = stage / 'Assets/Resources/Content/RagusaNarrative.json'
out.parent.mkdir(parents=True, exist_ok=True)
out.write_text(json.dumps(resource,ensure_ascii=False,indent=2)+'\n', encoding='utf-8')

baseline = workspace / 'consolidation/Assets/Scripts/CouncilDiplomacyUI.cs'
source = baseline.read_text(encoding='utf-8-sig')
needle = '        var d=CountryDiplomacy.Definitions[index];var ally=CouncilActions.Allies[index];'
assert source.count(needle)==1, 'Expected exactly one UnifiedDiplomaticPower hook.'
(stage / 'CouncilDiplomacyUI.patch').write_text(
    '--- a/Assets/Scripts/CouncilDiplomacyUI.cs\n'
    '+++ b/Assets/Scripts/CouncilDiplomacyUI.cs\n'
    '@@ -60,3 +60,4 @@\n'
    '     {\n'
    '         if(index<0||index>=CountryDiplomacy.Definitions.Length){DiplomaticRelations();return;}\n'
    '+        if(CountryDiplomacy.Definitions[index].Id=="ragusa"){RagusanCourt();return;}\n'
    '         var d=CountryDiplomacy.Definitions[index];var ally=CouncilActions.Allies[index];\n', encoding='utf-8')
tracked = [baseline, workspace/'consolidation/Assets/Scripts/CouncilUI.cs',
           workspace/'consolidation/Assets/Scripts/LocationHotspot.cs',
           stage.parent/'art/art-manifest.json', stage.parent/'mechanics/RagusaDiplomacy.cs']
(stage/'baseline-sha256.json').write_text(json.dumps({str(p.relative_to(workspace)).replace('\\','/'):
    hashlib.sha256(p.read_bytes()).hexdigest() for p in tracked},indent=2)+'\n',encoding='utf-8')
assert len(resource['characters'])==2
assert all(len(c['hover'])==12 for c in resource['characters'])
assert len({p['id'] for p in resource['passages']})==len(resource['passages'])
assert len(resource['scenes'])==2
print(f"Validated {len(resource['passages'])} passages, 24 hover remarks, two art variants.")

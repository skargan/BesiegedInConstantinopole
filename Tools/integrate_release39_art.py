"""Fill missing event mappings from the preserved handover; keep existing choices."""
from pathlib import Path
import json, re, shutil, uuid, hashlib
r=Path(__file__).resolve().parents[1]; res=r/'Assets/Resources'
source=r/'FutureDevelopment/OriginalArt'; hand=json.loads((source/'Besieged_art_manifest.json').read_text())
path=res/'Content/EventArtwork.json'; data=json.loads(path.read_text()); existing={x['node'] for x in data['entries']}
nodes=set()
for yarn in (r/'Assets/Yarn').rglob('*.yarn'):
    nodes.update(re.findall(r'^title:\s*(\S+)\s*$',yarn.read_text(encoding='utf-8-sig'),re.M))
template=(res/'EventArt/Covers/story19.png.meta').read_text()
added=[]; skipped=[]; copied={}
for node,asset in hand['event_node_to_asset_path'].items():
    if node in existing:continue
    if node not in nodes:skipped.append(node);continue
    filename=Path(asset).name; destination=res/'EventArt/Covers/Handover'/filename
    destination.parent.mkdir(parents=True,exist_ok=True)
    if asset not in copied:
        shutil.copy2(source/asset,destination)
        meta=Path(str(destination)+'.meta')
        if not meta.exists():meta.write_text(re.sub(r'(?m)^guid: .*$',f'guid: {uuid.uuid4().hex}',template),encoding='utf8')
        copied[asset]=hashlib.sha256(destination.read_bytes()).hexdigest()
    data['entries'].append({'node':node,'path':'EventArt/Covers/Handover/'+destination.stem,'source':asset+' (final-art handover e9bacf0)','fit':'contain','focusY':.5})
    added.append(node)
path.write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
(r/'Documentation/Release39/art-integration.json').write_text(json.dumps({'addedNodes':added,'keptExistingCount':len(existing),'sourceNodesAbsentFromCurrentYarn':skipped,'copiedOriginalHashes':copied},indent=2),encoding='utf8')
print('Added',len(added),'mappings using',len(copied),'originals; preserved',len(existing),'existing mappings; skipped absent nodes:',skipped)

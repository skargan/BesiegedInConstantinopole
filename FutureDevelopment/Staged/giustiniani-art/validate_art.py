from pathlib import Path
from PIL import Image
import json, hashlib, re

HERE=Path(__file__).resolve().parent
manifest=json.loads((HERE/'manifest.json').read_text(encoding='utf-8'))
checks=[]
def check(ok,label):
    checks.append({'passed':bool(ok),'check':label})
    if not ok: raise AssertionError(label)
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
for asset in manifest['assets']:
    p=HERE/'updated'/asset['destination']
    check(p.read_bytes()==Path(asset['source']).read_bytes(),asset['role']+' preserves supplied PNG byte-for-byte')
    check(sha(p)==asset['sha256'],asset['role']+' recorded hash matches staged asset')
    im=Image.open(p);im.verify()
    check(True,asset['role']+' decodes as a complete PNG')
    meta=p.with_suffix(p.suffix+'.meta').read_text()
    check('enableMipMap: 0' in meta and 'maxTextureSize: 2048' in meta and 'textureCompression: 0' in meta,asset['role']+' preserves sharp UI import settings')
    old=HERE/'baseline'/(asset['destination']+'.meta')
    if old.exists():check(old.read_bytes()==p.with_suffix(p.suffix+'.meta').read_bytes(),asset['role']+' existing meta and GUID unchanged')
    else:check(not (Path(manifest['baselineCheckout'])/asset['destination']).exists(),'new portrait resource does not overwrite an existing individual portrait')

figure_path='Assets/Resources/Content/CourtFigures.json'
old=json.loads((HERE/'baseline'/figure_path).read_text())
new=json.loads((HERE/'updated'/figure_path).read_text())
check([x for x in old['figures'] if x['id']!=0]==[x for x in new['figures'] if x['id']!=0],'all other court figure bounds unchanged')
figure=next(x for x in new['figures'] if x['id']==0)
image=Image.open(HERE/'updated'/manifest['assets'][0]['destination'])
check(image.mode=='RGBA' and image.getchannel('A').getextrema()[0]==0,'full-body sprite has real transparency, not a black rectangle')
box=image.getchannel('A').point(lambda a:255 if a>=16 else 0).getbbox()
check([figure[k] for k in ['x','y','width','height']]==[box[0],image.height-box[3],box[2]-box[0],box[3]-box[1]],'Unity bottom-left crop matches measured silhouette, including head, sword and feet')
check(0<=figure['x']<figure['x']+figure['width']<=image.width and 0<=figure['y']<figure['y']+figure['height']<=image.height,'court figure crop remains inside source texture')

event_path='Assets/Resources/Content/EventArtwork.json'
old=json.loads((HERE/'baseline'/event_path).read_text())
new=json.loads((HERE/'updated'/event_path).read_text())
def arrival(data):
    for rows in data.values():
        if isinstance(rows,list):
            for row in rows:
                if isinstance(row,dict) and row.get('node')=='story5':return row
old_entry=arrival(old);new_entry=arrival(new)
old_entry['source']=new_entry['source']
check(old==new,'event manifest changes only story5 source provenance; all routing and fitting unchanged')
check(new_entry['path']=='EventArt/Covers/story5' and new_entry['fit']=='contain','arrival remains bound to story5 and shows the full illustration')

portrait_path='Assets/Scripts/CharacterPortraits.cs'
old=(HERE/'baseline'/portrait_path).read_text()
new=(HERE/'updated'/portrait_path).read_text()
check('id==0?"UI/Identity/Characters/0"' in new,'Giustiniani portrait uses supplied individual asset')
check('if(id==0)return new Rect(0,0,2f/3f,1);' in new,'portrait square view keeps the face prominent without editing original PNG')
check(old[old.index('  if(id<0||id>8)'):]==new[new.index('  if(id<0||id>8)'):],'atlas cropping and rendering for all other characters unchanged')

summary={'status':'static checks passed; not Unity compiled or visually playtested','checksPassed':len(checks),'checks':checks}
(HERE/'validation.json').write_text(json.dumps(summary,indent=2)+'\n')
print(json.dumps({'passed':len(checks),'unityCompilation':'pending Version38 integration','runtimeVisualChecks':'pending'},indent=2))

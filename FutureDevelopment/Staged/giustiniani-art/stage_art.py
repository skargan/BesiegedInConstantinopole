from pathlib import Path
from PIL import Image
import hashlib, json, shutil, difflib, uuid

HERE = Path(__file__).resolve().parent
BASE = HERE.parent / 'consolidation'
LIVE = Path(r'C:/Users/Dainius/Documents/Codex/Besieged-release18')
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def write(rel, text):
    p = HERE / 'updated' / rel
    p.parent.mkdir(parents=True, exist_ok=True)
    p.write_text(text, encoding='utf-8', newline='\n')
def preserve(rel):
    p = HERE / 'baseline' / rel
    p.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(BASE / rel, p)
    return None if rel.endswith('.png') else (BASE / rel).read_text(encoding='utf-8-sig')
def patch(rel):
    old = (HERE/'baseline'/rel).read_text(encoding='utf-8-sig')
    new = (HERE/'updated'/rel).read_text(encoding='utf-8-sig')
    dest = HERE/'patches'/(Path(rel).name+'.patch')
    dest.parent.mkdir(parents=True, exist_ok=True)
    dest.write_text(''.join(difflib.unified_diff(old.splitlines(True), new.splitlines(True), fromfile='a/'+rel, tofile='b/'+rel)), encoding='utf-8')

sources = [
    ('full_sprite','codex-clipboard-00abfb2b-baaa-46ba-a723-c9855b5e4ede.png','Assets/Resources/UI/Identity/CourtFigures/0.png'),
    ('arrival','codex-clipboard-2b430e2a-2ae7-4821-92d2-da6d6f9d0421.png','Assets/Resources/EventArt/Covers/story5.png'),
    ('portrait','codex-clipboard-1268fa78-3910-4869-babc-36709ad6439a.png','Assets/Resources/UI/Identity/Characters/0.png'),
]
assets = []
for role, filename, rel in sources:
    source=Path(r'C:/Users/Dainius/AppData/Local/Temp')/filename
    dest=HERE/'updated'/rel; dest.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(source,dest)
    im=Image.open(source)
    rec={'role':role,'source':str(source),'destination':rel,'sha256':sha(dest),'bytes':dest.stat().st_size,'size':list(im.size),'mode':im.mode,'originalBytesPreserved':source.read_bytes()==dest.read_bytes()}
    if 'A' in im.getbands():
        alpha=im.getchannel('A'); bbox=alpha.point(lambda v:255 if v>=16 else 0).getbbox()
        rec.update(alphaExtrema=list(alpha.getextrema()),bboxAlpha16TopLeftExclusive=list(bbox),spriteRectBottomLeftXYWH=[bbox[0],im.height-bbox[3],bbox[2]-bbox[0],bbox[3]-bbox[1]])
    if (BASE/rel).exists():
        preserve(rel)
        preserve(rel+'.meta')
        shutil.copyfile(BASE/(rel+'.meta'),HERE/'updated'/(rel+'.meta'))
        rec['metaPolicy']='preserve existing GUID and importer byte-for-byte'
    else:
        meta=(BASE/'Assets/Resources/UI/Identity/CourtFigures/0.png.meta').read_text()
        import re
        meta=re.sub(r'guid: [0-9a-f]+','guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'besieged:giustiniani-portrait:user-supplied:2026-11-03').hex,meta,count=1)
        meta=meta.replace('  isReadable: 1','  isReadable: 0')
        write(rel+'.meta',meta)
        rec['metaPolicy']='new standalone Texture2D; no mipmaps, no compression, clamp, max size 2048'
    assets.append(rec)

rel='Assets/Scripts/CharacterPortraits.cs'; text=preserve(rel)
old='public static string Resource(int id)=>id>=0&&id<=8?"UI/Identity/court-portraits":System.Array.IndexOf(People,id)>=0?"UI/Identity/Characters/"+id:null;'
new='public static string Resource(int id)=>id==0?"UI/Identity/Characters/0":id>=1&&id<=8?"UI/Identity/court-portraits":System.Array.IndexOf(People,id)>=0?"UI/Identity/Characters/"+id:null;'
assert text.count(old)==1
text=text.replace(old,new)
needle=' public static Rect UV(int id){\n'
assert text.count(needle)==1
text=text.replace(needle,needle+'  // Supplied Giustiniani portrait: retain the original bitmap; crop the empty right-hand room in the view only.\n  if(id==0)return new Rect(0,0,2f/3f,1);\n')
write(rel,text);patch(rel)

rel='Assets/Resources/Content/CourtFigures.json'; original=preserve(rel); data=json.loads(original)
figure=next(x for x in data['figures'] if x['id']==0)
old_figure=dict(figure)
figure.update(sourceWidth=1024,sourceHeight=1536,x=141,y=10,width=687,height=1510)
write(rel,json.dumps(data,indent=2,ensure_ascii=False)+'\n');patch(rel)

rel='Assets/Resources/Content/EventArtwork.json'; original=preserve(rel); data=json.loads(original)
arrival=next(x for x in data['entries'] if x['node']=='story5') if 'entries' in data else None
if arrival is None:
    for values in data.values():
        if isinstance(values,list):
            arrival=next((x for x in values if isinstance(x,dict) and x.get('node')=='story5'),None)
            if arrival:break
assert arrival
old_arrival=dict(arrival)
arrival['source']='codex-clipboard-2b430e2a-2ae7-4821-92d2-da6d6f9d0421.png (user-supplied Giustiniani arrival, 3 November 2026)'
assert arrival['path']=='EventArt/Covers/story5'
write(rel,json.dumps(data,indent=2,ensure_ascii=False)+'\n');patch(rel)

manifest={'releaseTarget':'Version38','baselineCheckout':str(BASE),'liveReferenceCheckout':str(LIVE),'date':'2026-11-03','characterId':0,'arrivalNode':'story5','assets':assets,'portraitViewUV':[0,0,2/3,1],'oldFigure':old_figure,'newFigure':figure,'oldArrivalEntry':old_arrival,'newArrivalEntry':arrival,'baselineFiles':[]}
for p in sorted((HERE/'baseline').rglob('*')):
    if p.is_file():
        rel=p.relative_to(HERE/'baseline').as_posix()
        manifest['baselineFiles'].append({'path':rel,'sha256':sha(p),'liveSha256':sha(LIVE/rel) if (LIVE/rel).exists() else None})
manifest['noChangeNeeded']=['PalaceFigureArt.cs reads the existing ID-0 rectangle from CourtFigures.json; no code patch required.','Other eight atlas portraits and the shared atlas bitmap remain unchanged.','Arrival event ID, choices and effects remain unchanged.']
(HERE/'manifest.json').write_text(json.dumps(manifest,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
write('Documentation/Artwork/Giustiniani-2026-11-03.json',json.dumps(manifest,indent=2,ensure_ascii=False)+'\n')
print(json.dumps({'staged':len(assets),'modifiedSource':3,'target':'Version38','spriteAlphaCrop':figure,'portraitUV':manifest['portraitViewUV']},indent=2))

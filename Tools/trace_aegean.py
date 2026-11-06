"""Trace connected painted islands; keep atlas-space geometry independent of gameplay."""
import json
from pathlib import Path
from collections import deque
import numpy as np
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
im=Image.open(ROOT/'Assets/Resources/UI/Maps/WarTheatreGeography.png').convert('RGB')
a=np.asarray(im).astype(int); H,W=a.shape[:2]
# The painted land is warm; the sea is blue. Trace at native artwork resolution.
land=(a[:,:,0]>a[:,:,2]*1.08)&(a[:,:,0]>65)
seen=np.zeros((H,W),bool); components=[]
for y in range(390,695):
 for x in range(590,1010):
  if seen[y,x] or not land[y,x]: continue
  q=deque([(x,y)]); seen[y,x]=True; pixels=[]
  while q:
   px,py=q.popleft();pixels.append((px,py))
   for nx,ny in ((px-1,py),(px+1,py),(px,py-1),(px,py+1)):
    if 0<=nx<W and 0<=ny<H and land[ny,nx] and not seen[ny,nx]:
     seen[ny,nx]=True;q.append((nx,ny))
  if 3<=len(pixels)<7000: components.append(pixels)
out=im.crop((590,390,1010,695)).resize((1260,915));draw=ImageDraw.Draw(out)
for i,p in enumerate(components):
 x,y=np.mean(p,axis=0);draw.text(((x-590)*3,(y-390)*3),str(i),fill='red',stroke_width=1,stroke_fill='white')
out.save(ROOT/'Builds/aegean-components.png')
(ROOT/'Builds/aegean-components.json').write_text(json.dumps(components))
print('Island components:',len(components))

def outline(pixels):
 cells=set(map(tuple,pixels)); edges={}
 for x,y in cells:
  for neighbour,start,end in [((x,y-1),(x,y),(x+1,y)),((x+1,y),(x+1,y),(x+1,y+1)),((x,y+1),(x+1,y+1),(x,y+1)),((x-1,y),(x,y+1),(x,y))]:
   if neighbour not in cells: edges.setdefault(start,[]).append(end)
 loops=[]
 while edges:
  start=next(iter(edges));p=start;loop=[]
  while True:
   loop.append(p); nxt=edges[p].pop()
   if not edges[p]: del edges[p]
   p=nxt
   if p==start:break
  loops.append(loop)
 loop=max(loops,key=len)
 # Remove only collinear raster vertices: no broad shortcuts across coves.
 return [p for i,p in enumerate(loop) if (p[0]-loop[i-1][0])*(loop[(i+1)%len(loop)][1]-p[1]) != (p[1]-loop[i-1][1])*(loop[(i+1)%len(loop)][0]-p[0])]

if '--apply' in __import__('sys').argv:
 path=ROOT/'Assets/Resources/Content/WarTheatre.json';raw=path.read_text(encoding='utf-8-sig');data=json.loads(raw)
 names={21:'Psara',22:'Chios',24:'Oinousses',25:'Andros',26:'Salamis',27:'Tinos',28:'Aegina',29:'Kea',30:'Samos',31:'Mykonos',33:'Hydra',34:'Ikaria',36:'Kythnos',37:'Syros',40:'Spetses',42:'Patmos',43:'Serifos',44:'Naxos',45:'Paros',46:'Kalymnos',48:'Sifnos',51:'Kos',52:'Amorgos',58:'Milos',59:'Ios',61:'Nisyros',63:'Rhodes',64:'Astypalaia',67:'Tilos',70:'Santorini',72:'Kythera',74:'Anafi',75:'Karpathos',76:'Antikythera',77:'Crete',80:'Kasos',82:'Gavdos'}
 names.update({14:'Lesbos',15:'Negroponte',17:'Skyros',41:'Patmos',42:'Lipsi',46:'Kalymnos and Leros',57:'Symi'})
 genoese={14,21,22,24,30,34,35,39};knights={41,42,46,49,50,51,57,61,63,66,67,71,73};moreote={33,40,69}
 existing={14:'lesbos',15:'negroponte',22:'chios',44:'naxos',63:'hospitallers',72:'kythera',77:'crete'}
 changed=[]
 for i in [14,15,17]+list(range(21,len(components))):
  pixels=components[i];poly=outline(pixels)
  if len(poly)<3:continue
  group='genoa' if i in genoese else 'hospitallers' if i in knights else 'morea' if i in moreote else 'athens' if i==26 else 'venice'
  name=names.get(i,('Hospitaller' if group=='hospitallers' else 'Genoese' if group=='genoa' else 'Venetian')+' islets')
  ident=existing.get(i,'aegean_'+str(i));r=next((r for r in data['regions'] if r['id']==ident),None)
  if r is None:
   r=dict(id=ident,name=name,kind='island',group=group,role='',note='',referenceName=name,summary='',front={'x':0,'y':0});data['regions'].append(r)
  x,y=np.mean(pixels,axis=0);r['points']=[{'x':x/W,'y':1-y/H} for x,y in poly];r['label']={'x':float(x/W),'y':float(1-y/H)}
  if ident not in existing.values():
   r['summary']=name+'. '+('The Knights of Saint John hold these island approaches to Rhodes.' if group=='hospitallers' else 'An Aegean possession connected to the Genoese maritime world.' if group=='genoa' else 'A Latin island lordship within the Venetian maritime sphere.' if group=='venice' else 'An island dependency of the neighbouring Greek mainland.')
   r['name']=name;r['referenceName']=name
  specific={14:'The Gattilusi rule Lesbos from Mytilene, their Genoese connections reaching across the Aegean.',22:'The Giustiniani administer Chios through the Genoese Maona; mastic and maritime trade sustain its wealth.',25:'Andros is held by the Sommaripa, one of the Latin houses established among the Aegean islands.',30:'Samos lies within the Genoese island possessions, its exposed settlements vulnerable to raids and depopulation.',34:'Ikaria is held by the Arangio family, amid the Genoese interests of the eastern Aegean.',44:'Naxos is the seat of the Crispo dukes of the Archipelago, surrounded by the estates of other Latin island lords.',64:'The Querini hold Astypalaia, their castle commanding the white houses and surrounding sea.',75:'The Cornaro hold Karpathos and neighbouring Kasos, island lordships linked to the Venetian world.',80:'Kasos belongs to the Cornaro possessions alongside Karpathos, between Crete and Rhodes.'}
  if i in specific:r['summary']=specific[i]
  changed.append(ident)
 # Preserve unrelated catalogue formatting and content.
 decoder=json.JSONDecoder();start=raw.index('[',raw.index('"regions"'))+1;pos=start;spans=[]
 while True:
  while raw[pos].isspace() or raw[pos]==',':pos+=1
  if raw[pos]==']':break
  obj,end=decoder.raw_decode(raw,pos);spans.append((pos,end,obj['id']));pos=end
 replacements={r['id']:r for r in data['regions'] if r['id'] in changed}
 for begin,end,ident in reversed(spans):
  if ident in replacements:raw=raw[:begin]+json.dumps(replacements.pop(ident),ensure_ascii=False,indent=2)+raw[end:]
 end=raw.rfind(']');raw=raw[:end].rstrip()+''.join(',\n'+json.dumps(r,ensure_ascii=False,indent=2) for r in replacements.values())+'\n'+raw[end:]
 path.write_text(raw,encoding='utf-8')
 print('Traced',len(changed),'territories')
 # Preview generated from exactly the polygon data used by the game.
 overlay=Image.new('RGBA',im.size);d=ImageDraw.Draw(overlay)
 colours={'venice':(210,111,110,115),'genoa':(211,190,87,115),'hospitallers':(174,94,193,125),'morea':(155,80,200,110),'athens':(209,146,91,115)}
 for r in data['regions']:
  if r['id'] in changed:
   pts=[(p['x']*W,(1-p['y'])*H) for p in r['points']];d.polygon(pts,fill=colours[r['group']],outline=(245,213,137,255))
 Image.alpha_composite(im.convert('RGBA'),overlay).crop((590,390,1010,695)).resize((1260,915)).save(ROOT/'Builds/aegean-traced.png')

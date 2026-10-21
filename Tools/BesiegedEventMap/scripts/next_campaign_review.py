"""Overlay source-backed omissions and authored next-release stories, never pretending they are runtime."""
from pathlib import Path
import copy,json,hashlib
B=Path(__file__).resolve().parents[1]
p=B/'dist/data.json';d=json.loads(p.read_text(encoding='utf-8'))
prefix='next:'
d['events']=[n for n in d['events'] if not n['id'].startswith(prefix)]
d['edges']=[e for e in d['edges'] if not e['id'].startswith(prefix)]
d['flowSections']=[s for s in d['flowSections'] if not s['id'].startswith('next-')]
d['sources']=[s for s in d['sources'] if s['id']!='next-court-source']
nodes=[];edges=[]
spec=json.loads((B/'planning/next-campaign/court-stories.json').read_text(encoding='utf-8'))
text=(B/'planning/next-campaign/Court stories for review.txt').read_text(encoding='utf-8')
d['sources'].append(dict(id='next-court-source',name='The Rooms Behind the Throne — authored next-release stories',kind='authored-design',locator='planning/next-campaign/court-stories.json',sha256=hashlib.sha256(text.encode()).hexdigest(),text=text))
def link(a,b,label,conditions=None):
 edges.append(dict(id=prefix+'edge-'+str(len(edges)),source=a,target=b,label=label,kind='manual',choiceId='',conditions=conditions or [],origin='authored',relation='condition',note='Planned story transition; NOT a live Unity dispatch.'))
originals=[('random4','uniqueHiddenChamber','Unique · once per campaign','Small expedition probabilities total99% in original. Proposed correction: no-find53→54%; treasure1% unchanged. Grant25%treasure advantage is source-authored; balance review needed.'),('random6','uniqueOttomanShipwreck','Unique · once per campaign','Some salvage chances and costs unspecified in source; proposed completion is in Implementation brief. Preserve original four approaches.'),('random14','uniqueBrutalMurder','Unique · investigation chain','Original is part-written. Proposed evidence and verdict continuations require authoring/integration; not yet scheduled.'),('random15','recurringLiveAndLetLive','Recurring · source explicitly says recurring','This document includes a recurring event. Preserve character-specific options; complete costs and benefits before integration.')]
for i,(old,code,category,review) in enumerate(originals):
 source=next(n for n in d['events'] if n['id']=='design:'+old)
 n=copy.deepcopy(source)
 n.update(id=prefix+code,code=code,collection='proposal',sourceEventId=source['id'],flowRole='attention',section='next-originals',position={'x':55+i%2*650,'y':70+i//2*230},title=source['title'],category=category,status='Original draft · pending Unity integration',implementation='Original source; NOT in live Unity',timing='Proposed priority pool; no live dispatch',notes='Original document ID '+old+' collides with a DIFFERENT live event. New stable ID: '+code+'. '+review,review=[review],isChain=code=='uniqueBrutalMurder')
 for j,c in enumerate(n['choices']):c['id']=n['id']+':choice-'+str(j)
 nodes.append(n)
for e in spec['events']:
 n=dict(id=prefix+e['id'],code=e['id'],title=e['title'],collection='proposal',category='Authored court story · chapter '+str(e['chapter']),origin='authored',status='Written · pending Unity integration',sourceId='next-court-source',sourceLine=0,sourceEventId='',raw=e['text'],description=e['text'],choices=[],effects=[],conditions=['Route remains open; character present; previous chapter resolved' if e['chapter']>1 else 'Siege day7or later; first meeting'],reads=[],writes=[],review=[],notes='Fictional adult character. '+spec['rules']['timing']+' Choices and effects are design specifications, not current engine commands.',entry=False,external=True,flowRole='chain',section='next-romance',position={'x':55+['helena','anna','theodora','damaris'].index(e['character'])*350,'y':70+(e['chapter']-1)*190},timing='At least15calendar days after preceding chapter' if e['chapter']>1 else 'First encounter from siege day7',implementation='Authored design only',isChain=True)
 for c in e['choices']:
  n['choices'].append(dict(id=prefix+c['id'],label=c['label'],line=0,extraction='authored',conditions=[k+': '+str(v) for k,v in c['effects'].items() if k.startswith('requires')],effects=[],outcomes=[],text=c['outcome']+'\nProposed state changes: '+json.dumps(c['effects'],ensure_ascii=False)))
 nodes.append(n)
 if e['chapter']>1 and e['character']!='damaris':link(prefix+e['id'][:-1]+str(e['chapter']-1),n['id'],'At least15days; route still open',['Not declined, departed or dead'])
 if e['character']=='damaris' and e['chapter']>3:link(prefix+'courtDamaris'+str(e['chapter']-1),n['id'],'At least15days; scandal continues')
for root in ['proposal:courtLadyLetters','proposal:courtLadyMarriage']:
 if any(n['id']==root for n in d['events']):link(root,prefix+'courtDamaris3','PLANNED extension · existing Helena renamed Damaris',['15days after existing follow-up'])
d['events']+=nodes;d['edges']+=edges
d['flowSections'] += [dict(id='next-originals',title='Original unique stories · missing from Unity · review for inclusion'),dict(id='next-romance',title='Next-release court stories · authored, not yet playable')]
d['revision']='2026-10-21-current-plus-authored-review-v11'
p.write_text(json.dumps(d,ensure_ascii=False,indent=2),encoding='utf-8')
(B/'dist/data.js').write_text('window.BESIEGED_DATA = '+json.dumps(d,ensure_ascii=False).replace('</','<\\/')+';\n',encoding='utf-8')
assert len({n['id'] for n in d['events']})==len(d['events'])
ids={n['id'] for n in d['events']}
assert all(e['source'] in ids and e['target'] in ids for e in edges)
assert len(nodes)==26
print('Added26clearly labelled review nodes, including4original omissions and22authored court chapters. No runtime nodes changed.')

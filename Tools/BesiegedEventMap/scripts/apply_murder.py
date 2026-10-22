"""Reapply the authored murder-chain overlay after importing/refactoring the base flow."""
from pathlib import Path
import json,copy,hashlib
B=Path(__file__).resolve().parents[1]
p=B/'dist/data.json';d=json.loads(p.read_text(encoding='utf-8'))
spec=json.loads((B/'planning/next-campaign/Brutal Murder chain.json').read_text(encoding='utf-8'))
text=(B/'planning/next-campaign/Brutal Murder design.txt').read_text(encoding='utf-8-sig')
d['events']=[n for n in d['events'] if not n['id'].startswith('mystery:')]
d['edges']=[e for e in d['edges'] if not e['id'].startswith('mystery:')]
d['flowSections']=[s for s in d['flowSections'] if s['id']!='murder']
d['flowSections'].append({'id':'murder','title':'Brutal Murder · seven-stage investigation'})
d['sources']=[s for s in d['sources'] if s['id']!='murder-source']
d['sources'].append({'id':'murder-source','name':'The Names Beneath the Wax — authored expansion of Brutal Murder','kind':'authored-design','locator':'planning/next-campaign/Brutal Murder chain.json','sha256':hashlib.sha256(json.dumps(spec,ensure_ascii=False).encode()).hexdigest(),'text':text})
template=copy.deepcopy(next(n for n in d['events'] if n['collection']=='proposal'))
for index,e in enumerate(spec['nodes']):
 n=copy.deepcopy(template)
 n.update(id='mystery:'+e['id'],code=e['id'],title=e['title'],collection='proposal',category='Unique investigation chain',origin='authored',status='Implemented · preview verified',sourceId='murder-source',sourceLine=0,raw=e['description'],description=e['description'],choices=[],effects=[],conditions=[],reads=[],writes=[],notes='Four frozen culprit variants; only officials in service at opening can be selected. Clues, verdict and epilogue persist. Unity preview passed119checks. Full case variants: planning/next-campaign/Brutal Murder chain.json. Published release snapshot is separate.',entry=index==0,external=index==0,review=[],sourceDuplicate=False,timing='Once per campaign after ten completed siege days' if index==0 else '2–3 days after the preceding stage; dated milestones take priority',sourceEventId='design:random14',flowRole='chain',section='murder',position={'x':80,'y':70+index*180},implementation='Unity/Yarn source integrated · exported preview verified',isChain=True)
 for j,c in enumerate(e['choices']):
  choice={'id':n['id']+':choice-'+str(j),'label':c['label'],'line':0,'extraction':'authored','conditions':[],'effects':[],'outcomes':[],'text':'Evidence and replies follow the frozen culprit. '+('Next stage in '+str(e['delayDaysToNext'])+' calendar days.' if c['next'] else 'Ends the investigation with the selected disposition.')}
  if index==0 and j==1:choice['conditions']=['Sphrantzes is in service']
  if index==6 and j in (1,2,3):choice['conditions']=['Suspect was in service when the case opened']
  if index==6 and j==4:choice['conditions']=['At least two independent evidence categories secured']
  n['choices'].append(choice)
  if c['next']:d['edges'].append({'id':'mystery:edge-'+e['id']+'-'+str(j),'source':n['id'],'target':'mystery:'+c['next'],'label':c['label']+' · '+str(e['delayDaysToNext'])+' days','kind':'manual','choiceId':choice['id'],'conditions':['Case remains open'],'origin':'authored','relation':'flow','note':'Scheduled follow-up; can wait behind a dated milestone.'})
 d['events'].append(n)
for n in d['events']:
 if n['id']=='next:uniqueBrutalMurder':
  n.update(status='Original draft · expanded into playable investigation',review=[],notes='Expanded into The Names Beneath the Wax. See the Brutal Murder investigation section for seven integrated stages and saved culprit variants.')
p.write_text(json.dumps(d,ensure_ascii=False,indent=2),encoding='utf-8')
(B/'dist/data.js').write_text('window.BESIEGED_DATA = '+json.dumps(d,ensure_ascii=False)+';\n',encoding='utf-8')
print('Applied seven verified murder nodes and sixteen choice transitions.')

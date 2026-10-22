"""Preserve the five health events alongside the separate court conversation map."""
from pathlib import Path
import argparse,json,copy,hashlib,re
parser=argparse.ArgumentParser();parser.add_argument('--repo',required=True);args=parser.parse_args()
B=Path(__file__).resolve().parents[1];repo=Path(args.repo)
text=(repo/'Assets/Yarn/Test/Emperor_health.yarn').read_text(encoding='utf-8-sig')
p=B/'dist/data.json';d=json.loads(p.read_text(encoding='utf-8'));source='emperor-health-source'
d['sources']=[s for s in d['sources'] if s['id']!=source]
d['sources'].append(dict(id=source,name='Emperor health events · Unity Yarn',kind='yarn',locator='Assets/Yarn/Test/Emperor_health.yarn',sha256=hashlib.sha256(text.encode()).hexdigest(),text=text))
d['events']=[n for n in d['events'] if not n['id'].startswith('emperor-health:')]
template=next(n for n in d['events'] if n['collection']=='proposal')
for match in re.finditer(r'title: (emperorHealth([0-4]))\s*---(.*?)===',text,re.S):
 code,k,body=match.group(1),int(match.group(2)),match.group(3);title=re.search(r'<b>(.*?)</b>',body).group(1);label=re.search(r'-> (.*?)\n',body).group(1)
 n=copy.deepcopy(template);id='emperor-health:'+code
 n.update(id=id,code=code,title=title,collection='proposal',category='Personal health',origin='authored',status='Implemented · exported preview',sourceId=source,sourceLine=0,raw=body,description=body.split('->')[0].strip(),choices=[dict(id=id+':choice-0',label=label,line=0,extraction='authored',conditions=['An evening drinking with the court'] if k==0 else [],effects=[],outcomes=[],text='EmperorHealthEvent '+str(k)+'; inclusive health roll; health clamped to 0–100%.')],effects=[],conditions=['Siege has begun','At least 30 calendar days since this event last appeared']+(['An evening drinking with the court'] if k==0 else []),reads=['fHealth'],writes=['fHealth'],notes='Single-choice recurring health event. Fatal damage uses the existing Emperor-death ending. Hangover requires drinking; no automatic health event is forced by ordinary conversation.',entry=True,external=True,review=[],sourceDuplicate=False,timing='Eligible in the daily siege event pool; 30-day individual cooldown',flowRole='random',section='random',position={'x':80+k*340,'y':80},implementation='Unity/Yarn; separate EmperorPreview build',isChain=False)
 d['events'].append(n)
# Preserve the archived source wording; update only the curated playable assassination choice.
for n in d['events']:
 if n['collection']=='proposal':
  for c in n.get('choices',[]):
   if 'fencing skill is {Fencing()}' in c.get('label',''):c['label']=c['label'].replace('fencing skill is {Fencing()}','Martial skill is {MartialSkill()} / 10')
p.write_text(json.dumps(d,ensure_ascii=False,indent=2),encoding='utf-8');(B/'dist/data.js').write_text('window.BESIEGED_DATA = '+json.dumps(d,ensure_ascii=False)+';\n',encoding='utf-8')
print('Added five recurring health events without replacing existing map edits.')

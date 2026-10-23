"""Build a curated developer flow from the freshly imported Unity sources."""
from pathlib import Path
import json,copy,re,hashlib
B=Path(__file__).resolve().parents[1]
d=json.loads((B/'dist/data.json').read_text(encoding='utf-8'))
spec=json.loads((B/'planning/preparation.json').read_text(encoding='utf-8'))
archive=B/'planning/preparation-v4-reference.json'
if not archive.exists():archive.write_text(json.dumps(spec,ensure_ascii=False,indent=2),encoding='utf-8')
spec=json.loads(archive.read_text(encoding='utf-8'))
runtime={n['code']:n for n in d['events'] if n['collection']=='runtime'}
pool=['story2','story8','story10','story11','story6','story15','story19','story18','story14','prepIntelligence','prepNaval','prepGrain','story16','story17','prepScorched','story13','story12','story9','prepAdventurers','story7','story20']
aliases={'prepFortress':'rumeli-warning','prepNaval':'naval-warfare','prepGrain':'grain-missions','prepGalataGovernor':'galata-charter','giovanniWarning':'giovanni-demands','giovanniThrone':'giovanni-throne',**{'galata'+str(i):'galata-'+str(i) for i in range(26,33)}}
key=lambda code:aliases.get(code,code)
pid=lambda code:'proposal:'+key(code)
flow=[];links=[];spec['events']=[];spec['links']=[]
def node(code,role,section,x,y,timing=''):
 n=copy.deepcopy(runtime[code]);n.update(id=pid(code),collection='proposal',sourceEventId=runtime[code]['id'],flowRole=role,section=section,position={'x':x,'y':y},timing=timing,implementation='Unity source',status='Implemented',external=role in ['selectable','unlocked','random','conditional'],entry=code=='story0',review=[])
 for i,c in enumerate(n['choices']):c['id']=n['id']+':choice-'+str(i)
 n['category']={'fixed':'Dated milestone','slot':'Flexible month','selectable':'Base preparation','unlocked':'Career unlock','random':'Standalone random event','chain':'Event chain','conditional':'Conditional event','attention':'Unscheduled content'}.get(role,role)
 flow.append(n);return n
def link(a,b,label,kind='manual',conditions=None,relation='flow'):
 links.append(dict(id='flow:'+str(len(links)),source=a,target=b,label=label,kind=kind,choiceId='',conditions=conditions or [],origin='authored',relation=relation,note='Mapped from Unity scheduler / dispatch code; conditional edges are not guaranteed to fire.'))
months=[m for m in spec['months'] if m['date']!='Siege consequences'];previous=None;row=0
for m in months:
 if m['date']=='March 1453':m.update(events=['council-march-1453'],type='Flexible preparation')
 if m['date']=='January 1453':m['type']='Fixed arrival + follow-up'
 for k in m['events']:
  if k.startswith('council-'):
   n=dict(id='proposal:'+k,code=k,title=m['date']+' — choose one',collection='proposal',category='Flexible month',origin='authored',status='Implemented scheduler',sourceId=next(s['id'] for s in d['sources'] if s['name'].endswith('PreparationCampaign.cs')),sourceLine=0,sourceEventId='',raw='',description='Choose one unused preparation from the pool beside the timeline, or pass this month. This slot is part of the Unity scheduler, not an additional story.',choices=[],effects=[],conditions=[],reads=[],writes=[],review=[],notes='',entry=False,external=False,flowRole='slot',section='calendar',position={'x':55,'y':70+row*150},timing=m['date'],implementation='Unity source')
   flow.append(n)
  else:
   code=next((a for a,b in aliases.items() if b==k),k);n=node(code,'fixed','calendar',55,70+row*150,m['date'])
  if previous:link(previous,n['id'],'Next month' if k!='story4' else 'Same-month follow-up','schedule')
  previous=n['id'];row+=1
for i,code in enumerate(pool):
 unlocked=code in ['prepGrain','story16','story17','prepScorched'];n=node(code,'unlocked' if unlocked else 'selectable','pool',470+(i%3)*330,70+(i//3)*170,'One flexible month; once per campaign')
 threshold={'prepGrain':1,'story16':2,'story17':3,'prepScorched':2}.get(code)
 n['access']={'kind':'career-achievements' if unlocked else 'base','scope':'profile' if unlocked else 'campaign','minimumAchievements':threshold}
 if unlocked:n['conditions'].insert(0,str(threshold)+' persistent career achievements')
 if code=='story16':n['conditions']+=['September 1452 or later','Successful April reconnaissance raises infiltration from 45% to 75%']
 if code=='story15':n['conditions'].insert(0,'Galata remains neutral')
 if code=='story14':n['notes']='Appointing a commander also gives +10 army support, once.'
 if code=='prepAdventurers':n['notes']='Invite one of six candidates. Arrival is reported after 30 calendar days and tracked in Court; arrival is a notification, not a separate Yarn event. Each arrived visitor now grants a specialist bonus, has loyalty, and can become siege commander through Court or the commander preparation. Command strengthens their bonus. Personal storylines remain future work.'
 if code=='story18':n['notes']='Preparation gives +50% on the next successful fleet report. Later mission changes use a major action in the Fleet menu; there is no separate reassignment event.'
 if code in ['prepScorched','prepNaval','prepGrain']:n['review']=['Review the new costs and rewards after playtesting; these are provisional balance values.']
 if unlocked:n['notes']+=' Unlock count is a provisional playtest setting, based on persistent main-menu achievements.'
for i,code in enumerate(['prepGalataGovernor','story18a','story18b']):node(code,'chain','pool',470+(i%3)*330,1320,'Only after the relevant preparation choice')
# Calendar slots deliberately do not fan out into 135 overlapping arrows.
link(pid('prepFortress'),pid('story16'),'Successful reconnaissance: +30 infiltration points',conditions=['Reconnaissance succeeded'],relation='influence')
life=['lifeSails','lifeBread','lifeWell','lifeGhost','lifeBarrel','lifeCobbler','lifeDrawing','lifeCandles','lifeCoin','lifeWedding','lifeOnions','lifeQuiet','lifeInsult','lifeStones','lifeSaints','lifeRoof','lifeSong','lifeGenerosity','lifeGunner','lifeLights']
hard=['hardStorm','hardCatastrophicStorm','hardCistern','hardHorse','hardRats','hardDead','hardGrave','hardItch','hardBrothel','hardWoman','hardFavourite','hardChild','hardCoin','hardSand','hardHead','hardScreams','hardMissing','sermonPride']
randoms=[code for code in runtime if re.fullmatch(r'random\d+',code)]+life+hard
for i,code in enumerate(randoms):node(code,'random','random',55+(i%4)*330,70+(i//4)*170,'Optional siege-day event; eligibility checked by game code')
node('random8a','chain','random',385,2110,'25 calendar days after planting gardens')
link(pid('random8'),pid('random8a'),'Harvest due after 25 days',conditions=['Gardens planted'])
runtime['random8']['chainRoot']=True
for i,code in enumerate([c for c in runtime if (c.startswith('hard') or c.startswith('sermon')) and c not in hard]):node(code,'chain','random',55,3000+i*170,'Scheduled investigation or sermon follow-up')

for i,code in enumerate(['lifeFalseHope','lifeMorea','lifeMercenaries','lifeRefugees','lifeBarrelOwner','lifeSurprise','lifeSaintsDebate','lifeSaintsQuestion','combatAssassin','lifeGunnerResult']):node(code,'chain','random',55+(i%4)*330,2320+(i//4)*170,'Connected follow-up; see incoming conditions')
for code,prob in [('lifeFalseHope',55),('lifeMorea',15),('lifeMercenaries',15),('lifeRefugees',15)]:link(pid('lifeSails'),pid(code),str(prob)+'% · next day',conditions=['Sighting resolved next calendar day; announcing does not change arrival odds'],relation='condition')
link(pid('lifeGunner'),pid('lifeGunnerResult'),'Supervised training: return in 5 days',conditions=['Mentor assigned; both temporarily unavailable'],relation='condition')
link(pid('lifeBarrel'),pid('lifeBarrelOwner'),'Owner appears after 3 days',conditions=['Barrel distributed, sold or reserved'],relation='condition')
for n in flow:
 if n['code'] in life:n['notes']='Recurring siege event; 30-day cooldown. Costs and rewards follow the approved daily-life batch.'
 if n['code']=='lifeWell':n['notes']+=' Personal intervention success rate70% is provisional.'
 if n['code']=='lifeMorea':n['notes']='Merchant reward100gold is provisional. Confiscation alone adds a ship. Merchant favour uses a separate persistent0–100 score, starting50.'
 if n['code']=='lifeRefugees':n['notes']='Accepting adds exactly200residents, not200population units. Fractional food and tax contributions carry across days.'

conditional=['foodShortage','galata26','galata27','galata28','galata29','galata30','galata31','galata32','giovanniWarning','giovanniThrone','shah1','shah2','shah3','courtLady','courtLadyLetters','courtLadyMarriage']
for i,code in enumerate(conditional):node(code,'conditional','chains',55+(i%3)*390,70+(i//3)*185,'Condition-triggered; not a compulsory sequence')
link(pid('courtLady'),pid('courtLadyLetters'),'Private affair: intercepted letter',conditions=['Chose A: private affair','At least 5 calendar days later; optional draw'],relation='condition')
links[-1]['choiceId']=pid('courtLady')+':choice-0'
link(pid('courtLady'),pid('courtLadyMarriage'),'Marriage: court scandal',conditions=['Chose C: marriage','At least 5 calendar days later; optional draw'],relation='condition')
links[-1]['choiceId']=pid('courtLady')+':choice-2'
for n in flow:
 if n['code'].startswith('courtLady'):
  n['notes']='Fictional adult court intrigue. One occurrence per campaign; follow-up is eligible after five days, not guaranteed on day five. Matches use active secular courtiers; historical marital status is not modelled.'
  n['review']=['Playtest reputation costs and loyalty rewards for this new intrigue.']
link(pid('giovanniWarning'),pid('giovanniThrone'),'Warning unresolved; loyalty ≤20',conditions=['Giovanni alive; autonomous Galata; earlier warning'],relation='condition')
link(pid('shah1'),pid('shah2'),'Gift storyline progresses',relation='condition')
link(pid('shah2'),pid('shah3'),'Later conditional follow-up',relation='condition')
for code,threshold in zip(['galata26','galata27','galata28','galata29','galata30','galata31','galata32'],[100,70,60,50,40,30,0]):
 n=next(n for n in flow if n['code']==code);n['conditions']=['Goodwill '+('≥' if threshold>=50 else '≤')+' '+str(threshold),'Siege active; Galata not under Turkish control; once per campaign'];n['notes']='Eligible optional event. It is not the next step of a seven-event chain.'
for i,code in enumerate(['story21','story24']):
 n=node(code,'attention','attention',55+(i%3)*360,70+(i//3)*170,'No active dispatch in the new campaign');n['review']=['Decide whether to schedule, redesign or retire this event. It exists in source but is not routed into the new campaign.'];n['external']=False;n['status']='Needs a scheduling decision'
# Follow the implemented HardNext scheduler commands, retaining choice association.
for n in flow:
 if n['code'] in hard:n['notes']='Approved siege hardship; 30-day repeat cooldown. Investigated named chains run once per campaign.'
 if n['code'] in ['hardStorm','hardCatastrophicStorm']:n['notes']='Recurring siege event; 30-day cooldown. One uniformly selected incoming maritime relief expedition: delay10days or lose its undelivered cargo. Open-water missions lose0–5ships; harbour mission protected. Ottomans lose1–10ships bounded by remaining fleet.'
 if n['code']=='story23':n['notes']+=' Opening infiltration:1agent50%,2agents30%,3agents17%,5agents3%; costs5000gold, replacing oldfixed5reward.'
 if n['code'].startswith('sermon'):n['notes']='Gennadius Scholarius, historical anti-Unionist monk. The seven-sin game arc and all speeches are authored fiction, not historical quotations. Scripture is paraphrased. Seven calendar days between chapters; choices can end the chain.'
 for choice,effects in [(None,n['effects'])]+[(c,c['effects']) for c in n['choices']]:
  for e in effects:
   m=re.fullmatch(r'HardNext (Grave|Sand|Woman|Missing|Sermon) (\d+) (\d+)',e['command'])
   if not m or int(m[2])==99:continue
   target=('sermon'+['','Pride','Greed','Lust','Envy','Gluttony','Wrath','Sloth'][int(m[2])]) if m[1]=='Sermon' else 'hard'+m[1]+m[2]
   link(n['id'],pid(target),'Follow-up in '+m[3]+' days',conditions=['Unless this choice closes the chain; due chapters queued'],relation='condition')
   if choice:links[-1]['choiceId']=choice['id']
for i,n in enumerate(n for n in flow if n['section']=='random'):n['position']={'x':55+(i%4)*330,'y':70+(i//4)*170}
# Keep actual Yarn choice jumps, including governance and the victorious fleet.
by_runtime={n.get('sourceEventId'):n for n in flow if n.get('sourceEventId')}
for e in d['edges']:
 if e['kind']!='jump' or e['source'] not in by_runtime or e['target'] not in by_runtime:continue
 a,b=by_runtime[e['source']],by_runtime[e['target']];mapped=copy.deepcopy(e);mapped.update(id='current:'+e['id'],source=a['id'],target=b['id']);mapped['choiceId']=''
 if e['choiceId']:
  orig=next(n for n in d['events'] if n['id']==e['source']);index=next((i for i,c in enumerate(orig['choices']) if c['id']==e['choiceId']),None)
  if index is not None:mapped['choiceId']=a['choices'][index]['id']
 links.append(mapped)
for n in flow:
 n['isChain']=any(e['source']==n['id'] or e['target']==n['id'] for e in links if e['kind']!='schedule' and e.get('relation')!='influence')
 if n['flowRole']=='random' and n['isChain']:n['category']='Random event · chain start'
spec.update(version=6,title='Current Unity campaign flow',status='Synced from desktop Unity source; map edits do not change game code',months=months,baseProjectPool=[key(c) for c in pool if c not in ['prepGrain','story16','story17','prepScorched']],unlockedProjectPool=[key(c) for c in ['prepGrain','story16','story17','prepScorched']],projectPool=[key(c) for c in pool])
spec['profileUnlocks']['minimumCounts']={'grain-missions':1,'story16':2,'story17':3,'prepScorched':2};spec['profileUnlocks']['suggestedCounts']=spec['profileUnlocks']['minimumCounts'].copy()
spec['decisions']=[{'question':'Which unscheduled stories should return?','recommendation':'Review the two cards in Needs attention; they are excluded from the playable flow.'},{'question':'How demanding should career unlocks be?','recommendation':'Current playtest counts: grain 1, Rumeli 2, Greek Fire 3, scorched earth 2. Tune after playtesting.'},{'question':'Are the new naval, grain and scorched-earth rewards balanced?','recommendation':'The visible choices show the implemented values. Treat them as provisional playtest numbers.'}]
for n in flow:spec['events'].append(dict(key=n['id'][9:],title=n['title'],access=n.get('access'),lane=n['category'],group='preparation' if n['section']=='pool' else n['section'],choices=n['choices'],status=n['status']))
d['events']=[n for n in d['events'] if n['collection']!='proposal']+flow
d['edges']=[e for e in d['edges'] if not e['source'].startswith('proposal:') and not(e['source'].startswith('runtime:') and e['kind']=='schedule')]+links
for n in d['events']:
 if n['collection']=='runtime':
  mapped=by_runtime.get(n['id']);n['external']=bool(mapped and mapped['section']!='attention');n['timing']=mapped['timing'] if mapped else 'Utility / fallback node';n['entry']=n['code']=='story0'
d.update(preparation=spec,revision='2026-10-21-current-flow-v10')
d['flowSections']=[{'id':'calendar','title':'Dated milestones & flexible months'},{'id':'pool','title':'Closed preparation pool · choose one per flexible month'},{'id':'random','title':'Siege random events · chain or standalone'},{'id':'chains','title':'Conditional events & storylines'},{'id':'attention','title':'Needs a scheduling decision'}]
(B/'planning/preparation.json').write_text(json.dumps(spec,ensure_ascii=False,indent=2),encoding='utf-8')
(B/'dist/data.json').write_text(json.dumps(d,ensure_ascii=False,indent=2),encoding='utf-8')
(B/'dist/data.js').write_text('window.BESIEGED_DATA = '+json.dumps(d,ensure_ascii=False).replace('</','<\\/')+';\n',encoding='utf-8')
print('Current flow:',len(flow),'nodes;',len(links),'links; 21 preparations; 2 scheduling decisions.')
if (B/'planning/next-campaign/court-stories.json').exists():
 import runpy
 runpy.run_path(str(B/'scripts/next_campaign_review.py'))

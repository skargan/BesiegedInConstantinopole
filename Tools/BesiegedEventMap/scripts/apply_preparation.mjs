// Apply the durable authored proposal after a fresh source import. Never rewrites game files.
import {readFile,writeFile} from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {createHash} from 'node:crypto';
const base=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
const specText=await readFile(path.join(base,'planning/preparation.json'),'utf8');
const spec=JSON.parse(specText), data=JSON.parse(await readFile(path.join(base,'dist/data.json'),'utf8'));
data.events=data.events.filter(n=>n.collection!=='proposal');
data.edges=data.edges.filter(e=>!e.source.startsWith('proposal:'));
data.sources=data.sources.filter(s=>s.id!=='src-preparation-proposal');
data.sources.push({id:'src-preparation-proposal',name:'Preparation framework · authored 21 October 2026',kind:'authored-proposal',locator:'planning/preparation.json · user decisions plus explicitly provisional recommendations; not an executable scheduler',sha256:createHash('sha256').update(specText).digest('hex'),text:specText});
const id=k=>'proposal:'+k;
for(const item of spec.events){
 const source=data.events.find(n=>n.id===item.source), n=source?JSON.parse(JSON.stringify(source)):{};
 const choices=item.choices?item.choices.map((c,i)=>({id:id(item.key)+':choice:'+c.key,label:c.label,text:c.text,conditions:c.conditions||[],effects:[],outcomes:[],extraction:'authored',decisionStatus:c.status||'Proposed'})):(n.choices||[]).map((c,i)=>({...c,id:id(item.key)+':choice:'+c.id.split(':').pop(),label:item.labels?.[i]||c.label,referenceChoiceId:c.id}));
 const own={id:id(item.key),code:item.source?item.source.split(':')[1]:item.key,title:item.title||n.title,collection:'proposal',category:item.lane,origin:'authored',status:item.status||'Proposed',sourceId:source?.sourceId||'src-preparation-proposal',sourceLine:source?.sourceLine||0,sourceEventId:source?.id||'',raw:source?.raw||'',description:item.description||n.description||'',notes:item.notes||'',timing:item.when,entry:item.key==='story0',external:!!item.external,effects:[],reads:[],writes:[],conditions:item.conditions||[],review:item.questions||[],choices,position:{x:item.column*390,y:spec.lanes.indexOf(item.lane)*200},implementation:'planning-only',countGroup:item.group||'preparation',access:item.access||null,opportunity:item.opportunity||null};
 if(source)own.notes+='\nSource event: '+source.id+'. Copied commands, where shown, are existing source effects to review; they do not implement this calendar. Original excerpt remains below.';
 data.events.push(own);
}
for(const [i,e] of spec.links.entries()){
 const src=data.events.find(n=>n.id===id(e.from));
 const choice=e.choice===undefined?'':src.choices[e.choice]?.id;
 if(choice===undefined)throw Error('Invalid choice index: '+e.from);
 data.edges.push({id:'proposal-link:'+e.from+':'+e.to+':'+(e.choice??'event')+':'+e.type,source:id(e.from),target:id(e.to),kind:'manual',label:e.label,choiceId:choice,conditions:e.conditions||[],origin:'authored',relation:e.type,note:e.note||'PROPOSED '+e.type+' relationship. Does not add an executable Yarn jump or prove the destination must fire.'});
}
data.preparation=spec;
data.revision='2026-10-21-preparation-v4';
data.importNotes=data.importNotes.filter(s=>!s.startsWith('Preparation proposal:'));
data.importNotes.push('Preparation proposal: authored collection with its own stable IDs. Calendar links, prerequisites and optional effects have distinct labels. Original preparation mechanics are superseded for this proposal only.');
await writeFile(path.join(base,'dist/data.json'),JSON.stringify(data,null,2));
await writeFile(path.join(base,'dist/data.js'),'window.BESIEGED_DATA = '+JSON.stringify(data).replaceAll('</','<\\/')+';\n');
console.log(JSON.stringify({proposalEvents:spec.events.length,proposalLinks:spec.links.length,groups:Object.fromEntries([...new Set(spec.events.map(e=>e.group||'preparation'))].map(g=>[g,spec.events.filter(e=>(e.group||'preparation')===g).length]))}));

(function (root, factory) {
  const api = factory();
  if (typeof module === 'object' && module.exports) module.exports = api;
  else root.EventGraph = api;
})(typeof globalThis !== 'undefined' ? globalThis : this, function () {
  'use strict';
  const edgeKinds = ['jump', 'schedule', 'manual'];
  function validate(data) {
    const errors = [];
    if (!data || data.schemaVersion !== 1) return ['Expected schemaVersion 1.'];
    if (!Array.isArray(data.events) || !Array.isArray(data.edges) || !Array.isArray(data.sources)) return ['events, edges and sources must be arrays.'];
    if (data.events.length > 5000 || data.edges.length > 20000) return ['Dataset exceeds the 5,000 event / 20,000 link limit.'];
    const ids = new Set(), sourceIds = new Set();
    for (const s of data.sources) {
      if (!s || typeof s.id !== 'string' || typeof s.name !== 'string' || typeof s.text !== 'string' || sourceIds.has(s.id)) errors.push('Invalid or duplicate source.');
      else sourceIds.add(s.id);
    }
    const strings = ['id','code','title','collection','category','origin','status','description','notes','raw'];
    const lists = ['choices','effects','conditions','reads','writes','review'];
    for (const n of data.events) {
      if (!n || strings.some(k => typeof n[k] !== 'string') || lists.some(k => !Array.isArray(n[k]))) { errors.push('Each event needs string fields and choices/effects/conditions/reads/writes/review arrays.'); continue; }
      if (!n.id.trim() || ids.has(n.id)) errors.push('Duplicate or empty event ID: '+n.id);
      ids.add(n.id);
      if (!['runtime','design','catalogue','proposal'].includes(n.collection)) errors.push('Unknown collection: '+n.collection);
      if (!['imported','authored','sample'].includes(n.origin)) errors.push('Unknown origin on '+n.id);
      if (n.sourceId && !sourceIds.has(n.sourceId)) errors.push('Unknown source on '+n.id);
      if (['conditions','reads','writes','review'].some(k => n[k].some(x => typeof x !== 'string'))) errors.push('Event lists must contain strings: '+n.id);
      if (n.position && (!Number.isFinite(n.position.x) || !Number.isFinite(n.position.y))) errors.push('Invalid saved position on '+n.id);
      const cs = new Set();
      for (const c of n.choices) {
        if (!c || typeof c.id !== 'string' || typeof c.label !== 'string' || typeof c.text !== 'string' || !Array.isArray(c.conditions) || c.conditions.some(x=>typeof x!=='string') || !Array.isArray(c.effects) || !Array.isArray(c.outcomes)) { errors.push('Invalid choice on '+n.id); continue; }
        if (cs.has(c.id)) errors.push('Duplicate choice ID on '+n.id); cs.add(c.id);
        for (const o of c.outcomes) if (!o || typeof o.text!=='string' || !Number.isFinite(o.probability) || o.probability<0 || o.probability>100) errors.push('Invalid probability outcome on '+n.id);
      }
      const effects = n.effects.concat(...n.choices.filter(c=>c && Array.isArray(c.effects)).map(c=>c.effects));
      for (const e of effects) if (!e || ['command','variable','operation','value'].some(k=>typeof e[k]!=='string') || !Array.isArray(e.guards) || e.guards.some(g=>typeof g!=='string')) errors.push('Invalid effect on '+n.id);
    }
    const linkIds = new Set();
    for (const e of data.edges) {
      if (!e || ['id','source','target','label','kind','choiceId','note','origin'].some(k => typeof e[k] !== 'string') || !Array.isArray(e.conditions) || e.conditions.some(x=>typeof x!=='string')) { errors.push('Invalid link record.'); continue; }
      if (!edgeKinds.includes(e.kind)) errors.push('Unknown link kind: '+e.kind);
      if (linkIds.has(e.id)) errors.push('Duplicate link ID: '+e.id); linkIds.add(e.id);
      const source = data.events.find(n=>n.id===e.source);
      if (!source) errors.push('Link source does not exist: '+e.source);
      if (e.choiceId && source && !source.choices.some(c=>c.id===e.choiceId)) errors.push('Link references a missing choice: '+e.id);
      // Missing targets are allowed: they are one of the key things the map diagnoses.
    }
    return [...new Set(errors)];
  }
  function walk(start, edges, direction = 'down') {
    const visited = new Set(Array.isArray(start) ? start : [start]), queue = [...visited];
    const adjacency = new Map();
    for (const e of edges) { const a=direction==='up'?e.target:e.source, b=direction==='up'?e.source:e.target; if(!adjacency.has(a))adjacency.set(a,[]); adjacency.get(a).push(b); }
    while (queue.length) for (const next of adjacency.get(queue.shift()) || []) if (!visited.has(next)) { visited.add(next); queue.push(next); }
    return visited;
  }
  function cycles(nodes, edges) {
    const ids = new Set(nodes.map(n=>n.id)); let index=0;
    const stack=[], on=new Set(), indices=new Map(), low=new Map(), results=[];
    const adjacency=new Map(nodes.map(n=>[n.id,[]]));
    for(const e of edges)if(ids.has(e.source)&&ids.has(e.target))adjacency.get(e.source).push(e.target);
    function visit(v) {
      indices.set(v,index);low.set(v,index++);stack.push(v);on.add(v);
      for(const w of adjacency.get(v)) { if(!indices.has(w)){visit(w);low.set(v,Math.min(low.get(v),low.get(w)));}else if(on.has(w))low.set(v,Math.min(low.get(v),indices.get(w))); }
      if(low.get(v)===indices.get(v)){const component=[];let w;do{w=stack.pop();on.delete(w);component.push(w);}while(w!==v);if(component.length>1||adjacency.get(v).includes(v))results.push(component);}
    }
    for(const n of nodes)if(!indices.has(n.id))visit(n.id);
    return results;
  }
  function analyze(data, collection) {
    const nodes=data.events.filter(n=>n.collection===collection), ids=new Set(nodes.map(n=>n.id));
    const links=data.edges.filter(e=>ids.has(e.source)), incoming=new Set(links.map(e=>e.target)), outgoing=new Set(links.map(e=>e.source));
    const roots=nodes.filter(n=>n.entry||n.external).map(n=>n.id), reachable=walk(roots,links);
    const findings=[];
    const add=(type,severity,node,message,edgeId='')=>findings.push({type,severity,node,message,edgeId});
    for(const e of links)if(!data.events.some(n=>n.id===e.target))add('Missing target','error',e.source,'Unresolved target '+e.target+' ('+e.kind+')',e.id);
    const codes=new Map();for(const n of nodes){if(!codes.has(n.code))codes.set(n.code,[]);codes.get(n.code).push(n);}
    for(const [code,group]of codes)if(group.length>1)for(const n of group)add('Duplicate code','error',n.id,code+' is used by '+group.length+' imported records.');
    for(const n of nodes){
      if(!n.entry&&!n.external&&!reachable.has(n.id)&&!n.review.length)add('Unreached','review',n.id,'No structural path from a declared entry or external trigger. Scheduling outside these sources may exist.');
      if(!incoming.has(n.id)&&!outgoing.has(n.id))add('Isolated','info',n.id,'No explicit or scheduled links. Random/enemy events can legitimately be dispatched externally.');
      for(const r of n.review)add('Source review','review',n.id,r);
      for(const c of n.choices){
        if(!c.effects.length&&!c.outcomes.length&&!c.text.trim()&&!data.edges.some(e=>e.source===n.id&&e.choiceId===c.id))add('Empty choice','review',n.id,'No outcome supplied for: '+c.label);
        if(c.outcomes.length){const sum=c.outcomes.reduce((a,o)=>a+o.probability,0);if(Math.abs(sum-100)>.01)add('Probability total','review',n.id,'Choice outcome percentages sum to '+sum+'%: '+c.label);}
      }
    }
    for(const group of cycles(nodes,links))add('Cycle','info',group[0],'Cycle: '+group.join(' → ')+'. May be intentional.');
    return {nodes,links,findings,roots,reachable,counts:{events:nodes.length,choices:nodes.reduce((s,n)=>s+n.choices.length,0),links:links.length,errors:findings.filter(f=>f.severity==='error').length,review:findings.filter(f=>f.severity==='review').length}};
  }
  function correlations(nodes, selected) {
    const node=nodes.find(n=>n.id===selected);if(!node)return [];
    const result=[];
    for(const other of nodes){if(other.id===node.id)continue;
      for(const v of node.writes.filter(v=>v!=='chance'&&other.reads.includes(v)))result.push({id:'state:'+node.id+':'+other.id+':'+v,source:node.id,target:other.id,kind:'state',label:v,conditions:[],note:'Possible influence: writer → condition reader. Values, order and branch feasibility are not proven.'});
      for(const v of other.writes.filter(v=>v!=='chance'&&node.reads.includes(v)))result.push({id:'state:'+other.id+':'+node.id+':'+v,source:other.id,target:node.id,kind:'state',label:v,conditions:[],note:'Possible influence: writer → condition reader. Values, order and branch feasibility are not proven.'});
    }
    return result;
  }
  function layout(nodes,edges) {
    const ids=new Set(nodes.map(n=>n.id)), depth=new Map(nodes.map(n=>[n.id,0]));
    const inDegree=new Map(nodes.map(n=>[n.id,0])), adj=new Map(nodes.map(n=>[n.id,[]]));
    for(const e of edges)if(ids.has(e.source)&&ids.has(e.target)&&e.kind!=='state'){inDegree.set(e.target,inDegree.get(e.target)+1);adj.get(e.source).push(e.target);}
    const queue=nodes.filter(n=>!inDegree.get(n.id)).map(n=>n.id), done=new Set();
    while(queue.length){const v=queue.shift();done.add(v);for(const w of adj.get(v)){depth.set(w,Math.max(depth.get(w),depth.get(v)+1));inDegree.set(w,inDegree.get(w)-1);if(inDegree.get(w)===0)queue.push(w);}}
    const positions={}, lanes=new Map();let disconnected=0;
    for(const n of nodes){
      const connected=edges.some(e=>e.kind!=='state'&&(e.source===n.id||e.target===n.id));
      let d=depth.get(n.id), lane;
      if(!connected){d=disconnected%4;lane=4+Math.floor(disconnected++/4);}else {lane=lanes.get(d)||0;lanes.set(d,lane+1);}
      positions[n.id]=n.position||{x:70+d*350,y:75+lane*165};
    }
    return positions;
  }
  function refreshDerived(n){
    const effects=n.effects.concat(...n.choices.map(c=>c.effects));
    n.writes=[...new Set(effects.map(e=>e.variable).filter(Boolean))].sort();
    const conditions=n.conditions.concat(...n.choices.map(c=>c.conditions),...effects.map(e=>e.guards));
    const names=[];for(const s of conditions)for(const m of s.matchAll(/\$(\w+)|Get(?:Int|Float|Bool)\("?(\w+)"?\)/g))names.push(m[1]||m[2]);
    n.reads=[...new Set(names)].sort();return n;
  }
  function populationEffect(e){
    if(e.variable!=='iPopulation')return null;
    const values=String(e.value).trim().split(/\s+/);
    if(!values.every(v=>/^[+-]?\d+$/.test(v)))return null;
    const number=v=>(Number(v)*1000).toLocaleString('en-GB');
    const delta=v=>(Number(v)>0?'+':'')+number(v);
    if(e.operation==='Change'&&values.length===1)return 'City population '+delta(values[0])+' people';
    if(e.operation==='SetInt'&&values.length===1)return 'City population = '+number(values[0])+' people';
    if(e.operation==='RndChange'&&values.length===2)return 'City population '+delta(values[0])+' to '+delta(values[1])+' people (random, in steps of 1,000)';
    return null;
  }
  return {validate,walk,cycles,analyze,correlations,layout,refreshDerived,populationEffect};
});

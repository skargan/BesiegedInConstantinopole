/* Small executable rules for the developer rehearsal, not the Unity runtime. */
(function(root,factory){if(typeof module==='object'&&module.exports)module.exports=factory();else root.PreparationRules=factory();})(typeof window==='object'?window:globalThis,function(){
  'use strict';
  const fresh=()=>({bookings:{},achievementCount:0,useSuggestedThresholds:false,galataState:'neutral',siegeActive:false,landDelayDays:0});
  function availability(spec,key,month,state){
    const event=spec.events.find(e=>e.key===key),reasons=[];
    if(!event||!spec.projectPool.includes(key))return {available:false,reasons:['Not a selectable preparation']};
    const row=spec.months.find(m=>m.date===month);
    if(!row?.type.startsWith('Flexible'))reasons.push('This is a fixed month');
    if(state.bookings[month])reasons.push('This month already has a commitment');
    if(Object.values(state.bookings).includes(key))reasons.push('Already selected for another month');
    if(key==='story15'&&state.galataState!=='neutral')reasons.push('Galata must be neutral');
    if(key==='story16'&&spec.months.findIndex(m=>m.date===month)<spec.months.findIndex(m=>m.date==='September 1452'))reasons.push('Available from September 1452');
    if(event.access?.kind==='career-achievements'){
      const threshold=state.useSuggestedThresholds?spec.profileUnlocks.suggestedCounts[key]:spec.profileUnlocks.minimumCounts[key];
      if(!Number.isInteger(threshold))reasons.push('Achievement threshold awaits a decision');
      else if(state.achievementCount<threshold)reasons.push(`Requires ${threshold} persistent achievements; preview has ${state.achievementCount}`);
    }
    return {available:reasons.length===0,reasons};
  }
  function commit(spec,state,key,month){
    if(key==='pass'){
      if(!spec.months.some(m=>m.date===month&&m.type.startsWith('Flexible'))||state.bookings[month])throw Error('Month is not available');
    }else{const a=availability(spec,key,month,state);if(!a.available)throw Error(a.reasons.join('; '));}
    return {...state,bookings:{...state.bookings,[month]:key}};
  }
  function attackEligibility(spec,state){
    const rule=spec.politics.ottomanAttack,reasons=[];
    if(!rule.eligibleStates.includes(state.galataState))reasons.push('Requires direct imperial or autonomous Giovanni control');
    if(rule.requiresSiege&&!state.siegeActive)reasons.push('The siege has not begun');
    if(rule.blockedDuringLandDelay&&state.landDelayDays>0)reasons.push(`Ottoman land operations blocked for ${state.landDelayDays} more days`);
    return {available:reasons.length===0,reasons};
  }
  function governance(spec,state,choice){
    if(!state.captureConfirmed||!state.governancePending||!['varangians','giovanni'].includes(state.captureRoute))throw Error('Successful capture with governance pending is required');
    if(!['retain','commission','fief','now','neutral'].includes(choice))throw Error('Unknown governance choice');
    if(['commission','fief','now'].includes(choice)&&!state.giovanniPresent)throw Error('Giovanni must be alive and present');
    const next={...state,governancePending:false};
    next.galataState=choice==='now'?'giovanni':choice==='neutral'?'neutral':'imperial';
    next.giovanniLoyaltyDelta=(state.giovanniLoyaltyDelta||0)+(choice==='now'?spec.politics.grants.immediateLoyalty:choice==='fief'?spec.politics.grants.postwarLoyalty:0);
    if(choice==='now')next.defenceIncreasePending=true; // Deliberately no invented number.
    if(choice==='fief')next.galataPostwarPromise=true;
    return next;
  }
  return {fresh,availability,commit,attackEligibility,governance};
});

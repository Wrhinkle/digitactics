// Presentation only: callers must commit the award before enqueueing it.
const DT=window.DT;
function createLootNoticeQueue(){
  const names={'Daybreak Box':'daily','Cipher Box':'vault','Cipher Key':'key'},seen=new Set(),recent=[];
  let current=null,pending=null,serial=0,elapsed=0;
  const order={daily:0,vault:1,key:2};
  const snapshot=()=>current&&({serial:current.serial,items:Object.values(current.items).sort((a,b)=>order[a.id]-order[b.id]).map(i=>({id:i.id,name:i.name,count:i.count,source:i.id==='daily'?'Activity reward · 2 active days completed':i.rounds===1?`Round drop · ${i.lastRound} complete`:`Round drops · ${i.rounds} rounds, latest ${i.lastRound}`}))});
  const dismiss=()=>{current=pending;pending=null;elapsed=0;};
  return {
    earned({id,round,drops}){
      const known=drops.filter(name=>Object.hasOwn(names,name));if(!known.length||seen.has(id))return false;
      seen.add(id);recent.push(id);while(recent.length>1000)seen.delete(recent.shift());
      let target;if(!current){target=current={serial:++serial,items:{}};elapsed=0;}else target=pending||(pending={serial:++serial,items:{}});
      for(const name of new Set(known)){const key=names[name],item=target.items[key]||(target.items[key]={id:key,name,count:0,rounds:0,lastRound:''});item.count+=known.filter(n=>n===name).length;item.rounds++;item.lastRound=round;}
      return true;
    },
    advance(seconds,paused=false){if(!Number.isFinite(seconds)||seconds<0)throw new Error('Invalid notice time');if(!current||paused)return;elapsed+=seconds;if(elapsed>=10)dismiss();},
    dismiss,clear(){current=pending=null;elapsed=0;seen.clear();recent.length=0;},
    get current(){return snapshot();},get elapsed(){return elapsed;},get pendingCount(){return pending?Object.values(pending.items).reduce((n,i)=>n+i.count,0):0;}
  };
}
const queue=createLootNoticeQueue();
DT.LootNotices={earned:award=>queue.earned(award),clear:()=>queue.clear()};
if(typeof document!=='undefined'){
  const css=document.createElement('style');css.textContent=`
  #lootAwardNotice{position:fixed;left:50%;bottom:24px;z-index:1500;transform:translateX(-50%);width:min(780px,calc(100vw - 48px));box-sizing:border-box;padding:18px 22px;background:rgba(5,15,26,.98);border:2px solid #c8aa6e;box-shadow:0 14px 60px #000b;font-family:var(--font-body,sans-serif);color:#edf3fa;border-radius:10px;animation:lootNoticeIn .22s ease-out}
  #lootAwardNotice[hidden]{display:none}#lootAwardNotice h2{font-size:24px;line-height:1.2;color:#f0d58f;margin:0 0 16px;font-family:var(--font-display,sans-serif);letter-spacing:.06em}
  #lootAwardNotice .loot-award-items{display:grid;grid-template-columns:repeat(var(--items),minmax(0,1fr));gap:16px}
  #lootAwardNotice .loot-award-item{display:flex;gap:12px;min-width:0;align-items:flex-start}#lootAwardNotice svg{width:44px;height:44px;flex:none;color:#61d5ef}#lootAwardNotice .daily svg{color:#f0c96a}
  #lootAwardNotice .loot-award-name{font-size:20px;font-weight:800;line-height:1.3;margin:0 0 5px}#lootAwardNotice .loot-award-source{font-size:16px;line-height:1.4;color:#c0cddd;margin:0}#lootAwardNotice footer{display:flex;gap:16px;align-items:center;justify-content:space-between;margin-top:18px}#lootAwardNotice .loot-award-hint{font-size:16px;line-height:1.4;color:#c0cddd;margin:0}#lootAwardNotice button{min-width:112px;min-height:38px;font:700 17px var(--font-body,sans-serif);padding:8px 14px;color:#f5e1ab;background:#26394b;border:1px solid #c8aa6e;cursor:pointer}#lootAwardNotice button:focus-visible{outline:3px solid #fff;outline-offset:3px}#lootAwardNotice button:hover{background:#38546b}
  #lootAwardNotice .loot-award-progress{position:absolute;bottom:0;left:0;height:3px;background:#e7c97f;width:var(--remaining,100%);border-radius:0 0 8px 8px}
  @keyframes lootNoticeIn{from{opacity:0;transform:translate(-50%,18px)}to{opacity:1;transform:translate(-50%,0)}}body.reduce-motion #lootAwardNotice{animation:none}@media(prefers-reduced-motion:reduce){#lootAwardNotice{animation:none}}@media(max-width:620px){#lootAwardNotice{bottom:12px;padding:14px;width:calc(100vw - 24px)}#lootAwardNotice .loot-award-items{grid-template-columns:1fr;gap:10px}#lootAwardNotice h2{font-size:21px;margin-bottom:10px}#lootAwardNotice .loot-award-name{font-size:18px}#lootAwardNotice footer{margin-top:12px}}
  `;document.head.appendChild(css);
  const panel=document.createElement('section');panel.id='lootAwardNotice';panel.hidden=true;panel.setAttribute('aria-label','Loot earned');
  panel.innerHTML='<div role="status" aria-live="polite" aria-atomic="true"><h2>LOOT EARNED</h2><div class="loot-award-items"></div></div><footer><p class="loot-award-hint">Saved in Loot. Open boxes to discover their contents.</p><button type="button" aria-label="Dismiss loot earned notification">Dismiss</button></footer><div class="loot-award-progress" aria-hidden="true"></div>';
  document.body.appendChild(panel);
  panel.querySelector('button').onclick=()=>queue.dismiss();
  panel.addEventListener('keydown',event=>{if(event.key==='Escape'){event.preventDefault();event.stopPropagation();queue.dismiss();}});
  const icon=id=>`<svg viewBox="0 0 48 48" aria-hidden="true" fill="none" stroke="currentColor" stroke-width="3" stroke-linejoin="round">${id==='key'?'<circle cx="16" cy="16" r="9"/><path d="M22 22L40 40M31 31L36 26M36 36L41 31"/>':'<path d="M7 21V12Q7 7 13 7H35Q41 7 41 12V21M7 21H41V40H7Z"/><path d="M5 21H43M21 21V29H27V21"/>'}</svg>`;
  let shown=0,last=performance.now();
  function frame(now){
    const blocked=document.hidden||!!DT.KillScreens?.active||!!document.querySelector('dialog[open]');
    // A frame timestamp can precede module initialization within the same frame.
    queue.advance(Math.max(0,Math.min(.25,(now-last)/1000)),blocked||panel.matches(':hover')||panel.contains(document.activeElement));last=now;
    const notice=queue.current;
    panel.hidden=!notice||blocked;
    if(notice&&notice.serial!==shown){
      shown=notice.serial;const list=panel.querySelector('.loot-award-items');list.replaceChildren();list.style.setProperty('--items',notice.items.length);
      for(const item of notice.items){const row=document.createElement('div');row.className='loot-award-item '+item.id;row.innerHTML=icon(item.id)+'<div><p class="loot-award-name"></p><p class="loot-award-source"></p></div>';row.querySelector('.loot-award-name').textContent=`${item.name} ×${item.count}`;row.querySelector('.loot-award-source').textContent=item.source;list.appendChild(row);}
    }
    panel.style.setProperty('--remaining',`${100*(1-queue.elapsed/10)}%`);requestAnimationFrame(frame);
  }
  requestAnimationFrame(frame);
  window.addEventListener('storage',event=>{if(event.key==='dt.mysteryLoot.v1'&&event.newValue===null)queue.clear();});
}

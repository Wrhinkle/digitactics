// Composed playback evidence: every resolver mapping, every distinct stage/spec,
// all charge/basic families, and target collection/release/clear boundaries.
const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const {once}=require('node:events'),{finished}=require('node:stream/promises');
const root=path.resolve(__dirname,'../..');
const DT=require('../../tools/headless.cjs').load({strict:true});
const constants=new Function('window',fs.readFileSync(path.join(root,'js/moves.js'),'utf8').replace(/^export /gm,'')+';return {palette:PALETTE,stageScale:STAGE_SCALE};')({DT});

(async()=>{
  const create=await require('./move-reference.cjs')();
  const variants={},groups=[],seenMoves=new Set(),seenBasics=new Set(),seenTypes=new Set(),charges=new Set(),families=new Set();
  let index=0,stageMappings=0,basicMappings=0;
  for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper']){
    DT.Sets.activate(set.id,pace);
    const variant=`${set.id}/${pace}`,lines=Object.values(DT.LINE),cases=[];
    variants[variant]={};
    for(const line of lines){
      const moves=line.abil.map(name=>DT.Moves.specFor(name,line)),basic=[DT.Moves.basicFor(line,false),DT.Moves.basicFor(line,true)];
      variants[variant][line.id]={type:line.type,radius:line.radius||0,moves,basic};
      stageMappings+=moves.length;basicMappings+=basic.length;
      for(let stage=0;stage<3;stage++){
        const spec=moves[stage],key=JSON.stringify([stage,spec,line.type,line.radius||0]);
        charges.add(spec.charge||'aura');
        if(!seenMoves.has(key)){
          seenMoves.add(key);
          const n=seenMoves.size;
          cases.push({mode:'demo',line:line.id,stage,height:n%4===0?null:[1.3,1.65,2][stage]+(line.tier===5?.15:0)+n%3*.07,
            specialDuration:[null,.08,.65,2.4][n%4],targets:n%7===0?[]:[{x:2.1,z:1.7},{x:1.2,z:2.3}]});
        }
        for(let ranged=0;ranged<2;ranged++)for(const big of [false,true]){
          const b=basic[ranged],basicKey=JSON.stringify([stage,b,big]);families.add(b.kind);
          if(seenBasics.has(basicKey))continue;seenBasics.add(basicKey);
          cases.push({mode:'basic',line:line.id,stage,ranged:!!ranged,big,height:seenBasics.size%5===0?0:1.1+stage*.37});
        }
      }
      const typeBranch=['aoe','slam','drain','heal','stun'].includes(line.type)?line.type:'other';
      if(!seenTypes.has(typeBranch)){
        seenTypes.add(typeBranch);
        for(let stage=0;stage<3;stage++)for(const scenario of ['current-target','empty-area','empty','collected','chain','clear','zero-height']){
          cases.push({mode:scenario,line:line.id,stage,height:scenario==='zero-height'?0:null,
            specialDuration:[null,.1,1.7][stage],targets:[{x:2.1,z:1.7},{x:1.2,z:2.3}]});
        }
      }
    }
    groups.push({set:set.id,pace,variant,cases});
  }
  const destination=path.join(root,'native/tests/fixtures/move-playback.jsonl.gz');
  const gzip=zlib.createGzip(),output=fs.createWriteStream(destination);gzip.pipe(output);
  async function write(row){if(!gzip.write(JSON.stringify(row)+'\n'))await once(gzip,'drain');}
  await write({kind:'catalog',schema:1,variants,...constants,coverage:{stageMappings,basicMappings,distinctMoves:seenMoves.size,distinctBasics:seenBasics.size,charges:[...charges],families:[...families]}});
  for(const group of groups){
    DT.Sets.activate(group.set,group.pace);
    for(const item of group.cases){
      const seed=54321+index*43,line=DT.LINE[item.line],name=`${group.variant}/${item.line}/${item.stage}/${item.mode}/${index++}`;
      const reference=create(seed,DT),{fx,SourceMoves,snapshot,step,nextRandom}=reference;
      const playback=Object.create(SourceMoves.prototype);playback.fx=fx;playback.pending=null;
      const from={x:-1.8,y:.12,z:-1},to={x:2.1,z:1.7};
      const view=item.height==null&&item.specialDuration==null?null:{height:item.height,clips:item.specialDuration==null?{}:{special:{duration:item.specialDuration}}};
      const begin=current=>playback.begin(line.abil[item.stage],line,item.stage,from,view,current);
      switch(item.mode){
        case 'demo':playback.demo(line.abil[item.stage],item.stage,from,item.targets,{line,view});break;
        case 'basic':playback.basic(line,item.stage,from,to,item.height,item.big,item.ranged);break;
        case 'current-target':begin(to);playback.flush();break;
        case 'empty-area':begin(null);playback.area({x:1.6,z:1.9},1.75);playback.flush();break;
        case 'empty':begin(null);playback.flush();break;
        case 'collected':begin({x:9,z:9});playback.target(item.targets[0],{x:-3,z:-2});playback.target(item.targets[1]);playback.area({x:1.6,z:1.9},.9);playback.flush();break;
        case 'chain':begin(to);playback.target(item.targets[0]);begin(item.targets[1]);playback.target(item.targets[1]);playback.flush();break;
        case 'clear':begin(to);playback.target(item.targets[0]);playback.clear();playback.flush();break;
        case 'zero-height':begin(to);playback.target(to);playback.flush();break;
        default:throw Error(item.mode);
      }
      // Primitive fixtures already compare every particle. For thousands of
      // composed cases retain counts plus eight distributed states per pool.
      const capture=()=>{
        const value=snapshot();
        for(const key of ['add','alpha']){
          const particles=value[key],n=particles.length;
          const indices=[...new Set([0,1,2,3,Math.floor(n/4),Math.floor(n/2),Math.floor(n*3/4),n-1])].filter(i=>i>=0&&i<n).sort((a,b)=>a-b);
          value[key]={count:n,samples:indices.map(index=>({index,value:particles[index]}))};
        }
        return value;
      };
      const initial=capture(),frames=[],count=item.mode==='basic'?80:180;
      const release=item.specialDuration==null?(DT.Moves.specFor(line.abil[item.stage],line).release??.3)*.7:
        Math.min(DT.Moves.specFor(line.abil[item.stage],line).release??.3,Math.min(item.specialDuration,1.1)*.45);
      let seconds=0,releaseSampled=false;
      for(let frame=0;frame<count;frame++){
        const dt=frame%17===0?.05:frame%7===0?.027:1/60;seconds+=dt;step(dt);
        const afterRelease=!releaseSampled&&seconds>=release+.03;
        if(afterRelease)releaseSampled=true;
        const sample=afterRelease||frame===22||frame===54||frame===count-1;
        frames.push(sample?{dt,snapshot:capture()}:{dt});
      }
      await write({kind:'playback',name,variant:group.variant,...item,seed,from,to,initial,frames,nextRandom:nextRandom()});
      if(index%500===0)console.log(`Captured ${index} native move playback references.`);
    }
  }
  gzip.end();await finished(output);
  console.log(`Exported ${stageMappings} stage mappings, ${basicMappings} basic mappings, and ${index} composed cases (${fs.statSync(destination).size.toLocaleString()} bytes).`);
})().catch(error=>{console.error(error);process.exitCode=1;});

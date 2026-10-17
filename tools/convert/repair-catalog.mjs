// Remove parked attack-only props and name the otherwise anonymous Gaomon rig.
import {NodeIO} from '@gltf-transform/core';
import {fileURLToPath} from 'url';
const io=new NodeIO();
for(const [file,drop,bonePattern] of [
 ['rip_cat_ts_palmon_chr348.glb',['chr348.001'],null],
 ['rip_cat_ts_toyagumon_chr708.glb',['chr708.003'],/^J_fire$/],
 ['rip_cat_ts_falcomon_chr705.glb',[],/^J_shuriken/],
 ['rip_cat_ts_gaomon_chr055.glb',[],null],
]){
 const filename=fileURLToPath(new URL('../../models/src/'+file,import.meta.url)),doc=await io.read(filename);
 for(const n of doc.getRoot().listNodes()){
  if(drop.includes(n.getName()))n.setMesh(null).setSkin(null);
  if(!bonePattern||!n.getMesh()||!n.getSkin())continue;
  const joints=n.getSkin().listJoints(),remove=new Set(joints.map((b,i)=>bonePattern.test(b.getName())?i:-1).filter(i=>i>=0));
  for(const p of n.getMesh().listPrimitives()){
   const js=p.getAttribute('JOINTS_0'),ws=p.getAttribute('WEIGHTS_0');if(!js||!ws)continue;
   const hidden=i=>js.getElement(i,[]).reduce((sum,j,k)=>sum+(remove.has(j)?ws.getElement(i,[])[k]:0),0)>.5;
   const indices=p.getIndices()?.getArray()||Array.from({length:js.getCount()},(_,i)=>i),kept=[];
   for(let i=0;i<indices.length;i+=3)if(![indices[i],indices[i+1],indices[i+2]].every(hidden))kept.push(...indices.slice(i,i+3));
   if(!kept.length)p.dispose();else p.setIndices(doc.createAccessor().setType('SCALAR').setArray(new Uint32Array(kept)).setBuffer(doc.getRoot().listBuffers()[0]));
  }
 }
 if(file.includes('gaomon')){
  const names={J_joint11:'J_chest',J_joint12:'J_head',J_joint21:'J_shoulderR',J_joint22:'J_elbowR',J_joint23:'J_hand_r',J_joint28:'J_shoulderL',J_joint29:'J_elbowL',J_joint30:'J_hand_l',J_joint38:'J_tail01'};
  for(const n of doc.getRoot().listNodes())if(names[n.getName()])n.setName(names[n.getName()]);
 }
 await io.write(filename,doc);console.log(file);
}

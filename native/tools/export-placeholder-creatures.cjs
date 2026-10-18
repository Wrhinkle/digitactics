// Export the original procedural fallback creature, including its exact source geometry/material colors.
const fs=require('fs'),path=require('path');
const {chromium}=require('../artifacts/audio-tools/node_modules/playwright');
const root=path.resolve(__dirname,'../..'),DT=require('../../tools/headless.cjs').load({strict:true});
async function main(){
 const source=fs.readFileSync(path.join(root,'js/render3d.js'),'utf8');
 const family=source.slice(source.indexOf('export const familyColor'),source.indexOf('// ---------- model library')).replace('export const','const');
 const placeholder=source.slice(source.indexOf('const GEO ='),source.indexOf('// A cloned skinned model')).replace('export function','function');
 const module=`import * as THREE from '/vendor/three/build/three.module.js';let DT;${family}\n${placeholder}\nexport function creature(data){DT=data;return makePlaceholder(data.line);}export {THREE};`;
 const variants={};
 for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper']){
  DT.Sets.activate(set.id,pace);
  variants[`${set.id}/${pace}`]=Object.values(DT.LINE).map(line=>({line,TRAITS:DT.TRAITS,ATTR:DT.ATTR}));
 }
 const browser=await chromium.launch({channel:'msedge',headless:true});
 try{
  const page=await browser.newPage();
  await page.route('http://creature.local/**',async route=>{
   const u=new URL(route.request().url());
   if(u.pathname==='/')return route.fulfill({contentType:'text/html',body:'<html></html>'});
   if(u.pathname==='/creature.js')return route.fulfill({contentType:'application/javascript',body:module});
   const file=path.resolve(root,'.'+u.pathname);
   if(!file.startsWith(root+path.sep))throw Error('Unsafe resource');
   return route.fulfill({contentType:'application/javascript',body:fs.readFileSync(file)});
  });
  await page.goto('http://creature.local/');
  const output=await page.evaluate(async variants=>{
   const {creature}=await import('/creature.js'),geometries=[],parts=[];let first=true;const colors={};
   for(const [variant,rows]of Object.entries(variants)){
    colors[variant]={};
    for(const row of rows){
     const object=creature(row);object.updateMatrixWorld(true);const mats=[];
     object.traverse(o=>{if(!o.isMesh)return;mats.push(o.material.color.toArray());
      if(first){const g=o.geometry;let index=geometries.findIndex(x=>x.uuid===g.uuid);if(index<0){index=geometries.length;geometries.push({uuid:g.uuid,positions:Array.from(g.attributes.position.array),normals:Array.from(g.attributes.normal.array),indices:Array.from(g.index.array)});}parts.push({geometry:index,matrix:o.matrixWorld.toArray()});}
      o.material.dispose();
     });
     first=false;colors[variant][row.line.id]=mats;
    }
   }
   return {geometries:geometries.map(({uuid,...g})=>g),parts,colors};
  },JSON.parse(JSON.stringify(variants)));
  fs.writeFileSync(path.join(root,'native/content/placeholder-creatures.json'),JSON.stringify(output));
  console.log(`Exported ${output.parts.length} original creature parts with ${Object.keys(output.colors).length} ruleset color maps.`);
 }finally{await browser.close();}
}
main().catch(e=>{console.error(e);process.exitCode=1});

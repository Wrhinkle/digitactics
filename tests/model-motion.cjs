// Validate actual renderer templates and cloned skeletons, with contact sheets for visual review.
// node tests/model-motion.cjs <playwright> <evidence-dir>
const {chromium}=require(process.argv[2]||'playwright'),{spawn}=require('child_process');
const fs=require('fs'),path=require('path'),assert=require('assert/strict'),vm=require('vm');
const root=path.join(__dirname,'..'),out=path.resolve(process.argv[3]);fs.mkdirSync(out,{recursive:true});
const index=JSON.parse(fs.readFileSync(path.join(root,'models/index.json'))),ctx={DT:{}};ctx.globalThis=ctx;vm.createContext(ctx);vm.runInContext(fs.readFileSync(path.join(root,'js/models.js'),'utf8'),ctx);
(async()=>{let browser;const server=spawn(process.execPath,['serve.cjs'],{cwd:root,env:{...process.env,PORT:'8211'},windowsHide:true,stdio:'ignore'});
try{await new Promise(r=>setTimeout(r,500));browser=await chromium.launch({args:['--use-angle=d3d11']});const p=await browser.newPage(),errors=[];
p.on('pageerror',e=>{errors.push(String(e));console.error(String(e));});p.on('console',m=>{if(/PropertyBinding.*(not found|No target)/i.test(m.text()))errors.push(m.text());});
await p.goto('http://localhost:8211/tests/model-motion.html');await p.waitForFunction(()=>window.auditReady);
const selected=[];
for(const file of new Set(Object.values(index))){const b=fs.readFileSync(path.join(root,file)),g=JSON.parse(b.subarray(20,20+b.readUInt32LE(12)));
 if(file.endsWith('rip_l_garurumon_greymon_statue.glb'))continue; // scenery skeleton, no articulated creature bones
 const keys=Object.keys(index).filter(k=>index[k]===file),key=keys.find(k=>ctx.DT.MODEL_SOURCES[k]?.anim===false)||keys[0];
 if((g.skins?.length&&(!g.animations?.length||ctx.DT.MODEL_SOURCES[key]?.anim===false))||['agumon:0','extra:Bakemon','extra:Vademon','giz:Jijimon'].includes(key))selected.push([key,file]);
}
const results=[];
for(const [key,file]of selected){const r=await p.evaluate(([k,f])=>inspectModel(k,f),[key,file]);const pictures=r.pictures;delete r.pictures;results.push(r);
 assert.ok(r.motion.idle>.005,key+' idle moves cloned bones');
 for(const role of ['attack','special','win'])assert.ok(r.motion[role]>.005,key+' '+role+' moves cloned bones');
 assert.ok(r.eyes.every(e=>e.textured&&e.uv),key+' eyes have texture and UVs');
 const sheet=await p.evaluate(async({pictures,key})=>{const c=document.createElement('canvas');c.width=1200;c.height=330;const g=c.getContext('2d');g.fillStyle='#111';g.fillRect(0,0,c.width,c.height);g.fillStyle='#fff';g.font='14px sans-serif';g.fillText(key,8,18);for(let i=0;i<pictures.length;i++){const im=new Image();im.src=pictures[i].image;await im.decode();g.drawImage(im,i*300,30);g.fillStyle='#fff';g.fillText(pictures[i].role,i*300+8,48);}return c.toDataURL('image/png');},{pictures,key});
 fs.writeFileSync(path.join(out,key.replace(/[^a-z0-9]/gi,'_')+'.png'),Buffer.from(sheet.split(',')[1],'base64'));console.log('PASS '+key+' '+Object.keys(r.motion).join(', '));
}
fs.writeFileSync(path.join(out,'result.json'),JSON.stringify(results,null,2));assert.deepEqual(errors,[]);console.log('PASS '+results.length+' renderer models');
}finally{await browser?.close();server.kill();}})().catch(e=>{console.error(e);process.exitCode=1;});

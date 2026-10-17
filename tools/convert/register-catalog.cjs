// Register converted catalog assets without replacing established roster mappings.
const fs=require('fs'),path=require('path');
const root=path.resolve(__dirname,'../..'),rows=require('./catalog-imports.json');
const games={Links:'Digimon Links',CyberSleuth:'Digimon Story Cyber Sleuth: Complete Edition',TimeStranger:'Digimon Story Time Stranger',NextOrder:'Digimon World: Next Order',Masters:'Digimon Masters',ReArise:'Digimon ReArise',NewCentury:'Digimon New Century',digimonnewcentury:'Digimon New Century',ReDigitize:'Digimon World Re:Digitize'};
const names={
 'Elecmon:chr564':'Elecmon', 'ShineGreymon:chr776':'ShineGreymon Burst Mode',
 'CS_Crusadermon:chr902':'Crusadermon NX','CS_Leopardmon:chr903':'Leopardmon NX','CS_Leopardmon:chr743':'Leopardmon Leopard Mode',
 'CS_Arcadiamon_Ultra:chr912':'Arcadiamon Ultra (Boss)','CS_Examon:chr350':'Examon (Boss)',
 'L_Gallantmon:chr126':'ChaosGallantmon','L_Gallantmon:chr013':'Gallantmon Crimson Mode',
 'L_MirageGaogamon:chr777':'MirageGaogamon Burst Mode','L_Rosemon:chr779':'Rosemon Burst Mode',
 'TS_Goburimon:chr111':'Goblimon','TS_Goburimon:chr578':'SnowGoblimon',
 'TS_Solarmon_Hagurumon:chr009':'Hagurumon','TS_Solarmon_Hagurumon:chr728':'Solarmon',
 'TS_Terriermon_Lopmon_and_Assistant:chr701':'Terriermon','TS_Terriermon_Lopmon_and_Assistant:chr750':'Lopmon',
 'TS_Bubbmon:chr387':'Pabumon','TS_Funbeemon:chr042':'FanBeemon','TS_Gottsumon:chr595':'Gotsumon',
 'TS_Mochimon:chr388':'Motimon','TS_Plotmon:chr361':'Salamon','TS_Piyomon:chr307':'Biyomon',
 'TS_Shakomon:chr709':'Syakomon','TS_Pukamon:chr514':'Bukamon','TS_Pyocomon:chr510':'Yokomon',
 'TS_Tunomon:chr438':'Tsunomon','TS_Tyumon:chr020':'Chuumon','TS_V_mon:chr114':'Veemon',
 'TS_Elecmon_Elecmon_Aegiomon:chr564':'Elecmon','TS_PicoDevimon:chr097':'DemiDevimon',
 'RD_Dodomon:main':'Dorimon','MarineDevimon2:main':'MarineDevimon',
};
const mi=JSON.parse(fs.readFileSync(path.join(root,'models/index.json'))),credits={};
for(const r of rows.filter(r=>!r.reused)){
 const file='models/files/'+r.file,b=fs.readFileSync(path.join(root,file)),g=JSON.parse(b.subarray(20,20+b.readUInt32LE(12)));
 const raw=fs.readFileSync(path.join(root,'models/src',r.file)),j=JSON.parse(raw.subarray(20,20+raw.readUInt32LE(12)));
 const form=names[r.folder+':'+r.code]||r.form,key='catalog:'+r.folder+':'+r.code;
 mi[key]=file;credits[key]={form,uid:'rip-'+r.name.toLowerCase(),rip:true,name:`${form} (${games[r.game]||r.game})`,url:r.source,author:'The Models Resource',license:'Game rip',
  ...(j.animations?.some(a=>/noesis/i.test(a.name))?{anim:false}:{}),
  ...(r.folder==='CS_PlatinumNumemon'?{fallbackRoles:['idle']}:{}),catalog:true};
 if(!g.meshes?.length)throw Error('Empty converted model '+file);
}
fs.writeFileSync(path.join(root,'models/index.json'),JSON.stringify(mi,null,1)+'\n');
const f=path.join(root,'js/models.js');let s=fs.readFileSync(f,'utf8').replace(/\n  \/\/ CATALOG-START[\s\S]*?\/\/ CATALOG-END\n/,'\n');
s=s.replace('\n  // MODEL-OPTIONS',`\n  // CATALOG-START (tools/convert/register-catalog.cjs)\n  Object.assign(DT.MODEL_SOURCES, ${JSON.stringify(credits,null,2)});\n  // CATALOG-END\n\n  // MODEL-OPTIONS`);fs.writeFileSync(f,s);console.log('Registered '+Object.keys(credits).length+' additional catalog models');

const fs=require('fs'),path=require('path'),vm=require('vm'),zlib=require('zlib');
const root=path.resolve(__dirname,'../..'),read=name=>fs.readFileSync(path.join(root,'js',name),'utf8');
const catalog=JSON.parse(fs.readFileSync(path.join(root,'native/src/DigiTactics.Storage/cosmetics.json'),'utf8'));
const trees=JSON.parse(fs.readFileSync(path.join(root,'native/tests/fixtures/evolution-trees.json'),'utf8'));
const LINE=require('../../tools/headless.cjs').load({strict:true}).LINE;
const cosmetics=read('cosmetics.js'),evolution=read('legendevo.js'),farmSource=read('farm.js'),time=1792260300000;
function load(wallet,legend,farm){
 const storage={'dt.cosmetics.v1':JSON.stringify(wallet),'dt.legendevo.v1':JSON.stringify(legend)};
 if(farm)storage['dt.farm.v1']=JSON.stringify(farm);
 class Clock extends Date{constructor(...args){super(...(args.length?args:[time]));}static now(){return time;}}
 const ctx={DT:{LINE},console,Date:Clock,setInterval:()=>0,addEventListener:()=>{},localStorage:{getItem:key=>storage[key]||null,setItem:(key,value)=>storage[key]=value}};ctx.window=ctx;vm.createContext(ctx);
 vm.runInContext(cosmetics,ctx);for(const [kind,items] of Object.entries(catalog))ctx.DT.Cosmetics.register(kind,items);
 vm.runInContext('(function(){'+evolution+'})();',ctx);vm.runInContext('(function(){'+farmSource+'})();',ctx);return ctx.DT;
}
const rows=[];
for(const tree of trees)for(let tier=0;tier<tree.tiers.length;tier++)for(let mode=0;mode<6;mode++){
 const id=tree.root,form=tree.tiers[tier][0],ideal=[10,20,30,40][tier];
 const wallet={bits:1000,owned:{legend:catalog.legend.map(x=>x.id)},equipped:{legend:id},history:[]};
 const legend={v:1,legends:{[id]:{xp:2000,form,path:[],dex:[id,form],nonce:0,care:{bond:mode,battle:mode,food:mode}}},games:[]};
 const farm=JSON.parse(JSON.stringify(load(wallet,legend).Farm._save()));const pet=farm.pets[id];
 pet.born=time-(mode+.5)*86400000;pet.weight=ideal*[.69,.7,.71,1.39,1.4,1.41][mode];
 pet.mistakes=mode;pet.poopNeglect=mode%3;pet.overfeed=mode;pet.goodBond=mode*1.7;pet.goodFood=mode*2.3;
 pet.str=mode*23;pet.spd=mode*3;pet.wis=mode*2;pet.sta=mode;pet.sick=mode===3;pet.energy=mode===1?14:80;pet.hunger=mode===2?9:80;
 pet.tantrum=mode===4?time+600000:0;pet.disc=mode===4?20:70;pet.flags.asleep=mode===0;
 const source=load(wallet,legend,farm);rows.push({id,time,wallet,legend,farm:source.Farm._save(),card:source.Farm.card(id)});
}
fs.writeFileSync(path.join(root,'native/tests/fixtures/farm-card.json.gz'),zlib.gzipSync(JSON.stringify(rows)));
console.log(`Exported ${rows.length} original Farm.card projections across every legend/tier and care boundary.`);

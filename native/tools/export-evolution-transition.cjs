const fs=require('fs'),path=require('path');
const root=path.resolve(__dirname,'../..'),source=fs.readFileSync(path.join(root,'js/legendevo-ui.js'),'utf8');
const start=source.indexOf('      t += dt;'),end=source.indexOf('      } else if (!flashed)',start);
if(start<0||end<0)throw Error('Evolution transition source changed');
const sample=Function('dt',`let t=0;const stage={scene:{},floor:{}},make=()=>({group:{scale:{setScalar(s){this.value=s}},rotation:{}}});const oldV=make(),newV=make();${source.slice(start,end)} } return {time:t,white:!!stage.scene.overrideMaterial,showNew:newV.group.visible,scale:oldV.group.scale.value,yaw:oldV.group.rotation.y};`.replace('const stage={scene:{},floor:{}}','const stage={scene:{},floor:{},white:true}'));
const times=[0,.5,.500001,.9,.900001,2.699999,...Array.from({length:2700},(_,i)=>i/1000)];
fs.writeFileSync(path.join(root,'native/tests/fixtures/evolution-transition.json'),JSON.stringify(times.map(sample)));
console.log(`Exported ${times.length} original transformation samples.`);

const fs=require('fs'),path=require('path'),{pathToFileURL}=require('url');
(async()=>{
 const root=path.resolve(__dirname,'../..');
 const THREE=await import(pathToFileURL(path.join(root,'vendor/three/build/three.module.js')).href);
 const source=fs.readFileSync(path.join(root,'js/render3d.js'),'utf8');
 function shape(pattern,ctor) {const match=source.match(pattern);if(!match)throw Error('Original loot geometry changed');return new THREE[ctor](...match[1].split(',').map(Number));}
 function primitive(g){const vertices=[];for(let i=0;i<g.attributes.position.count;i++){const p=g.attributes.position,n=g.attributes.normal,u=g.attributes.uv;vertices.push(p.getX(i),p.getY(i),p.getZ(i),n.getX(i),n.getY(i),n.getZ(i),u.getX(i),u.getY(i),0,0,0,0,1,0,0,0);}return {vertices,indices:Array.from(g.index.array)};}
 const result={core:primitive(shape(/this\.orbGeo = new THREE\.SphereGeometry\(([^)]+)\)/,'SphereGeometry')),glow:primitive(shape(/this\.orbGlowGeo = new THREE\.SphereGeometry\(([^)]+)\)/,'SphereGeometry')),burst:primitive(shape(/burst: new THREE\.RingGeometry\(([^)]+)\)/,'RingGeometry'))};
 fs.writeFileSync(path.join(root,'native/content/loot-geometry.json'),JSON.stringify(result)+'\n');
 console.log('Exported original orb core, glow and collection burst geometry.');
})().catch(e=>{console.error(e);process.exitCode=1;});

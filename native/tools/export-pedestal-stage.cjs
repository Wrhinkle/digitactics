// Export the original client Stage construction and Canvas2D hex top. Native
// rendering uses these meshes/materials; browser JavaScript is build-time only.
const fs = require('node:fs'), path = require('node:path'), crypto = require('node:crypto'), {pathToFileURL} = require('node:url');
const root = path.resolve(__dirname, '../..');
const source = fs.readFileSync(path.join(root,'js/client.js'),'utf8').replace(/\r\n/g,'\n');
const slice = (start,end) => {
  const a = source.indexOf(start), b = source.indexOf(end,a);
  if (a < 0 || b < 0) throw new Error(`Missing pedestal source block: ${start}`);
  return source.slice(a,b);
};
const constructor = slice('    this.scene = new THREE.Scene();','    this.key = null;');
const hex = slice('function hexTexture() {','\nlet stage = null;');
const cameraCode = slice('      const t = Math.tan((this.cam.fov * Math.PI) / 360);','\n    }\n    if (this.view)');
const ringCode = source.match(/this\.ringMat\.color\.setHSL\([^;]+;/)?.[0];
const legendHeight = Number(source.match(/createView\(key, \{ ring: false, yaw: 0, height: ([\d.]+) \}/)?.[1]);
if (!ringCode || !Number.isFinite(legendHeight)) throw new Error('Missing pedestal ring or model height.');

(async () => {
  const THREE = await import(pathToFileURL(path.join(root,'vendor/three/build/three.module.js')));
  const texture = new THREE.Texture(); texture.colorSpace = THREE.SRGBColorSpace;
  const host = {};
  new Function('THREE','hexTexture',constructor).call(host,THREE,()=>texture);
  const names = ['drum','base','gold','ring','ring2','top'], geometry = {};
  const transform = node => ({position:node.position.toArray(),rotation:[node.rotation.x,node.rotation.y,node.rotation.z],scale:node.scale.toArray()});
  host.scene.updateMatrixWorld(true);
  host.ped.children.forEach((mesh,index) => {
    const vertices = [], g = mesh.geometry, p = g.attributes.position, n = g.attributes.normal, uv = g.attributes.uv;
    for (let i = 0; i < p.count; i++) vertices.push(p.getX(i),p.getY(i),p.getZ(i),n.getX(i),n.getY(i),n.getZ(i),uv.getX(i),1-uv.getY(i),0,0,0,0,0,0,0,0);
    const m = mesh.material;
    // Materialize GPU Float32 values before JSON so decimal-to-single parsers
    // cannot choose the other side of a halfway UV introduced by flipY.
    geometry[names[index]] = {vertices:Array.from(Float32Array.from(vertices),n=>n===0?0:n),indices:g.index?Array.from(g.index.array):Array.from({length:p.count},(_,i)=>i),
      ...transform(mesh),material:{color:m.color.toArray(),roughness:m.roughness??0,metalness:m.metalness??0,
        unlit:!!m.isMeshBasicMaterial,transparent:m.transparent,opacity:m.opacity,depthWrite:m.depthWrite,side:m.side,map:m.map?'hex.png':null}};
  });
  const lights = host.scene.children.filter(n=>n.isLight).map(light=>({
    kind:light.isHemisphereLight?'hemisphere':'directional',color:light.color.toArray(),intensity:light.intensity,
    ground:light.groundColor?.toArray()??null,position:light.position.toArray(),target:light.target?.position.toArray()??null
  }));
  const scene = {schema:1,legendHeight,lights,camera:{fov:host.cam.fov,near:host.cam.near,far:host.cam.far},
    texture:{width:256,height:256,colorSpace:'srgb',minFilter:texture.minFilter,magFilter:texture.magFilter,wrapS:texture.wrapS,wrapT:texture.wrapT,generateMipmaps:texture.generateMipmaps}};
  const out = path.join(root,'native/content/pedestal-stage'); fs.mkdirSync(out,{recursive:true});
  fs.writeFileSync(path.join(out,'geometry.json'),JSON.stringify(geometry)+'\n');
  fs.writeFileSync(path.join(out,'scene.json'),JSON.stringify(scene)+'\n');
  const frameCamera = new Function(cameraCode), frameRing = new Function(ringCode), cameras = [], rings = [];
  for (const [width,height] of [[384,240],[240,384],[320,320],[1280,180],[180,720],[1,1],[1920,1080],[252,200]]) {
    host.cam.aspect = width/height; frameCamera.call(host); host.cam.updateMatrixWorld(true);
    const target = new THREE.Vector3(); host.cam.getWorldDirection(target); target.multiplyScalar((.38-host.cam.position.y)/target.y).add(host.cam.position);
    cameras.push({width,height,eye:host.cam.position.toArray(),target:target.toArray(),fov:host.cam.fov,near:host.cam.near,far:host.cam.far});
  }
  for (const time of [0,.016,.1,.25,.5,Math.PI/4,1,Math.PI/2,2,3*Math.PI/4,3,5,12.5,100,1000]) {
    host.t = time; frameRing.call(host); rings.push({time,color:host.ringMat.color.toArray()});
  }
  const parts = Object.fromEntries(Object.entries(geometry).map(([name,g]) => [name,{...transform(host.ped.children[names.indexOf(name)]),
    material:g.material,vertices:g.vertices.length/16,indices:g.indices.length,
    // JSON serializes -0 as 0; hash the same canonical values consumed natively.
    vertexSha256:crypto.createHash('sha256').update(Buffer.from(Float32Array.from(g.vertices,n=>n===0?0:n).buffer)).digest('hex'),
    indexSha256:crypto.createHash('sha256').update(Buffer.from(new Uint32Array(g.indices).buffer)).digest('hex')} ]));

  const {chromium} = require('../artifacts/audio-tools/node_modules/playwright');
  const browser = await chromium.launch({channel:'msedge',headless:true,args:['--disable-gpu','--disable-webgl','--disable-accelerated-2d-canvas']});
  let png, samples;
  try {
    const page = await browser.newPage();
    const exported = await page.evaluate(hex => {
      const run = new Function('THREE',hex+'\nreturn hexTexture().image;');
      const canvas = run({CanvasTexture:class{constructor(image){this.image=image;}},SRGBColorSpace:'srgb'});
      const positions = [[128,128],[0,0],[128,0],[255,255],[64,128],[128,64],[196,196],[14,24]];
      return {png:canvas.toDataURL('image/png').split(',')[1],samples:positions.map(([x,y])=>({x,y,rgba:Array.from(canvas.getContext('2d').getImageData(x,y,1,1).data)}))};
    },hex);
    png = Buffer.from(exported.png,'base64'); samples = exported.samples;
  } finally {await browser.close();}
  fs.writeFileSync(path.join(out,'hex.png'),png);
  const fixture = {schema:1,scene,parts,cameras,rings,texture:{sha256:crypto.createHash('sha256').update(png).digest('hex'),samples}};
  fs.writeFileSync(path.join(root,'native/tests/fixtures/pedestal-stage.json'),JSON.stringify(fixture)+'\n');
  console.log(`Exported ${names.length} original pedestal meshes, lighting, 256px hex texture, ${cameras.length} camera cases and ${rings.length} ring colors.`);
})().catch(error=>{console.error(error);process.exitCode=1;});

import { rewards } from './mystery-content.js';
// Ten arenas with different silhouettes, scenery and animated landmarks; the playable hex grid stays clear.
export function mysteryBoards({ THREE:T, std,mesh,island,sea,inst,setInst }) {
  const titles=['Lantern Orchard','Lily Observatory','Honeycomb Haven','Cloudmill Landing','Petal Pavilion','Prism Vault','Orbital Foundry','Crystal Relay','Eclipse Sanctuary','Meteor Garden'];
  return rewards('board',s=>({name:titles[s.p*5+s.i],icon:'◇',desc:s.p?'An illuminated vault beyond the digital horizon.':'A quiet daybreak retreat with moving garden landmarks.',
    sky:s.p?'#100e29':'#203b50',fog:[s.p?'#100e29':'#203b50',32,68],hemi:['#d9edff','#33314d',1.5],sun:['#fff0d6',2.3],
    slab:s.p?'#20243b':'#405453',benchSlab:'#303b49',slot:'#475768',ped:'#576779',tiles:{mine:'#345b8b',enemy:'#85435c',enemyPlan:'#503347'},
    props(ctx){
      const n=s.p*5+s.i;
      sea(ctx,{color:s.p?'#161932':'#234c61',amp:.05,rough:.5,metal:.2});island(ctx,s.p?'#343349':'#547363','#343b50');
      const glow=std({color:s.color,emissive:s.color,emissiveIntensity:.6,metalness:.25,roughness:.4});
      const stone=std({color:s.p?'#666783':'#927e68',roughness:.8});
      // Monument behind the arena: each family has its own geometry and motion.
      const center=new T.Group();ctx.g.add(center);center.position.set(10,2.1,-5.8);
      const shapes=[()=>new T.SphereGeometry(.9,12,8),()=>new T.TorusGeometry(1.3,.16,8,32),()=>new T.CylinderGeometry(1.3,1.3,.55,6),()=>new T.BoxGeometry(.25,3,.5),()=>new T.ConeGeometry(1.5,1.4,5),()=>new T.OctahedronGeometry(1.4),()=>new T.TorusKnotGeometry(1,.2,48,8),()=>new T.CylinderGeometry(.2,1.1,2.5,5),()=>new T.TorusGeometry(1.5,.4,8,32),()=>new T.IcosahedronGeometry(1.3)];
      const monument=new T.Mesh(shapes[n](),glow);center.add(monument);
      if(n===3){const cross=new T.Mesh(new T.BoxGeometry(3,.25,.5),glow);center.add(cross);}
      const props=[];for(let j=0;j<12;j++){const a=Math.PI+j*Math.PI/11;props.push({x:Math.cos(a)*11.5,z:Math.sin(a)*10.4,y:.8,ry:a,s:.45+(j%3)*.12});}
      const geometry=n%3===0?new T.ConeGeometry(.6,1.8,6):n%3===1?new T.TorusGeometry(.5,.12,6,16):new T.OctahedronGeometry(.7);
      const satellites=inst(ctx,geometry,glow,props,{moving:true});
      for(const x of [-10.5,10.5])mesh(ctx,new T.CylinderGeometry(.7,1,1.1,6),stone,{x,y:.5,z:-5});
      const add=(geo,mat,x,y,z,o={})=>mesh(ctx,geo,mat,{x,y,z,...o});
      const wood=std({color:'#63484a'}),leaf=std({color:['#477b62','#609a83','#ad8637','#e1edf5','#e4a9c4'][s.i],roughness:.9});
      const garden=[[-9.6,-5.5],[-10,-1.6],[-7.5,-8.3],[7.5,-8.3],[10,-1.6]];
      if(n===0||n===4)for(const [x,z] of garden){
        add(new T.CylinderGeometry(.19,.35,1.8,7),wood,x,.85,z);
        add(new T.IcosahedronGeometry(1.35,1),leaf,x,2.05,z,{sy:.65});
        for(const dx of [-.7,.7])add(new T.SphereGeometry(.22,8,6),glow,x+dx,1.35,z+.3);
      }
      if(n===1)for(const [x,z] of garden){
        add(new T.CylinderGeometry(1.4,1.4,.12,24),std({color:'#346d88',metalness:.4,roughness:.15}),x,.12,z);
        for(let k=0;k<6;k++){const a=k*Math.PI/3;add(new T.SphereGeometry(.55,8,5),leaf,x+Math.cos(a)*.6,.28,z+Math.sin(a)*.6,{sy:.12});}
        add(new T.ConeGeometry(.5,.8,6),glow,x,.65,z);
      }
      if(n===2)for(const [x,z] of garden)for(let k=0;k<4;k++){
        const h=.8+k*.5;add(new T.CylinderGeometry(.65,.65,h,6),k%2?glow:leaf,x+(k%2)*.95,h/2,z+Math.floor(k/2)*.9);
      }
      if(n===3)for(const [x,z] of garden){
        for(let k=0;k<3;k++)add(new T.SphereGeometry(.95,10,6),leaf,x+(k-1)*.7,.35,z,{sy:.45});
        add(new T.ConeGeometry(.65,2.5,6),stone,x,1.4,z);
        const blades=new T.Group();blades.position.set(x,2.65,z+.45);ctx.g.add(blades);
        for(const a of [0,Math.PI/2]){const b=new T.Mesh(new T.BoxGeometry(2.2,.25,.18),glow);b.rotation.z=a;blades.add(b);}
        ctx.tick((dt,t)=>{blades.rotation.z=t*.6;});
      }
      if(n===4){
        for(const x of [-1.7,1.7])add(new T.CylinderGeometry(.13,.13,2.3,6),wood,x,1.15,-9.2);
        add(new T.ConeGeometry(2.7,1.1,4),glow,0,2.8,-9.2,{ry:Math.PI/4});
      }
      if(n===5||n===7)for(const [x,z] of garden)for(let k=0;k<3;k++){
        const h=1.4+k*.7;add(new T.ConeGeometry(.55,h,5),k===1?glow:stone,x+(k-1)*.6,h/2+.1,z+Math.abs(k-1)*.35,{rz:(k-1)*.2});
      }
      if(n===6)for(const x of [-9.5,9.5]){
        const rings=new T.Group();rings.position.set(x,1.8,-4.4);ctx.g.add(rings);
        for(let k=0;k<3;k++){const ring=new T.Mesh(new T.TorusGeometry(1.4,.07,6,36),glow);ring.rotation.set(k*.7,k*.8,0);rings.add(ring);}
        ctx.tick((dt,t)=>{rings.rotation.y=t*.2;rings.rotation.z=Math.sin(t*.3)*.3;});
        add(new T.SphereGeometry(.6,12,8),stone,x,1.8,-4.4);
      }
      if(n===8){
        for(const [x,z] of garden)add(new T.CylinderGeometry(.35,.6,2.1,8),stone,x,1.05,z);
        add(new T.TorusGeometry(1.45,.14,8,40),glow,-9.5,2.5,-5.6);
        add(new T.SphereGeometry(1.25,20,12),std({color:'#120f27',roughness:1}),-9.5,2.5,-5.65);
      }
      if(n===9)for(const [x,z] of garden){
        add(new T.IcosahedronGeometry(1.1,0),stone,x,.6,z,{sy:.6});
        const meteor=add(new T.IcosahedronGeometry(.5,0),glow,x+.2,1.7,z);
        ctx.tick((dt,t)=>{meteor.position.y=1.7+Math.sin(t+x)*.3;meteor.rotation.y=t*.3;});
      }
      ctx.tick((dt,t)=>{monument.rotation.y=t*(.15+n*.025);if(n===3||n===6||n===8)center.rotation.z=t*.25;
        props.forEach((p,j)=>{p.y=.8+Math.sin(t*(.6+n*.05)+j)*.22;p.ry+=dt*.2;setInst(satellites,j,p);});satellites.instanceMatrix.needsUpdate=true;});
    },
  }));
}

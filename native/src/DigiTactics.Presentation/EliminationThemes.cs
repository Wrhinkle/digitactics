namespace DigiTactics.Presentation;

// Procedural backgrounds ported from js/killscreens.js and js/mystery-content.js.
// Keep timing/geometry in double precision so source fixture traces remain comparable.
public static class EliminationThemes
{
    private static readonly Dictionary<string,(string Rim,string Text)> colors=new()
    {
        ["basic"]=("#4fb3ff","#bfe2ff"),
        ["gate"]=("#8ff3ff","#dffbff"),
        ["courage"]=("#ffb347","#ffe2b0"),
        ["glitch"]=("#ff4fd8","#ffd0f6"),
        ["pillar"]=("#fff3b0","#fffbe6"),
        ["ocean"]=("#9aa8c8","#d8def0"),
        ["firewall"]=("#ff7a2a","#ffd9b8"),
        ["stream"]=("#5affb0","#d0ffe8"),
        ["mega"]=("#ff3a3a","#ffd0d0"),
        ["confetti"]=("#ffd84a","#fff2c0"),
        ["sting"]=("#ff5a6a","#ffd0d6"),
    };
    private static readonly string[][] mysteryColors = [["#ffc76b","#7de0cf","#ed91b9","#a9c7ff","#b8e986"],["#cb9aff","#5ce5ff","#ff719e","#a3ffcb","#ffcc6e"]];
    public static string Resolve(string? variant,bool pve=false)
    {
        if(pve || variant=="eliminated")return "sting";
        string key=variant?.StartsWith("ks_")==true?variant[3..]:variant??"basic";
        return colors.ContainsKey(key)||MysteryKey(key,out _,out _)?key:"basic";
    }
    public static EliminationColor Rim(string? variant,bool pve=false)
    {
        string key=Resolve(variant,pve);return EliminationColor.Parse(MysteryKey(key,out int pool,out int index)?mysteryColors[pool][index%5]:colors[key].Rim);
    }
    public static EliminationColor Text(string? variant,bool pve=false)
    {
        string key=Resolve(variant,pve);return EliminationColor.Parse(MysteryKey(key,out _,out _)?"#ffffff":colors[key].Text);
    }
    public static bool Silhouette(string? variant) => Resolve(variant)=="mega";
    private static bool MysteryKey(string key,out int pool,out int index)
    {
        pool=key.StartsWith("mystery_vault_")?1:0;index=-1;
        string prefix="mystery_"+(pool==1?"vault":"daily")+"_killscreen_";
        return key.StartsWith(prefix) && int.TryParse(key[prefix.Length..],out index) && --index>=0 && index<20 && key==prefix+(index+1);
    }
    public static void Draw(EliminationCanvas c,string? variant,bool pve,double w,double h,double t)
    {
        string key=Resolve(variant,pve);
        if(MysteryKey(key,out int pool,out int index)) {Mystery(c,w,h,t,pool,index);return;}
        switch(key)
        {
            case "basic": Basic(c,w,h,t); break;
            case "gate": Gate(c,w,h,t); break;
            case "courage": Courage(c,w,h,t); break;
            case "glitch": Glitch(c,w,h,t); break;
            case "pillar": Pillar(c,w,h,t); break;
            case "ocean": Ocean(c,w,h,t); break;
            case "firewall": Firewall(c,w,h,t); break;
            case "stream": Stream(c,w,h,t); break;
            case "mega": Mega(c,w,h,t); break;
            case "confetti": Confetti(c,w,h,t); break;
            case "sting": Sting(c,w,h,t); break;
        }
    }
    private static double R(double i,double k=0) {double x=Math.Sin(i*127.1+k*311.7)*43758.5453;return x-Math.Floor(x);}
    private static double clamp01(double x)=>Math.Clamp(x,0,1);
    private static double easeOut(double x)=>1-Math.Pow(1-clamp01(x),3);
    private static void fillBg(EliminationCanvas c,double w,double h,string a,string b)
    {var g=c.createLinearGradient(0,0,0,h);g.addColorStop(0,a);g.addColorStop(1,b);c.fillStyle=g;c.fillRect(0,0,w,h);}
    private static void glow(EliminationCanvas c,double x,double y,double r,string col,double alpha=1)
    {var g=c.createRadialGradient(x,y,0,x,y,Math.Max(1,r));g.addColorStop(0,col);g.addColorStop(1,"rgba(0,0,0,0)");c.globalAlpha=alpha;c.fillStyle=g;c.fillRect(x-r,y-r,r*2,r*2);c.globalAlpha=1;}
    private static void poly(EliminationCanvas c,double x,double y,double r,int n,double rot)
    {c.beginPath();for(int i=0;i<=n;i++){double a=rot+(i/(double)n)*Math.PI*2;if(i==0)c.moveTo(x+Math.Cos(a)*r,y+Math.Sin(a)*r);else c.lineTo(x+Math.Cos(a)*r,y+Math.Sin(a)*r);}}
    private static void Basic(EliminationCanvas c,double w,double h,double t)
    {
      fillBg(c, w, h, "#0b1830", "#04070d");
      for (int i = 0; i < 18; i++) {
        double y = R(i, 1) * h;
        double len = 60 + R(i, 5) * w * 0.25;
        double x = ((R(i, 2) * w + t * (180 + R(i, 3) * 420)) % (w + len)) - len;
        c.fillStyle = FormattableString.Invariant($"rgba(80,170,255,{0.12 + 0.3 * R(i, 4)})");
        c.fillRect(x, y, len, 1 + R(i, 6) * 3);
      }
      c.fillStyle = "rgba(0,0,0,0.18)";
      for (double y = 0; y < h; y += 3) c.fillRect(0, y, w, 1);
    }

    private static void Gate(EliminationCanvas c,double w,double h,double t)
    {
      fillBg(c, w, h, "#06182a", "#020509");
      double cx = w / 2, cy = h * 0.45, m = Math.Min(w, h);
      double open = easeOut(t / 0.9);
      glow(c, cx, cy, m * (0.25 + 0.45 * open), "rgba(160,250,255,0.9)", 0.35 + 0.4 * open);
      c.lineWidth = Math.Max(1.5, m * 0.008);
      for (int k = 0; k < 4; k++) {
        c.strokeStyle = FormattableString.Invariant($"rgba(140,240,255,{0.75 - k * 0.15})");
        poly(c, cx, cy, m * (0.16 + 0.09 * k) * (0.6 + 0.4 * open), 6, t * (k % 2 != 0 ? 0.7 : -0.5) + k);
        c.stroke();
      }
      // gate halves sliding apart
      double gap = open * m * 0.2;
      c.fillStyle = "rgba(4,20,34,0.85)";
      c.fillRect(cx - gap - m * 0.12, cy - m * 0.24, m * 0.12, m * 0.48);
      c.fillRect(cx + gap, cy - m * 0.24, m * 0.12, m * 0.48);
      for (int i = 0; i < 70; i++) {
        double a = R(i, 1) * Math.PI * 2;
        double d = (1 - ((R(i, 2) + t * (0.3 + R(i, 3) * 0.4)) % 1)) * m * 0.8;
        c.fillStyle = FormattableString.Invariant($"rgba(200,255,255,{0.3 + R(i, 4) * 0.6})");
        double s = 2 + R(i, 5) * 4;
        c.fillRect(cx + Math.Cos(a) * d - s / 2, cy + Math.Sin(a) * d * 0.7 - s / 2, s, s);
      }
    }

    private static void Courage(EliminationCanvas c,double w,double h,double t)
    {
      fillBg(c, w, h, "#2a0d02", "#0d0300");
      double cx = w / 2, cy = h * 0.45, m = Math.Max(w, h);
      double n = 26;
      for (int i = 0; i < n; i++) {
        double a = (i / (double)n) * Math.PI * 2 + t * 0.25;
        c.fillStyle = i % 2 != 0 ? "rgba(255,140,30,0.22)" : "rgba(255,210,90,0.16)";
        c.beginPath();
        c.moveTo(cx, cy);
        c.arc(cx, cy, m, a, a + Math.PI / n);
        c.fill();
      }
      double pulse = 1 + 0.08 * Math.Sin(t * 9);
      double r = Math.Min(w, h) * 0.13 * easeOut(t / 0.5) * pulse;
      glow(c, cx, cy, r * 3.2, "rgba(255,190,80,1)", 0.7);
      c.fillStyle = "#ffd46a";
      for (int i = 0; i < 8; i++) {
        double a = (i / (double)8) * Math.PI * 2 - t * 0.6;
        c.beginPath();
        c.moveTo(cx + Math.Cos(a - 0.22) * r, cy + Math.Sin(a - 0.22) * r);
        c.lineTo(cx + Math.Cos(a) * r * 1.7, cy + Math.Sin(a) * r * 1.7);
        c.lineTo(cx + Math.Cos(a + 0.22) * r, cy + Math.Sin(a + 0.22) * r);
        c.fill();
      }
      c.beginPath(); c.arc(cx, cy, r, 0, Math.PI * 2); c.fill();
      for (int i = 0; i < 60; i++) {
        double a = R(i, 1) * Math.PI * 2;
        double d = ((R(i, 2) + t * (0.4 + R(i, 3) * 0.5)) % 1) * Math.Min(w, h) * 0.9;
        c.fillStyle = FormattableString.Invariant($"rgba(255,{(int)(180 + R(i, 4) * 70)},90,{1 - d / (Math.Min(w, h) * 0.9)})");
        c.fillRect(cx + Math.Cos(a) * d, cy + Math.Sin(a) * d, 3, 3);
      }
    }

    private static void Glitch(EliminationCanvas c,double w,double h,double t)
    {
      fillBg(c, w, h, "#14041e", "#040108");
      double step = Math.Floor(t * 14);
      for (int i = 0; i < 46; i++) {
        double k = step * 64 + i;
        c.fillStyle = R(k, 1) < 0.5 ? FormattableString.Invariant($"rgba(0,240,255,{0.15 + R(k, 2) * 0.35})") : FormattableString.Invariant($"rgba(255,40,200,{0.15 + R(k, 2) * 0.35})");
        c.fillRect(R(k, 3) * w, R(k, 4) * h, 10 + R(k, 5) * w * 0.3, 2 + R(k, 6) * h * 0.06);
      }
      // deletion bar sweeping down
      double y = ((t * 0.55) % 1) * h;
      c.fillStyle = "rgba(255,255,255,0.12)";
      c.fillRect(0, y, w, h * 0.04);
      c.fillStyle = "rgba(0,0,0,0.25)";
      for (double yy = 0; yy < h; yy += 2) c.fillRect(0, yy, w, 1);
    }

    private static void Pillar(EliminationCanvas c,double w,double h,double t)
    {
      fillBg(c, w, h, "#0c1230", "#03040c");
      double cx = w / 2, grow = easeOut(t / 0.6);
      double bw = w * 0.16 * grow;
      var g = c.createLinearGradient(cx - bw, 0, cx + bw, 0);
      g.addColorStop(0, "rgba(255,240,170,0)");
      g.addColorStop(0.5, "rgba(255,250,225,0.85)");
      g.addColorStop(1, "rgba(255,240,170,0)");
      c.fillStyle = g;
      c.fillRect(cx - bw, 0, bw * 2, h);
      c.strokeStyle = "rgba(255,240,180,0.6)";
      c.lineWidth = 2;
      for (int k = 0; k < 4; k++) {
        double p = (t * 0.45 + k / (double)4) % 1;
        c.globalAlpha = 1 - p;
        c.beginPath();
        c.ellipse(cx, h * (0.95 - p * 0.9), w * (0.08 + 0.12 * p), h * 0.03, 0, 0, Math.PI * 2);
        c.stroke();
      }
      c.globalAlpha = 1;
      for (int i = 0; i < 90; i++) {
        double x = cx + (R(i, 1) - 0.5) * w * 0.6 * (0.3 + R(i, 2));
        double y = h - ((R(i, 3) * h + t * (60 + R(i, 4) * 160)) % h);
        c.fillStyle = FormattableString.Invariant($"rgba(255,245,200,{0.25 + R(i, 5) * 0.7})");
        c.fillRect(x, y, 2, 2 + R(i, 6) * 4);
      }
    }

    private static void Ocean(EliminationCanvas c,double w,double h,double t)
    {
      fillBg(c, w, h, "#0e121c", "#05060a");
      glow(c, w * 0.75, h * 0.2, Math.Min(w, h) * 0.22, "rgba(210,215,230,0.55)");
      c.fillStyle = "rgba(225,228,238,0.85)";
      c.beginPath(); c.arc(w * 0.75, h * 0.2, Math.Min(w, h) * 0.06, 0, Math.PI * 2); c.fill();
      string[] cols = ["#1a2030", "#161b28", "#121622", "#0e111a", "#0a0c13", "#07080d"];
      for (int k = 0; k < cols.Length; k++) {
        double baseline = h * (0.5 + k * 0.09);
        c.fillStyle = cols[k];
        c.beginPath();
        c.moveTo(0, h);
        for (double x = 0; x <= w; x += 8) {
          double y = baseline + Math.Sin(x * (0.012 + k * 0.003) + t * (0.9 + k * 0.25) + k) * h * 0.025 + Math.Sin(x * 0.031 - t * 1.7 + k * 2) * h * 0.01;
          c.lineTo(x, y);
        }
        c.lineTo(w, h);
        c.fill();
      }
      c.fillStyle = "rgba(150,160,190,0.06)";
      c.fillRect(0, h * 0.45, w, h * 0.12);
    }

    private static void Firewall(EliminationCanvas c,double w,double h,double t)
    {
      fillBg(c, w, h, "#2a0705", "#090202");
      double cols = 12, rows = 7, bw = w / cols, bh = h / rows;
      double brk = 0.55, cx = w / 2, cy = h * 0.45;
      if (t > brk && t < brk + 0.3) { c.fillStyle = FormattableString.Invariant($"rgba(255,200,120,{0.6 * (1 - (t - brk) / 0.3)})"); c.fillRect(0, 0, w, h); }
      for (int r = 0; r < rows; r++) for (int q = 0; q < cols; q++) {
        double x = q * bw + (r % 2 != 0 ? bw / 2 : 0) - bw / 4, y = r * bh;
        double k = r * cols + q;
        double dx = 0, dy = 0, rot = 0, a = 1;
        if (t > brk) {
          double e = t - brk;
          double vx = (x + bw / 2 - cx) * (1.2 + R(k, 1)), vy = (y + bh / 2 - cy) * (1.2 + R(k, 2));
          dx = vx * e; dy = vy * e + 300 * e * e; rot = (R(k, 3) - 0.5) * 6 * e; a = clamp01(1 - e / 1.6);
        }
        if (a <= 0) continue;
        c.save();
        c.globalAlpha = a;
        c.translate(x + bw / 2 + dx, y + bh / 2 + dy);
        c.rotate(rot);
        c.fillStyle = FormattableString.Invariant($"rgba({(int)(170 + R(k, 4) * 60)},{(int)(40 + R(k, 5) * 40)},10,0.85)");
        c.fillRect(-bw / 2 + 2, -bh / 2 + 2, bw - 4, bh - 4);
        c.strokeStyle = FormattableString.Invariant($"rgba(255,170,60,{(t < brk ? 0.5 + 0.5 * Math.Sin(t * 20 + k) : 0.8)})");
        c.lineWidth = 1.5;
        c.strokeRect(-bw / 2 + 2, -bh / 2 + 2, bw - 4, bh - 4);
        c.restore();
      }
      glow(c, cx, cy, Math.Min(w, h) * 0.5 * easeOut((t - brk) / 0.5), "rgba(255,140,40,0.8)", t > brk ? 0.6 : 0);
    }

    private static void Stream(EliminationCanvas c,double w,double h,double t)
    {
      fillBg(c, w, h, "#02100a", "#000403");
      double cx = w / 2, cy = h * 0.45, m = Math.Max(w, h) * 0.5;
      c.lineCap = "round";
      for (int i = 0; i < 150; i++) {
        double a = R(i, 1) * Math.PI * 2;
        double z = (R(i, 2) + t * (0.5 + R(i, 3) * 0.6)) % 1;
        double d0 = m * 0.05 / (1.05 - z), d1 = d0 * (1.08 + z * 0.3);
        c.strokeStyle = R(i, 4) < 0.7 ? FormattableString.Invariant($"rgba(90,255,170,{z})") : FormattableString.Invariant($"rgba(160,240,255,{z})");
        c.lineWidth = 1 + z * 2.5;
        c.beginPath();
        c.moveTo(cx + Math.Cos(a) * d0, cy + Math.Sin(a) * d0);
        c.lineTo(cx + Math.Cos(a) * d1, cy + Math.Sin(a) * d1);
        c.stroke();
      }
      glow(c, cx, cy, Math.Min(w, h) * 0.18, "rgba(160,255,210,0.7)", 0.6);
    }

    private static void Mega(EliminationCanvas c,double w,double h,double t)
    {
      fillBg(c, w, h, "#3a0606", "#0a0101");
      // lightning flashes at fixed times
      foreach (double ft in new double[] {0.25, 1.6, 2.7}) {
        double e = t - ft;
        if (e < 0 || e > 0.25) continue;
        c.fillStyle = FormattableString.Invariant($"rgba(255,220,220,{0.35 * (1 - e / 0.25)})");
        c.fillRect(0, 0, w, h);
        c.strokeStyle = FormattableString.Invariant($"rgba(255,240,240,{1 - e / 0.25})");
        c.lineWidth = 2;
        c.beginPath();
        double x = w * (0.15 + R(ft * 10, 1) * 0.7), y = 0;
        c.moveTo(x, y);
        for (int s = 0; s < 8; s++) { x += (R(ft * 10 + s, 2) - 0.5) * w * 0.08; y += h * 0.07; c.lineTo(x, y); }
        c.stroke();
      }
      c.fillStyle = "#120202";
      c.fillRect(0, h * 0.82, w, h * 0.18);
      for (int i = 0; i < 40; i++) {
        double x = R(i, 1) * w, y = h * 0.82 - ((R(i, 2) * h * 0.6 + t * (20 + R(i, 3) * 50)) % (h * 0.6));
        c.fillStyle = FormattableString.Invariant($"rgba(255,90,60,{0.2 + R(i, 4) * 0.5})");
        c.fillRect(x, y, 2, 2);
      }
    }

    private static void Confetti(EliminationCanvas c,double w,double h,double t)
    {
      fillBg(c, w, h, "#1c0b34", "#07030f");
      double sway = Math.Sin(t * 0.8) * w * 0.05;
      var g = c.createLinearGradient(0, 0, 0, h);
      g.addColorStop(0, "rgba(255,240,180,0.45)");
      g.addColorStop(1, "rgba(255,240,180,0)");
      c.fillStyle = g;
      c.beginPath();
      c.moveTo(w / 2 - w * 0.04 + sway, 0); c.lineTo(w / 2 + w * 0.04 + sway, 0);
      c.lineTo(w / 2 + w * 0.22, h); c.lineTo(w / 2 - w * 0.22, h);
      c.fill();
      string[] cols = ["#ffd84a", "#ff5a8a", "#4fb3ff", "#5adf8a", "#e58bff", "#ffffff"];
      for (int i = 0; i < 130; i++) {
        double x = R(i, 1) * w + Math.Sin(t * 2 + i) * 18;
        double y = ((R(i, 2) * h * 1.2 + t * (70 + R(i, 3) * 120)) % (h * 1.2)) - h * 0.1;
        c.save();
        c.translate(x, y);
        c.rotate(t * (2 + R(i, 4) * 4) + i);
        c.fillStyle = cols[i % cols.Length];
        c.fillRect(-4, -2, 8 * Math.Abs(Math.Cos(t * 3 + i)) + 1, 4);
        c.restore();
      }
    }

    private static void Sting(EliminationCanvas c,double w,double h,double t)
    {
      fillBg(c, w, h, "#24070c", "#080204");
      c.fillStyle = FormattableString.Invariant($"rgba(255,90,106,{0.25 * (1 - clamp01(t / 0.6))})");
      c.fillRect(0, 0, w, h);
      c.fillStyle = "rgba(255,90,106,0.5)";
      double x = easeOut(t / 0.4) * w;
      c.fillRect(0, h * 0.5 - 1, x, 2);
    }
    private static void Mystery(EliminationCanvas c,double w,double h,double t,int pool,int index)
    {
        string color=mysteryColors[pool][index%5];
        c.fillStyle=pool!=0?"#0e081f":"#081a24";c.fillRect(0,0,w,h);double m=Math.Min(w,h),cx=w/2,cy=h*.42;
        c.save();c.translate(cx,cy);c.rotate(Math.Sin(t*.3)*.2);
        for(int j=0;j<4+index%5;j++)
        {
            c.strokeStyle=color;c.globalAlpha=.5-j*.045;c.lineWidth=2+j%3;
            c.beginPath();c.ellipse(0,0,m*(.12+j*.065),m*(.12+j*.065)*(index%2!=0?.55:1),t*(j%2!=0?-.2:.2),0,Math.PI*2);c.stroke();
        }
        for(int j=0;j<36;j++)
        {
            double a=j*2.399+t*(.18+index*.015),r=m*(.1+((j/36d+t*.07)%1)*.6),sz=2+(j+index)%6;
            c.globalAlpha=.3+(j%4)*.16;c.fillStyle=j%3!=0?color:"#fff";c.save();c.translate(Math.Cos(a)*r,Math.Sin(a)*r);c.rotate(a);
            if(index%2!=0){c.beginPath();c.moveTo(0,-sz*2);c.lineTo(sz,0);c.lineTo(0,sz*2);c.lineTo(-sz,0);c.closePath();c.fill();}
            else c.fillRect(-sz,-sz,sz*2,sz*2);
            c.restore();
        }
        c.restore();
    }
}

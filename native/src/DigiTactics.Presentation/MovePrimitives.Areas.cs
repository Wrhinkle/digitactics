namespace DigiTactics.Presentation;

public static partial class MovePrimitives
{
    // Delivery primitives translated from js/moveprims.js. Durations and emissions
    // intentionally retain the source effect's timing and deterministic random order.
    public static void Cross(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        var tg = Spots(c, o);
        var ca = Col(c, o.String("color"));
        double camYaw = Math.Atan2(fx.CameraPosition.X - c.C.X, fx.CameraPosition.Z - c.C.Z);
        for (int ti = 0; ti < tg.Length; ti++)
        {
            var b = tg[ti];
            double s = o.Number("size", 1.2) * Math.Sqrt(c.S), t0 = d + ti * 0.05;
            MoveMesh? v = null, f = null, g = null;
            fx.Run(t0, o.Number("dur", 0.8), k =>
            {
                if (v is null)
                {
                    v = fx.Mesh("cross"); f = fx.Mesh("crossFlat"); g = fx.Mesh("cross");
                    v.Color = ca; g.Color = White; f.Color = ca;
                }
                double rise = Math.Min(1, k / 0.25), fade = k > 0.65 ? (1 - k) / 0.35 : 1;
                v.Position.Set(b.X, s * (0.4 + 0.7 * rise), b.Z); v.Rotation.Set(0, camYaw, 0);
                g!.Position.Set(b.X, s * (0.4 + 0.7 * rise), b.Z); g.Rotation.Set(0, camYaw, 0);
                v.Scale.SetScalar(s * (0.6 + 0.5 * rise));
                g!.Scale.SetScalar(s * (0.45 + 0.35 * rise));
                f!.Position.Set(b.X, 0.17, b.Z);
                f.Rotation.Y = fx.Now * 0.5;
                f.Scale.SetScalar(s * 1.2 * rise);
                v.Opacity = fade * 0.85; g.Opacity = fade * 0.5; f.Opacity = fade * 0.6;
            }, () => { if (v is not null) { fx.Free(v); fx.Free(f!); fx.Free(g!); } });
            fx.Pillar(t0, b.X, b.Z, ca, 0.35 * s, 6, 0.7);
            if (o.Flag("orbs"))
            {
                (double X, double Y)[] points = [(0, 2), (0, 1), (0, 0), (0, -1), (-1, 1), (1, 1), (0, 3), (-2, 1), (2, 1), (0, -2)];
                for (int j = 0; j < points.Length; j++)
                {
                    var (px, py) = points[j];
                    var oc = j % 2 != 0 ? c.Cb : ca;
                    fx.Run(t0 + j * 0.03, 0.7, k =>
                    {
                        double x = b.X + Math.Cos(camYaw) * px * 0.55 * s, z = b.Z - Math.Sin(camYaw) * px * 0.55 * s;
                        double sz = 0.9 * s * (k < 0.8 ? 1 : (1 - k) * 5), y = 1.4 * s + py * 0.55 * s - k * k * 1.2 * s;
                        fx.Quad("glow", x, y, z, sz, sz, oc, 0.9);
                        fx.Ball(x, y, z, sz * 0.12, White, 0.9);
                    });
                }
            }
            Impact(fx, c, o.String("impact", "holy"), b.X, b.Z, t0 + 0.2, o.Number("impactSize", 1), b.Y);
        }
    }

    public static void Vortex(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        var tg = o.String("to") == "targets" ? c.T : [c.C];
        double dur = o.Number("dur", 0.9);
        var ca = Col(c, o.String("color")); var cb = c.Cb; var cell = CellOf(o.String("cell"), "wisp");
        foreach (var b in tg)
        {
            double r = o.Number("size", c.R + 0.8) * (o.Flag("size") ? c.S : 1);
            MoveMesh? m = null, m2 = null, t = null;
            fx.Run(d, dur, (k, dt) =>
            {
                if (m is null)
                {
                    m = fx.Mesh("swirl"); m2 = fx.Mesh("swirl"); t = fx.Mesh("torus");
                    m.Color = ca; m2.Color = cb; t.Color = cb;
                }
                double g = k < 0.2 ? k / 0.2 : k > 0.75 ? (1 - k) / 0.25 : 1;
                m.Position.Set(b.X, 0.18, b.Z); m.Rotation.Y = -fx.Now * 5; m.Scale.SetScalar(r * g);
                m2!.Position.Set(b.X, 0.22, b.Z); m2.Rotation.Y = -fx.Now * 8; m2.Scale.SetScalar(r * 0.6 * g);
                m.Opacity = g; m2.Opacity = g * 0.8;
                t!.Position.Set(b.X, 0.3 + k * 1.2, b.Z); t.Rotation.X = Math.PI / 2; t.Scale.SetScalar(r * (1 - k * 0.7)); t.Opacity = g * 0.8;
                var n = MoveEffects.Round(dt * 110 * c.S);
                for (int i = 0; i < n; i++)
                {
                    double a = fx.Random() * Tau, radius = r * Rnd(fx, 0.6, 1);
                    fx.Add.Add(b.X + Math.Cos(a) * radius, Rnd(fx, 0.1, 0.4), b.Z + Math.Sin(a) * radius,
                        -Math.Sin(a) * radius * 4 - Math.Cos(a) * radius * 1.6, Rnd(fx, 0.5, o.Flag("up") ? 5 : 2),
                        Math.Cos(a) * radius * 4 - Math.Sin(a) * radius * 1.6, Pick(fx, ca, cb), 0.45, 0.4 * c.S, cell, MovePreset.None);
                }
            }, () => { if (m is not null) { fx.Free(m); fx.Free(m2!); fx.Free(t!); } });
        }
        foreach (var t in c.T.Length > 0 ? c.T : [c.C])
            Impact(fx, c, o.String("impact", "dark"), t.X, t.Z, d + dur * 0.5, o.Number("impactSize", 1), 0.8);
    }

    public static void Portal(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        var ca = Col(c, o.String("color")); var cb = c.Cb; var b = c.T.Length > 0 ? c.T[0] : c.C;
        double s = o.Number("size", 1.3) * Math.Sqrt(c.S), dur = o.Number("dur", 1);
        double camYaw = Math.Atan2(fx.CameraPosition.X - b.X, fx.CameraPosition.Z - b.Z);
        double gx = b.X - Math.Sin(camYaw) * 0.4, gz = b.Z - Math.Cos(camYaw) * 0.4, gy = 0.4 + 1.1 * s;
        MoveMesh? ring = null, ring2 = null, sw = null;
        fx.Run(d, dur, (k, dt) =>
        {
            if (ring is null)
            {
                ring = fx.Mesh("torus"); ring2 = fx.Mesh("torus"); sw = fx.Mesh("swirlUp");
                ring.Color = ca; ring2.Color = White; sw.Color = cb;
            }
            double open = k < 0.25 ? k / 0.25 : k > 0.8 ? (1 - k) / 0.2 : 1;
            ring.Position.Set(gx, gy, gz); ring.Rotation.Set(0, camYaw, 0);
            ring2!.Position.Set(gx, gy, gz); ring2.Rotation.Set(0, camYaw, 0);
            sw!.Position.Set(gx, gy, gz); sw.Rotation.Set(0, camYaw, 0);
            ring.Scale.Set(s * open, s * 1.25 * open, s);
            ring2!.Scale.Set(s * 0.92 * open, s * 1.15 * open, s * 0.5);
            sw!.RotateZ(dt * 6); sw.Scale.Set(s * 0.9 * open, s * 1.1 * open, 1);
            ring.Opacity = 0.9 * open; ring2.Opacity = 0.5 * open; sw.Opacity = 0.75 * open;
            fx.Quad("glow", gx, gy, gz, s * 2.5 * open, s * 2.5 * open, ca, 0.4 * open);
            var n = MoveEffects.Round(dt * 140);
            for (int i = 0; i < n; i++)
            {
                var t = c.T.Length > 0 ? c.T[i % c.T.Length] : b;
                double sx = t.X + Rnd(fx, -0.4, 0.4), sy = Rnd(fx, 0.2, 1.4), sz = t.Z + Rnd(fx, -0.4, 0.4);
                const double life = 0.35;
                fx.Add.Add(sx, sy, sz, (gx - sx) / life, (gy - sy) / life, (gz - sz) / life,
                    Pick(fx, ca, cb), life, 0.3 * c.S, Pick(fx, "spark", "star"), MovePreset.Shrink);
            }
        }, () => { if (ring is not null) { fx.Free(ring); fx.Free(ring2!); fx.Free(sw!); } });
        foreach (var t in c.T.Length > 0 ? c.T : [c.C])
            Impact(fx, c, o.String("impact", "holy"), t.X, t.Z, d + dur * 0.75, o.Number("impactSize", 1.1), 0.9);
        fx.Later(d + dur * 0.78, () => fx.Flash(0, gx, gy, gz, White, s * 4, 0.3));
    }

    public static void Spikes(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        foreach (var b in Spots(c, o))
        {
            double n = o.Number("count", 6); var a = c.P;
            for (int i = 1; i <= n; i++)
            {
                double k = i / n, x = a.X + (b.X - a.X) * k + Rnd(fx, -0.15, 0.15), z = a.Z + (b.Z - a.Z) * k + Rnd(fx, -0.15, 0.15);
                double big = 0.5 + 0.5 * k;
                SpikeAt(fx, d + i * o.Number("gap", 0.04), x, z, 0.22 * c.S * o.Number("size", 1) * big,
                    1.3 * c.S * o.Number("size", 1) * big, o.String("kind", "ice")!, 0.4, Rnd(fx, -0.3, 0.3));
                fx.Later(d + i * o.Number("gap", 0.04), () => fx.Burst(fx.Add, x, 0.2, z, 4, 2.2,
                    Col(c, o.String("color")), White, 0.4, 0.16, o.String("kind") == "ice" || !o.Flag("kind") ? "shard" : "square", MovePreset.Debris, 2));
            }
            Impact(fx, c, o.String("impact"), b.X, b.Z, d + n * o.Number("gap", 0.04), o.Number("impactSize", 1), b.Y);
        }
    }

    public static void Whip(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        var tg = Spots(c, o); double n = o.Number("count", 1); var ca = Col(c, o.String("color"));
        for (int ti = 0; ti < tg.Length; ti++)
        {
            var b = tg[ti];
            for (int w = 0; w < n; w++)
            {
                var a = Launch(fx, c, o.String("from", "hands"), w);
                double dur = o.Number("dur", 0.4), sway = o.Number("sway", 1) * (w % 2 != 0 ? -1 : 1);
                double dx = b.X - a.X, dy = b.Y - a.Y, dz = b.Z - a.Z, length = Math.Sqrt(dx * dx + dz * dz);
                if (length == 0) length = 1;
                double px = -dz / length, pz = dx / length, t0 = d + w * 0.06 + ti * 0.04;
                MoveRibbon? r = null, r2 = null;
                fx.Run(t0, dur, k =>
                {
                    if (r is null)
                    {
                        r = fx.Ribbon(); r.Color = ca; r.Taper = 0.6;
                        if (o.Flag("lightning")) { r2 = fx.Ribbon(); r2.Color = White; r2.Taper = 0.6; }
                    }
                    double reach = k < 0.45 ? 1 - Math.Pow(1 - k / 0.45, 2) : 1 - (k - 0.45) / 0.55 * 0.4;
                    for (int ribbonIndex = 0; ribbonIndex < 2; ribbonIndex++)
                    {
                        var ribbon = ribbonIndex == 0 ? r : r2;
                        if (ribbon is null) continue;
                        ribbon.Count = 20;
                        for (int i = 0; i < 20; i++)
                        {
                            double t = i / 19.0 * reach;
                            double curl = Math.Sin(t * Math.PI) * sway * (1 - k * 0.8) * length * 0.25 + Math.Sin(t * 9 - k * 14) * 0.12 * (1 - t);
                            double j = o.Flag("lightning") ? 0.12 : 0;
                            ribbon.Points[i].Set(a.X + dx * t + px * curl + Rnd(fx, -j, j), a.Y + dy * t + Math.Sin(t * Math.PI) * 0.6 + Rnd(fx, -j, j), a.Z + dz * t + pz * curl + Rnd(fx, -j, j));
                        }
                        ribbon.Width = o.Number("width", 0.12) * c.S * (ReferenceEquals(ribbon, r2) ? 0.4 : 1);
                        ribbon.Build(k > 0.75 ? (1 - k) * 4 : 1);
                    }
                    var tip = r.Points[19];
                    fx.Add.Add(tip.X, tip.Y, tip.Z, Rnd(fx, -1, 1), Rnd(fx, -1, 1), Rnd(fx, -1, 1), Pick(fx, ca, c.Cb),
                        0.2, 0.3 * c.S, o.Flag("lightning") ? "spark" : o.IsFalse("leafy") ? "glow" : "leaf", MovePreset.Drift);
                }, () => { if (r is not null) fx.FreeRibbon(r); if (r2 is not null) fx.FreeRibbon(r2); });
                if (w == 0) Impact(fx, c, o.String("impact"), b.X, b.Z, t0 + dur * 0.42, o.Number("impactSize", 0.9), b.Y);
            }
        }
    }

    public static void Swarm(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        var tg = Spots(c, o); double n = o.Number("count", 10);
        var cell = CellOf(o.String("cell"), "bubble"); var sys = SysFor(fx, cell);
        var ca = Col(c, o.String("color")); var cb = Col(c, o.String("color2"), c.Cb);
        for (int i = 0; i < n; i++)
        {
            int index = i; var b = tg[i % tg.Length]; double sp = o.Number("spread", 0.4), a0 = i / n * Tau;
            var a = o.Flag("fan")
                ? new MoveVector(c.P.X + Math.Cos(a0) * sp, c.A.Y + 0.4 + Math.Sin(a0 * 2) * 0.2, c.P.Z + Math.Sin(a0) * sp)
                : new MoveVector(c.A.X + Rnd(fx, -sp, sp), c.A.Y + Rnd(fx, -sp, sp) * 0.5, c.A.Z + Rnd(fx, -sp, sp));
            double tx = b.X + Rnd(fx, -0.3, 0.3), tz = b.Z + Rnd(fx, -0.3, 0.3);
            double length = Math.Sqrt((tx - a.X) * (tx - a.X) + (tz - a.Z) * (tz - a.Z));
            double dur = Math.Max(0.2, length / (o.Number("speed", 8) * Rnd(fx, 0.8, 1.2)));
            double side = Rnd(fx, -1, 1) * o.Value("wobble", 0.8), hold = o.Number("hold", 0), t0 = d + i * o.Number("gap", 0.03);
            var color = fx.Random() < 0.5 ? ca : cb;
            double size = o.Number("size", 0.35) * c.S * Rnd(fx, 0.8, 1.2);
            if (hold != 0) fx.Run(t0, hold, _ => sys.Add(a.X, a.Y, a.Z, 0, 0, 0, ReferenceEquals(sys, fx.Alpha) && !o.Flag("tint") ? White : color,
                0.05, size * 2, cell, MovePreset.None, 1, o.Flag("spin") ? fx.Now * o.Number("spin", 0) : 0));
            fx.Fly(t0 + hold, dur, a.X, a.Y, a.Z, tx, b.Y, tz, o.Value("arc", 0.5), (x, y, z, _, _) =>
            {
                sys.Add(x, y, z, 0, 0, 0, ReferenceEquals(sys, fx.Alpha) && !o.Flag("tint") ? White : color, 0.05, size * 2, cell,
                    MovePreset.None, 1, o.Flag("spin") ? fx.Now * o.Number("spin", 0) + index : 0);
                if (!o.IsFalse("glow")) fx.Add.Add(x, y, z, 0, 0, 0, color, 0.05, size * 2.4, "glow", MovePreset.None, 0.35, 0);
            }, null, side);
        }
        double last = d + (n - 1) * o.Number("gap", 0.03) + o.Number("hold", 0) + 0.3;
        foreach (var t in tg) Impact(fx, c, o.String("impact"), t.X, t.Z, Math.Min(last, d + 0.6 + o.Number("hold", 0)), o.Number("impactSize", 0.9), t.Y);
    }

    public static void Shock(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        var at = o.String("to") == "center" ? c.C : o.String("to") == "targets" && c.T.Length > 0 ? c.T[0] : c.P;
        double radius = o.Number("size", c.R * 0.85) * (o.Flag("size") ? Math.Sqrt(c.S) : 1), t0 = d;
        var ca = Col(c, o.String("color")); var cb = c.Cb;
        if (o.Flag("hammer"))
        {
            fx.Run(d, 0.16, k => fx.Quad("glow", at.X, 3.2 - k * 3, at.Z, 1.4 * c.S, 2 * c.S, ca, 0.9));
            t0 += 0.16;
        }
        fx.GroundRing(t0, at.X, at.Z, ca, radius * 1.15, 0.5);
        fx.GroundRing(t0 + 0.06, at.X, at.Z, ca, radius * 1.3, 0.6, "ring");
        MoveMesh? torus = null;
        fx.Run(t0, 0.45, k =>
        {
            if (torus is null) { torus = fx.Mesh("torus"); torus.Color = cb; torus.Rotation.X = Math.PI / 2; }
            torus.Position.Set(at.X, 0.25 + k * 0.3, at.Z); torus.Scale.SetScalar(0.3 + (1 - Math.Pow(1 - k, 2)) * radius); torus.Opacity = 0.6 * (1 - k);
        }, () => { if (torus is not null) fx.Free(torus); });
        fx.Later(t0, () =>
        {
            fx.Flash(0, at.X, 0.4, at.Z, ca, radius * 0.9, 0.3);
            if (!o.IsFalse("debris"))
                for (int i = 0; i < 26 * c.S; i++)
                {
                    double a = fx.Random() * Tau, s = Rnd(fx, 2, 5) * Math.Sqrt(radius);
                    fx.Alpha.Add(at.X, 0.2, at.Z, Math.Cos(a) * s, Rnd(fx, 3, 6), Math.Sin(a) * s, Pick(fx, Rock, Rock2), 0.9, 0.2, "square", MovePreset.Debris);
                }
            fx.RingBurst(fx.Alpha, at.X, 0.2, at.Z, 16, 4 * Math.Sqrt(radius), Smoke, 0.7, 0.8, "smoke", MovePreset.Smoke);
            fx.Burst(fx.Add, at.X, 0.3, at.Z, 18 * c.S, 4, ca, cb, 0.5, 0.4, o.Flag("cell") ? CellOf(o.String("cell")) : "spark", MovePreset.Drift, 2);
        });
        foreach (var t in c.T) Impact(fx, c, o.String("impact"), t.X, t.Z, t0 + 0.08, o.Number("impactSize", 0.7), t.Y);
        if (c.T.Length == 0) Impact(fx, c, o.String("impact"), at.X, at.Z, t0, o.Number("impactSize", 0.8), 0.4);
    }

    public static void Dome(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        var p = c.P; var ca = Col(c, o.String("color")); var cb = c.Cb;
        double radius = o.Number("size", 1.1) * c.S;
        MoveMesh? sphere = null, torus = null;
        fx.Run(d, 0.9, (k, dt) =>
        {
            if (sphere is null) { sphere = fx.Mesh("sphere"); torus = fx.Mesh("torus"); sphere.Color = ca; torus.Color = cb; }
            sphere.Position.Set(p.X, 0.8, p.Z);
            sphere.Scale.SetScalar(radius * (k < 0.15 ? k / 0.15 : 1 + 0.06 * Math.Sin(k * 30) * (1 - k)));
            sphere.Opacity = 0.28 * (1 - k * 0.9);
            torus!.Rotation.X = Math.PI / 2; torus.Position.Set(p.X, 0.2 + k * 1.6 * c.S, p.Z);
            torus.Scale.SetScalar(radius * (0.9 + 0.3 * Math.Sin(k * Math.PI))); torus.Opacity = 1 - k;
            if (o.Flag("orbit"))
            {
                var cell = CellOf(o.String("orbit"));
                for (int i = 0; i < o.Number("count", 4); i++)
                {
                    double a = fx.Now * 4 + i / o.Number("count", 4) * Tau;
                    SysFor(fx, cell).Add(p.X + Math.Cos(a) * radius * 1.1, 0.8 + Math.Sin(a * 2) * 0.3, p.Z + Math.Sin(a) * radius * 1.1,
                        0, 0, 0, i % 2 != 0 ? ca : cb, 0.05, 0.6 * c.S, cell, MovePreset.None, 1, fx.Now * 6);
                }
            }
            if (fx.Random() < dt * 30)
            {
                double a = fx.Random() * Tau, b = Rnd(fx, -0.2, 1);
                fx.Add.Add(p.X + Math.Cos(a) * radius * Math.Sqrt(1 - b * b), 0.8 + b * radius, p.Z + Math.Sin(a) * radius * Math.Sqrt(1 - b * b),
                    0, 0, 0, cb, 0.35, 0.45 * c.S, "hex", MovePreset.Pop);
            }
        }, () => { if (sphere is not null) { fx.Free(sphere); fx.Free(torus!); } });
        fx.Later(d, () => fx.Burst(fx.Add, p.X, 0.9, p.Z, 26, 2.5, ca, White, 0.6, 0.25, "spark", MovePreset.Drift, 0));
    }

    public static void Heal(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        var ca = Col(c, o.String("color"), HexColor("#7dff9a")); var cb = Col(c, o.String("color2"), HexColor("#fff6a0"));
        var cell = CellOf(o.String("cell"), "star");
        for (int i = 0; i < c.T.Length; i++)
        {
            var b = c.T[i]; double t0 = d + i * 0.05;
            if (!o.IsFalse("motes") && Dist(b, c.P) > 0.3)
            {
                double life = Dist(c.P, b) / 9;
                fx.Later(t0, () =>
                {
                    for (int j = 0; j < 8; j++)
                        fx.Add.Add(c.A.X, c.A.Y, c.A.Z, (b.X - c.A.X) / life + Rnd(fx, -1, 1), (1.2 - c.A.Y) / life + Rnd(fx, 0, 2),
                            (b.Z - c.A.Z) / life + Rnd(fx, -1, 1), Pick(fx, ca, cb), life, 0.3, "glow", MovePreset.None);
                });
                t0 += life;
            }
            fx.Later(t0, () =>
            {
                for (int j = 0; j < 16; j++)
                {
                    double a = fx.Random() * Tau, r = Rnd(fx, 0.1, 0.6);
                    SysFor(fx, cell).Add(b.X + Math.Cos(a) * r, Rnd(fx, 0.1, 0.6), b.Z + Math.Sin(a) * r, 0, Rnd(fx, 1.2, 2.4), 0,
                        Pick(fx, ca, cb), Rnd(fx, 0.7, 1.1), 0.3 * o.Number("size", 1), cell, cell is "petal" or "leaf" or "feather" ? MovePreset.Flutter : MovePreset.None);
                }
                fx.GroundRing(0, b.X, b.Z, ca, 1.2, 0.5, "ring");
                fx.Flash(0, b.X, 0.8, b.Z, ca, 1.2, 0.3);
            });
        }
    }

    public static void Tornado(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        var tg = o.String("to") == "center" ? [c.C] : Spots(c, o);
        double h = o.Number("height", 2.8) * c.S, dur = o.Number("dur", 0.7);
        var ca = Col(c, o.String("color")); var cb = c.Cb; var cell = CellOf(o.String("cell"), "streak"); var sys = SysFor(fx, cell);
        foreach (var b in tg)
        {
            double r0 = o.Number("size", 0.6) * (o.String("to") == "center" ? Math.Max(1, c.R * 0.6) : 1) * c.S;
            fx.Run(d, dur, (_, dt) =>
            {
                var n = MoveEffects.Round(dt * 150 * c.S);
                for (int i = 0; i < n; i++)
                {
                    double a = fx.Random() * Tau, y = fx.Random() * h * 0.3, r = r0 * (0.4 + y / h);
                    sys.Add(b.X + Math.Cos(a) * r, y, b.Z + Math.Sin(a) * r, -Math.Sin(a) * 7 * r, h * Rnd(fx, 1.4, 2.2), Math.Cos(a) * 7 * r,
                        Pick(fx, ca, cb), 0.45, o.Number("psize", 0.35) * c.S, cell, MovePreset.Swirl);
                }
            });
            fx.GroundRing(d, b.X, b.Z, ca, r0 * 2, dur, "ring");
        }
        foreach (var t in tg) Impact(fx, c, o.String("impact", "wind"), t.X, t.Z, d + dur * 0.3, o.Number("impactSize", 0.9), 0.9);
    }

    public static void Rings(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        var tg = Spots(c, o); double n = o.Number("count", 3); var ca = Col(c, o.String("color"));
        for (int ti = 0; ti < tg.Length; ti++)
        {
            var b = tg[ti];
            for (int i = 0; i < n; i++)
            {
                int index = i; var a = Launch(fx, c, o.String("from"));
                double length = Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Z - a.Z) * (b.Z - a.Z));
                double dur = o.Number("dur", Math.Min(0.5, length / 12)), t0 = d + i * o.Number("gap", 0.08) + ti * 0.03;
                MoveMesh? m = null;
                fx.Fly(t0, dur, a.X, a.Y, a.Z, b.X, b.Y, b.Z, 0, (x, y, z, k, _) =>
                {
                    if (m is null) { m = fx.Mesh("torus"); m.Color = index % 2 != 0 ? c.Cb : ca; }
                    m.Position.Set(x, y, z); m.LookAt(b.X, b.Y, b.Z);
                    m.Scale.SetScalar(o.Number("size", 0.35) * c.S * (0.6 + k)); m.Opacity = 1 - k * 0.4;
                }, () => { if (m is not null) fx.Free(m); });
                if (i == 0) Impact(fx, c, o.String("impact", "wind"), b.X, b.Z, t0 + dur, o.Number("impactSize", 0.9), b.Y);
            }
        }
    }

    public static void Volley(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        var tg = Spots(c, o); double n = o.Number("count", 10); var ca = Col(c, o.String("color")); var cell = CellOf(o.String("cell"), "streak");
        for (int i = 0; i < n; i++)
        {
            var b = tg[i % tg.Length]; var a = Launch(fx, c, o.String("from", o.Value("guns", 0) == 1 ? "mouth" : "hands"), i);
            double sp = o.Value("spread", 0.35), tx = b.X + Rnd(fx, -sp, sp), ty = b.Y + Rnd(fx, -sp, sp) * 0.5, tz = b.Z + Rnd(fx, -sp, sp);
            double length = Math.Sqrt((tx - a.X) * (tx - a.X) + (ty - a.Y) * (ty - a.Y) + (tz - a.Z) * (tz - a.Z));
            double life = length / o.Number("speed", 26), t0 = d + i * o.Number("rate", 0.035);
            fx.Later(t0, () =>
            {
                fx.Add.Add(a.X, a.Y, a.Z, (tx - a.X) / life, (ty - a.Y) / life, (tz - a.Z) / life, ca, life, o.Number("size", 0.5) * c.S, cell, MovePreset.None);
                fx.Add.Add(a.X, a.Y, a.Z, 0, 0, 0, c.Cb, 0.06, 0.5 * c.S, "spark", MovePreset.None, 1);
            });
            fx.Later(t0 + life, () => fx.Burst(fx.Add, tx, ty, tz, 4, 3, ca, White, 0.2, 0.18, "spark", MovePreset.Spark, 1));
        }
        foreach (var t in tg) Impact(fx, c, o.String("impact", "metal"), t.X, t.Z, d + 0.1, o.Number("impactSize", 0.8), t.Y);
    }

    public static void Streak(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        var b = Spots(c, o)[0]; var a = c.P; var ca = Col(c, o.String("color"));
        MoveRibbon? ribbon = null; double dur = o.Number("dur", 0.3);
        fx.Run(d, dur, (k, dt) =>
        {
            if (ribbon is null) { ribbon = fx.Ribbon(); ribbon.Color = ca; ribbon.Taper = 0.7; }
            double reach = Math.Min(1, k * 2.2), tail = Math.Max(0, k * 1.6 - 0.6);
            ribbon.Count = 10;
            for (int i = 0; i < 10; i++) { double t = tail + (reach - tail) * (i / 9.0); ribbon.Points[i].Set(a.X + (b.X - a.X) * t, 0.8, a.Z + (b.Z - a.Z) * t); }
            ribbon.Width = o.Number("width", 0.9) * c.S; ribbon.Build(1 - k * 0.6);
            double hx = a.X + (b.X - a.X) * reach, hz = a.Z + (b.Z - a.Z) * reach;
            if (reach < 1) EmitTrail(fx, o.String("trail", "flame")!, hx, 0.8, hz, dt, c.S * 1.6, ca, c.Cb);
        }, () => { if (ribbon is not null) fx.FreeRibbon(ribbon); });
        Impact(fx, c, o.String("impact"), b.X, b.Z, d + dur * 0.45, o.Number("impactSize", 1.2), b.Y);
    }

    public static void Wheel(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        var ca = Col(c, o.String("color")); var cb = c.Cb;
        foreach (var b in Spots(c, o))
        {
            var a = c.A; double length = Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Z - a.Z) * (b.Z - a.Z));
            double dur = o.Number("dur", Math.Min(0.5, length / 10)), radius = o.Number("size", 0.5) * c.S, ang = Math.Atan2(b.X - a.X, b.Z - a.Z);
            MoveMesh? t = null, t2 = null;
            fx.Fly(d, dur, a.X, a.Y, a.Z, b.X, b.Y, b.Z, 0.2, (x, y, z, _, dt) =>
            {
                if (t is null) { t = fx.Mesh("torus"); t2 = fx.Mesh(o.Flag("saw") ? "tri" : "torus"); t.Color = ca; t2.Color = cb; }
                t.Position.Set(x, y, z); t.RotationOrder = "YXZ"; t.Rotation.Set(0, ang + Math.PI / 2, -fx.Now * 25);
                t2!.Position.Set(x, y, z); t2.RotationOrder = "YXZ"; t2.Rotation.Set(0, ang + Math.PI / 2, -fx.Now * 25);
                t.Scale.Set(radius, radius, radius * 3); t2!.Scale.SetScalar(radius * 0.75);
                var n = MoveEffects.Round(dt * 160); var cell = CellOf(o.String("cell"), "flame");
                for (int i = 0; i < n; i++)
                {
                    double angle = fx.Random() * Tau, ox = Math.Cos(angle) * radius * Math.Cos(ang), oz = -Math.Cos(angle) * radius * Math.Sin(ang);
                    fx.Add.Add(x + ox, y + Math.Sin(angle) * radius, z + oz, Rnd(fx, -0.5, 0.5), Rnd(fx, 0, 1), Rnd(fx, -0.5, 0.5),
                        Pick(fx, ca, cb), 0.3, 0.4 * c.S, cell, cell == "flame" ? MovePreset.FireTrail : MovePreset.Drift);
                }
            }, () => { if (t is not null) { fx.Free(t); fx.Free(t2!); } });
            Impact(fx, c, o.String("impact"), b.X, b.Z, d + dur, o.Number("impactSize", 1), b.Y);
        }
    }

    public static void Net(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        var ca = Col(c, o.String("color"), White);
        foreach (var b in Spots(c, o))
        {
            var a = c.A; double length = Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Z - a.Z) * (b.Z - a.Z)), dur = Math.Min(0.4, length / 10);
            MoveMesh? m = null; double s = o.Number("size", 0.9) * c.S;
            fx.Run(d, dur + 0.7, k =>
            {
                if (m is null) { m = fx.Mesh("web"); m.Color = ca; }
                double t = Math.Min(1, k * (dur + 0.7) / dur);
                m.Position.Set(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t + Math.Sin(Math.PI * t) * 0.5, a.Z + (b.Z - a.Z) * t);
                m.Billboard = true; m.Scale.SetScalar(s * (0.25 + 0.75 * t)); m.Opacity = k > 0.75 ? (1 - k) * 4 : 1;
            }, () => { if (m is not null) fx.Free(m); });
            Impact(fx, c, o.String("impact", "poison"), b.X, b.Z, d + dur, o.Number("impactSize", 0.7), b.Y);
        }
    }

    public static void Pillar(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        var tg = Spots(c, o); var ca = Col(c, o.String("color"));
        for (int i = 0; i < tg.Length; i++)
        {
            var b = tg[i];
            fx.Pillar(d + i * 0.05, b.X, b.Z, ca, o.Number("size", 0.55) * c.S, o.Number("height", 8), o.Number("dur", 0.7));
            fx.Pillar(d + i * 0.05, b.X, b.Z, White, o.Number("size", 0.55) * 0.4 * c.S, o.Number("height", 8), o.Number("dur", 0.7));
            Impact(fx, c, o.String("impact", "holy"), b.X, b.Z, d + i * 0.05 + 0.08, o.Number("impactSize", 0.9), b.Y);
        }
    }

    public static void Rain(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        var tg = o.String("to") == "targets" && c.T.Length > 0 ? c.T : [c.C];
        var cell = CellOf(o.String("cell"), "petal"); var sys = SysFor(fx, cell); var ca = Col(c, o.String("color")); var cb = c.Cb;
        foreach (var b in tg)
        {
            double radius = o.String("to") == "targets" ? 0.8 : Math.Max(1.2, c.R);
            fx.Run(d, o.Number("dur", 0.8), (_, dt) =>
            {
                var n = MoveEffects.Round(dt * o.Number("count", 80));
                for (int i = 0; i < n; i++)
                {
                    double a = fx.Random() * Tau, r = Math.Sqrt(fx.Random()) * radius;
                    sys.Add(b.X + Math.Cos(a) * r, o.Number("height", 3.2), b.Z + Math.Sin(a) * r, Rnd(fx, -0.5, 0.5), -Rnd(fx, 2, 3.5), Rnd(fx, -0.5, 0.5),
                        Pick(fx, ca, cb), 1.1, o.Number("size", 0.32) * c.S, cell, MovePreset.Flutter);
                }
            });
        }
        foreach (var t in c.T) Impact(fx, c, o.String("impact", "leaf"), t.X, t.Z, d + 0.4, o.Number("impactSize", 0.7), t.Y);
    }

    public static void Glyph(MoveEffects fx, MoveContext c, MoveOptions o, double d) =>
        Glyph(fx, o.String("glyph", "tri")!, o.String("place") == "ground" ? new MoveVector(c.P.X, 0.2, c.P.Z) : c.A,
            c.Yaw, Col(c, o.String("color")), o.Number("size", 1) * c.S, d, o.Number("dur", 0.6), o.String("place") == "ground");

    public static void Drain(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        foreach (var t in c.T.Length > 0 ? c.T : [c.C]) fx.Later(d, () => Flow(fx, t.X, t.Z, c.P.X, c.P.Z));
    }
}

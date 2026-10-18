namespace DigiTactics.Presentation;

public static partial class MovePrimitives
{
    // Delivery primitives translated from js/moveprims.js. Scheduling, random draws,
    // and particle order intentionally match the source choreography.
    public static void Orb(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        double n = o.Number("count", 1);
        var targets = Spots(c, o);
        for (int t = 0; t < targets.Length; t++)
        {
            for (int i = 0; i < n; i++)
            {
                var b = targets[t];
                var a = Launch(fx, c, o.String("from"), i);
                double length = Dist(a, b);
                double duration = o.Number("dur", Math.Min(.6, Math.Max(.15, length / o.Number("speed", 14))));
                var head = MakeHead(fx, o, c.S, Col(c, o.String("color")), Col(c, o.String("core"), c.Cb), c);
                double side = o.Number("side", 0) * (i % 2 != 0 ? 1 : -1) * (n > 1 ? 1 : 0)
                    + (o.Flag("wobble") ? Rnd(fx, -o.Number("wobble", 0), o.Number("wobble", 0)) : 0);
                double tx = b.X + (n > 1 ? Rnd(fx, -.25, .25) : 0);
                double tz = b.Z + (n > 1 ? Rnd(fx, -.25, .25) : 0);
                double delay = d + i * o.Number("gap", .07) + t * .03;
                fx.Fly(delay, duration, a.X, a.Y, a.Z, tx, b.Y, tz, o.Value("arc", .3), (x, y, z, k, dt) =>
                {
                    head.Draw(x, y, z);
                    if (o.Flag("trail")) EmitTrail(fx, o.String("trail")!, x, y, z, dt, o.Number("size", .25) * 4 * c.S, Col(c, o.String("color")), c.Cb);
                    if (o.Flag("wings"))
                    {
                        foreach (int sign in new[] { -1, 1 })
                        {
                            double px = Math.Cos(c.Yaw) * sign, pz = -Math.Sin(c.Yaw) * sign;
                            fx.Add.Add(x, y, z, px * 3, Rnd(fx, .5, 1.5), pz * 3, Pick(fx, c.Ca, c.Cb), .25, .5 * c.S, "flame", MovePreset.FireTrail);
                        }
                    }
                }, head.Done, side);
                if (!o.IsFalse("muzzle")) fx.Flash(delay, a.X, a.Y, a.Z, c.Cb, .8 * c.S * (o.Number("size", .25) / .25), .15);
                Impact(fx, c, o.String("impact"), tx, tz, delay + duration, o.Number("impactSize", Math.Sqrt(o.Number("size", .25) / .3)), b.Y);
            }
        }
    }

    public static void Breath(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        var targets = Spots(c, o);
        var b = targets[0];
        var a = Launch(fx, c, o.String("from", "mouth"));
        double length = Math.Max(1, Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Z - a.Z) * (b.Z - a.Z)));
        double speed = o.Number("speed", 9) * Math.Sqrt(c.S);
        double life = length / speed * 1.15, duration = o.Number("dur", .45);
        string cell = CellOf(o.String("cell"), "flame"), mix = o.Flag("mix") ? CellOf(o.String("mix")) : cell;
        var ca = Col(c, o.String("color"));
        var cb = c.Cb;
        double ux = (b.X - a.X) / length, uz = (b.Z - a.Z) / length, uy = (b.Y - a.Y) / length;
        double width = o.Number("width", .5) * c.S, rate = o.Number("rate", 240) * .7 * Math.Sqrt(c.S);
        double size = o.Number("size", .4) * c.S;
        var particles = SysFor(fx, cell);
        fx.Run(d, duration, (k, dt) =>
        {
            double n = MoveEffects.Round(rate * dt) + 1;
            for (int i = 0; i < n; i++)
            {
                double sx = Rnd(fx, -width, width), sy = Rnd(fx, -width, width) * .6;
                double velocity = speed * Rnd(fx, .85, 1.15);
                particles.Add(a.X, a.Y, a.Z, ux * velocity - uz * sx * velocity / length,
                    uy * velocity + sy * velocity / length, uz * velocity + ux * sx * velocity / length,
                    fx.Random() < .3 ? cb : ca, life * Rnd(fx, .8, 1.1), size * Rnd(fx, .6, 1.1), fx.Random() < .25 ? mix : cell, MovePreset.Breath);
            }
            if (fx.Random() < .5) fx.Add.Add(a.X, a.Y, a.Z, 0, 0, 0, cb, .06, .7 * c.S, "glow", MovePreset.None, .9, 0);
        });
        foreach (var target in targets) Impact(fx, c, o.String("impact"), target.X, target.Z, d + Math.Min(duration * .6, length / speed), o.Number("impactSize", 1), target.Y);
    }

    public static void Beam(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        var targets = Spots(c, o);
        double duration = o.Number("dur", .45), charge = o.Number("charge", 0), width = o.Number("width", .16) * Math.Sqrt(c.S);
        var ca = Col(c, o.String("color"));
        var cb = Col(c, o.String("core"), White);
        for (int ti = 0; ti < targets.Length; ti++)
        {
            var b = targets[ti];
            int beamCount = o.Flag("twin") ? 2 : 1;
            for (int i = 0; i < beamCount; i++)
            {
                var a = o.Flag("twin") ? Launch(fx, c, o.String("from", "chest"), i) : Launch(fx, c, o.String("from"));
                double dx = b.X - a.X, dy = b.Y - a.Y, dz = b.Z - a.Z;
                double length = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                if (length == 0) length = 1;
                double over = o.Value("overshoot", .6);
                var end = new MoveVector(b.X + dx / length * over, b.Y + dy / length * over, b.Z + dz / length * over);
                double beamLength = length + over, t0 = d + ti * .04;
                MoveMesh? outer = null, inner = null;
                if (charge > 0) ChargeGlow(fx, a, ca, cb, width * 5, t0, charge);
                fx.Run(t0 + charge, duration, (k, dt) =>
                {
                    if (outer is null)
                    {
                        outer = fx.Mesh("beam");
                        inner = fx.Mesh("beam");
                        outer.Color = ca;
                        inner.Color = cb;
                        foreach (var mesh in new[] { outer, inner })
                        {
                            mesh.Position.Set((a.X + end.X) / 2, (a.Y + end.Y) / 2, (a.Z + end.Z) / 2);
                            mesh.LookAt(end.X, end.Y, end.Z);
                        }
                    }
                    double gain = k < .12 ? k / .12 : k > .7 ? (1 - k) / .3 : 1;
                    double wobble = 1 + Math.Sin(fx.Now * 60) * .08;
                    outer.Scale.Set(width * gain * wobble, width * gain * wobble, beamLength);
                    inner!.Scale.Set(width * .3 * gain, width * .3 * gain, beamLength);
                    outer.Opacity = .55 * gain;
                    inner.Opacity = .85 * gain;
                    fx.Quad("glow", a.X, a.Y, a.Z, width * 7 * gain, width * 7 * gain, ca, .7 * gain);
                    double count = MoveEffects.Round(dt * 70 * c.S);
                    for (int j = 0; j < count; j++)
                    {
                        double t = fx.Random();
                        fx.Add.Add(a.X + dx * t, a.Y + dy * t, a.Z + dz * t,
                            Rnd(fx, -1.5, 1.5), Rnd(fx, -1, 1.5), Rnd(fx, -1.5, 1.5), Pick(fx, ca, cb), .25,
                            width * 1.1, o.Flag("spiral") ? "ring" : "spark", MovePreset.Drift, .8);
                    }
                    if (o.Flag("spiral"))
                    {
                        double angle = fx.Now * 30, px = -dz / length, pz = dx / length, t = fx.Now * 3 % 1;
                        fx.Add.Add(a.X + dx * t + px * Math.Cos(angle) * width * 2,
                            a.Y + dy * t + Math.Sin(angle) * width * 2,
                            a.Z + dz * t + pz * Math.Cos(angle) * width * 2, 0, 0, 0, cb, .2, width * 2.5, "glow", MovePreset.None);
                    }
                }, () =>
                {
                    if (outer is not null) { fx.Free(outer); fx.Free(inner!); }
                });
                if (o.Flag("glyph")) Glyph(fx, o.String("glyph")!, a, c.Yaw, ca, width * 7, t0, charge + duration);
                if (i == 0) Impact(fx, c, o.String("impact"), b.X, b.Z, t0 + charge + .05, o.Number("impactSize", 1), b.Y);
                if (i == 0 && duration > .4) Impact(fx, c, o.String("impact"), b.X, b.Z, t0 + charge + duration * .6, o.Number("impactSize", 1) * .7, b.Y);
            }
        }
    }

    public static void Missiles(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        double count = o.Number("count", 2);
        var targets = Spots(c, o);
        for (int i = 0; i < count; i++)
        {
            var b = targets[i % targets.Length];
            var a = Launch(fx, c, o.String("from", "chest"), i);
            double duration = o.Number("dur", .55), side = o.Value("curve", 1.2) * (i % 2 != 0 ? 1 : -1) * Rnd(fx, .7, 1.1);
            double tx = b.X + Rnd(fx, -.2, .2), tz = b.Z + Rnd(fx, -.2, .2), size = o.Number("size", .14) * c.S;
            MoveMesh? body = null;
            double lastX = a.X, lastY = a.Y, lastZ = a.Z, delay = d + i * o.Number("gap", .08);
            fx.Fly(delay, duration, a.X, a.Y, a.Z, tx, b.Y, tz, o.Value("arc", 1.6), (x, y, z, k, dt) =>
            {
                if (body is null)
                {
                    body = fx.Mesh("body");
                    body.Color = HexColor(o.Flag("organic") ? "#d8c6aa" : "#a7b0bc");
                    body.Emissive = HexColor(o.Flag("organic") ? "#3a1810" : "#101418");
                }
                body.Position.Set(x, y, z);
                body.Scale.Set(size, size, size * 2.8);
                body.LookAt(x + (x - lastX) * 10, y + (y - lastY) * 10, z + (z - lastZ) * 10);
                double bx = x - (x - lastX) * 2, by = y - (y - lastY) * 2, bz = z - (z - lastZ) * 2;
                lastX = x; lastY = y; lastZ = z;
                EmitTrail(fx, "flame", bx, by, bz, dt, size * 2.2, c.Ca, c.Cb);
                if (!o.IsFalse("smoke")) EmitTrail(fx, "smoke", bx, by, bz, dt, size * 4, c.Ca, c.Cb);
            }, () => { if (body is not null) fx.Free(body); }, side);
            fx.Flash(delay, a.X, a.Y, a.Z, c.Cb, .7 * c.S, .15);
            Impact(fx, c, o.String("impact", "blast"), tx, tz, delay + duration, o.Number("impactSize", 1), b.Y);
        }
    }

    public static void Bolt(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        var ca = Col(c, o.String("color"));
        var cb = c.Cb;
        double width = o.Number("width", .09) * c.S;
        var targets = Spots(c, o);
        if (o.String("from") == "caster")
        {
            for (int i = 0; i < targets.Length; i++)
            {
                var b = targets[i];
                var a = Launch(fx, c, o.String("launch", "mouth"));
                fx.Bolt(d + i * .03, a.X, a.Y, a.Z, b.X, b.Y, b.Z, ca, o.Number("dur", .3), width, .35, 14);
                fx.Bolt(d + i * .03 + .04, a.X, a.Y, a.Z, b.X, b.Y, b.Z, cb, o.Number("dur", .3) * .8, width * .5, .25, 14);
                fx.Flash(d, a.X, a.Y, a.Z, cb, .9 * c.S, .2);
                Impact(fx, c, o.String("impact", "zap"), b.X, b.Z, d + .03, o.Number("impactSize", 1), b.Y);
            }
            return;
        }
        double height = o.Number("height", 7);
        List<MoveVector> strikes = [.. targets];
        if (o.Flag("area"))
        {
            for (int i = 0; i < (int)o.Number("area", 0); i++)
            {
                double angle = fx.Random() * Tau, radius = Math.Sqrt(fx.Random()) * c.R;
                strikes.Add(new(c.C.X + Math.Cos(angle) * radius, .2, c.C.Z + Math.Sin(angle) * radius));
            }
        }
        for (int i = 0; i < strikes.Count; i++)
        {
            var b = strikes[i];
            double count = o.Number("count", 2), t0 = d + i * .05;
            for (int j = 0; j < count; j++)
                fx.Bolt(t0 + j * .05, b.X + Rnd(fx, -.6, .6), height, b.Z + Rnd(fx, -.6, .6), b.X, .1, b.Z,
                    j % 2 != 0 ? cb : ca, .3, width * (j != 0 ? .6 : 1), .45, 16);
            fx.Pillar(t0, b.X, b.Z, ca, .25 * c.S, height, .25);
            Impact(fx, c, o.String("impact", "zap"), b.X, b.Z, t0 + .02, o.Number("impactSize", 1) * (i < targets.Length ? 1 : .6), .3);
        }
    }

    public static void Slash(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        var targets = Spots(c, o);
        double count = o.Number("count", 2);
        var color = Col(c, o.String("color"));
        for (int ti = 0; ti < targets.Length; ti++)
        {
            var b = targets[ti];
            for (int i = 0; i < count; i++)
            {
                double basis = o.Flag("vertical") ? Math.PI / 2 : .5;
                double angle = basis + (i % 2 != 0 ? -1 : 1) * o.Value("spread", .9) + Rnd(fx, -.3, .3);
                SlashMesh(fx, d + ti * .03 + i * o.Number("gap", .08), b.X + Rnd(fx, -.15, .15),
                    (b.Y != 0 ? b.Y : .8) + .1, b.Z + Rnd(fx, -.15, .15), color, o.Number("size", 1) * c.S,
                    angle, o.Flag("claw") ? "claw" : "crescent", o.Number("dur", .22));
                if (i == 0) Impact(fx, c, o.String("impact", "hit"), b.X, b.Z, d + ti * .03 + .04, o.Number("impactSize", .8), b.Y);
            }
        }
    }

    public static void Dash(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        var b = Spots(c, o)[0];
        var from = c.From ?? c.P;
        double duration = o.Number("dur", .2), count = o.Number("ghosts", 5), height = c.H;
        var ca = Col(c, o.String("color"));
        for (int i = 0; i < count; i++)
        {
            double k = i / Math.Max(1, count - 1);
            double x = from.X + (b.X - from.X) * k * .85, z = from.Z + (b.Z - from.Z) * k * .85;
            var ghostColor = i % 2 != 0 ? c.Cb : ca;
            double ghostY = height * .5 + Math.Sin(Math.PI * k) * .8;
            fx.Run(d + k * duration, .35, kk => fx.Quad("glow", x, ghostY, z, height * .65 * (1 + kk * .3), height * 1.1, ghostColor, .7 * (1 - kk)));
        }
        MoveRibbon? ribbon = null;
        fx.Run(d, duration + .2, k =>
        {
            if (ribbon is null) { ribbon = fx.Ribbon(); ribbon.Color = ca; ribbon.Taper = 1; }
            double reach = Math.Min(1, k * (duration + .2) / duration), tail = Math.Max(0, (k - .5) * 2);
            ribbon.Count = 12;
            for (int i = 0; i < 12; i++)
            {
                double t = tail + (reach - tail) * (i / 11d);
                ribbon.Points[i].Set(from.X + (b.X - from.X) * t, .7 + Math.Sin(Math.PI * t) * .8, from.Z + (b.Z - from.Z) * t);
            }
            ribbon.Width = .5 * c.S;
            ribbon.Build(1 - k * .7);
        }, () => { if (ribbon is not null) fx.FreeRibbon(ribbon); });
        for (int i = 0; i < 24; i++)
        {
            double k = fx.Random();
            fx.Add.Add(from.X + (b.X - from.X) * k, Rnd(fx, .3, 1.3), from.Z + (b.Z - from.Z) * k,
                Rnd(fx, -.5, .5), Rnd(fx, 0, .6), Rnd(fx, -.5, .5), Pick(fx, ca, c.Cb), Rnd(fx, .25, .5), .3, "spark", MovePreset.Drift);
        }
        if (!o.Has("slashes") || o.Value("slashes", double.NaN) != 0)
            Slash(fx, c, new MoveOptions(("count", o.Number("slashes", 2)), ("claw", o.Raw("claw")),
                ("size", o.Number("size", 1.2)), ("color", o.Raw("color")), ("impact", o.Raw("impact")),
                ("spread", o.Raw("spread")), ("to", o.Raw("to"))), d + duration);
    }

    public static void Erupt(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        string kind = o.String("kind", "flame")!;
        List<MoveVector> points = [];
        if (o.String("to") == "center" || o.Flag("area"))
        {
            double count = o.Number("count", 5);
            points.Add(new(c.C.X, 0, c.C.Z));
            for (int i = 1; i < count; i++)
            {
                double angle = i / (count - 1) * Tau + Rnd(fx, -.3, .3), radius = c.R * Rnd(fx, .45, .85);
                points.Add(new(c.C.X + Math.Cos(angle) * radius, 0, c.C.Z + Math.Sin(angle) * radius));
            }
        }
        else points.AddRange(Spots(c, o));
        var ca = Col(c, o.String("color"));
        var cb = c.Cb;
        double height = o.Number("height", 2.2) * c.S;
        for (int i = 0; i < points.Count; i++)
        {
            var point = points[i];
            double t0 = d + i * .06;
            fx.GroundRing(t0, point.X, point.Z, ca, .9 * c.S, .25);
            if (kind is "ice" or "rock" or "thorn" or "metal")
            {
                for (int j = 0; j < 4; j++)
                    SpikeAt(fx, t0 + .08 + j * .03, point.X + Rnd(fx, -.35, .35), point.Z + Rnd(fx, -.35, .35),
                        .2 * c.S, height * Rnd(fx, .45, .8), kind, .45, Rnd(fx, -.35, .35));
                fx.Later(t0 + .08, () => fx.Burst(kind == "ice" ? fx.Add : fx.Alpha, point.X, .3, point.Z,
                    12, 4, kind == "ice" ? ca : Rock, kind == "ice" ? White : Rock2, .7, .22,
                    kind == "ice" ? "shard" : kind == "thorn" ? "leaf" : "square", MovePreset.Debris, 3));
            }
            else
            {
                string cell = kind == "water" ? "drop" : kind == "dark" ? "wisp" : "flame";
                fx.Pillar(t0 + .08, point.X, point.Z, ca, .35 * c.S, height * 1.3, .45);
                fx.Run(t0 + .08, .3, (k, dt) =>
                {
                    double count = MoveEffects.Round(dt * 200 * c.S);
                    for (int j = 0; j < count; j++)
                        fx.Add.Add(point.X + Rnd(fx, -.25, .25), .1, point.Z + Rnd(fx, -.25, .25), Rnd(fx, -.6, .6),
                            height * Rnd(fx, 2, 3.2), Rnd(fx, -.6, .6), Pick(fx, ca, cb), Rnd(fx, .35, .55),
                            .5 * c.S, cell, kind == "water" ? MovePreset.Debris : MovePreset.Flame);
                });
            }
            Impact(fx, c, o.String("impact"), point.X, point.Z, t0 + .1, o.Number("impactSize", .8) * (i != 0 ? .7 : 1), .5);
        }
    }

    public static void Meteor(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        double count = o.Number("count", 1);
        for (int i = 0; i < count; i++)
        {
            double angle = fx.Random() * Tau, radius = i != 0 ? c.R * Rnd(fx, .3, .8) : 0;
            var b = new MoveVector(c.C.X + Math.Cos(angle) * radius, .3, c.C.Z + Math.Sin(angle) * radius);
            double height = o.Number("height", 9);
            var a = new MoveVector(b.X - 3.5, height, b.Z - 2.5);
            double duration = o.Number("dur", .5);
            var head = MakeHead(fx, new MoveOptions(("size", o.Number("size", .6) * (i != 0 ? .6 : 1))), c.S, Col(c, o.String("color")), c.Cb, c);
            double t0 = d + i * .12;
            fx.Fly(t0, duration, a.X, a.Y, a.Z, b.X, b.Y, b.Z, 0, (x, y, z, k, dt) =>
            {
                head.Draw(x, y, z);
                EmitTrail(fx, o.String("trail", "flame")!, x, y, z, dt, o.Number("size", .6) * 3 * c.S, Col(c, o.String("color")), c.Cb);
                EmitTrail(fx, "smoke", x, y + .3, z, dt, o.Number("size", .6) * 3 * c.S);
            }, head.Done);
            Impact(fx, c, o.String("impact", "blast"), b.X, b.Z, t0 + duration, o.Number("impactSize", 1.6) * (i != 0 ? .6 : 1), .4);
            fx.Later(t0 + duration, () =>
            {
                fx.Burst(fx.Alpha, b.X, .3, b.Z, 20, 5, Rock, Rock2, .9, .2, "square", MovePreset.Debris, 4);
                fx.GroundRing(0, b.X, b.Z, Col(c, o.String("color")), c.R * 1.4 + 1, .6);
            });
        }
    }

    public static void Sphere(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        var targets = Spots(c, o);
        var b = targets[0];
        var a = o.String("from") == "mouth" ? c.A : o.String("from") == "hands"
            ? new MoveVector(c.A.X + Math.Sin(c.Yaw) * .4, c.A.Y, c.A.Z + Math.Cos(c.Yaw) * .4)
            : new MoveVector(c.P.X, c.H + .9 * c.S, c.P.Z);
        double grow = o.Value("grow", .35), size = o.Number("size", .55) * c.S;
        var ca = Col(c, o.String("color"));
        var cb = Col(c, o.String("core"), c.Cb);
        fx.Run(d, grow, (k, dt) =>
        {
            double radius = size * (.2 + .8 * k);
            fx.Ball(a.X, a.Y, a.Z, radius * .6, cb, .9);
            fx.Quad("glow", a.X, a.Y, a.Z, radius * 3.6, radius * 3.6, ca, .8, fx.Now * 4);
            double count = MoveEffects.Round(dt * 100);
            for (int i = 0; i < count; i++)
            {
                double angle = fx.Random() * Tau, vertical = Rnd(fx, -1, 1), reach = radius * 4;
                double x = Math.Cos(angle) * Math.Sqrt(1 - vertical * vertical) * reach;
                double y = vertical * reach, z = Math.Sin(angle) * Math.Sqrt(1 - vertical * vertical) * reach;
                fx.Add.Add(a.X + x, a.Y + y, a.Z + z, -x * 5, -y * 5, -z * 5, Pick(fx, ca, cb), .2,
                    .25 * c.S, o.Flag("cell") ? CellOf(o.String("cell")) : "spark", MovePreset.None);
            }
        });
        double length = Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Z - a.Z) * (b.Z - a.Z));
        double duration = o.Number("dur", Math.Min(.55, Math.Max(.2, length / 12)));
        fx.Fly(d + grow, duration, a.X, a.Y, a.Z, b.X, b.Y, b.Z, .3, (x, y, z, k, dt) =>
        {
            fx.Ball(x, y, z, size * .6, cb, .9);
            fx.Quad("glow", x, y, z, size * 3.6, size * 3.6, ca, .8, fx.Now * 4);
            EmitTrail(fx, o.String("trail", "spark")!, x, y, z, dt, size * 2, ca, cb);
        });
        foreach (var target in targets) Impact(fx, c, o.String("impact"), target.X, target.Z, d + grow + duration, o.Number("impactSize", 1.4), target.Y);
    }

    public static void Wave(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        var targets = Spots(c, o);
        double count = o.Number("count", 1);
        var ca = Col(c, o.String("color"));
        for (int ti = 0; ti < targets.Length; ti++)
        {
            var b = targets[ti];
            for (int i = 0; i < count; i++)
            {
                var a = c.P;
                double angle = Math.Atan2(b.X - a.X, b.Z - a.Z);
                double length = Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Z - a.Z) * (b.Z - a.Z)) + 1.2;
                double duration = o.Number("dur", Math.Min(.45, length / 14)), width = o.Number("width", 1.4) * Math.Sqrt(c.S);
                MoveMesh? mesh = null, glow = null;
                double t0 = d + i * .1 + ti * .03;
                int waveIndex = i;
                fx.Run(t0, duration, (k, dt) =>
                {
                    if (mesh is null)
                    {
                        mesh = fx.Mesh("crescent"); mesh.Color = ca;
                        glow = fx.Mesh("crescent"); glow.Color = White;
                    }
                    double x = a.X + Math.Sin(angle) * length * k, z = a.Z + Math.Cos(angle) * length * k;
                    foreach (var item in new[] { mesh, glow! })
                    {
                        item.Position.Set(x, .9 * c.S, z);
                        item.RotationOrder = "YXZ";
                        item.Rotation.Set(0, angle, (waveIndex % 2 != 0 ? -1 : 1) * .5 + Math.PI / 2 + (o.Flag("flat") ? Math.PI / 2 : 0));
                    }
                    mesh.Scale.SetScalar(width * (.8 + k * .4));
                    glow!.Scale.SetScalar(width * (.65 + k * .35));
                    mesh.Opacity = k > .8 ? (1 - k) * 5 : 1;
                    glow.Opacity = mesh.Opacity * .35;
                    double particles = MoveEffects.Round(dt * 80);
                    for (int j = 0; j < particles; j++)
                        fx.Add.Add(x + Rnd(fx, -.3, .3) * width, Rnd(fx, .2, 1.6) * c.S, z + Rnd(fx, -.3, .3) * width,
                            0, Rnd(fx, 0, .5), 0, Pick(fx, ca, White), .3, .25, "spark", MovePreset.Drift);
                }, () => { if (mesh is not null) { fx.Free(mesh); fx.Free(glow!); } });
                Impact(fx, c, o.String("impact", "hit"), b.X, b.Z, t0 + duration * ((length - 1.2) / length), o.Number("impactSize", 1.1), b.Y);
            }
        }
    }

    public static void Arrows(MoveEffects fx, MoveContext c, MoveOptions o, double d)
    {
        var targets = Spots(c, o);
        double count = o.Number("count", 8);
        var ca = Col(c, o.String("color"));
        string cell = CellOf(o.String("cell"), "streak");
        for (int i = 0; i < count; i++)
        {
            var b = targets[i % targets.Length];
            double tx = b.X + Rnd(fx, -.5, .5), tz = b.Z + Rnd(fx, -.5, .5);
            var a = o.Flag("rain") ? new MoveVector(tx - 1, 6, tz - .6) : Launch(fx, c, "sky");
            double dx = tx - a.X, dy = .2 - a.Y, dz = tz - a.Z;
            double length = Math.Sqrt(dx * dx + dy * dy + dz * dz), life = length / 22, t0 = d + i * o.Number("gap", .035);
            fx.Later(t0, () =>
            {
                fx.Add.Add(a.X, a.Y, a.Z, (tx - a.X) / life, (.2 - a.Y) / life, (tz - a.Z) / life,
                    ca, life, o.Number("size", .7) * c.S, cell, MovePreset.None);
                fx.Add.Add(a.X, a.Y, a.Z, (tx - a.X) / life, (.2 - a.Y) / life, (tz - a.Z) / life,
                    White, life, o.Number("size", .7) * .6 * c.S, cell, MovePreset.None);
            });
            fx.Later(t0 + life, () =>
            {
                fx.Burst(fx.Add, tx, .25, tz, 6, 2.5, ca, White, .3, .2, "spark", MovePreset.Drift, 1);
                fx.Add.Add(tx, .2, tz, 0, 0, 0, ca, .25, .8, "ring", MovePreset.Pop);
            });
        }
        foreach (var target in targets) Impact(fx, c, o.String("impact", "holy"), target.X, target.Z, d + .3, o.Number("impactSize", .7), target.Y);
    }
}

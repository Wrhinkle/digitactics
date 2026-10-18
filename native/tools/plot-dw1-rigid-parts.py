"""CPU-only orthographic anatomy diagnostic, deliberately without texture changes."""
import colorsys
import json
import sys
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

OUT = Path(__file__).resolve().parents[1] / "artifacts" / "dw1-rig-diagnosis"
FONT = ImageFont.truetype("C:/Windows/Fonts/consola.ttf", 18)
SMALL = ImageFont.truetype("C:/Windows/Fonts/consola.ttf", 14)


def panel(image, parts, box, axes, label, labels=True, fit=None):
    draw = ImageDraw.Draw(image)
    x, y, w, h = box
    horizontal, vertical, depth = axes
    points = fit or [p for part in parts for p in part["points"]]
    lo = [min(p[a] for p in points) for a in range(3)]
    hi = [max(p[a] for p in points) for a in range(3)]
    scale = min((w - 45) / max(hi[horizontal] - lo[horizontal], .001),
                (h - 70) / max(hi[vertical] - lo[vertical], .001))
    center = [(a + b) / 2 for a, b in zip(lo, hi)]

    def project(p):
        return (x + w / 2 + (p[horizontal] - center[horizontal]) * scale,
                y + h / 2 + 15 - (p[vertical] - center[vertical]) * scale)

    polygons = []
    for part in parts:
        color = colorsys.hsv_to_rgb((part["index"] * .61803398875) % 1, .58, .92)
        for face in part["triangles"]:
            vertices = [part["points"][i] for i in face]
            a, b, c = vertices
            u, v = [b[i] - a[i] for i in range(3)], [c[i] - a[i] for i in range(3)]
            n = [u[1] * v[2] - u[2] * v[1], u[2] * v[0] - u[0] * v[2], u[0] * v[1] - u[1] * v[0]]
            length = max(sum(q * q for q in n) ** .5, .00001)
            shade = .50 + .50 * abs(n[depth]) / length
            polygons.append((sum(p[depth] for p in vertices) / 3,
                             [project(p) for p in vertices], tuple(int(v * shade * 255) for v in color)))
    for _, vertices, color in sorted(polygons):
        draw.polygon(vertices, fill=color, outline=tuple(max(0, c - 25) for c in color))
    if labels:
        for part in parts:
            p = [(a + b) / 2 for a, b in zip(part["bounds"]["min"], part["bounds"]["max"])]
            px, py = project(p)
            text = str(part["index"])
            draw.text((px, py), text, fill="white", font=FONT, stroke_width=2, stroke_fill="black", anchor="mm")
    draw.rectangle((x, y, x + w - 1, y + h - 1), outline="#455166")
    draw.text((x + 10, y + 8), label, font=FONT, fill="white")


for name in (sys.argv[1:] or ["DW1_Agumon", "DW1_Airdramon", "DW1_Akatorimon"]):
    data = json.loads((OUT / (name + ".json")).read_text())
    image = Image.new("RGB", (1800, 820), "#101720")
    draw = ImageDraw.Draw(image)
    for i, axes in enumerate(((0, 1, 2), (2, 1, 0), (0, 2, 1))):
        panel(image, data["parts"], (i * 600, 0, 600, 600), axes, ("Front +Z", "Side +X", "Top +Y")[i])
    for i, part in enumerate(data["parts"]):
        draw.text((12 + (i // 7) * 600, 615 + (i % 7) * 27), f'{part["index"]}: {part["name"]}', font=SMALL, fill="white")
    image.save(OUT / (name + ".parts.png"))
    rows = (len(data["parts"]) + 3) // 4
    image = Image.new("RGB", (1600, rows * 320), "#101720")
    for i, part in enumerate(data["parts"]):
        panel(image, [part], ((i % 4) * 400, (i // 4) * 320, 400, 320), (2, 1, 0),
              f'{part["index"]}: {part["name"]}', False)
    image.save(OUT / (name + ".isolated.png"))
    print(OUT / (name + ".parts.png"))
    motion_path = OUT / (name + ".motion.json")
    if motion_path.exists():
        motion = json.loads(motion_path.read_text())
        points = [p for part in motion["frames"][0]["parts"] for p in part["points"]]
        lo = [min(p[a] for p in points) for a in range(3)]
        hi = [max(p[a] for p in points) for a in range(3)]
        pad = max(hi[a] - lo[a] for a in range(3)) * .12
        fit = [[v - pad for v in lo], [v + pad for v in hi]]
        for label, axes in (("front", (0, 1, 2)), ("side", (2, 1, 0))):
            image = Image.new("RGB", (2100, 1500), "#101720")
            for i, frame in enumerate(motion["frames"]):
                panel(image, frame["parts"], ((i % 7) * 300, (i // 7) * 300, 300, 300), axes,
                      f'{frame["role"]} {frame["fraction"]:.2f}', False, fit)
            image.save(OUT / (name + ".motion-" + label + ".png"))
            blend_path = OUT / (name + ".blends.json")
            if blend_path.exists():
                blends = json.loads(blend_path.read_text())
                image = Image.new("RGB", (2100, 300), "#101720")
                for i, frame in enumerate(blends["frames"]):
                    panel(image, frame["parts"], (i * 300, 0, 300, 300), axes,
                          f'{frame["role"]} {frame["fraction"]:.2f}', False, fit)
                image.save(OUT / (name + ".blends-" + label + ".png"))

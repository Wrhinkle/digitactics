"""Label native audit sheets and build compact temporal evidence. Never assigns visual approval."""
import argparse
import json
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("directory", type=Path)
    parser.add_argument("--start", type=int, default=0)
    parser.add_argument("--count", type=int, default=2**31 - 1)
    parser.add_argument("--overviews", action="store_true")
    args = parser.parse_args()
    font_path = Path("C:/Windows/Fonts/consola.ttf")
    font = ImageFont.truetype(str(font_path), 14)
    title_font = ImageFont.truetype(str(font_path), 17)
    records = []
    for path in sorted(args.directory.glob("[0-9][0-9][0-9][0-9].json")):
        data = json.loads(path.read_text(encoding="utf-8"))
        if not args.start <= data["Index"] < args.start + args.count or data.get("Status") in ("render-error", "alias-unreviewed"):
            continue
        source = args.directory / data["Image"]
        raw = Image.open(source).convert("RGB")
        cell, frames, roles = data["Cell"], data["Frames"], data["Roles"]
        samples = {(s["Role"], s["Frame"]): s for s in data["Samples"]}
        labeled_path = source.with_suffix(".review.webp")
        if not labeled_path.exists():
            labeled = Image.new("RGB", (cell * frames, (cell + 40) * len(roles) + 28), "#07101c")
            draw = ImageDraw.Draw(labeled)
            draw.text((8, 4), f'{data["Index"]:04} {data["Source"]}   STATUS: UNREVIEWED', font=title_font, fill="#f0e6d2")
            for row, role in enumerate(roles):
                for frame in range(frames):
                    sample = samples[role, frame]
                    x, y = frame * cell, 28 + row * (cell + 40)
                    labeled.paste(raw.crop((x, row * cell, x + cell, (row + 1) * cell)), (x, y + 40))
                    color = "#ff9583" if sample["EdgePixels"] > 5 or sample["VisiblePixels"] < 40 else "#d0bd8b"
                    draw.text((x + 4, y + 2), f'{role} {sample["Time"]:.3f}s', font=font, fill=color)
                    draw.text((x + 4, y + 20), f'clip:{sample["Clip"] or "NONE"}', font=font, fill="#a7b2c7")
            labeled.save(labeled_path, "WEBP", quality=88, method=4)
        animation_path = source.with_suffix(".animated.webp")
        if not animation_path.exists():
            temporal = []
            for frame in range(frames):
                canvas = Image.new("RGB", (cell * 3, (cell + 28) * 2 + 28), "#07101c")
                draw = ImageDraw.Draw(canvas)
                draw.text((5, 5), f'{data["Index"]:04} {data["Source"]}', font=font, fill="#f0e6d2")
                for row, role in enumerate(roles):
                    x, y = row % 3 * cell, 28 + row // 3 * (cell + 28)
                    sample = samples[role, frame]
                    draw.text((x + 3, y + 3), f'{role} {sample["Time"]:.3f}s', font=font, fill="#d0bd8b")
                    canvas.paste(raw.crop((frame * cell, row * cell, (frame + 1) * cell, (row + 1) * cell)), (x, y + 28))
                temporal.append(canvas)
            temporal[0].save(animation_path, "WEBP", save_all=True, append_images=temporal[1:], duration=150, loop=0, quality=82, method=4)
        records.append((data, source))
        raw.close()
    if args.overviews:
        # Five sources per overview. Each role shows initial/middle/end at the same fixed camera/scale.
        for offset in range(0, len(records), 5):
            subset = records[offset:offset + 5]
            thumb = 192
            page = Image.new("RGB", (thumb * 15, (thumb + 44) * len(subset)), "#07101c")
            draw = ImageDraw.Draw(page)
            for row, (data, source) in enumerate(subset):
                raw = Image.open(source).convert("RGB")
                y = row * (thumb + 44)
                draw.text((6, y + 3), f'{data["Index"]:04} {data["Source"]}   {len(data["Flags"])} technical flags; visually unreviewed', font=title_font, fill="#f0e6d2")
                for role_index, role in enumerate(data["Roles"][:5]):
                    for sample_index, frame in enumerate((0, data["Frames"] // 2, data["Frames"] - 1)):
                        x = (role_index * 3 + sample_index) * thumb
                        draw.text((x + 3, y + 25), role + (" start" if sample_index == 0 else " mid" if sample_index == 1 else " end"), font=font, fill="#a7b2c7")
                        cell = data["Cell"]
                        page.paste(raw.crop((frame * cell, role_index * cell, (frame + 1) * cell, (role_index + 1) * cell)).resize((thumb, thumb)), (x, y + 44))
                raw.close()
            page.save(args.directory / f'overview-{subset[0][0]["Index"]:04}-{subset[-1][0]["Index"]:04}.webp', "WEBP", quality=90, method=4)
    manifest = [{"index": data["Index"], "sourceKey": data["Source"], "contactSheet": str(source.with_suffix(".review.webp")), "temporalEvidence": str(source.with_suffix(".animated.webp")), "textureViews": str(args.directory / data["TextureImage"]) if data.get("TextureImage") else None, "nativeRecord": str(source.with_suffix(".json")), "status": "UNVERIFIED"} for data, source in records]
    (args.directory / f"review-batch-{args.start:04}-{min(args.start + args.count - 1, max((data['Index'] for data, _ in records), default=args.start)):04}.json").write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    print(f"Prepared temporal review evidence for {len(records)} sources; no visual statuses changed.")


if __name__ == "__main__":
    main()

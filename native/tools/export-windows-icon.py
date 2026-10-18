"""Keep the original application artwork in Windows executable icon resources."""
from pathlib import Path
from PIL import Image

root = Path(__file__).resolve().parents[2]
with Image.open(root / "icons/icon-512.png") as source:
    rgba = source.convert("RGBA")
    rgba.save(root / "icons/digitactics.ico", format="ICO",
              sizes=[(size, size) for size in (16, 24, 32, 48, 64, 128, 256)])
    # The game's PNG decoder deliberately accepts only non-interlaced RGBA8.
    # Original artwork is RGB; add an opaque alpha channel without resampling.
    window = root / "icons/digitactics-window.png"
    rgba.save(window, format="PNG")
    with Image.open(window) as decoded:
        if decoded.mode != "RGBA" or decoded.size != rgba.size or decoded.tobytes() != rgba.tobytes():
            raise ValueError("Window icon conversion changed the original artwork.")
    if window.read_bytes()[24:29] != bytes((8, 6, 0, 0, 0)):
        raise ValueError("Window icon is not non-interlaced RGBA8.")
print("Windows executable and RGBA window icons exported from icons/icon-512.png.")

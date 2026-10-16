"""Builds dist/DigiTactics.exe: one windowed executable with the game and 3D models inside.

Run: desktop\\.venv\\Scripts\\python desktop\\build.py
Needs: pip install -r desktop\\requirements.txt (pywebview, pyinstaller, pillow)
Rebuild js/bundle.js first (node tools/build.cjs) if you changed render3d.js or ui.js.
"""
import shutil
import sys
from pathlib import Path

import PyInstaller.__main__

HERE = Path(__file__).resolve().parent
ROOT = HERE.parent
BUILD = HERE / "build"
STAGE = BUILD / "game"

# Files the page actually loads. vendor/ is not needed: three.js is inside js/bundle.js.
GAME_FILES = ["index.html", "style.css", "js/data.js", "js/items.js", "js/sets.js", "js/combat.js", "js/duos.js", "js/game.js", "js/lockstep.js",
              "js/models.js", "js/credits-data.js", "CREDITS.md", "THIRD_PARTY_NOTICES.txt", "docs/credits/MODELS.md", "docs/credits/model-assets.json", "docs/credits/contributors.json", "docs/art/set-carousel-art.json", "js/cosmetics.js", "js/bundle.js", "models/index.json",
              "icons/favicon.svg", "icons/favicon-16.png", "icons/favicon-32.png", "icons/apple-touch-icon.png", "icons/icon-192.png", "icons/icon-512.png", "icons/site.webmanifest"]


def stage_game():
    if STAGE.exists():
        shutil.rmtree(STAGE)
    # set packages (js/sets/<id>.js, -mech.js, -art.js): every file index.html can load
    set_files = sorted(str(p.relative_to(ROOT)).replace("\\", "/") for p in (ROOT / "js" / "sets").glob("*.js"))
    for rel in GAME_FILES + set_files:
        dst = STAGE / rel
        dst.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(ROOT / rel, dst)
    for sub in ("files", "portraits", "legends", "items"):
        models = ROOT / "models" / sub
        if models.exists():
            shutil.copytree(models, STAGE / "models" / sub)
    # client art (js/client.js): Home banners, mode cards, emblems, renders
    if (ROOT / "assets" / "client").exists():
        shutil.copytree(ROOT / "assets" / "client", STAGE / "assets" / "client")
    print("staged", sum(1 for _ in STAGE.rglob("*") if _.is_file()), "files")


def make_icon() -> Path:
    from PIL import Image, ImageDraw
    ico = BUILD / "icon.ico"
    img = Image.new("RGBA", (256, 256), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.ellipse((28, 12, 228, 244), fill=(255, 243, 176), outline=(255, 176, 46), width=14)  # Digi-Egg
    for x, y, r in ((92, 90, 26), (160, 140, 32), (100, 180, 20)):
        d.ellipse((x - r, y - r, x + r, y + r), fill=(255, 122, 69))
    img.save(ico, sizes=[(256, 256), (64, 64), (48, 48), (32, 32), (16, 16)])
    return ico


def main():
    BUILD.mkdir(exist_ok=True)
    stage_game()
    icon = make_icon()
    PyInstaller.__main__.run([
        str(HERE / "digitactics.py"),
        "--name", "DigiTactics",
        "--onefile",
        "--windowed",  # no console window
        "--noconfirm",
        "--icon", str(icon),
        "--add-data", f"{STAGE}{';' if sys.platform == 'win32' else ':'}game",
        "--distpath", str(ROOT / "dist"),
        "--workpath", str(BUILD / "pyinstaller"),
        "--specpath", str(BUILD),
    ])
    print("built", ROOT / "dist" / ("DigiTactics.exe" if sys.platform == "win32" else "DigiTactics"))


if __name__ == "__main__":
    main()

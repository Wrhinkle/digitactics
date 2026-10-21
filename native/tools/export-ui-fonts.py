"""Reproduce the bundled static Inter faces without a runtime font dependency.

Install the generator once (development only):
  python -m pip install --no-cache-dir --target native/artifacts/font-tools fonttools==4.60.1
Then run from any directory:
  python native/tools/export-ui-fonts.py
  python native/tools/export-ui-fonts.py --check

The source variable font and both its embedded notices and inter/OFL.txt remain
unchanged. Only the listed weights (600, 700, 800) at the default opsz=14 are
instantiated; no glyphs are removed. Timestamps remain those of the pinned source for byte-stable output.
"""

import argparse
import hashlib
import io
import json
from pathlib import Path
import sys

NATIVE = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(NATIVE / "artifacts/font-tools"))
try:
    import fontTools
    from fontTools.ttLib import TTFont
    from fontTools.varLib.instancer import instantiateVariableFont
except ImportError as error:
    raise SystemExit("Install fonttools==4.60.1 using the command in this script's header.") from error

VERSION = "4.60.1"
FONTS = NATIVE / "content/fonts"
SOURCE = "inter/Inter[opsz,wght].ttf"
# The original client uses Inter 600 for buttons and names, 700 for headings and 800 for Bits/prices.
OUTPUTS = {"inter/Inter-SemiBold.ttf": 600, "inter/Inter-Bold.ttf": 700, "inter/Inter-ExtraBold.ttf": 800}
SOURCE_SHA256 = "29160a80ff49ddcab2c97711247e08b1fab27a484a329ce8b813d820dc559031"
LICENSE_SHA256 = "5b9321a4298cfeb6b34354164a1c3afc3db114569984c502b9b35d988fd58c57"


def digest(data):
    return hashlib.sha256(data).hexdigest()


def generate(source, weight):
    font = TTFont(io.BytesIO(source), recalcTimestamp=False)
    axes = {axis.axisTag: axis.defaultValue for axis in font["fvar"].axes}
    if axes != {"opsz": 14, "wght": 400}:
        raise ValueError(f"Unexpected source axes: {axes}")
    notices = [record.toUnicode() for record in font["name"].names if record.nameID in (0, 13, 14)]
    instantiateVariableFont(font, {"opsz": 14, "wght": weight}, inplace=True, updateFontNames=True, static=True)
    if "fvar" in font or font["OS/2"].usWeightClass != weight:
        raise ValueError(f"Generated font is not a static {weight}-weight face")
    if [record.toUnicode() for record in font["name"].names if record.nameID in (0, 13, 14)] != notices:
        raise ValueError("Generated font changed its embedded copyright/license notices")
    target = io.BytesIO()
    font.save(target, reorderTables=True)
    return target.getvalue()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check", action="store_true", help="Verify the committed font and provenance without changing files")
    args = parser.parse_args()
    if fontTools.__version__ != VERSION:
        raise SystemExit(f"Expected fonttools {VERSION}, found {fontTools.__version__}")
    source = (FONTS / SOURCE).read_bytes()
    if digest(source) != SOURCE_SHA256 or digest((FONTS / "inter/OFL.txt").read_bytes()) != LICENSE_SHA256:
        raise SystemExit("Pinned Inter source or license differs")
    provenance_path = FONTS / "provenance.json"
    provenance = json.loads(provenance_path.read_text(encoding="utf-8"))
    for index, (output_path, weight) in enumerate(OUTPUTS.items()):
        output = generate(source, weight)
        if output != generate(source, weight):
            raise SystemExit("Font instancing was not byte-stable")
        entry = {
            "path": output_path,
            "size": len(output),
            "sha256": digest(output),
            "derivedFrom": SOURCE,
            "sourceSha256": SOURCE_SHA256,
            "license": "inter/OFL.txt",
            "generator": "native/tools/export-ui-fonts.py",
            "fontToolsVersion": VERSION,
            "axes": {"opsz": 14, "wght": weight},
        }
        existing = next((file for file in provenance["files"] if file["path"] == output_path), None)
        if args.check:
            if not (FONTS / output_path).is_file() or (FONTS / output_path).read_bytes() != output or existing != entry:
                raise SystemExit(f"Generated {output_path} or provenance differs; run this script without --check")
        else:
            (FONTS / output_path).write_bytes(output)
            provenance["files"] = [file for file in provenance["files"] if file["path"] != output_path]
            provenance["files"].insert(1 + index, entry)
        print(f"{'Verified' if args.check else 'Generated'} {output_path}: {len(output)} bytes, sha256 {digest(output)}")
    if not args.check:
        provenance_path.write_text(json.dumps(provenance, indent=2) + "\n", encoding="utf-8", newline="\n")



if __name__ == "__main__":
    main()

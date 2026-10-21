"""Export the GPOS pair kerning of the bundled UI faces for the native text painter.

ImGui applies no kerning, while the original client (Chromium) applies each face's GPOS 'kern'
feature. The native painter (NativeUiPaint) adds these pair adjustments so text widths, wrapping
and letter positions match. Only pairs of printable ASCII plus a few punctuation marks the client
uses are exported, in font units.

Install the generator once (development only):
  python -m pip install --no-cache-dir --target native/artifacts/font-tools fonttools==4.60.1
Then run from any directory:
  python native/tools/export-ui-kerning.py
  python native/tools/export-ui-kerning.py --check
"""

import argparse
import hashlib
import json
from pathlib import Path
import sys

NATIVE = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(NATIVE / "artifacts/font-tools"))
try:
    import fontTools
    from fontTools.ttLib import TTFont
except ImportError as error:
    raise SystemExit("Install fonttools==4.60.1 using the command in this script's header.") from error

VERSION = "4.60.1"
FONTS = NATIVE / "content/fonts"
OUTPUT = FONTS / "kerning.json"
FACES = [
    "inter/Inter[opsz,wght].ttf",
    "inter/Inter-SemiBold.ttf",
    "inter/Inter-Bold.ttf",
    "inter/Inter-ExtraBold.ttf",
    "chakrapetch/ChakraPetch-SemiBold.ttf",
    "chakrapetch/ChakraPetch-Bold.ttf",
]
CHARACTERS = [chr(code) for code in range(32, 127)] + list("·’…•–—")


def pair_lookups(font):
    gpos = font["GPOS"].table
    indices = sorted({index for record in gpos.FeatureList.FeatureRecord if record.FeatureTag == "kern" for index in record.Feature.LookupListIndex})
    for index in indices:
        lookup = gpos.LookupList.Lookup[index]
        subtables = lookup.SubTable
        if lookup.LookupType == 9:
            subtables = [subtable.ExtSubTable for subtable in subtables if subtable.ExtensionLookupType == 2]
        elif lookup.LookupType != 2:
            continue
        yield subtables


def advance(value):
    return getattr(value, "XAdvance", 0) or 0 if value is not None else 0


def pair_value(subtables, first, second):
    """First subtable in the lookup that covers the pair decides it (OpenType lookup semantics)."""
    for subtable in subtables:
        coverage = subtable.Coverage.glyphs
        if first not in coverage:
            continue
        if subtable.Format == 1:
            pair_set = subtable.PairSet[coverage.index(first)]
            for record in pair_set.PairValueRecord:
                if record.SecondGlyph == second:
                    return advance(record.Value1)
            continue
        class1 = subtable.ClassDef1.classDefs.get(first, 0)
        class2 = subtable.ClassDef2.classDefs.get(second, 0)
        return advance(subtable.Class1Record[class1].Class2Record[class2].Value1)
    return 0


def export(path):
    font = TTFont(FONTS / path)
    cmap = font.getBestCmap()
    glyphs = {character: cmap[ord(character)] for character in CHARACTERS if ord(character) in cmap}
    lookups = list(pair_lookups(font))
    pairs = {}
    for a, first in glyphs.items():
        for b, second in glyphs.items():
            value = sum(pair_value(subtables, first, second) for subtables in lookups)
            if value:
                pairs[a + b] = value
    return {"unitsPerEm": font["head"].unitsPerEm, "pairs": dict(sorted(pairs.items()))}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check", action="store_true", help="Verify the committed table without changing files")
    args = parser.parse_args()
    if fontTools.__version__ != VERSION:
        raise SystemExit(f"Expected fonttools {VERSION}, found {fontTools.__version__}")
    table = {"schemaVersion": 1, "generator": "native/tools/export-ui-kerning.py", "fontToolsVersion": VERSION,
             "faces": {path: {"sourceSha256": hashlib.sha256((FONTS / path).read_bytes()).hexdigest(), **export(path)} for path in FACES}}
    text = json.dumps(table, ensure_ascii=False, separators=(",", ":")) + "\n"
    if args.check:
        if not OUTPUT.is_file() or OUTPUT.read_text(encoding="utf-8") != text:
            raise SystemExit("kerning.json differs; run this script without --check")
    else:
        OUTPUT.write_text(text, encoding="utf-8", newline="\n")
    print(f"{'Verified' if args.check else 'Generated'} {OUTPUT.name}: " + ", ".join(f"{Path(p).name} {len(f['pairs'])}" for p, f in table["faces"].items()))


if __name__ == "__main__":
    main()

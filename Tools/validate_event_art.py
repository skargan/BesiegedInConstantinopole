"""Read-only event-art preflight. Uses the production manifest; no asset rewriting."""
import argparse
import json
import math
from pathlib import Path
import struct
import sys

ROOT = Path(__file__).resolve().parents[1]
RESOURCES = ROOT / "Assets/Resources"


def validate(manifest):
    errors, notes, seen = [], [], set()
    try:
        data = json.loads(manifest.read_text(encoding="utf-8-sig"))
        entries = data["entries"]
        if not isinstance(entries, list) or not entries:
            raise ValueError("entries must be a nonempty list")
    except (OSError, ValueError, KeyError, TypeError) as exc:
        return [f"Cannot read manifest: {exc}"], [], 0
    for number, entry in enumerate(entries):
        if not isinstance(entry, dict):
            errors.append(f"Entry {number}: expected object")
            continue
        node = entry.get("node")
        label = str(node or number)
        if not isinstance(node, str) or not node.strip():
            errors.append(f"{label}: missing node ID")
        elif node in seen:
            errors.append(f"{label}: duplicate node ID")
        else:
            seen.add(node)
        if not isinstance(entry.get("source"), str) or not entry["source"].strip():
            errors.append(f"{label}: missing source provenance")
        if entry.get("fit", "contain") not in ("contain", "cover"):
            errors.append(f"{label}: fit must be contain or cover")
        focus = entry.get("focusY", .5)
        if isinstance(focus, bool) or not isinstance(focus, (int, float)) or not math.isfinite(focus) or not 0 <= focus <= 1:
            errors.append(f"{label}: focusY must be finite and between 0 and 1")
        path = entry.get("path")
        if not isinstance(path, str) or not path.strip():
            errors.append(f"{label}: missing resource path")
            continue
        asset = (RESOURCES / (path + ".png")).resolve()
        if not asset.is_relative_to(RESOURCES.resolve()):
            errors.append(f"{label}: resource path escapes Resources")
            continue
        try:
            with asset.open("rb") as image:
                header = image.read(33)
            if len(header) != 33 or header[:8] != b"\x89PNG\r\n\x1a\n" or header[12:16] != b"IHDR":
                raise ValueError("invalid PNG header")
            width, height = struct.unpack(">II", header[16:24])
            if not width or not height:
                raise ValueError("empty image dimensions")
            if not Path(str(asset) + ".meta").exists():
                errors.append(f"{label}: missing Unity meta")
            if max(width, height) > 4096:
                notes.append(f"{label}: {width}x{height}; review runtime memory/import cap")
            if width < 1024 or height < 576:
                notes.append(f"{label}: {width}x{height}; inspect enlargement quality")
            if abs(width / height - 16 / 9) > .08:
                notes.append(f"{label}: {width}x{height}; legacy/non-wide aspect accepted, inspect composition")
        except (OSError, ValueError) as exc:
            errors.append(f"{label}: {exc}")
    return errors, notes, len(entries)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--manifest", type=Path, default=RESOURCES / "Content/EventArtwork.json")
    parser.add_argument("--details", action="store_true")
    args = parser.parse_args()
    errors, notes, count = validate(args.manifest)
    print(f"Event artwork: {count} mappings; {len(errors)} errors; {len(notes)} advisory notes.")
    for error in errors:
        print("ERROR: " + error)
    if args.details:
        for note in notes:
            print("NOTE: " + note)
    sys.exit(1 if errors else 0)

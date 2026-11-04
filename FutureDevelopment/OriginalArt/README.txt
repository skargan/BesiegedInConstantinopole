BESIEGED — FINAL ORIGINAL ART HANDOVER

DELIVERY
121 final event-cover PNGs (60 earlier + 61 completion), 11 explicit reuse
mappings covering 132 event nodes, and all 14 original stat icon PNGs.
Missing original files: NONE.
All 135 PNG files preserve the original bytes. Only portable filenames changed.
18 corrected final covers replace 18 superseded drafts; drafts are excluded.

TARGET SOURCE
Repository: skargan/BesiegedInConstantinopole
Version: Version36.9
Commit: 28fa420ac58c222574c8f5b3d43cf871543ffb7a
All 132 covered event IDs were verified with exact case in this commit's active
Yarn sources. Each appears once. SOURCE_VERIFICATION.json records source hashes.

PACKAGE LAYOUT
event-covers/<event_node>.png — 121 originals, 1536 x 1024 RGB
stat-icons/<stat>.png — 14 originals, 1254 x 1254 RGBA
stat-icons/<stat>.png.meta — 14 existing Unity metadata files from PR5
Besieged_art_manifest.json — authoritative asset and event-node mapping
Besieged_art_coverage.csv — 146 rows: 121 direct nodes + 11 reuse nodes + 14 stats
SOURCE_VERIFICATION.json — current source audit and immutable provenance
SHA256SUMS.txt — checksums for every other file in this archive

INTEGRATION HANDOFF
Extract this folder and read event_node_to_asset_path from the JSON manifest.
Paths are relative to that manifest. Resolve IDs with their exact capitalization.
Reuse entries reference an existing cover and deliberately have no duplicate PNG.
Use original_image_file_name and sha256 to reconcile earlier Desktop copies.
The packaged stat icon PNGs and .meta files match PR5 commit
b8a3d858bb8b8867940b4a0bb12eebc3f5b8683d byte for byte.
The manifest records their existing repository paths and Unity GUIDs. If PR5's
icons are already present, these are the same assets, not an additional icon set.
Preserve existing GUIDs when importing the accompanying icon metadata.

SCOPE
This package contains art, original icon metadata and manifests only. It contains
no game source, C# helper, Yarn, scenes or prefabs. No assets are assigned in the
playable game by this handover; the local integration agent performs that step.
No image generation, image editing or game source changes were performed here.
Source compatibility checks are not a Unity compilation, rendering or playtest.
Seven other active-source nodes fall outside the specifically requested art set:
prepPass, quietDay, random18, story11, story2, story21, story24.
They are separately listed in the manifest, not counted as missing originals.

INTEGRITY
All originals and archived entries were checked against their SHA-256 values.
From this extracted folder on a system with sha256sum, use:
  sha256sum -c SHA256SUMS.txt

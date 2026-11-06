"""Rebuild the eagle frame atlas from the preserved user GIF. Requires Pillow."""
from pathlib import Path
from PIL import Image
import json
import hashlib

repo = Path(__file__).resolve().parents[1]
source = repo / 'ArtSources/NamelessFortress/Animated-Eagle-Flight-Loop.gif'
target = repo / 'Assets/Resources/UI/Maps/Landmarks'
image = Image.open(source)
width, height = image.size
atlas = Image.new('RGBA', (8 * width, ((image.n_frames + 7) // 8) * height))
frames = []
for index in range(image.n_frames):
    image.seek(index)
    frame = image.convert('RGBA')
    if not frame.getbbox():
        frame = previous.copy()  # Empty final source frame must not blink out.
    previous = frame.copy()
    x, y = index % 8 * width, index // 8 * height
    atlas.paste(frame, (x, y))
    left, top, right, bottom = frame.getbbox()
    left, top = max(0, left - 2), max(0, top - 2)
    right, bottom = min(width, right + 2), min(height, bottom + 2)
    frames.append(dict(x=x+left, y=y+top, width=right-left, height=bottom-top,
                       seconds=image.info.get('duration', 100)/1000))
atlas.save(target / 'nameless-eagle-frames.png')
data = dict(source='Animated Eagle Flight Loop.gif', sourceSha256=hashlib.sha256(source.read_bytes()).hexdigest(),
            atlasWidth=atlas.width, atlasHeight=atlas.height, frames=frames)
(target / 'nameless-eagle-frames.json').write_text(json.dumps(data, indent=2)+'\n', encoding='utf-8')
print(f'{len(frames)} frames, {sum(f["seconds"] for f in frames):.2f}s')

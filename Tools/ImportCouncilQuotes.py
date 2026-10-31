"""Import the user's selected DOCX verbatim; never recreate deleted anthology entries."""
import argparse
import hashlib
import json
import re
from pathlib import Path
from docx import Document


def extract(source):
    paragraphs = [p for p in Document(source).paragraphs if p.text.strip()]
    entries = []
    sources = []
    for index, paragraph in enumerate(paragraphs):
        match = re.match(r'^(Q\d{3})\s+(.+)$', paragraph.text, re.S)
        if match:
            attribution = paragraphs[index + 1]
            source_id = re.search(r'\[(S\d{2})\]', attribution.text)
            if not source_id:
                raise ValueError('Missing attribution for ' + match[1])
            parts = re.split(r'\s+·\s+', attribution.text)
            entries.append(dict(
                id=match[1], text=match[2], attribution=attribution.text,
                displayAttribution=re.sub(r'\s+\[S\d{2}\]', '', ' · '.join(parts[:2])),
                sourceId=source_id[1], labels=parts[2:],
                sourceUrls=list(dict.fromkeys(h.url for h in attribution.hyperlinks if h.url)),
            ))
        source_match = re.match(r'^(S\d{2})\s{2,}(.+)$', paragraph.text)
        if source_match:
            description = paragraphs[index + 1].text
            links = paragraphs[index + 2].hyperlinks
            sources.append(dict(id=source_match[1], title=source_match[2], notes=description,
                                urls=list(dict.fromkeys(h.url for h in links if h.url))))
    if len({e['id'] for e in entries}) != len(entries):
        raise ValueError('Duplicate quotation IDs')
    if len({e['text'] for e in entries}) != len(entries):
        raise ValueError('Duplicate quotation texts require explicit review')
    source_ids = {s['id'] for s in sources}
    if any(e['sourceId'] not in source_ids for e in entries):
        raise ValueError('Quotation without a source register entry')
    # The header still says 400; the edited body deliberately omits eight entries.
    omitted = sorted({f'Q{i:03}' for i in range(1, 401)} - {e['id'] for e in entries})
    digest = hashlib.sha256(Path(source).read_bytes()).hexdigest()
    return dict(schemaVersion=1, catalogueVersion='selected-2026-10-31-v1',
                sourceFile=Path(source).name, sourceSha256=digest, importedCount=len(entries),
                absentOriginalIds=omitted, quotes=entries, sources=sources)


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('source', type=Path)
    parser.add_argument('output', type=Path)
    args = parser.parse_args()
    data = extract(args.source)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(dict(imported=data['importedCount'], sources=len(data['sources']),
                         omitted=data['absentOriginalIds'], sourceSha256=data['sourceSha256']), indent=2))

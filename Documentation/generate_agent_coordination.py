"""Regenerate Word and text from editable content and original message JSON files."""
from pathlib import Path
import json
from docx import Document
from docx.shared import Inches, Pt, RGBColor
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
HERE=Path(__file__).resolve().parent
OUT=HERE.parent
c=json.loads((HERE/'agent_coordination_content.json').read_text(encoding='utf8'))
a=json.loads((HERE/'agent_coordination_messages.json').read_text(encoding='utf8'))
d=Document(); s=d.sections[0]
s.top_margin=s.bottom_margin=Inches(.62)
s.left_margin=s.right_margin=Inches(.7)
for name in ['Normal','Title','Heading 1','Heading 2']:
    style=d.styles[name];style.font.name='Calibri';style.font.color.rgb=RGBColor(0,0,0)
d.styles['Normal'].font.size=Pt(10)
d.styles['Normal'].paragraph_format.space_after=Pt(7)
d.styles['Heading 1'].font.size=Pt(16)
d.styles['Heading 2'].font.size=Pt(11)
d.styles['Title'].font.size=Pt(24)
for e in d.styles.element.xpath('//w:pBdr'):e.getparent().remove(e)
foot=s.footer.paragraphs[0];foot.alignment=2
foot.add_run('Besieged • Agent coordination | ').font.size=Pt(8)
field=OxmlElement('w:fldSimple');field.set(qn('w:instr'),'PAGE');foot._p.append(field)
text=[c['title'],c['subtitle'],'Snapshot: '+c['snapshot']]
d.add_paragraph(c['title'],'Title');d.add_paragraph(c['subtitle'])
d.add_paragraph('Current instructions and ownership appear first. Confirmed decisions and release evidence follow. The final archive preserves original messages; historical instructions there may have been superseded.')
for index,(title,paragraphs) in enumerate(c['sections']):
    heading=d.add_heading(title,1)
    if index:heading.paragraph_format.page_break_before=True
    text.extend(['',title,''])
    for para in paragraphs:d.add_paragraph(para);text.extend([para,''])
d.add_heading('VERSION CHANGELOG',1).paragraph_format.page_break_before=True
text.extend(['','VERSION CHANGELOG','',c['history_note'],''])
d.add_paragraph(c['history_note'])
for entry in c['changelog']:
    heading='Version '+entry['version']+' — '+entry['status']
    d.add_heading(heading,2);text.extend([heading,''])
    for field in ['changes','evidence','pending','publication']:
        value=field.capitalize()+': '+entry[field]
        d.add_paragraph(value);text.extend([value,''])
d.add_heading('INTERAGENT MESSAGE LOG',1).paragraph_format.page_break_before=True
messages=a['messages'];coverage=a['coverage']
intro=f"{len(messages)} recovered original messages, from {messages[0]['timestamp']} to {messages[-1]['timestamp']}. Times below are recorded transport times in UTC; a message may also state its sender’s local time. Historical workstream aliases are mapped to their current designations. Message bodies are verbatim, including original spelling, spacing and encoding defects."
notes=[intro,coverage['method'],coverage['gaps'],'The archive records attempted sends, not proof of acknowledgement. Source references use the original session filename, line and call ID. No unrelated private chat content is included. Earlier snapshot instructions and release claims do not override the current sections above.']
text.extend(['','INTERAGENT MESSAGE LOG',''])
for note in notes:d.add_paragraph(note);text.extend([note,''])
for i,m in enumerate(messages,1):
    heading=f"{i:03d} | {m['timestamp']} | {m['sender']} to {m['recipient']}"
    p=d.add_paragraph();p.paragraph_format.space_before=Pt(7);p.paragraph_format.space_after=Pt(3);p.paragraph_format.keep_with_next=True
    r=p.add_run(heading);r.bold=True;r.font.size=Pt(9.5)
    p=d.add_paragraph(m['text']);p.paragraph_format.space_after=Pt(3);p.paragraph_format.line_spacing=1
    for r in p.runs:r.font.size=Pt(9.5)
    refs=[f"{Path(ref['source']).name}:{ref['line']} | {ref['call_id']}" for ref in m['occurrences']]
    provenance='Source: '+'; '.join(refs)
    # Keep provenance with at least the final body line, without forcing an
    # entire long message onto one page or changing the original message body.
    p.paragraph_format.widow_control=True
    p.add_run().add_break()
    source_run=p.add_run(provenance)
    source_run.font.size=Pt(7.5);source_run.font.color.rgb=RGBColor(80,80,80)
    p.paragraph_format.space_after=Pt(5)
    text.extend([heading,m['text'],provenance,''])
d.save(OUT/'AGENT COORDINATION.docx')
(OUT/'AGENT COORDINATION.txt').write_text('\n'.join(text),encoding='utf8')
print(f'Generated guide and {len(messages)} verbatim messages')

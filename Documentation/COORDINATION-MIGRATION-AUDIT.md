# Coordination migration audit

Supporting evidence report; not a governing instruction document. 6 November2026.

## Coverage ledger

| Source section | Coverage | Disposition |
| --- | --- | --- |
| AGENT INSTRUCTIONS | All 16 paragraphs | AGENTS sections1–7; current sections2/4/8. Word ownership and every-message ceremony superseded. |
| ARCHITECTURE | All 11 paragraphs | Current section3, journal/status in5; detailed specs retained. |
| DECISIONS | All 25 paragraphs | Current7; supersessions in Archive2/3 and reconciliation below. |
| COORDINATION | All 43 paragraphs | Historical handovers indexed in Archive4/7; latest acknowledged ownership in current6/10. Earlier release/ownership snapshots historical. |

## Original evidence hashes

- `AGENTS.md` at `b1e12b1`: SHA-256 `8f1586f0a317800dcde631bb24d64443cf9055f173fef023f426af6c580b4284`.
- `AGENT COORDINATION.docx` at `b1e12b1`: SHA-256 `46bf5f767df6e952a24781aea3233c4e6f471c38a904612568a6182bed450fdb`.
  Word body inspected: 599151 characters; contains instructions, architecture, decisions and message-log headings. Original retained; not a separate current authority.
- `AGENT COORDINATION.txt` at `b1e12b1`: SHA-256 `9b83099429a0761eb52f997b141acd6b794dc55a0d34fce2d4170f0ea9f3733a`.
- `Documentation/agent_coordination_content.json` at `b1e12b1`: SHA-256 `2b06f54910eec154a14fa2e6af3a2defc03da59ce4575ab0178a0d9f7a891aae`.
- `Documentation/agent_coordination_messages.json` at `b1e12b1`: SHA-256 `54970dfc8a6ff9a09368ec5258464477c5695cf49525f12820087f30d42fb238`.
- `Documentation/AI_GAME_STUDIO.txt` at `b1e12b1`: SHA-256 `b6f6506d524c98f480b8d999bf5ecaffa831d6b589d2872cf9d866f3b927c1c9`.
- `Documentation/PROJECT_BRIEF.txt` at `b1e12b1`: SHA-256 `7ca28ede8e76bb7c735a8fc0ca9c5e67483ce8c21c32fb7dc81e6504924a665f`.
- `Documentation/ARCHITECTURE_MAP.txt` at `b1e12b1`: SHA-256 `aecc81832802878a8e277038660120bcfaf4f32eebfa316418977a7641888f91`.
- `Documentation/PROJECT_STATUS.txt` at `b1e12b1`: SHA-256 `40deb24a8ce2f9fa871f127683d6a9413aadcb2d46e385e99ab493af688e8649`.
- `Documentation/DECISIONS.txt` at `b1e12b1`: SHA-256 `1cd37e5e7dc96d9679b41453f93df97cc3b131df8340c23d567d79a3e1c1edbe`.
- `Documentation/Development priorities and design philosophy.txt` at `b1e12b1`: SHA-256 `5741d66fc6a18ace11768dc175cbff37be89ca0541aad0c8a3e94b95a9d100c8`.
- `Documentation/ART_BIBLE.txt` at `b1e12b1`: SHA-256 `823e894d26af23734e5f273b20084149b6c6322d373cfd23b2f4538168c3e6a4`.

## Preserved message capture coverage

```json
{
  "sources": [
    "C:\\Users\\Dainius\\.codex\\sessions\\2026\\09\\16\\rollout-2026-09-16T09-23-45-01a0a8e2-b837-7953-bbb3-acf52c000d0c.jsonl",
    "C:\\Users\\Dainius\\.codex\\sessions\\2026\\10\\21\\rollout-2026-10-21T14-55-01-01a15e50-9560-7403-9c1d-159fb220af65.jsonl"
  ],
  "method": "Original send_message_to_thread tool-call prompts, restricted to named workstream IDs. Verbatim message body; transport timestamps in UTC. Identical sender, recipient and text merged with every occurrence retained. Delivery acknowledgement is not inferred.",
  "gaps": "OrbanX original session is not available in these local archives. Incoming-only deliveries and nonliteral dynamic prompts are not guaranteed captured. Other conversations and internal subagents are outside this archive.",
  "unique_messages": 772,
  "duplicate_occurrences": 0,
  "unparsed_calls": 0
}
```

## Remaining limits

- Original OrbanX session unavailable; capture excludes other chats and internal subagents. Sending does not prove receipt.
- Longer approved character scripts live in the project court-review area outside this Git tree; do not claim the repository contains their full approved texts.
- Historical architecture documents contain dated debt/status claims. Current code takes precedence for existence, approved intent for desired behaviour. No full dependency or campaign-balance audit performed.
- Biševo envoy eligibility remains unanswered. Live Steam configuration and complete epilogue integration remain future work.
- Release evidence was inspected locally; public43 download was not repeated for this documentation-only task.
- No gameplay code, asset or build configuration changes belong to the migration commit.

## Executed migration validation — 6 November 2026

- All three root Markdown documents parse as UTF-8; 33 local Markdown references resolve.
- Both legacy utility scripts pass Python syntax parsing. Four guard checks pass: both reject missing explicit output, exporter rejects repository-root output, extractor rejects an existing evidence file.
- Explicit legacy reproduction succeeds into a temporary folder (Word and text); this checks reproduction, not page-by-page Word layout.
- Original Word bytes match the base; original text/JSON match after Git working-tree line-ending normalisation. No changes to these originals are staged. Recorded provenance hashes describe Git blobs, not CRLF checkout bytes.
- git diff --check passes. Commit scope is the three canonical files, this audit, and two legacy documentation utilities only. No gameplay, assets or build configuration modified.
- Public release and private-build tests are cited evidence, not newly rerun game tests.

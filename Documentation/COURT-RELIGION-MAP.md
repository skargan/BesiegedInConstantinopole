# Court-religion map correction — 6 November 2026

Owner: ConstantineX. Presentation-only change; no religion, diplomacy, acceptance or ownership mechanics changed.

- The former Foreign powers selector is named **Non-Ottoman Powers**.
- Catholic map colours now use the existing structured `CountryDiplomacy.Definitions[].Catholic` classification, rather than requiring a narrative label to equal `Catholic` exactly. The previous comparison incorrectly coloured `Catholic court` and `Catholic dynasty · Orthodox Ruthenian lands` grey.
- Gazaria is correctly classified through its existing diplomatic definition even though it has no entry in the older parallel narrative-profile array.
- Existing Orthodox/Muslim profiles remain the source for other recognised courts; fallback mappings cover contextual Savoy/Po lordships and Candar.
- Bosnia, Lithuania, Genoese Gazaria, Cyprus, Athens and the Ionian lordships display Catholic court colours. User explicitly confirmed Catholic Bosnia after the historical distinction was explained.
- Unclassified is retained as a defensive fallback for genuinely unmapped future content; it appears in the legend only if the current catalogue contains an unmapped court. It is not a new religion or a claim about historical uncertainty.

Historical check: Stjepan Tomaš ruled Bosnia in1453 and had adopted Catholicism. The Museum of the Bosnian Kingdom reproduces papal correspondence recognising his Catholic adherence in1445; the Croatian Encyclopaedia describes his1446 Catholic marriage and church patronage. Court religion does not imply a homogeneous population.

Sources:
- https://muzejbosanskogkraljevstva.ba/en/calendar-of-medieval-bosnia/
- https://www.enciklopedija.hr/clanak/stjepan-tomas

Focused runtime verification uses `-court-map-capture` to run the existing map suite without unrelated Ionian agreement checks. It verifies every current region has a classification, six requested courts render Catholic colour, legend/filter/navigation, and captures both1280×720/1600×900. Evidence: private preview Captures/CourtReligion. Public Version41 is unchanged pending publication.

Result: Windows preview export succeeded;146 map checks passed plus campaign initialisation. Both requested-resolution screenshots visually checked for the corrected palette; all96 current map regions classified. No publication in this pass.

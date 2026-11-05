# Mercenary implementation checkpoint

5 November 2026, ConstantineX. Active base-game system; supersedes the deferred-DLC status in the archived Malta proposal.

Authoritative definitions: MercenaryRoster.All (20 companies, editable price multipliers, packages, risk, ports and durations). Authoritative runtime state: iMerc_{company}_{field} keys in the existing campaign dictionary, serialized by existing saves. No parallel campaign simulation or external contract file. RestoreState clears iMerc fields before applying a loaded snapshot to prevent stale contracts surviving a rewind.

States: Unhired → Travelling → Decision/Payment → Arrived/Lost. Signing checks availability before spending. Incident roll, variant, date, delay and opposing ship loss are stored at signing; advancement uses no RNG. Handled flag and terminal-state guards ensure once-only consequences. Half-price balance remains independent of any surcharge. Decision and payment states stop movement until resolved. Calendar integration runs through CampaignPersonnelCalendar.Advance, including intervening preparation days; a preparation month aggregates its dispatches in the turn report.

UI: MercenaryUI, Malta landmark and Diplomacy menu entry. Fixed daily route progress in MercenaryMapMarkers; delays hold the marker rather than moving it backwards. Hover displays public contract details only. Incident selection stays hidden until its scheduled day. Unusual contract opportunities occupy at most one of the five monthly offers. Current special bargains: sanctuary and the politically gated Rhodian detachment.

Manpower: iMercForeignInfantry/Archers/Cavalry credits avoid charging imported regulars against domestic recruitment. Loss callbacks reduce credits proportionally while units remain merged. Incoming operational ship/cannon crews are included in city population, with additional available engineers added separately. No domestic training order is created.

Art: original OpenAI-generated mercenary-brokers.png (exec-6ff37295-348c-41aa-9e01-d5dd528dc1e6.png) and 5×4 captains.png atlas (exec-bfe35344-e1d1-468f-b2c9-f1106eac8d3c.png). Background prompt: realistic fifteenth-century Castrum Maris broker room overlooking harbour, contracts and Mediterranean soldiers, warm gold/deep blue, menu space on right. Atlas prompt: twenty distinct medieval captains in exact roster order, five columns/four rows, no captions, period equipment, navy ground. No third-party imagery copied. Visual inspection completed in the running broker screen.

Final verification: BuildRelease40.Build (achievement art exports + SaveGameTests + Windows player), -mercenary-capture: 275 checks passed. Evidence in Builds/Version-40/Captures/Mercenaries/checks.txt and screenshots. Initial testing caught an incorrect automated-save path in the harness, stale mercenary data on restore (fixed), and a portrait component conflict (fixed). No failed attempts presented as completed checks.

Future expansion: more non-cash bargains, a richer fate for defecting artillery/ships, bespoke incident artwork, grouping overlapping voyages, fully separate roster data asset, additional captain prose and balancing over long campaigns. No extra wages, faction debt, or permanent tactical formation subsystem added.

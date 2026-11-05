# Ragusa envoy presence and return route — 6 November 2026

Main Ragusa Return (including Escape/GoBack) now opens War Beyond the Walls. Subpages return to the Ragusan main page. A resident envoy is required to expose the current negotiator, their conversation target and private food/troop agreement controls. No envoy, outbound and returning states use the new empty WaitingTerrace painting and have no negotiator names or portrait hotspots. After arrival, only the current monthly representative is named and interactive; the original two-person paintings remain, with the inactive man serving as background company. The two simultaneous name buttons and double-favour display are removed. Hover speech is shown only while speaking, avoiding an empty panel at rest.

RagusaDiplomacy checks resident presence when RequestFood or StartContract is called, before payment, troop reservation or random rolls. Pending shipment delivery, contract resolution and survivor return do not depend on continued residence. Existing monthly limits, prices, cargo quantities, storm handling, troop losses and return dates remain unchanged. No additional retention explanation is added to player-facing text.

New WaitingTerrace.png: OpenAI image generation edit exec-1d66e679-48d2-4a4c-a121-8f2a701aa44d.png, based on existing GazulusForeground. Removes both people while retaining chair, chess table, loggia and sea view. Original paintings preserved.

Verification: Windows preview build succeeded; 169 focused checks passed at 1280x720 and 1600x900. All four envoy states and both monthly spokesmen checked for naming, target/control visibility, artwork, text fit and Return route. Non-resident calls reject without spending or RNG changes. Signed food and troop agreements survive recall and actual save/load; food delivers once, contract resolves and survivors return without a resident envoy. Updated DiplomaticExpansionCapture’s Ragusa fixture to establish a resident envoy; that broader suite was not rerun. Public Version 41 unchanged; ready next publication.

MarketMerchant.json and the daily closing line in CharacterChatter.cs were read for the user’s quote review and remain unchanged.

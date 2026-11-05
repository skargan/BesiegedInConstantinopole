# Version 41 integration checkpoint

Owner: ConstantineX; Theodoro isolated UI/art supplied by JohanX. Main is authoritative.

Authoritative state remains VariableSingleton dictionaries, CountryDiplomacy, SharedProvisions, TrainingPool and MercenaryContracts. No second diplomatic simulation was added. Candar and Ragusa use prefixed persisted keys. Gazaria is appended at index31; old country indices are unchanged. The Ottoman province `candar` remains Paphlagonia. Copa is a map-only encounter, with one extra contract definition at index20 excluded from normal rotating Malta offers.

Calendar hooks: CampaignPersonnelCalendar -> RagusaDiplomacy.Advance / CircassianOutpost.Advance. Candar delay is consumed by eligible Ottoman tactical execution only, after emergencies. RecruitmentLogistics reserves Ragusa's deployed troops; SharedProvisions excludes people abroad from city food demand. TrainingPool `expeditionReturn` carries survivors home without charging manpower twice.

Copa food deducted at departure, cargo saved separately, no origin damage/ownership teleport. Outcome rolled once per voyage; delivery marks consumed before payment. Existing contract saves handle Copa recruitment and arrival balance. Local failed negotiations consume the advance, not a camp agent or envoy.

Pending user decisions: Copa food quantity (provisional100/week); Theodoro numerical cooperation. Explicitly deferred: Bank takeover routes, Theodoro reincorporation/hiring house. Original staged designs remain recoverable; they are not runtime completion evidence.

Acceptance: final exported checks in Builds/Version-41/Captures/DiplomaticExpansion/checks.txt and Mercenaries/checks.txt. Visual checks at1280x720/1920x1080. Build log outside repository. Publication receipt in coordination changelog.

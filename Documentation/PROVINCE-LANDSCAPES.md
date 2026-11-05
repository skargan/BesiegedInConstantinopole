# Province landscapes — 5 November 2026

Eleven generated historical-fiction landscapes are installed in `Assets/Resources/UI/MidnightMosaic/Scenes/Provinces/`, keyed by existing province ID. ProvincialConflictUI overview and CouncilProvincialUI agent-network pages load them through SceneBackdrop. No province simulation, ownership, intelligence gating or battle illustration routing changed.

| Province ID | Landscape inspiration | Historical reference |
|---|---|---|
| thrace | Uzunkopru bridge, Ergene floodplain | https://whc.unesco.org/en/tentativelists/6042/ |
| bulgaria | Tsarevets and Yantra gorge | https://www.veliko-tarnovo.bg/bg/zabelezhitelnosti/arhitekturno-muzeen-rezervat-tsarevets/ |
| macedonia | Thessaloniki Byzantine walls, Thermaic Gulf | https://whc.unesco.org/en/list/456 |
| bithynia | Bursa citadel and Uludag | https://whc.unesco.org/en/list/1452/ |
| karasi | Pergamon acropolis and Bakircay plain | https://whc.unesco.org/en/list/1457 |
| aydin | Ayasuluk fortress and Ephesus plain | https://muze.gov.tr/muze-detay?DistId=MRK&SectionId=IAO01 |
| mentese | Kaunos/Dalyan rock tombs and lagoon | https://whc.unesco.org/en/tentativelists/5906 |
| germiyan | Kutahya castle and plateau | https://www.kulturportali.gov.tr/turkiye/kutahya/gezilecekyer/kutahya-kales |
| anatolia | Ankara citadel and dry uplands | https://ankara.gov.tr/ankara-kalesi |
| candar | Kastamonu castle and wooded valleys | https://kastamonu.ktb.gov.tr/TR-173067/kastamonu-kalesi-ve-saat-kulesi.html |
| armeniakon | Amasya Harsena fortress, Pontic tombs and Yesilirmak | https://whc.unesco.org/en/tentativelists/6039/ |

These are representative regional compositions, not measured reconstructions of exact fifteenth-century views. Fortress silhouettes, vegetation and settlements contain artistic interpretation. Avoid modern restorations, clock towers and later Ottoman monuments. Generated with OpenAI image generation; no third-party photographs copied. Original generated file references are in `province-art-files.json`.

Art uses the game's warm gold/deep blue palette, 16:9 composition, native PNG sources, no mipmaps and existing high-quality import settings. SceneBackdrop preserves aspect ratio with cover cropping. The inspection panel groups existing controls over the right side. All eleven original paintings visually reviewed. Runtime verification pending below.

Development check: `-province-landscape-capture`; output `Captures/ProvinceLandscapes`. Checks all eleven resource mappings and text overflow at 1280x720 and 1920x1080, plus return navigation; captures sample screens. This is presentation QA, not a rebalance test.

Verified completion: Unity TechnologyPreview build succeeded; final capture run reports 329 passing assertions. All eleven backgrounds and Return navigation checked at 1280x720 and 1920x1080. Active rebel army controls and revealed supply figures checked in a 1920x1080 fixture. Final Thrace army screenshot visually reviewed. Private preview only; publication remains separate.

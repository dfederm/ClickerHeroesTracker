CREATE TABLE [ComputedStats] (
    [UploadId]                         INT      NOT NULL,
    [TitanDamage]                      REAL     DEFAULT ((0)) NOT NULL,
    [SoulsSpent]                       REAL     DEFAULT ((0)) NOT NULL,
    [HeroSoulsSacrificed]              REAL     DEFAULT ((0)) NOT NULL,
    [TotalAncientSouls]                REAL     DEFAULT ((0)) NOT NULL,
    [TranscendentPower]                REAL     DEFAULT ((0)) NOT NULL,
    [Rubies]                           REAL     DEFAULT ((0)) NOT NULL,
    [HighestZoneThisTranscension]      REAL     DEFAULT ((0)) NOT NULL,
    [HighestZoneLifetime]              REAL     DEFAULT ((0)) NOT NULL,
    [AscensionsThisTranscension]       REAL     DEFAULT ((0)) NOT NULL,
    [AscensionsLifetime]               REAL     DEFAULT ((0)) NOT NULL,
    [MaxTranscendentPrimalReward]      REAL     DEFAULT ((0)) NOT NULL,
    [BossLevelToTranscendentPrimalCap] REAL     DEFAULT ((0)) NOT NULL
);

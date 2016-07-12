CREATE TABLE [dbo].[ComputedStats] (
    [UploadId]                         INT      NOT NULL,
    [OptimalLevel]                     SMALLINT DEFAULT ((0)) NOT NULL,
    [SoulsPerHour]                     BIGINT   DEFAULT ((0)) NOT NULL,
    [SoulsPerAscension]                BIGINT   DEFAULT ((0)) NOT NULL,
    [AscensionTime]                    SMALLINT DEFAULT ((0)) NOT NULL,
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
    [BossLevelToTranscendentPrimalCap] REAL     DEFAULT ((0)) NOT NULL,
    CONSTRAINT [FK_ComputedStats_ToUpload] FOREIGN KEY ([UploadId]) REFERENCES [dbo].[Uploads] ([Id])
);

CREATE CLUSTERED INDEX [UploadIdIndex]
    ON [dbo].[ComputedStats]([UploadId] ASC);

CREATE TABLE [dbo].[ComputedStats] (
    [UploadId]                         INT        NOT NULL,
    [TitanDamage]                      FLOAT (53) DEFAULT ((0)) NOT NULL,
    [SoulsSpent]                       FLOAT (53) DEFAULT ((0)) NOT NULL,
    [HeroSoulsSacrificed]              FLOAT (53) DEFAULT ((0)) NOT NULL,
    [TotalAncientSouls]                FLOAT (53) DEFAULT ((0)) NOT NULL,
    [TranscendentPower]                FLOAT (53) DEFAULT ((0)) NOT NULL,
    [Rubies]                           FLOAT (53) DEFAULT ((0)) NOT NULL,
    [HighestZoneThisTranscension]      FLOAT (53) DEFAULT ((0)) NOT NULL,
    [HighestZoneLifetime]              FLOAT (53) DEFAULT ((0)) NOT NULL,
    [AscensionsThisTranscension]       FLOAT (53) DEFAULT ((0)) NOT NULL,
    [AscensionsLifetime]               FLOAT (53) DEFAULT ((0)) NOT NULL,
    [MaxTranscendentPrimalReward]      FLOAT (53) DEFAULT ((0)) NOT NULL,
    [BossLevelToTranscendentPrimalCap] FLOAT (53) DEFAULT ((0)) NOT NULL,
    CONSTRAINT [FK_ComputedStats_ToUpload] FOREIGN KEY ([UploadId]) REFERENCES [dbo].[Uploads] ([Id])
);


GO
CREATE CLUSTERED INDEX [UploadIdIndex]
    ON [dbo].[ComputedStats]([UploadId] ASC);


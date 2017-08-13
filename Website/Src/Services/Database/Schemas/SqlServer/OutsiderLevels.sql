CREATE TABLE [dbo].[OutsiderLevels] (
    [OutsiderId] TINYINT NOT NULL,
    [UploadId]   INT     NOT NULL,
    [Level]      REAL    NOT NULL,
    CONSTRAINT [FK_OutsiderLevels_ToUploads] FOREIGN KEY ([UploadId]) REFERENCES [dbo].[Uploads] ([Id])
);

CREATE CLUSTERED INDEX [UploadIdIndex]
    ON [dbo].[OutsiderLevels]([UploadId] ASC);

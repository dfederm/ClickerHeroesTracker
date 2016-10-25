CREATE TABLE [dbo].[AncientLevels] (
    [AncientId] TINYINT    NOT NULL,
    [UploadId]  INT        NOT NULL,
    [Level]     FLOAT (53) NOT NULL,
    CONSTRAINT [FK_AncientLevels_ToUploads] FOREIGN KEY ([UploadId]) REFERENCES [dbo].[Uploads] ([Id])
);

CREATE CLUSTERED INDEX [UploadIdIndex]
    ON [dbo].[AncientLevels]([UploadId] ASC);

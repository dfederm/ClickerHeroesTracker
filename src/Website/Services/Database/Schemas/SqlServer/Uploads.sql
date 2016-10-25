CREATE TABLE [dbo].[Uploads] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [UserId]        NVARCHAR (450) NULL,
    [UploadTime]    DATETIME2 (0)  DEFAULT (getutcdate()) NOT NULL,
    [UploadContent] VARCHAR (MAX)  NOT NULL,
    [PlayStyle]     VARCHAR (128)  NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Uploads_ToUser] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);

CREATE NONCLUSTERED INDEX [UserIdIndex]
    ON [dbo].[Uploads]([UserId] ASC);

CREATE NONCLUSTERED INDEX [nci_wi_Uploads_B442C93DEDE59AC173EA]
    ON [dbo].[Uploads]([UserId] ASC, [UploadTime] ASC);

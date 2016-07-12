CREATE TABLE [dbo].[Rivals] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [UserId]      NVARCHAR (450) NOT NULL,
    [RivalUserId] NVARCHAR (450) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_UserId_ToUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_RivalUserId_ToUsers] FOREIGN KEY ([RivalUserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);

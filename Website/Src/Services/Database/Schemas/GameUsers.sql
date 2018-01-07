CREATE TABLE [dbo].[GameUsers] (
    [Id]           CHAR (16)      NOT NULL,
    [PasswordHash] CHAR (16)      NOT NULL,
    [UserId]       NVARCHAR (450) NOT NULL,
    PRIMARY KEY CLUSTERED ([UserId]),
    CONSTRAINT [FK_GameUsers_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);

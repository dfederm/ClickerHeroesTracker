CREATE TABLE [dbo].[GameUsers] (
    [Id]           NVARCHAR(450)      NOT NULL,
    [PasswordHash] NVARCHAR(450)      NOT NULL,
    [UserId]       NVARCHAR (450) NOT NULL,
    PRIMARY KEY CLUSTERED ([UserId] ASC),
    CONSTRAINT [FK_GameUsers_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);

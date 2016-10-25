CREATE TABLE [dbo].[UserSettings] (
    [UserId]       NVARCHAR (450) NOT NULL,
    [SettingId]    TINYINT        NOT NULL,
    [SettingValue] NVARCHAR (128) NOT NULL,
    CONSTRAINT [FK_UserSettings_ToUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);

CREATE CLUSTERED INDEX [UserIdIndex]
    ON [dbo].[UserSettings]([UserId] ASC);

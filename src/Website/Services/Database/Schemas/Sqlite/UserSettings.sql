CREATE TABLE [UserSettings] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [UserId]       NVARCHAR (128) NOT NULL,
    [SettingId]    TINYINT        NOT NULL,
    [SettingValue] NVARCHAR (128) NOT NULL
);

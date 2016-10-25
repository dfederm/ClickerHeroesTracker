CREATE TABLE [Uploads] (
    [Id]            INTEGER PRIMARY KEY AUTOINCREMENT,
    [UserId]        NVARCHAR (128) NULL,
    [UploadTime]    DATETIME2 (0)  DEFAULT (datetime('now','utc')) NOT NULL,
    [UploadContent] VARCHAR (8000) NOT NULL,
    [PlayStyle]     VARCHAR (128)  NOT NULL
);

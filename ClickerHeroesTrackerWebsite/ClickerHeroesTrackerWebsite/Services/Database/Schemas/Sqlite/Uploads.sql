CREATE TABLE [Uploads] (
    [Id]            INTEGER PRIMARY KEY AUTOINCREMENT, /* This should be INT IDENTITY (1, 1) NOT NULL, but Sqlite doesn't support that */
    [UserId]        NVARCHAR (128) NULL,
    [UploadTime]    DATETIME2 (0)  DEFAULT (datetime('now','utc')) NOT NULL, /* The default should be getutcdate(), but Sqlite doesn't support that */
    [UploadContent] VARCHAR (8000)  NOT NULL /* This should be VARCHAR(MAX) but sqlite doesn't support "max" */
);

CREATE TABLE [Uploads] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [UserId]        NVARCHAR (128) NULL,
    [UploadTime]    DATETIME2 (0)  NOT NULL,
    [UploadContent] VARCHAR (8000)  NOT NULL /* This should be VARCHAR(MAX) but sqlite doesn't support "max" */
);

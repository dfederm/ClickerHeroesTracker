CREATE TABLE [dbo].[Clans] (
    [Name]             NVARCHAR (450) NOT NULL,
    [CurrentRaidLevel] INT            NOT NULL,
    [ClanMasterId]     CHAR (16)      NOT NULL,
    PRIMARY KEY CLUSTERED ([Name] ASC)
);

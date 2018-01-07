CREATE TABLE [dbo].[Clans] (
    [Name]             NVARCHAR (450) NOT NULL,
    [CurrentRaidLevel] INT            NOT NULL,
    [ClanMasterId]     NVARCHAR (450)  NOT NULL,
    PRIMARY KEY CLUSTERED ([Name] ASC)
);

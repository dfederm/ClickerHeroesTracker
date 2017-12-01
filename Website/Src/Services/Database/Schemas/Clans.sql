CREATE TABLE [dbo].[Clans] (
    [Id]     INT	IDENTITY (1, 1) NOT NULL,
    [Name]       NVARCHAR (450) NOT NULL,
    [CurrentRaidLevel]    INT        NOT NULL,
    [MemberCount]    INT        NOT NULL
);

CREATE CLUSTERED INDEX [Id]
    ON [dbo].[Clans]([Id] ASC);

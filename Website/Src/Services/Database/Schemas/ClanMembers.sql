CREATE TABLE [dbo].[ClanMembers] (
    [Id]          NVARCHAR(32)      NOT NULL,
    [Nickname]    NVARCHAR (450) NOT NULL,
    [HighestZone] INT            NOT NULL,
    [ClanName]    NVARCHAR (450) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ClanMembers_Clans] FOREIGN KEY ([ClanName]) REFERENCES [dbo].[Clans] ([Name])
);


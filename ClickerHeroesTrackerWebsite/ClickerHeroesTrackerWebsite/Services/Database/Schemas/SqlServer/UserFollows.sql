CREATE TABLE [dbo].[UserFollows] (
    [UserId]       NVARCHAR (450) NOT NULL,
    [FollowUserId] NVARCHAR (450) NOT NULL,
    CONSTRAINT [FK_UserFollows_UserId_ToUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_UserFollows_FollowUserId_ToUsers] FOREIGN KEY ([FollowUserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);

CREATE CLUSTERED INDEX [UserIdIndex]
    ON [dbo].[UserFollows]([UserId] ASC);

CREATE NONCLUSTERED INDEX [FollowUserIdIndex]
    ON [dbo].[UserFollows]([FollowUserId] ASC);

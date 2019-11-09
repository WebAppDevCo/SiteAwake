CREATE TABLE [dbo].[SiteMetadata] (
    [Id]             BIGINT        IDENTITY (1, 1) NOT NULL,
    [AccountId]      BIGINT        NOT NULL,
    [Url]            VARCHAR (512) NOT NULL,
    [Interval]       SMALLINT      NOT NULL,
    [AlertsEnabled]  BIT           CONSTRAINT [DF_SiteMetadata_AlertsEnabled] DEFAULT ((0)) NOT NULL,
    [AlertSent]      BIT           CONSTRAINT [DF_SiteMetadata_AlertSent] DEFAULT ((0)) NOT NULL,
    [Processing]     BIT           CONSTRAINT [DF_SiteMetadata_Processing] DEFAULT ((0)) NOT NULL,
    [LastWakeUpCall] DATETIME      NULL,
    [Created]        DATETIME      NOT NULL,
    [Modified]       DATETIME      NULL,
    CONSTRAINT [PK_SiteMetadata] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_SiteMetadata_Account] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[Account] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);












GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_SiteMetadata]
    ON [dbo].[SiteMetadata]([Url] ASC);




GO



CREATE TABLE [dbo].[Communication] (
    [Id]                  BIGINT         IDENTITY (1, 1) NOT NULL,
    [SiteMetadataId]      BIGINT         NOT NULL,
    [Status]              NVARCHAR (256) NOT NULL,
    [Message]             NVARCHAR (512) NULL,
    [MillisecondsElapsed] BIGINT         NOT NULL,
    [WakeUpCall]          DATETIME       NOT NULL,
    [Created]             DATETIME       NOT NULL,
    CONSTRAINT [PK_Communication] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Communication_SiteMetadata] FOREIGN KEY ([SiteMetadataId]) REFERENCES [dbo].[SiteMetadata] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);










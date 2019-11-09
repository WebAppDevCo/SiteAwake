CREATE TABLE [dbo].[Account] (
    [Id]         BIGINT         IDENTITY (1, 1) NOT NULL,
    [Email]      NVARCHAR (256) NOT NULL,
    [Enabled]    BIT            CONSTRAINT [DF_Account_Enabled] DEFAULT ((0)) NOT NULL,
    [Subscribed] BIT            CONSTRAINT [DF_Account_Subscribed] DEFAULT ((0)) NOT NULL,
    [Verified]   BIT            CONSTRAINT [DF_Account_Verified] DEFAULT ((0)) NOT NULL,
    [Cancelled]  BIT            CONSTRAINT [DF_Account_Cancelled] DEFAULT ((0)) NOT NULL,
    [Created]    DATETIME       NOT NULL,
    [Modified]   DATETIME       NULL,
    CONSTRAINT [PK_Account] PRIMARY KEY CLUSTERED ([Id] ASC)
);


















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Account]
    ON [dbo].[Account]([Email] ASC);


CREATE TABLE [dbo].[Subscription] (
    [Id]                       BIGINT         IDENTITY (1, 1) NOT NULL,
    [AccountId]                BIGINT         NOT NULL,
    [TransactionId]            NVARCHAR (256) NOT NULL,
    [TransactionCode]          SMALLINT       NOT NULL,
    [TransactionStatus]        SMALLINT       NOT NULL,
    [AuthCode]                 NVARCHAR (64)  NOT NULL,
    [SubscriptionId]           NVARCHAR (256) NOT NULL,
    [SubscriptionStatus]       SMALLINT       NOT NULL,
    [CustomerProfileId]        NVARCHAR (64)  NOT NULL,
    [CustomerPaymentProfileId] NVARCHAR (64)  NOT NULL,
    [Created]                  DATETIME       NOT NULL,
    CONSTRAINT [PK_Subscription] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Subscription_Account] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[Account] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Subscription]
    ON [dbo].[Subscription]([AccountId] ASC);


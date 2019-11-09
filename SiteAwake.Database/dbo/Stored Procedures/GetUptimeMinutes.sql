-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
--	dbo.GetUptimeMinutes 513
CREATE PROCEDURE dbo.GetUptimeMinutes
	@SiteMetadataId bigint
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @Uptime int

    select max([Created]) Created, [Status]
	into #t_comm
	from [dbo].[Communication] with (nolock)
	where SiteMetadataId = @SiteMetadataId
	group by [SiteMetadataId], [Status]

	if exists (select * from #t_comm where [Status] = 'ERROR')
	BEGIN
		select @Uptime = ISNULL(DATEDIFF(MI, (select Created from #t_comm where [Status] = 'ERROR'), (select Created from #t_comm where [Status] = 'OK')), 0)
	END
	else
	BEGIN
		select @Uptime = ISNULL(DATEDIFF(MI, (select Created from dbo.SiteMetadata where Id = @SiteMetadataId), (select Created from #t_comm where [Status] = 'OK')), 0)
	END

	select CASE WHEN @Uptime < 0 THEN 0 ELSE @Uptime END as Uptime

	drop table #t_comm
END
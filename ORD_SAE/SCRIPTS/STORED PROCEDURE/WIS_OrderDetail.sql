USE [SAE70Empre01]
GO

/****** Object:  StoredProcedure [dbo].[WIS_OrderDetail]    Script Date: 26/10/2020 17:35:13 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[WIS_OrderDetail]
	-- Add the parameters for the stored procedure here
	@IdOrder VARCHAR(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT c.CVE_ART IdProd,
		   i.DESCR Description,
		   c.CANT Quantity,
		   c.PREC Price,
		   c.TOTIMP4 VAT,
		   c.DESCU Disscount,
		   c.TOT_PARTIDA TotalItem
	FROM PAR_COMPO01 c, INVE01 i 
	WHERE c.CVE_DOC=@IdOrder AND c.CVE_ART=i.CVE_ART
END
GO



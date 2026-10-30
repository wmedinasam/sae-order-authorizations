USE [SAE70Empre01]
GO

/****** Object:  StoredProcedure [dbo].[WIS_OrderHeader]    Script Date: 26/10/2020 17:35:20 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[WIS_OrderHeader]
	-- Add the parameters for the stored procedure here
	@IdOrder VARCHAR(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT c.CVE_DOC IdOrder,
		   p.NOMBRE Supplier,
		   c.CAN_TOT Total,
		   c.FECHA_DOC DateOrder
	FROM COMPO01 c, PROV01 p 
	WHERE c.CVE_DOC=@IdOrder AND c.CVE_CLPV=p.CLAVE
END
GO



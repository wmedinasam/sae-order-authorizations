USE [SAE70Empre01]
GO

/****** Object:  View [dbo].[WIS_viewLogView]    Script Date: 26/10/2020 17:33:12 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

--********************** VISTA LOG
CREATE VIEW [dbo].[WIS_viewLogView] as
SELECT 
	logTable.CorrelatorId, 
	logTable.LogDate as FechaRegistro, 
	logTable.IdOrder as [NoOrden], 
	logTable.IdSupplier as [Proveedor],
	logTable.Descr as Descripcion, 
	us.FULL_NAME AS [Usuario]
FROM [dbo].[WIS_WorkLog] logTable INNER JOIN
[dbo].[WIS_USER] us ON us.ID = logTable.UserId



GO



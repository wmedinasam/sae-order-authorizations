USE [SAE70Empre01]
GO

/****** Object:  View [dbo].[WIS_viewOrders]    Script Date: 26/10/2020 17:33:18 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[WIS_viewOrders] AS
SELECT
	ID, IdOrder, IdSupplier,p.NOMBRE as Vendor, OrdDate, Status, Total, obs.STR_OBS AS Observations
FROM [WIS_Orders] wo INNER JOIN
PROV01 p ON p.CLAVE = wo.IdSupplier INNER JOIN
COMPO01 o ON o.CVE_DOC = wo.IdOrder LEFT OUTER JOIN
OBS_DOCC01 obs ON obs.CVE_OBS = o.CVE_OBS



GO



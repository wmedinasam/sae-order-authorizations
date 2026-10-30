CREATE DATABASE ORDENES_COMPRA
GO
USE ORDENES_COMPRA
CREATE TABLE EMPRESA
(
Id Int identity(1,1),
Nombre varchar(50),
Direccion varchar(50),
DB_SAE varchar(50),
Sufijo varchar(10),
Estado bit,
primary key(Id)
)

CREATE TABLE ROL
(
Id int identity(1,1),
Nombre varchar(50),
Descripcion varchar(50),
EsAdministrador bit,
Estado bit,
primary key(Id)
)

CREATE TABLE ROL_EMPRESA
(
RolId int,
EmpresaId int
)

CREATE TABLE WIS_USER
(
ID int IDENTITY(1,1) NOT NULL,
FULL_NAME varchar(50) NULL,
NICK_NAME varchar(15) NULL,
PASSWORD varchar(50) NULL,
EMAIL varchar(50) NULL,
SEND_MAIL bit NULL,
ROL_ID int,
ACTIVATED_DATE datetime NULL,
USER_ACTIVATES int NULL,
Primary key(ID)
)

CREATE TABLE WIS_CONFIGURATION
(
ID int IDENTITY(1,1) NOT NULL,
[KEY] varchar(25) NULL,
VALUE varchar(255) NULL,
primary key(ID)
)

CREATE TABLE WIS_Orders(
ID int IDENTITY(1,1) NOT NULL,
EmpresaId int,
IdOrder varchar(20) NOT NULL,
IdSupplier varchar(10) NOT NULL,
OrdDate datetime NOT NULL,
Status varchar(1) NOT NULL,
Total float NOT NULL,
Primary key (ID)
) 

CREATE TABLE dbo.WIS_WorkLog(
Id int IDENTITY(1,1) NOT NULL,
EmpresaId int,
CorrelatorId varchar(250) NOT NULL,
LogDate datetime NOT NULL,
IdOrder varchar(25) NOT NULL,
Descr varchar(250) NOT NULL,
UserId varchar(50) NOT NULL,
IdSupplier nchar(10) NULL,
Primary key(Id)
)

INSERT INTO [dbo].[ROL]
           ([Nombre]
           ,[Descripcion]
           ,[EsAdministrador]
           ,[Estado])
     VALUES
           ('Admin'
           ,'Administrador'
           ,1
           ,1)

INSERT INTO [WIS_USER]
VALUES ('administrador','admin','0LUgYoMMkrK9hwpKJIMogA==','admin@admin.com',0,(SELECT TOP 1 Id FROM [ROL] WHERE Nombre = 'Admin'),GETDATE(),1)
INSERT INTO [dbo].[WIS_CONFIGURATION]
           ([KEY]
           ,[VALUE])
     VALUES
           ('MailFrom'
           ,'')
INSERT INTO [dbo].[WIS_CONFIGURATION]
           ([KEY]
           ,[VALUE])
     VALUES
           ('MailPassword'
           ,'')
INSERT INTO [dbo].[WIS_CONFIGURATION]
           ([KEY]
           ,[VALUE])
     VALUES
           ('MailSubject'
           ,'')
INSERT INTO [dbo].[WIS_CONFIGURATION]
           ([KEY]
           ,[VALUE])
     VALUES
           ('MailBody'
           ,'')

/****** Object:  StoredProcedure [dbo].[Sp_GetOrders]    Script Date: 22/06/2021 13:37:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Sp_GetOrders] 
@EmpresaId int,
@pDataBase AS VARCHAR(255),
@pSuffix AS VARCHAR(2)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
		EXEC('SELECT        wo.ID, wo.IdOrder, wo.IdSupplier, p.NOMBRE AS Vendor, wo.OrdDate, wo.Status, wo.Total, obs.STR_OBS AS Observations, e.Nombre AS Empresa
FROM            dbo.WIS_Orders AS wo INNER JOIN  
                         '+@pDataBase+'.dbo.PROV'+@pSuffix+' AS p ON p.CLAVE COLLATE DATABASE_DEFAULT = wo.IdSupplier INNER JOIN 
                         '+@pDataBase+'.dbo.COMPO'+@pSuffix+' AS o ON o.CVE_DOC COLLATE DATABASE_DEFAULT = wo.IdOrder LEFT OUTER JOIN
                         '+@pDataBase+'.dbo.OBS_DOCC'+@pSuffix+' AS obs ON obs.CVE_OBS = o.CVE_OBS
						 INNER JOIN EMPRESA e
						 ON wo.EmpresaId = e.Id
						 WHERE wo.EmpresaId = '''+@EmpresaId+'''')
END

/****** Object:  View [dbo].[WIS_viewLogView]    Script Date: 23/06/2021 22:26:45 ******/
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

/****** Object:  StoredProcedure [dbo].[Sp_OrderHeader]    Script Date: 23/06/2021 23:17:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Sp_OrderHeader] 
@pDataBase AS VARCHAR(255),
@pSuffix AS VARCHAR(2),
@IdOrder VARCHAR(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	EXEC('SELECT c.CVE_DOC IdOrder,
		   p.NOMBRE Supplier,
		   c.CAN_TOT Total,
		   c.FECHA_DOC DateOrder
	FROM '+@pDataBase+'.dbo.COMPO'+@pSuffix+' c, '+@pDataBase+'.dbo.PROV'+@pSuffix+' p 
	WHERE c.CVE_DOC='''+@IdOrder+''' AND c.CVE_CLPV=p.CLAVE')
END

/****** Object:  StoredProcedure [dbo].[Sp_OrderDetail]    Script Date: 23/06/2021 23:17:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Sp_OrderDetail] 
@pDataBase AS VARCHAR(255),
@pSuffix AS VARCHAR(2),
@IdOrder VARCHAR(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	EXEC('SELECT c.CVE_ART IdProd,
		   i.DESCR Description,
		   c.CANT Quantity,
		   c.PREC Price,
		   c.TOTIMP4 VAT,
		   c.DESCU Disscount,
		   c.TOT_PARTIDA TotalItem
	FROM '+@pDataBase+'.dbo.PAR_COMPO'+@pSuffix+' c, '+@pDataBase+'.dbo.INVE'+@pSuffix+' i 
	WHERE c.CVE_DOC='''+@IdOrder+''' AND c.CVE_ART=i.CVE_ART')
END
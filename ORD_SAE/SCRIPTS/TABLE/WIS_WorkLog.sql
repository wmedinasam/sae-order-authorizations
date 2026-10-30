USE [SAE70Empre01]
GO

/****** Object:  Table [dbo].[WIS_WorkLog]    Script Date: 26/10/2020 17:31:48 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[WIS_WorkLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CorrelatorId] [varchar](250) NOT NULL,
	[LogDate] [datetime] NOT NULL,
	[IdOrder] [varchar](25) NOT NULL,
	[Descr] [varchar](250) NOT NULL,
	[UserId] [varchar](50) NOT NULL,
	[IdSupplier] [nchar](10) NULL
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO



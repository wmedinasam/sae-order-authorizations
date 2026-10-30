USE [SAE70Empre01]
GO

/****** Object:  Table [dbo].[WIS_Orders]    Script Date: 26/10/2020 17:31:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[WIS_Orders](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[IdOrder] [varchar](20) NOT NULL,
	[IdSupplier] [varchar](10) NOT NULL,
	[OrdDate] [datetime] NOT NULL,
	[Status] [varchar](1) NOT NULL,
	[Total] [float] NOT NULL
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO



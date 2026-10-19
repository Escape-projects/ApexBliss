USE [ComputerClub]
GO
/****** Объект:  Table [dbo].[Clients]    Дата создания скрипта: 19.10.2026 21:30:00 ******/ 
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Clients](
	[client_id] [int] IDENTITY(1,1) NOT NULL,
	[client_name] [varchar](50) NOT NULL,
	[client_status] [varchar](20) NOT NULL,
 CONSTRAINT [PK_Clients] PRIMARY KEY CLUSTERED 
(
	[client_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Объект:  Table [dbo].[Computers]    Дата создания скрипта: 19.10.2026 21:30:00 ******/ 
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Computers](
	[computer_id] [int] IDENTITY(1,1) NOT NULL,
	[computer_name] [varchar](50) NOT NULL,
	[computer_status] [varchar](10) NOT NULL,
 CONSTRAINT [PK_Computers] PRIMARY KEY CLUSTERED 
(
	[computer_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Объект:  Table [dbo].[Sessions]    Дата создания скрипта: 19.10.2026 21:30:00 ******/ 
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Sessions](
	[session_id] [int] IDENTITY(1,1) NOT NULL,
	[computer_id] [int] NOT NULL,
	[client_id] [int] NOT NULL,
	[session_started_at] [datetimeoffset](7) NOT NULL,
	[session_ended_at] [datetimeoffset](7) NULL,
 CONSTRAINT [PK_Sessions] PRIMARY KEY CLUSTERED 
(
	[session_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Clients] ADD  CONSTRAINT [DF_Clients_status]  DEFAULT ('не за пк') FOR [client_status]
GO
ALTER TABLE [dbo].[Computers] ADD  CONSTRAINT [DF_Computers_status]  DEFAULT ('свободен') FOR [computer_status]
GO
ALTER TABLE [dbo].[Sessions] ADD  CONSTRAINT [DF_Sessions_started_at]  DEFAULT (getdate()) FOR [session_started_at]
GO
ALTER TABLE [dbo].[Sessions] ADD  CONSTRAINT [DF_Sessions_ended_at]  DEFAULT (NULL) FOR [session_ended_at]
GO
ALTER TABLE [dbo].[Sessions]  WITH CHECK ADD  CONSTRAINT [FK_Sessions_Clients] FOREIGN KEY([client_id])
REFERENCES [dbo].[Clients] ([client_id])
GO
ALTER TABLE [dbo].[Sessions] CHECK CONSTRAINT [FK_Sessions_Clients]
GO
ALTER TABLE [dbo].[Sessions]  WITH CHECK ADD  CONSTRAINT [FK_Sessions_Computers] FOREIGN KEY([computer_id])
REFERENCES [dbo].[Computers] ([computer_id])
GO
ALTER TABLE [dbo].[Sessions] CHECK CONSTRAINT [FK_Sessions_Computers]
GO

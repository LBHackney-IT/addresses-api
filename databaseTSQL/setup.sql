CREATE DATABASE StubAdd;
GO

USE StubAdd;
GO

/****** Object:  Table [dbo].[hackney_address]    Script Date: 04/04/2019 12:32:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[hackney_address](
	[lpi_key] [varchar](14) NULL,
	[lpi_logical_status] [varchar](18) NULL,
	[lpi_start_date] [int] NOT NULL,
	[lpi_end_date] [int] NULL,
	[lpi_last_update_date] [int] NOT NULL,
	[usrn] [int] NOT NULL,
	[uprn] [float] NOT NULL,
	[parent_uprn] [float] NULL,
	[blpu_start_date] [int] NULL,
	[blpu_end_date] [int] NULL,
	[blpu_state] [smallint] NULL,
	[blpu_state_date] [int] NULL,
	[blpu_class] [varchar](4) NULL,
	[usage_description] [varchar](160) NULL,
	[usage_primary] [varchar](160) NULL,
	[property_shell] [bit] NULL,
	[easting] [numeric](12, 4) NULL,
	[northing] [numeric](12, 4) NULL,
	[organisation] [nvarchar](100) NULL,
	[sao_text] [nvarchar](90) NULL,
	[unit_number] [nvarchar](17) NULL,
	[lpi_level] [nvarchar](30) NULL,
	[pao_text] [nvarchar](90) NULL,
	[building_number] [nvarchar](17) NULL,
	[street_description] [nvarchar](100) NOT NULL,
	[locality] [nvarchar](35) NULL,
	[ward] [nvarchar](100) NULL,
	[town] [nvarchar](30) NULL,
	[county] [nvarchar](30) NULL,
	[postcode] [varchar](8) NULL,
	[postcode_nospace] [varchar](8) NULL,
	[planning_use_class] [varchar](50) NULL,
	[neverexport] [bit] NOT NULL,
	[longitude] [float] NULL,
	[latitude] [float] NULL,
	[gazetteer] [varchar](5) NOT NULL,
	[line1] [nvarchar](90) NULL,
	[line2] [nvarchar](120) NULL,
	[line3] [nvarchar](120) NULL,
	[line4] [nvarchar](30) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[national_address]    Script Date: 04/04/2019 12:32:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[national_address](
	[lpi_key] [varchar](14) NULL,
	[lpi_logical_status] [varchar](18) NULL,
	[lpi_start_date] [int] NOT NULL,
	[lpi_end_date] [int] NULL,
	[lpi_last_update_date] [int] NOT NULL,
	[usrn] [int] NOT NULL,
	[uprn] [float] NOT NULL,
	[parent_uprn] [float] NULL,
	[blpu_start_date] [int] NOT NULL,
	[blpu_end_date] [int] NULL,
	[blpu_state] [smallint] NULL,
	[blpu_state_date] [int] NOT NULL,
	[blpu_class] [varchar](6) NOT NULL,
	[usage_description] [varchar](1006) NOT NULL,
	[usage_primary] [varchar](250) NOT NULL,
	[property_shell] [bit] NULL,
	[easting] [numeric](12, 4) NULL,
	[northing] [numeric](12, 4) NULL,
	[organisation] [nvarchar](100) NULL,
	[sao_text] [nvarchar](90) NULL,
	[unit_number] [nvarchar](17) NULL,
	[lpi_level] [nvarchar](30) NULL,
	[pao_text] [nvarchar](90) NULL,
	[building_number] [nvarchar](17) NULL,
	[street_description] [nvarchar](100) NOT NULL,
	[locality] [varchar](100) NULL,
	[ward] [varchar](100) NULL,
	[town] [varchar](100) NULL,
	[county] [varchar](100) NULL,
	[postcode] [varchar](8) NULL,
	[postcode_nospace] [varchar](8) NULL,
	[planning_use_class] [varchar](50) NULL,
	[neverexport] [bit] NOT NULL,
	[longitude] [numeric](12, 9) NULL,
	[latitude] [numeric](12, 9) NULL,
	[gazetteer] [varchar](8) NOT NULL,
	[line1] [varchar](max) NULL,
	[line2] [varchar](max) NULL,
	[line3] [varchar](max) NULL,
	[line4] [varchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[hackney_xref]    Script Date: 04/04/2019 12:32:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[hackney_xref](
	[xref_key] [varchar](14) NULL,
	[uprn] [float] NOT NULL,
	[xref_code] [nvarchar](6) NULL,
	[xref_name] [nvarchar](100) NULL,
	[xref_value] [nvarchar](50) NULL,
	[xref_end_date] [date] NULL
) ON [PRIMARY]
GO
create view [dbo].[combined_address] as  SELECT * FROM dbo.hackney_address   UNION ALL  SELECT * FROM dbo.national_address   ; 
GO
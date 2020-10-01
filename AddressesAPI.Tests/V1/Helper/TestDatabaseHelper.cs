using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace AddressesAPI.Tests.V1.Helper
{
    public static class TestDataHelper
    {
        public static void InsertAddress(string key, SqlConnection db)
        {
            //var commandText = "insert into [dbo].[hackney_address] ([LPI_KEY], [LPI_LOGICAL_STATUS], [LPI_OFFICIAL_FLAG], [LPI_START_DATE], [LPI_END_DATE], [LPI_LAST_UPDATE_DATE], [USRN], [UPRN], [PARENT_UPRN], [BLPU_START_DATE], [BLPU_END_DATE], [BLPU_STATE], [BLPU_STATE_DATE], [BLPU_CLASS], [USAGE_DESCRIPTION], [USAGE_PRIMARY], [PROPERTY_SHELL], [EASTING], [NORTHING], [RPA], [ORGANISATION], [SAO_TEXT], [UNIT_NUMBER], [LPI_LEVEL], [PAO_TEXT], [BUILDING_NUMBER], [STREET_DESCRIPTION], [STREET_ADMIN], [LOCALITY], [WARD], [POSTALLY_ADDRESSABLE], [NEVEREXPORT], [TOWN], [POSTCODE], [POSTCODE_NOSPACE], [LONGITUDE], [LATITUDE], [GAZETTEER]) ";
            //commandText += " VALUES(@LPI_KEY,@LPI_LOGICAL_STATUS,@LPI_OFFICIAL_FLAG,@LPI_START_DATE,@LPI_END_DATE,@LPI_LAST_UPDATE_DATE,@USRN,@UPRN,@PARENT_UPRN,@BLPU_START_DATE,@BLPU_END_DATE,@BLPU_STATE,@BLPU_STATE_DATE,@BLPU_CLASS,@USAGE_DESCRIPTION,@USAGE_PRIMARY,@PROPERTY_SHELL,@EASTING,@NORTHING,@RPA,@ORGANISATION,@SAO_TEXT,@UNIT_NUMBER,@LPI_LEVEL,@PAO_TEXT,@BUILDING_NUMBER,@STREET_DESCRIPTION,@STREET_ADMIN,@LOCALITY,@WARD,@POSTALLY_ADDRESSABLE,@NEVEREXPORT,@TOWN,@POSTCODE,@POSTCODE_NOSPACE,@LONGITUDE,@LATITUDE,@GAZETTEER);";

            var commandText = "INSERT [dbo].[hackney_address] ([lpi_key], [lpi_logical_status], [lpi_start_date], [lpi_end_date], [lpi_last_update_date], [usrn], [uprn], [parent_uprn], [blpu_start_date], [blpu_end_date], [blpu_state], [blpu_state_date], [blpu_class], [usage_description], [usage_primary], [property_shell], [easting], [northing], [organisation], [sao_text], [unit_number], [lpi_level], [pao_text], [building_number], [street_description], [locality], [ward], [town], [county], [postcode], [postcode_nospace], [planning_use_class], [neverexport], [longitude], [latitude], [gazetteer], [line1], [line2], [line3], [line4]) VALUES (@LPI_KEY, N'Historical', 20060214, 20060824, 20080623, 20900579, 10008227619, 100023650149, 20060214, 20090821, NULL, 0, N'RD', N'Residential, Dwellings', N'Residential', 0, CAST(533580.0000 AS Numeric(12, 4)), CAST(184945.0000 AS Numeric(12, 4)), NULL, NULL, N'37-38', NULL, N'DALSTON CROSS SHOPPING CENTRE', N'64', N'KINGSLAND HIGH STREET', N'HACKNEY', N'DALSTON WARD', N'LONDON', N'HACKNEY', N'E8 2LX', N'E82LX', NULL, 0, -0.074904309495184729, 51.54759585320371, N'LOCAL', N'37-38 DALSTON CROSS SHOPPING CENTRE', N'64 KINGSLAND HIGH STREET', N'LONDON', N'')";

            var command = new SqlCommand(commandText, db);

            command.Parameters.Add("@LPI_KEY", SqlDbType.VarChar);
            command.Parameters["@LPI_KEY"].Value = key;

            command.ExecuteNonQuery();
            command.Dispose();
        }

        internal static void DeleteCrossRef(int uprn, SqlConnection db)
        {
            var commandText = "delete from [dbo].[hackney_xref] WHERE uprn = @uprn ";
            var command = new SqlCommand(commandText, db);
            command.Parameters.Add("@uprn", SqlDbType.VarChar);
            command.Parameters["@uprn"].Value = uprn;
            command.ExecuteNonQuery();
            command.Dispose();
        }

        internal static void DeleteAddress(string key, SqlConnection db)
        {
            var commandText = "delete from [dbo].[hackney_address] WHERE LPI_KEY = @LPI_KEY ";
            var command = new SqlCommand(commandText, db);
            command.Parameters.Add("@LPI_KEY", SqlDbType.VarChar);
            command.Parameters["@LPI_KEY"].Value = key;
            command.ExecuteNonQuery();
        }

        public static void InsertCrossRef(int uprn, SqlConnection db)
        {
            var commandText = "insert into [dbo].[hackney_xref] ([xref_key],[uprn],[xref_code],[xref_name],[xref_value],[xref_end_date]) ";
            commandText += " VALUES(@XREF_KEY,@UPRN,@XREF_CODE,@XREF_NAME,@XREF_VALUE,@XREF_END_DATE);";

            var command = new SqlCommand(commandText, db);

            command.Parameters.Add("@XREF_KEY", SqlDbType.VarChar);
            command.Parameters["@XREF_KEY"].Value = "5360X123456789";
            command.Parameters.Add("@UPRN", SqlDbType.Float);
            command.Parameters["@UPRN"].Value = uprn;
            command.Parameters.Add("@XREF_CODE", SqlDbType.VarChar);
            command.Parameters["@XREF_CODE"].Value = "5360CO";
            command.Parameters.Add("@XREF_NAME", SqlDbType.VarChar);
            command.Parameters["@XREF_NAME"].Value = "BX COMPANIES HOUSE";
            command.Parameters.Add("@XREF_VALUE", SqlDbType.VarChar);
            command.Parameters["@XREF_VALUE"].Value = "* (Y) 1CR";
            command.Parameters.Add("@XREF_END_DATE", SqlDbType.DateTime);
            command.Parameters["@XREF_END_DATE"].Value = DateTime.Now;
            command.ExecuteNonQuery();
        }

    }
}

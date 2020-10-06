using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.Data.SqlClient;

namespace AddressesAPI.Tests.V1.Helper
{
    public static class TestDataHelper
    {
        public static void InsertAddress(string key, SqlConnection db, DatabaseAddressRecord request = null, bool localOnly = true)
        {
            var database = localOnly ? "hackney_address" : "national_address";
            var commandText = $"INSERT [dbo].[{database}] ([lpi_key], [lpi_logical_status], [lpi_start_date], " +
                                       "[lpi_end_date], [lpi_last_update_date], [blpu_last_update_date], [usrn], [uprn], [parent_uprn], [blpu_start_date], " +
                                       "[blpu_end_date], [blpu_state], [blpu_state_date], [blpu_class], [usage_description], " +
                                       "[usage_primary], [property_shell], [easting], [northing], [organisation], [sao_text], " +
                                       "[unit_number], [lpi_level], [pao_text], [building_number], [street_description], [locality], " +
                                       "[ward], [town], [county], [postcode], [postcode_nospace], [planning_use_class], [neverexport], " +
                                       "[longitude], [latitude], [gazetteer], [line1], [line2], [line3], [line4]) " +
                                       "VALUES (@LPI_KEY, N'Historical', 20060214, 20060824, 20080623, 20080623, @USRN, @UPRN, 100023650149, " +
                                       "20060214, 20090821, NULL, 0, N'RD', N'Residential, Dwellings', N'Residential', 0, " +
                                       "CAST(533580.0000 AS Numeric(12, 4)), CAST(184945.0000 AS Numeric(12, 4)), NULL, NULL, N'37-38', " +
                                       "NULL, N'DALSTON CROSS SHOPPING CENTRE', N'64', N'KINGSLAND HIGH STREET', N'HACKNEY', " +
                                       "N'DALSTON WARD', @TOWN, N'HACKNEY', @POSTCODE, @POSTCODE_NO_SPACE, NULL, 0, " +
                                       "-0.074904309495184729, 51.54759585320371, N'LOCAL', @LINE_1, " +
                                       "@LINE_2, @LINE_3, @LINE_4)";

            var command = new SqlCommand(commandText, db);

            command.Parameters.AddWithValue("@LPI_KEY", key);
            command.Parameters.AddWithValue("@UPRN", request?.uprn ?? 10008227619);
            command.Parameters.AddWithValue("@USRN", request?.usrn ?? 20900579);
            command.Parameters.AddWithValue("@LINE_1", request?.line1 ?? "37-38 DALSTON CROSS SHOPPING CENTRE");
            command.Parameters.AddWithValue("@LINE_2", request?.line2 ?? "64 KINGSLAND HIGH STREET");
            command.Parameters.AddWithValue("@LINE_3", request?.line3 ?? "LONDON");
            command.Parameters.AddWithValue("@LINE_4", request?.line4 ?? "");
            command.Parameters.AddWithValue("@TOWN", request?.town ?? "LONDON");
            command.Parameters.AddWithValue("@POSTCODE", request?.postcode ?? "E8 2LX");
            command.Parameters.AddWithValue("@POSTCODE_NO_SPACE", request?.postcode?.Replace(" ", "") ?? "E82LX");

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
            command.Dispose();
        }

        public static void InsertCrossRef(int uprn, SqlConnection db, DatabaseCrossRefAddressRecord request = null)
        {
            var commandText = "insert into [dbo].[hackney_xref] ([xref_key],[uprn],[xref_code],[xref_name],[xref_value],[xref_end_date])";
            commandText += " VALUES(@XREF_KEY,@UPRN,@XREF_CODE,@XREF_NAME,@XREF_VALUE,@XREF_END_DATE);";

            var command = new SqlCommand(commandText, db);

            command.Parameters.AddWithValue("@XREF_KEY", request?.CrossRefKey ?? "5360X123456789");
            command.Parameters.AddWithValue("@UPRN", uprn);
            command.Parameters.AddWithValue("@XREF_CODE", request?.Code ?? "5360CO");
            command.Parameters.AddWithValue("@XREF_NAME", request?.Name ?? "BX COMPANIES HOUSE");
            command.Parameters.AddWithValue("@XREF_VALUE", request?.Value ?? "* (Y) 1CR");
            command.Parameters.AddWithValue("@XREF_END_DATE", request?.EndDate ?? DateTime.Now);

            command.ExecuteNonQuery();
            command.Dispose();
        }
    }
    public class DatabaseAddressRecord
    {
        [MaxLength(14)]
        public string lpi_key { get; set; }
        [MaxLength(14)]
        public string lpi_logical_status { get; set; }
        public int lpi_start_date { get; set; }
        public int? lpi_end_date { get; set; }
        public int lpi_last_update_date { get; set; }
        public int usrn { get; set; }
        public long uprn { get; set; }
        public float? parent_uprn { get; set; }
        public int? blpu_start_date { get; set; }
        public int? blpu_end_date { get; set; }
        public short? blpu_state { get; set; }
        public int? blpu_state_date { get; set; }
        [MaxLength(4)]
        public string blpu_class { get; set; }
        [MaxLength(160)]
        public string usage_description { get; set; }
        [MaxLength(250)]
        public string usage_primary { get; set; }
        public bool property_shell { get; set; }
        public decimal easting { get; set; }
        public decimal northing { get; set; }
        [MaxLength(100)]
        public string organisation { get; set; }
        [MaxLength(90)]
        public string sao_text { get; set; }
        [MaxLength(17)]
        public string unit_number { get; set; }
        [MaxLength(30)]
        public string lpi_level { get; set; }
        [MaxLength(90)]
        public string pao_text { get; set; }
        [MaxLength(17)]
        public string building_number { get; set; }
        [MaxLength(100)]
        public string street_description { get; set; }
        [MaxLength(35)]
        public string locality { get; set; }
        [MaxLength(100)]
        public string ward { get; set; }
        [MaxLength(30)]
        public string town { get; set; }
        [MaxLength(30)]
        public string county { get; set; }
        [MaxLength(8)]
        public string postcode { get; set; }
        [MaxLength(8)]
        public string postcode_nospace { get; set; }
        [MaxLength(50)]
        public string planning_use_class { get; set; }
        public bool neverexport { get; set; }
        public float longitude { get; set; }
        public float latitude { get; set; }
        [MaxLength(5)]
        public string gazetteer { get; set; }
        [MaxLength(90)]
        public string line1 { get; set; }
        [MaxLength(120)]
        public string line2 { get; set; }
        [MaxLength(120)]
        public string line3 { get; set; }
        [MaxLength(30)]
        public string line4 { get; set; }
        public short? paon_start_num { get; set; }
    }

    public class DatabaseCrossRefAddressRecord
    {
        [MaxLength(14)]
        public string CrossRefKey { get; set; }
        public long UPRN { get; set; }

        [MaxLength(6)]
        public string Code { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(50)]
        public string Value { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

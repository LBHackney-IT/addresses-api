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
            command.Parameters.AddWithValue("@UPRN", request?.Uprn ?? 10008227619);
            command.Parameters.AddWithValue("@USRN", request?.Usrn ?? 20900579);
            command.Parameters.AddWithValue("@LINE_1", request?.Line1 ?? "37-38 DALSTON CROSS SHOPPING CENTRE");
            command.Parameters.AddWithValue("@LINE_2", request?.Line2 ?? "64 KINGSLAND HIGH STREET");
            command.Parameters.AddWithValue("@LINE_3", request?.Line3 ?? "LONDON");
            command.Parameters.AddWithValue("@LINE_4", request?.Line4 ?? "");
            command.Parameters.AddWithValue("@TOWN", request?.Town ?? "LONDON");
            command.Parameters.AddWithValue("@POSTCODE", request?.Postcode ?? "E8 2LX");
            command.Parameters.AddWithValue("@POSTCODE_NO_SPACE", request?.Postcode?.Replace(" ", "") ?? "E82LX");

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
        public string Lpi_key { get; set; }
        [MaxLength(14)]
        public string Lpi_logical_status { get; set; }
        public int Lpi_start_date { get; set; }
        public int? Lpi_end_date { get; set; }
        public int Lpi_last_update_date { get; set; }
        public int Usrn { get; set; }
        public long Uprn { get; set; }
        public float? Parent_uprn { get; set; }
        public int? Blpu_start_date { get; set; }
        public int? Blpu_end_date { get; set; }
        public short? Blpu_state { get; set; }
        public int? Blpu_state_date { get; set; }
        [MaxLength(4)]
        public string Blpu_class { get; set; }
        [MaxLength(160)]
        public string Usage_description { get; set; }
        [MaxLength(250)]
        public string Usage_primary { get; set; }
        public bool Property_shell { get; set; }
        public decimal Easting { get; set; }
        public decimal Northing { get; set; }
        [MaxLength(100)]
        public string Organisation { get; set; }
        [MaxLength(90)]
        public string Sao_text { get; set; }
        [MaxLength(17)]
        public string Unit_number { get; set; }
        [MaxLength(30)]
        public string Lpi_level { get; set; }
        [MaxLength(90)]
        public string Pao_text { get; set; }
        [MaxLength(17)]
        public string Building_number { get; set; }
        [MaxLength(100)]
        public string Street_description { get; set; }
        [MaxLength(35)]
        public string Locality { get; set; }
        [MaxLength(100)]
        public string Ward { get; set; }
        [MaxLength(30)]
        public string Town { get; set; }
        [MaxLength(30)]
        public string County { get; set; }
        [MaxLength(8)]
        public string Postcode { get; set; }
        [MaxLength(8)]
        public string Postcode_nospace { get; set; }
        [MaxLength(50)]
        public string Planning_use_class { get; set; }
        public bool Neverexport { get; set; }
        public float Longitude { get; set; }
        public float Latitude { get; set; }
        [MaxLength(5)]
        public string Gazetteer { get; set; }
        [MaxLength(90)]
        public string Line1 { get; set; }
        [MaxLength(120)]
        public string Line2 { get; set; }
        [MaxLength(120)]
        public string Line3 { get; set; }
        [MaxLength(30)]
        public string Line4 { get; set; }
        public short? Paon_start_num { get; set; }
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

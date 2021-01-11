﻿// <auto-generated />
using System;
using AddressesAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace AddressesAPI.Infrastructure.Migrations
{
    [DbContext(typeof(AddressesContext))]
    [Migration("20201008123700_AddAddressesAndCrossReferencesTables")]
    partial class AddAddressesAndCrossReferencesTables
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("AddressesAPI.V1.Infrastructure.Address", b =>
                {
                    b.Property<string>("AddressKey")
                        .HasColumnName("lpi_key")
                        .HasColumnType("character varying(14)")
                        .HasMaxLength(14);

                    b.Property<string>("AddressStatus")
                        .HasColumnName("lpi_logical_status")
                        .HasColumnType("character varying(18)")
                        .HasMaxLength(18);

                    b.Property<string>("BuildingName")
                        .HasColumnName("pao_text")
                        .HasColumnType("character varying(90)")
                        .HasMaxLength(90);

                    b.Property<string>("BuildingNumber")
                        .HasColumnName("building_number")
                        .HasColumnType("character varying(17)")
                        .HasMaxLength(17);

                    b.Property<double>("Easting")
                        .HasColumnName("easting")
                        .HasColumnType("double precision");

                    b.Property<string>("Gazetteer")
                        .HasColumnName("gazetteer")
                        .HasColumnType("character varying(8)")
                        .HasMaxLength(8);

                    b.Property<double>("Latitude")
                        .HasColumnName("latitude")
                        .HasColumnType("double precision");

                    b.Property<string>("Line1")
                        .HasColumnName("line1")
                        .HasColumnType("character varying(200)")
                        .HasMaxLength(200);

                    b.Property<string>("Line2")
                        .HasColumnName("line2")
                        .HasColumnType("character varying(200)")
                        .HasMaxLength(200);

                    b.Property<string>("Line3")
                        .HasColumnName("line3")
                        .HasColumnType("character varying(200)")
                        .HasMaxLength(200);

                    b.Property<string>("Line4")
                        .HasColumnName("line4")
                        .HasColumnType("character varying(100)")
                        .HasMaxLength(100);

                    b.Property<string>("Locality")
                        .HasColumnName("locality")
                        .HasColumnType("character varying(100)")
                        .HasMaxLength(100);

                    b.Property<double>("Longitude")
                        .HasColumnName("longitude")
                        .HasColumnType("double precision");

                    b.Property<bool>("NeverExport")
                        .HasColumnName("neverexport")
                        .HasColumnType("boolean");

                    b.Property<double>("Northing")
                        .HasColumnName("northing")
                        .HasColumnType("double precision");

                    b.Property<string>("Organisation")
                        .HasColumnName("organisation")
                        .HasColumnType("character varying(100)")
                        .HasMaxLength(100);

                    b.Property<short?>("PaonStartNumber")
                        .HasColumnName("paon_start_num")
                        .HasColumnType("smallint");

                    b.Property<long?>("ParentUPRN")
                        .HasColumnName("parent_uprn")
                        .HasColumnType("bigint");

                    b.Property<string>("PlanningUseClass")
                        .HasColumnName("planning_use_class")
                        .HasColumnType("character varying(50)")
                        .HasMaxLength(50);

                    b.Property<string>("Postcode")
                        .HasColumnName("postcode")
                        .HasColumnType("character varying(8)")
                        .HasMaxLength(8);

                    b.Property<string>("PostcodeNoSpace")
                        .HasColumnName("postcode_nospace")
                        .HasColumnType("character varying(8)")
                        .HasMaxLength(8);

                    b.Property<int>("PropertyChangeDate")
                        .HasColumnName("blpu_last_update_date")
                        .HasColumnType("integer");

                    b.Property<int>("PropertyEndDate")
                        .HasColumnName("blpu_end_date")
                        .HasColumnType("integer");

                    b.Property<bool>("PropertyShell")
                        .HasColumnName("property_shell")
                        .HasColumnType("boolean");

                    b.Property<int>("PropertyStartDate")
                        .HasColumnName("blpu_start_date")
                        .HasColumnType("integer");

                    b.Property<string>("Street")
                        .HasColumnName("street_description")
                        .HasColumnType("character varying(100)")
                        .HasMaxLength(100);

                    b.Property<string>("Town")
                        .HasColumnName("town")
                        .HasColumnType("character varying(100)")
                        .HasMaxLength(100);

                    b.Property<long>("UPRN")
                        .HasColumnName("uprn")
                        .HasColumnType("bigint");

                    b.Property<int?>("USRN")
                        .HasColumnName("usrn")
                        .HasColumnType("integer");

                    b.Property<string>("UnitName")
                        .HasColumnName("sao_text")
                        .HasColumnType("character varying(90)")
                        .HasMaxLength(90);

                    b.Property<int?>("UnitNumber")
                        .HasColumnName("unit_number")
                        .HasColumnType("integer");

                    b.Property<string>("UsageCode")
                        .HasColumnName("blpu_class")
                        .HasColumnType("character varying(4)")
                        .HasMaxLength(4);

                    b.Property<string>("UsageDescription")
                        .HasColumnName("usage_description")
                        .HasColumnType("character varying(160)")
                        .HasMaxLength(160);

                    b.Property<string>("UsagePrimary")
                        .HasColumnName("usage_primary")
                        .HasColumnType("character varying(160)")
                        .HasMaxLength(160);

                    b.Property<string>("Ward")
                        .HasColumnName("ward")
                        .HasColumnType("character varying(100)")
                        .HasMaxLength(100);

                    b.HasKey("AddressKey");

                    b.ToTable("national_address","dbo");
                });

            modelBuilder.Entity("AddressesAPI.V1.Infrastructure.CrossReference", b =>
                {
                    b.Property<string>("CrossRefKey")
                        .HasColumnName("xref_key")
                        .HasColumnType("character varying(14)")
                        .HasMaxLength(14);

                    b.Property<string>("Code")
                        .HasColumnName("xref_code")
                        .HasColumnType("character varying(6)")
                        .HasMaxLength(6);

                    b.Property<DateTime?>("EndDate")
                        .HasColumnName("xref_end_date")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Name")
                        .HasColumnName("xref_name")
                        .HasColumnType("character varying(100)")
                        .HasMaxLength(100);

                    b.Property<long>("UPRN")
                        .HasColumnName("uprn")
                        .HasColumnType("bigint");

                    b.Property<string>("Value")
                        .HasColumnName("xref_value")
                        .HasColumnType("character varying(50)")
                        .HasMaxLength(50);

                    b.HasKey("CrossRefKey");

                    b.ToTable("hackney_xref","dbo");
                });
#pragma warning restore 612, 618
        }
    }
}

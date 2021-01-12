--
-- PostgreSQL database dump
--

-- Dumped from database version 12.4 (Debian 12.4-1.pgdg100+1)
-- Dumped by pg_dump version 12.4 (Debian 12.4-1.pgdg100+1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Data for Name: hackney_address; Type: TABLE DATA; Schema: dbo; Owner: postgres
--

CREATE TABLE dbo.hackney_address (
   lpi_key character varying(14) NOT NULL,
   lpi_logical_status character varying(18),
   usrn integer,
   uprn bigint NOT NULL,
   parent_uprn bigint,
   blpu_start_date integer NOT NULL,
   blpu_end_date integer NOT NULL,
   blpu_class character varying(6),
   blpu_last_update_date integer NOT NULL,
   usage_description character varying(550),
   usage_primary character varying(50),
   property_shell boolean NOT NULL,
   easting double precision NOT NULL,
   northing double precision NOT NULL,
   unit_number character varying(17),
   sao_text character varying(90),
   building_number character varying(17),
   pao_text character varying(90),
   paon_start_num smallint,
   street_description character varying(100),
   locality character varying(100),
   ward character varying(100),
   town character varying(100),
   postcode character varying(8),
   postcode_nospace character varying(8),
   planning_use_class character varying(50),
   neverexport boolean NOT NULL,
   longitude double precision NOT NULL,
   latitude double precision NOT NULL,
   gazetteer character varying(8),
   organisation character varying(100),
   line1 character varying(500),
   line2 character varying(500),
   line3 character varying(500),
   line4 character varying(500)
);
​
CREATE TABLE dbo.national_address (
   lpi_key character varying(14) NOT NULL,
   lpi_logical_status character varying(18),
   usrn integer,
   uprn bigint NOT NULL,
   parent_uprn bigint,
   blpu_start_date integer NOT NULL,
   blpu_end_date integer NOT NULL,
   blpu_class character varying(6),
   blpu_last_update_date integer NOT NULL,
   usage_description character varying(550),
   usage_primary character varying(50),
   property_shell boolean NOT NULL,
   easting double precision NOT NULL,
   northing double precision NOT NULL,
   unit_number character varying(17),
   sao_text character varying(90),
   building_number character varying(17),
   pao_text character varying(90),
   paon_start_num smallint,
   street_description character varying(100),
   locality character varying(100),
   ward character varying(100),
   town character varying(100),
   postcode character varying(8),
   postcode_nospace character varying(8),
   planning_use_class character varying(50),
   neverexport boolean NOT NULL,
   longitude double precision NOT NULL,
   latitude double precision NOT NULL,
   gazetteer character varying(8),
   organisation character varying(100),
   line1 character varying(500),
   line2 character varying(500),
   line3 character varying(500),
   line4 character varying(500)
);
​
CREATE VIEW dbo.combined_address AS
SELECT hackney_address.lpi_key,
   hackney_address.lpi_logical_status,
   hackney_address.usrn,
   hackney_address.uprn,
   hackney_address.parent_uprn,
   hackney_address.blpu_start_date,
   hackney_address.blpu_end_date,
   hackney_address.blpu_class,
   hackney_address.blpu_last_update_date,
   hackney_address.usage_description,
   hackney_address.usage_primary,
   hackney_address.property_shell,
   hackney_address.easting,
   hackney_address.northing,
   hackney_address.unit_number,
   hackney_address.sao_text,
   hackney_address.building_number,
   hackney_address.pao_text,
   hackney_address.paon_start_num,
   hackney_address.street_description,
   hackney_address.locality,
   hackney_address.ward,
   hackney_address.town,
   hackney_address.postcode,
   hackney_address.postcode_nospace,
   hackney_address.planning_use_class,
   hackney_address.neverexport,
   hackney_address.longitude,
   hackney_address.latitude,
   hackney_address.gazetteer,
   hackney_address.organisation,
   hackney_address.line1,
   hackney_address.line2,
   hackney_address.line3,
   hackney_address.line4
  FROM dbo.hackney_address
UNION ALL
SELECT national_address.lpi_key,
   national_address.lpi_logical_status,
   national_address.lpi_start_date,
   national_address.lpi_end_date,
   national_address.lpi_last_update_date,
   national_address.usrn,
   national_address.uprn,
   national_address.parent_uprn,
   national_address.blpu_start_date,
   national_address.blpu_end_date,
   national_address.blpu_class,
   national_address.blpu_last_update_date,
   national_address.usage_description,
   national_address.usage_primary,
   national_address.property_shell,
   national_address.easting,
   national_address.northing,
   national_address.unit_number,
   national_address.sao_text,
   national_address.building_number,
   national_address.pao_text,
   national_address.paon_start_num,
   national_address.street_description,
   national_address.locality,
   national_address.ward,
   national_address.town,
   national_address.postcode,
   national_address.postcode_nospace,
   national_address.planning_use_class,
   national_address.neverexport,
   national_address.longitude,
   national_address.latitude,
   national_address.gazetteer,
   national_address.organisation,
   national_address.line1,
   national_address.line2,
   national_address.line3,
   national_address.line4
  FROM dbo.national_address;
​
​
CREATE TABLE dbo.hackney_xref (
   xref_key character varying(14) NOT NULL,
   uprn bigint NOT NULL,
   xref_code character varying(6),
   xref_name character varying(100),
   xref_value character varying(50),
   xref_end_date timestamp without time zone
);


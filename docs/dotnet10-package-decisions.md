# .NET 10 package decisions

Record of every PackageReference change made for the net8.0 → net10.0 LTS upgrade (as of 21 Sep 2026). Versions are the latest stable on the chosen major line unless noted.

## AddressesAPI

| Package | Action | From → To | Reason |
|---|---|---|---|
| Amazon.Lambda.AspNetCoreServer | upgrade | 5.1.1 → 10.2.1 | Current Lambda ASP.NET host; `APIGatewayProxyFunction` still valid on net10 |
| Asp.Versioning.Mvc | add (replace) | Microsoft.AspNetCore.Mvc.Versioning 5.0.0 → 10.2.1 | Old package is deprecated (net5/net6 only); this is the supported net10 line |
| Asp.Versioning.Mvc.ApiExplorer | add (replace) | Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer 5.0.0 → 10.2.1 | Companion API explorer for Swagger docs per version |
| Dapper | delete | 2.0.35 → — | Unused: no Dapper types or `using` in the repo |
| Elasticsearch.Net | keep | 7.10.0 → 7.10.0 | Not required for net10. Server is still ES 7.9.3 |
| FluentValidation | keep | 8.1.3 → 8.1.3 | netstandard2.0 runs on net10. 11.x changes cascade, null-property, and Must behavior used by `.Validate()` |
| Microsoft.AspNetCore.Mvc.NewtonsoftJson | upgrade | 6.0.0 → 10.0.12 | Align with ASP.NET Core 10.0.12; keep Newtonsoft JSON contract |
| Microsoft.AspNetCore.Mvc.Versioning | delete | 5.0.0 → — | Replaced by Asp.Versioning.Mvc |
| Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer | delete | 5.0.0 → — | Replaced by Asp.Versioning.Mvc.ApiExplorer |
| Microsoft.EntityFrameworkCore | add | — → 10.0.12 | Pin EF Core to the 10.0.12 patch so Npgsql 10.0.3 (which allows >= 10.0.4) does not restore 10.0.4 in test projects |
| Microsoft.EntityFrameworkCore.Relational | add | — → 10.0.12 | Same pin as EF Core so Relational stays aligned |
| Microsoft.EntityFrameworkCore.Design | upgrade | 6.0.0 → 10.0.12 | EF tools aligned with EF 10; added `PrivateAssets=all` so it is not published to Lambda |
| Microsoft.EntityFrameworkCore.SqlServer | delete | 6.0.36 → — | Unused: runtime uses `UseNpgsql` only |
| NaturalSort.Extension | keep | 4.3.0 → 4.3.0 | Not required for net10 |
| NEST | keep | 7.10.0 → 7.10.0 | Not required for net10. Server is still ES 7.9.3. Not migrating to Elastic.Clients.Elasticsearch |
| NEST.JsonNetSerializer | keep | 7.10.0 → 7.10.0 | Stay paired with NEST 7.10.0 |
| Swashbuckle.AspNetCore | upgrade | 5.4.1 → 7.3.2 | Latest 7.x; unifies previous 5.4.1 / UI 7.1.0 skew. Avoided 8+/10.x OpenAPI 2 rewrite |
| Swashbuckle.AspNetCore.Swagger | delete | 5.4.1 → — | Covered by Swashbuckle.AspNetCore metapackage |
| Swashbuckle.AspNetCore.SwaggerGen | delete | 5.4.1 → — | Covered by Swashbuckle.AspNetCore metapackage |
| Swashbuckle.AspNetCore.SwaggerUI | delete | 7.1.0 → — | Covered by Swashbuckle.AspNetCore 7.3.2 |
| Npgsql.EntityFrameworkCore.PostgreSQL | upgrade | 6.0.29 → 10.0.3 | Latest Npgsql 10 provider; requires EF >= 10.0.4 (satisfied by Design 10.0.12). Legacy timestamp switch is set in `AddressesContext` so existing `timestamp without time zone` columns are not rewritten. Pending model diff is a real migration (`national_address.blpu_last_update_date` nullable), not an ignored warning |

## AddressesAPI.Tests

| Package | Action | From → To | Reason |
|---|---|---|---|
| AutoFixture | keep | 4.11.0 → 4.11.0 | Not required for net10 |
| DotNetEnv | delete | 1.4.0 → — | Unused: no `Env.Load` |
| Elasticsearch.Net | keep | 7.10.0 → 7.10.0 | Direct pin; not required to bump for net10 |
| FluentAssertions | keep | 5.10.3 → 5.10.3 | Kept 5.x: FA 6 breaks `StatusCode.Should().Be(int)` and async `Throw<>`. 5.x is Apache and runs on net10. FA 8 is the commercial-license line we are avoiding. |
| FluentValidation | keep | 8.1.3 → 8.1.3 | Match API. Tests use FluentValidation 8 `TestHelper` (`ShouldHaveValidationErrorFor`, `.Result.Errors`) |
| Microsoft.AspNetCore.Mvc.NewtonsoftJson | delete | 6.0.0 → — | Transitive via API project; no direct test usage |
| Microsoft.AspNetCore.Mvc.Versioning | delete | 5.0.0 → — | Transitive / unused in tests |
| Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer | delete | 5.0.0 → — | Transitive / unused in tests |
| Microsoft.CodeAnalysis.FxCopAnalyzers | delete | 3.0.0 → — | Deprecated; SDK 10 ships built-in analyzers |
| Microsoft.AspNetCore.Mvc.Testing | upgrade | 6.0.36 → 10.0.12 | Test host must match net10 TFM |
| Microsoft.EntityFrameworkCore.SqlServer | delete | 6.0.36 → — | Unused; tests use Npgsql via API |
| Microsoft.NET.Test.Sdk | upgrade | 17.12.0 → 18.10.1 | Current VSTest SDK with net10 support |
| NEST | keep | 7.10.0 → 7.10.0 | Direct pin; not required to bump for net10 |
| Npgsql.EntityFrameworkCore.PostgreSQL | delete | 6.0.29 → — | Transitive via AddressesAPI |
| NUnit3TestAdapter | upgrade | 4.6.0 → 6.3.0 | Current adapter; still runs NUnit 3.x |
| Bogus | keep | 25.0.4 → 25.0.4 | Not required for net10 |
| Moq | keep | 4.20.72 → 4.20.72 | Latest 4.x; Moq 5 is a different product |
| NUnit | upgrade | 3.13.3 → 3.14.0 | Latest 3.x; skip NUnit 4 attribute/breaking changes |

## Reindex

| Package | Action | From → To | Reason |
|---|---|---|---|
| Amazon.Lambda.Serialization.Json | upgrade | 1.8.0 → 3.0.0 | Current Newtonsoft Lambda serializer for .NET 10 |
| Amazon.Lambda.SQSEvents | upgrade | 1.2.0 → 3.0.1 | Current SQS event contract |
| AWSSDK.SQS | keep | 3.5.0.40 → 3.5.0.40 | Not required for net10. Skip AWS SDK v4 |
| NEST | keep | 7.10.0 → 7.10.0 | Same as API; server is ES 7.9.3 |
| NEST.JsonNetSerializer | keep | 7.10.0 → 7.10.0 | Paired with NEST 7.10.0 |
| Newtonsoft.Json | keep | 13.0.3 → 13.0.3 | Direct pin over the Lambda serializer transitive. 13.0.4 patch is not required for net10 |

## ReindexTests

| Package | Action | From → To | Reason |
|---|---|---|---|
| AutoFixture | keep | 4.18.1 → 4.18.1 | Already latest 4.x |
| FluentAssertions | keep | 5.10.3 → 5.10.3 | Same as API tests: stay on 5.x to avoid FA 6 assertion breaking changes |
| Microsoft.NET.Test.Sdk | upgrade | 17.12.0 → 18.10.1 | Match API tests |
| Moq | keep | 4.20.72 → 4.20.72 | Latest 4.x |
| NUnit | upgrade | 3.13.3 → 3.14.0 | Latest 3.x |
| NUnit3TestAdapter | upgrade | 4.0.0-beta.1 → 6.3.0 | Replace beta adapter; unify with API tests |
| System.ComponentModel.Annotations | delete | 4.7.0 → — | Inbox on net10; AutoFixture vuln override no longer needed |

## Tooling (not PackageReference)

| Tool | Action | From → To | Reason |
|---|---|---|---|
| Amazon.Lambda.Tools (build.sh) | upgrade | 4.0.0 → 7.0.0 | Required for `dotnet lambda package` with net10.0 |
| .NET SDK (global.json) | pin | unpinned 8.x → 10.0.300 (`rollForward: latestFeature`) | Requires 10.0.3xx+; local SDK is 10.0.302, CI `sdk:10.0` can roll forward to 10.0.401 |
| Docker / CircleCI images | upgrade | sdk:8.0 → sdk:10.0 | Build and format against .NET 10 |
| serverless.yml runtime | upgrade | dotnet8 → dotnet10 | AWS Lambda managed runtime for .NET 10 LTS |

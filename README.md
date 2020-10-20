# Addresses API

The Addresses API let you search the local and national address database. Local being the addresses which Hackney Council holds a record for.

This API is based on the [HackneyAddressesAPI](https://github.com/LBHackney-IT/HackneyAddressesAPI) with some improvements and updates.

## Stack

- .NET Core as a web framework.
- nUnit as a test framework.

## Contributing

### Setup

1. Install [Docker][docker-download].
2. Install [AWS CLI][AWS-CLI].
3. Clone this repository.
4. Rename the initial template.
5. Open it in your IDE.

### Development

#### Running the application

To serve the application, run it using your IDE of choice, we use Visual Studio CE and JetBrains Rider on Mac.

The application can also be served locally using Docker. It will be available on port 3000.

```sh
make build && make serve
```

#### Setting up the development database

On a separate terminal run:

```sh
make migrate-dev-database && make seed-dev-database
```

This will run migrations on the development database and then seed it with data. This data can then be retrieved by calling the endpoints locally.

### Running the tests

You can run the tests in a container:

```sh
make test
```

Or locally if you prefer:

```sh
dotnet test
```

In order to configure the local test database you should first run:

```sh
docker-compose up -d test-database
```

The migrations for the test database are run as part of the initial test setup.

### Release process

We use a pull request workflow, where changes are made on a branch and approved by one or more other maintainers before the developer can merge into `master` branch.

![Circle CI Workflow Example](docs/circle_ci_workflow.png)

Then we have an automated six step deployment process, which runs in CircleCI.

1. Automated tests (nUnit) are run to ensure the release is of good quality.
2. The application is deployed to development automatically, where we check our latest changes work well.
3. We manually confirm a staging deployment in the CircleCI workflow once we're happy with our changes in development.
4. The application is deployed to staging.
5. We manually confirm a production deployment in the CircleCI workflow once we're happy with our changes in staging.
6. The application is deployed to production.

Our staging and production environments are hosted by AWS. We would deploy to production per each feature/config merged into  `master`  branch.

### Creating A PR

To help with making changes to code easier to understand when being reviewed, we've added a PR template.
When a new PR is created on a repo that uses this API template, the PR template will automatically fill in the `Open a pull request` description textbox.
The PR author can edit and change the PR description using the template as a guide.

## Adding a Migration

For this API's Postgres database in RDS, we are using EF Core Code first migrations to manage its schema.
To make changes to the database structure e.g add columns, etc. Follow these steps:

1. If you haven't done so previously, you need to install the [dotnet ef cli tool](https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dotnet) by running `dotnet tool install --global dotnet-ef` in your terminal.
2. Make the changes you want to the database model in the code, namely in `AddressesContext` or any of the DbSet's listed within the file.
3. In your terminal, navigate to the project root folder and run `dotnet ef migrations add -o ./V1/Infrastructure/Migrations -p AddressesAPI NameOfThisMigration` to create the migration files. `NameOfThisMigration` should be replaced with your migration name e.g. `AddColumnNameToCrossReferencesTable`.
4. Go to the folder `/AddressesAPI/V1/Infrastructure/Migrations` and you should see two new files for the migration. In the one which doesn't end in `.Designer` you can check through the migration script to make sure everything is being created as you expect.
5. If the migration file looks wrong or you have missed something, you can either:

- Make sure the test database is running and then run:

```sh
CONNECTION_STRING="Host=127.0.0.1;Database=testdb;Username=postgres;Password=mypassword;" dotnet ef migrations remove -p AddressesAPI
```

- Or you can delete the migration files and then revert the changes to `AddressesContextModelSnapshot.cs`.
After which make the necessary changes to the context, then create the migration files again.

> Note: You must not commit any changes to any DbSet that is listed in `AddressesContext` without creating a migration file for the change. If not the change won't be reflected in the database and will cause errors.

## Static Code Analysis

### Using [FxCop Analysers](https://www.nuget.org/packages/Microsoft.CodeAnalysis.FxCopAnalyzers)

FxCop runs code analysis when the Solution is built.

Both the API and Test projects have been set up to **treat all warnings from the code analysis as errors** and therefore, fail the build.

However, we can select which errors to suppress by setting the severity of the responsible rule to none, e.g `dotnet_analyzer_diagnostic.<Category-or-RuleId>.severity = none`, within the `.editorconfig` file.
Documentation on how to do this can be found [here](https://docs.microsoft.com/en-us/visualstudio/code-quality/use-roslyn-analyzers?view=vs-2019).

## Agreed Testing Approach

- Use nUnit, FluentAssertions and Moq
- Always follow a TDD approach
- Tests should be independent of each other
- Gateway tests should interact with a real test instance of the database
- Test coverage should never go down
- All use cases should be covered by E2E tests
- Optimise when test run speed starts to hinder development
- Unit tests and E2E tests should run in CI
- Test database schemas should match up with production database schema
- Have integration tests which test from the PostgreSQL database to API Gateway

## Data Migrations

### A good data migration

- Record failure logs
- Automated
- Reliable
- As close to real time as possible
- Observable monitoring in place
- Should not affect any existing databases

## Contacts

### Active Maintainers

- **Selwyn Preston**, Lead Developer at London Borough of Hackney (selwyn.preston@hackney.gov.uk)
- **Mirela Georgieva**, Lead Developer at London Borough of Hackney (mirela.georgieva@hackney.gov.uk)
- **Matt Keyworth**, Lead Developer at London Borough of Hackney (matthew.keyworth@hackney.gov.uk)

### Other Contacts

- **Rashmi Shetty**, Product Owner at London Borough of Hackney (rashmi.shetty@hackney.gov.uk)

[docker-download]: https://www.docker.com/products/docker-desktop
[docker-compose]: https://docs.docker.com/compose/install
[made-tech]: https://madetech.com/
[AWS-CLI]: https://aws.amazon.com/cli/

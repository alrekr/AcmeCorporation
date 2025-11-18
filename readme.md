# AcmeCorporation. A simple raffle entry application

## Database prerequisites

This application expects a SQL Server database to be available. The appsettings.development.json file can contain a connection string (key: `AcmeConnectionString`) in the `ConnectionStrings` element you can make it fit your need; preferably though you should use `secrets.json`. 

An example connection string is:

```
Server=localhost;Database=AcmeRaffleMaltheHS;User Id=AcmeRaffleWriterLogin;Password=SuperStrongPassword123!;TrustServerCertificate=True
```

## Persistance 

The below example configuration creates a user AcmeRaffleWriter, which is created with these permissions:

- insert and read permissions to the `acme.RaffleParticipant` table, and
- insert, read and update permissions to the `acme.RaffleEntry` table.
 
### Example database setup script

The below script must be run as a user with permissions to create logins, users, databases and tables, eg. a `sysadmin`.

```sql
use master
go
create database AcmeRaffleMaltheHS;
create login [AcmeRaffleWriterLogin] with password=N'Please insert a new, strong password here', default_database=[AcmeRaffleMaltheHS], check_expiration=off, check_policy=off;

use AcmeRaffleMaltheHS
go

create schema Acme;

create table AcmeRaffleMaltheHS.Acme.RaffleParticipant (
	Id integer not null primary key identity(1,1),
	FirstName nvarchar(100) not null,
	LastName nvarchar(100) not null,
	Email nvarchar(256) not null
);

create table AcmeRaffleMaltheHS.Acme.RaffleEntry (
	Id integer not null primary key identity(1,1),
	ParticipantId integer not null references Acme.RaffleParticipant(Id),
	EntryDateTimeUtc datetime2 not null,
	SerialNumber nvarchar(50) not null
);

create user [AcmeRaffleWriter] for login [AcmeRaffleWriterLogin];

grant insert, select on acme.RaffleParticipant to [AcmeRaffleWriter];
grant insert, select on acme.RaffleEntry to [AcmeRaffleWriter];
go
```

## Identity setup

This application uses ASP.NET Core Identity for user management which is dependent on EF Core. The EF Core connection string is different from the persistence connection string to allow for increased security by using different database users for different purposes. The key for this connection string is `AcmeCorporationContextConnection`. The connection string should use a user with sysadmin privileges to facilitate migration. 

Example connectionstring:

```
Server=localhost;Integrated Security=SSPI;Database=AcmeRaffleMaltheHS;MultipleActiveResultSets=true;TrustServerCertificate=True
```

## First run

On first run (just start debugging as usually), the application will attempt to migrate the ASP.NET Core Identity tables. Then, it will attempt to seed an admin user. The admin user must be defined in `appsettings.json` or `secrets.json`. The admin user element is:

```json
"DefaultAdminUser": {
    "Email": string,
    "Password": string,
    "RoleName": string
  }
```

The RoleName is configurable, but the application can only handle the value "Administrator". 

## acme.http

The `acme.http` file shows how you can generate some serial numbers for testing purposes. It also shows how the database can be filled using `Bogus` for data generation. 

To use the `acme.http` file in Visual Studio, just start the application and then hit "Send Request" for the request you want to execute.
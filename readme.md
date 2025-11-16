# AcmeCorporation. A simple raffle entry application

## Database prerequisites
This application expects a SQL Server database to be available. The appsettings.development.json file contains a connection string that expects a local instance, but you can make it fit your need. The development configuration also expects a user AcmeRaffleWriter, which must have these permissions:

- insert and read permissions to the `acme.RaffleParticipant` table;
- insert, read and update permissions to the `acme.RaffleEntry` table; and,
- optionally deny read and write to all tables
 
## Example database setup script
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
	ParticipantFirstName nvarchar(100) not null,
	ParticipantLastName nvarchar(100) not null,
	ParticipantEmail nvarchar(256) not null,
	EntrySerialNumber nvarchar(50) not null,
	EntryCount integer not null,
);

create table AcmeRaffleMaltheHS.Acme.RaffleEntry (
	Id integer not null primary key identity(1,1),
	RaffleParticipantId integer not null references Acme.RaffleParticipant(Id),
	EntryDateTimeUtc datetime2 not null,
	SerialNumber nvarchar(50) not null
);

create user [AcmeRaffleWriter] for login [AcmeRaffleWriterLogin];

deny select, insert, update, delete on schema :: dbo to [AcmeRaffleWriter]; -- this is default, but let's be explicit just-in-case
deny select, insert, update, delete on schema :: acme to [AcmeRaffleWriter]; -- also default
grant insert, select on acme.RaffleParticipant to [AcmeRaffleWriter];
grant insert, select, update on acme.RaffleEntry to [AcmeRaffleWriter];
```

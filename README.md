# NagerHolidays API

![CI](https://github.com/kodeInInk/NagerHolidays/actions/workflows/ci.yml/badge.svg)
![CD](https://github.com/kodeInInk/NagerHolidays/actions/workflows/cd.yml/badge.svg)

This is a .NET 9 RestAPI project, that relies on public holiday data from [date.nager.at](https://date.nager.at). This project serves the purpose of showcasing backend and database specific patterns and operations, such as migrating a schema, storing data in SQL server, performing CRUD operations on the database, and exposing a few endpoints, easily accesible via an openApi.

## How to run it

You only need Docker Desktop. .NET and SQL Server are both stored in containers.

```bash
git clone https://github.com/kodeInInk/NagerHolidays
cd NagerHolidays
docker compose up --build
```

First boot takes a few minutes, as it fetches and populates holidays for all countries, from 50 years in the past till 50 years in the future, spanning to 101 total year (as of this current, 2026, that implies 1976 to 2076). After this initialisation, starts are instant since everything is stored in a volume.

```bash
docker compose down # stop container but keep data
docker compose down -v # stop and remove everything
```

### If you're on Apple Silicon

SQL Server only has a `linux/amd64` image, so you need Rosetta enabled:

**Docker Desktop → Settings → General → "Use Rosetta for x86_64/amd64 emulation on Apple Silicon"**

It'll be slow on first boot (~30–60s), however it has been documented and is, unfortunately, expected. As soon as a new stable MSSQL version is released, it is expected for this issue, of MSSQL 2022, to be fixed.


## Swagger

Once the application is up, swagger can be accessed on: **http://localhost:8080/swagger**

## The 3 custom endpoints

#### Last celebrated holidays
```
GET /holiday/lastCelebrated/{countryCode}?amount=3
```
Returns the most recent past holidays for a country.

#### Non-weekend holiday counts
```
GET /holiday/nonWeekendCounts?year={year}&countryCodes=NL,DE,RO
```
For each country, counts how many public holidays fall on a weekday that year. Sorted descending.

#### Common holidays between two countries
```
GET /holiday/common/{year}/{countryCodeA}/{countryCodeB}
```
Dates that are public holidays in both countries, with each country's local name for that day.

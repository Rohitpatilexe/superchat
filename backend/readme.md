## Some consistent setup;
- get into backend
```bash
dotnet restore
dotnet build # optional
dotnet run
```

- we will set the db with docker as
```bash
# Create a volume
docker volume create mssql_data
# Run the container
docker run -d \
  --name mssql_server \
  -e 'ACCEPT_EULA=Y' \
  -e 'MSSQL_SA_PASSWORD=YourStrong@Password123' \
  -p 1433:1433 \
  -v mssql_data:/var/opt/mssql \
  mcr.microsoft.com/mssql/server:2022-latest
```

- then set the .env variables

- note; if you made any changes to db aka Models then run this before running
```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```
- to get into our db container we will do this
```bash
# get into shell
docker exec -it mssql_server /bin/bash
# connect to sql server
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong@Password123' -C
# run sql queries
```
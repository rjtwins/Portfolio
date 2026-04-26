Migration commands:

dotnet ef migrations add <MigrationName> --project BCT.EF --startup-project BCT.Blazor
dotnet ef database update --project BCT.EF --startup-project BCT.Blazor
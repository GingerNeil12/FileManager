FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
COPY ["src/FileManager.WebApi/FileManager.WebApi.csproj", "src/FileManager.WebApi/"]
COPY ["src/FileManager.Application/FileManager.Application.csproj", "src/FileManager.Application/"]
COPY ["src/FileManager.Persistence/FileManager.Persistence.csproj", "src/FileManager.Persistence/"]
COPY ["src/FileManager.Domain/FileManager.Domain.csproj", "src/FileManager.Domain/"]
RUN dotnet restore "src/FileManager.WebApi/FileManager.WebApi.csproj"
COPY . .
WORKDIR "/src/src/FileManager.WebApi"
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FileManager.WebApi.dll"]

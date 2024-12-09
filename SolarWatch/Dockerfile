FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY SolarWatch.csproj .
RUN dotnet restore

COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Development
COPY --from=publish /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "SolarWatch.dll"]

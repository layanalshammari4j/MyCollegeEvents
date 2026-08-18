FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish "MyCollegeEvents.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyCollegeEvents.dll"]
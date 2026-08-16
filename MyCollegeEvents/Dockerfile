FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MyCollegeEvents/MyCollegeEvents.csproj", "MyCollegeEvents/"]
RUN dotnet restore "MyCollegeEvents/MyCollegeEvents.csproj"
COPY . .
WORKDIR "/src/MyCollegeEvents"
RUN dotnet publish "MyCollegeEvents.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyCollegeEvents.dll"]

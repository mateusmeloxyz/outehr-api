FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY OutEHR/src/ ./

RUN dotnet restore OutEHR.Api/OutEHR.Api.csproj
RUN dotnet publish OutEHR.Api/OutEHR.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "OutEHR.Api.dll"]

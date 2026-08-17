# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/AslSu.sln ./
COPY backend/src/AslSu.Domain/AslSu.Domain.csproj backend/src/AslSu.Domain/
COPY backend/src/AslSu.Application/AslSu.Application.csproj backend/src/AslSu.Application/
COPY backend/src/AslSu.Infrastructure/AslSu.Infrastructure.csproj backend/src/AslSu.Infrastructure/
COPY backend/src/AslSu.TrendyolGo/AslSu.TrendyolGo.csproj backend/src/AslSu.TrendyolGo/
COPY backend/src/AslSu.Api/AslSu.Api.csproj backend/src/AslSu.Api/
RUN dotnet restore backend/src/AslSu.Api/AslSu.Api.csproj

COPY backend/src/ backend/src/
RUN dotnet publish backend/src/AslSu.Api/AslSu.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AslSu.Api.dll"]

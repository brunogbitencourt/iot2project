FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Iot2Project.Domain/Iot2Project.Domain.csproj Iot2Project.Domain/
COPY Iot2Project.Application/Iot2Project.Application.csproj Iot2Project.Application/
COPY Iot2Project.Infrastructure/Iot2Project.Infrastructure.csproj Iot2Project.Infrastructure/
COPY Iot2Project.API/Iot2Project.API.csproj Iot2Project.API/

RUN dotnet restore Iot2Project.API/Iot2Project.API.csproj

COPY . .

WORKDIR /src/Iot2Project.API
RUN dotnet build Iot2Project.API.csproj -c Release -o /app/build
RUN dotnet publish Iot2Project.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "Iot2Project.API.dll"]

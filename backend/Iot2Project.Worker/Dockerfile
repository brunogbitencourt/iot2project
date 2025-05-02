# Dockerfile para o projeto Worker

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app


ENV ASPNETCORE_ENVIRONMENT=Development


COPY Iot2Project.Domain/Iot2Project.Domain.csproj ./Iot2Project.Domain/
COPY Iot2Project.Application/Iot2Project.Application.csproj ./Iot2Project.Application/
COPY Iot2Project.Infrastructure/Iot2Project.Infrastructure.csproj ./Iot2Project.Infrastructure/
COPY Iot2Project.Worker/Iot2Project.Worker.csproj ./Iot2Project.Worker/

RUN dotnet restore ./Iot2Project.Worker/Iot2Project.Worker.csproj

COPY . .

WORKDIR /app/Iot2Project.Worker
RUN dotnet publish "Iot2Project.Worker.csproj" -c Release -o /publish

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /publish .

ENTRYPOINT ["dotnet", "Iot2Project.Worker.dll"]

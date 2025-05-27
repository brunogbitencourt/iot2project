#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Iot2Project.API/Iot2Project.API.csproj", "Iot2Project.API/"]
COPY ["Iot2Project.Domain/Iot2Project.Domain.csproj", "Iot2Project.Domain/"]
COPY ["Iot2Project.Application/Iot2Project.Application.csproj", "Iot2Project.Application/"]
COPY ["Iot2Project.Infrastructure/Iot2Project.Infrastructure.csproj", "Iot2Project.Infrastructure/"]
RUN dotnet restore "./Iot2Project.API/Iot2Project.API.csproj"
COPY . .
WORKDIR "/src/Iot2Project.API"
RUN dotnet build "./Iot2Project.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Iot2Project.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Iot2Project.API.dll"]
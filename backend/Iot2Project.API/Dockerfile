# See https://aka.ms/customizecontainer for more info

# 1) Stage “base”: runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

# The container will listen on port 8080
EXPOSE 5000
EXPOSE 5000



# 2) Stage “build”: restore & build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["Iot2Project.API/Iot2Project.API.csproj", "Iot2Project.API/"]
COPY ["Iot2Project.Domain/Iot2Project.Domain.csproj", "Iot2Project.Domain/"]
COPY ["Iot2Project.Application/Iot2Project.Application.csproj", "Iot2Project.Application/"]
COPY ["Iot2Project.Infrastructure/Iot2Project.Infrastructure.csproj", "Iot2Project.Infrastructure/"]

# Restore NuGet packages
RUN dotnet restore "Iot2Project.API/Iot2Project.API.csproj"

# Copy the rest of the code
COPY . .

# Build the API project
WORKDIR "/src/Iot2Project.API"
RUN dotnet build "Iot2Project.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 3) Stage “publish”: publish artifacts
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Iot2Project.API.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# 4) Stage “final”: runtime image
FROM base AS final
WORKDIR /app

# Copy published output from the publish stage
COPY --from=publish /app/publish .

# Start the API
ENTRYPOINT ["dotnet", "Iot2Project.API.dll"]

# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os arquivos de projeto e restaura dependências
COPY Iot2Project.Worker/*.csproj ./Iot2Project.Worker/
COPY Iot2Project.Domain/*.csproj ./Iot2Project.Domain/
COPY Iot2Project.Infrastructure/*.csproj ./Iot2Project.Infrastructure/

RUN dotnet restore ./Iot2Project.Worker/Iot2Project.Worker.csproj

# Copia o restante dos arquivos e publica
COPY . .
RUN dotnet publish ./Iot2Project.Worker/Iot2Project.Worker.csproj -c Release -o /app/publish

# Etapa de runtime
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Iot2Project.Worker.dll"]

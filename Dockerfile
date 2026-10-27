
# Etapa 1: compilar el proyecto con .NET 10
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Restaurar dependencias
COPY SICOP.csproj ./
RUN dotnet restore SICOP.csproj

# Copiar código y publicar
COPY . .
RUN dotnet publish SICOP.csproj -c Release -o /app/publish --no-restore

# Etapa 2: ejecutar únicamente la API
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

# Puerto utilizado por OpenShift
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Copiar aplicación compilada
COPY --from=build /app/publish .

# Ejecutar sin privilegios de administrador
USER $APP_UID

ENTRYPOINT ["dotnet", "SICOP.dll"]

# ---------- Etapa 1: compilar y publicar ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar el .csproj y restaurar dependencias (aprovecha la cache de Docker)
COPY BackendApi.csproj ./
RUN dotnet restore BackendApi.csproj

# Copiar el resto del código y publicar en modo Release
COPY . ./
RUN dotnet publish BackendApi.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------- Etapa 2: imagen final (solo runtime, más liviana) ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

# Habilita Swagger y escucha en el puerto 8080 dentro del contenedor
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080
ENTRYPOINT ["dotnet", "BackendApi.dll"]

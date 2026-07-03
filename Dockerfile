# syntax=docker/dockerfile:1

# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar solo los .csproj primero para aprovechar la caché de restore.
COPY ["src/Dynamics_Leads.Api/Dynamics_Leads.Api.csproj", "src/Dynamics_Leads.Api/"]
COPY ["src/Dynamics_Leads.Application/Dynamics_Leads.Application.csproj", "src/Dynamics_Leads.Application/"]
COPY ["src/Dynamics_Leads.Domain/Dynamics_Leads.Domain.csproj", "src/Dynamics_Leads.Domain/"]
COPY ["src/Dynamics_Leads.Infrastructure/Dynamics_Leads.Infrastructure.csproj", "src/Dynamics_Leads.Infrastructure/"]
RUN dotnet restore "src/Dynamics_Leads.Api/Dynamics_Leads.Api.csproj"

# Copiar el resto del código y publicar.
COPY src/ src/
RUN dotnet publish "src/Dynamics_Leads.Api/Dynamics_Leads.Api.csproj" \
    -c Release -o /app/publish /p:UseAppHost=false

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Producción + escucha en el puerto 8080 dentro del contenedor.
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

# Carpeta de archivos subidos (montar un volumen para persistirlos).
RUN mkdir -p /app/Archivos && chown -R app:app /app/Archivos
VOLUME ["/app/Archivos"]

# Ejecutar como usuario no root (incluido en las imágenes .NET).
USER app

ENTRYPOINT ["dotnet", "Dynamics_Leads.Api.dll"]

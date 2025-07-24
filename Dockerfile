# Usa la imagen oficial de .NET SDK para build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Establece el directorio de trabajo
WORKDIR /app
# Copia los archivos del proyecto y restaura dependencias
COPY . .
RUN dotnet restore
# Publica la app en modo Release
RUN dotnet publish -c Release -o out
# Usa una imagen runtime de .NET
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
# Establece el directorio de trabajo
WORKDIR /app
# Copia los archivos publicados desde el contenedor de build
COPY --from=build /app/out .
# Expone el puerto (Render lo ignora pero es buena práctica)
EXPOSE 80
# Comando de inicio
ENTRYPOINT ["dotnet", "FirmaContrato.dll"]
# compila los proyectos del web service
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore "KameRequest.WebService/KameRequest.WebService.csproj"
RUN dotnet publish "KameRequest.WebService/KameRequest.WebService.csproj" -c Release -o /app/publish --no-restore

# imagen mas pequena que solo contiene lo necesario para ejecutar el servicio
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render entrega el puerto por la variable PORT
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000
ENTRYPOINT ["sh", "-c", "dotnet KameRequest.WebService.dll --urls http://0.0.0.0:${PORT:-10000}"]

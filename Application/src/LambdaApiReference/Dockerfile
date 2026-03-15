FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/LambdaApiReference/LambdaApiReference.csproj src/LambdaApiReference/
RUN dotnet restore src/LambdaApiReference/LambdaApiReference.csproj

COPY src/LambdaApiReference/ src/LambdaApiReference/
RUN dotnet publish src/LambdaApiReference/LambdaApiReference.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=80
EXPOSE 80

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LambdaApiReference.dll"]

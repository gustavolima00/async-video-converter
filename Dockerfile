# Etapa de construção
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app
COPY src/ .
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Etapa final
FROM mcr.microsoft.com/dotnet/aspnet:6.0
RUN apt-get update && apt-get install -y ffmpeg
WORKDIR /app
COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "Api.dll"]

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Dashboard/Dashboard.csproj", "Dashboard/"]
RUN dotnet restore "Dashboard/Dashboard.csproj"

COPY . .

RUN dotnet build "Dashboard/Dashboard.csproj" -c Release -o /app/build
RUN dotnet publish "Dashboard/Dashboard.csproj" --os linux --configuration Release --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base

RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV LANG de_DE.UTF-8
ENV LC_ALL de_DE.UTF-8

WORKDIR /app

COPY --from=build /app/publish .

USER $APP_UID
EXPOSE 8080

ENTRYPOINT ["dotnet", "Dashboard.dll"]
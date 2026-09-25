# ==============================
# Runtime
# ==============================

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080


# ==============================
# Build
# ==============================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY ["Backend.csproj", "./"]

RUN dotnet restore "Backend.csproj"

COPY . .

RUN dotnet build "Backend.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build


# ==============================
# Publish
# ==============================

FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "Backend.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false


# ==============================
# Final
# ==============================

FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Backend.dll"]
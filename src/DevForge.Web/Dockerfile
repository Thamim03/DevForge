# Use ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use .NET SDK image to build and publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/DevForge.Web/DevForge.Web.csproj", "src/DevForge.Web/"]
COPY ["src/DevForge.Application/DevForge.Application.csproj", "src/DevForge.Application/"]
COPY ["src/DevForge.Domain/DevForge.Domain.csproj", "src/DevForge.Domain/"]
COPY ["src/DevForge.Infrastructure/DevForge.Infrastructure.csproj", "src/DevForge.Infrastructure/"]
RUN dotnet restore "src/DevForge.Web/DevForge.Web.csproj"

# Copy remaining source code and compile
COPY . .
WORKDIR "/src/src/DevForge.Web"
RUN dotnet build "DevForge.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish output
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DevForge.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final production stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DevForge.Web.dll"]

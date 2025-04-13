# Use the official .NET image as a base image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# Use the SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["NewMyApp.Web/NewMyApp.Web.csproj", "NewMyApp.Web/"]
COPY ["NewMyApp.Core/NewMyApp.Core.csproj", "NewMyApp.Core/"]
COPY ["NewMyApp.Infrastructure/NewMyApp.Infrastructure.csproj", "NewMyApp.Infrastructure/"]
RUN dotnet restore "NewMyApp.Web/NewMyApp.Web.csproj"

COPY . .
WORKDIR "/src/NewMyApp.Web"
RUN dotnet build "NewMyApp.Web.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "NewMyApp.Web.csproj" -c Release -o /app/publish

# Use the base image to run the application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NewMyApp.Web.dll"]
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY backend/restaurant-reservation-react-aspnet.sln ./
COPY backend/Domain/Domain.csproj ./Domain/
COPY backend/Application/Application.csproj ./Application/
COPY backend/Infrastructure/Infrastructure.csproj ./Infrastructure/
COPY backend/API/API.csproj ./API/

# Restore dependencies
RUN dotnet restore

# Copy all source files
COPY backend/Domain/ ./Domain/
COPY backend/Application/ ./Application/
COPY backend/Infrastructure/ ./Infrastructure/
COPY backend/API/ ./API/

# Build and publish
WORKDIR /src/API
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# Expose port (Railway will set PORT env variable)
EXPOSE 8080

# Set environment
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "API.dll"]

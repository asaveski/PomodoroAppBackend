# Use the official .NET SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set working directory
WORKDIR /app

# Copy the project files
COPY . .

RUN curl https://api.nuget.org/v3/index.json
RUN apt-get update && apt-get install -y ca-certificates

# Restore using the local nuget.config from the image
RUN dotnet nuget list source && \
    dotnet restore --disable-parallel --verbosity detailed

# Build the project
RUN dotnet build -c Release || dotnet build

# Publish the application
RUN dotnet publish -c Release -o out || dotnet publish -o out

# Use the official .NET ASP.NET Core runtime image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set working directory
WORKDIR /app

# Copy the published app from the build container
COPY --from=build /app/out .

# Expose the internal port 8080 for the app to run
EXPOSE 8080

# Run the application (migrations now happen automatically in Program.cs)
CMD ["dotnet", "PomodoroAppBackend.dll"]
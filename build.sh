#!/bin/bash

set -e

echo "Building TaskApiService..."

# Restore NuGet packages
echo "Restoring packages..."
dotnet restore

# Build the solution
echo "Building solution..."
dotnet build --configuration Release --no-restore

# Run tests
echo "Running tests..."
dotnet test --configuration Release --no-build --verbosity normal

# Pack NuGet packages (if applicable)
echo "Creating packages..."
dotnet pack --configuration Release --no-build --output ./artifacts

echo "Build completed successfully!"

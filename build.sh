#!/bin/bash

# Build script for Netlify deployment
echo "🚀 Starting build process..."

# Install .NET 8.0 if not available
if ! command -v dotnet &> /dev/null; then
    echo "📦 Installing .NET 8.0..."
    curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version 8.0.0
    export PATH="$HOME/.dotnet:$PATH"
fi

# Restore dependencies
echo "📦 Restoring dependencies..."
dotnet restore

# Build the application
echo "🔨 Building application..."
dotnet build -c Release --no-restore

# Publish the application
echo "📤 Publishing application..."
dotnet publish -c Release -o ./publish --no-build

# Create wwwroot directory if it doesn't exist
mkdir -p ./publish/wwwroot

# Copy static files to wwwroot
echo "📁 Copying static files..."
cp -r ./wwwroot/* ./publish/wwwroot/ 2>/dev/null || echo "No wwwroot directory found"

echo "✅ Build completed successfully!"
echo "📂 Published files are in ./publish directory"
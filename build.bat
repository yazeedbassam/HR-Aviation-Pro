@echo off
REM Build script for Netlify deployment on Windows

echo 🚀 Starting build process...

REM Restore dependencies
echo 📦 Restoring dependencies...
dotnet restore

REM Build the application
echo 🔨 Building application...
dotnet build -c Release --no-restore

REM Publish the application
echo 📤 Publishing application...
dotnet publish -c Release -o ./publish --no-build

REM Create wwwroot directory if it doesn't exist
if not exist "./publish/wwwroot" mkdir "./publish/wwwroot"

REM Copy static files to wwwroot
echo 📁 Copying static files...
if exist "./wwwroot" (
    xcopy /E /I /Y "./wwwroot" "./publish/wwwroot"
) else (
    echo No wwwroot directory found
)

echo ✅ Build completed successfully!
echo 📂 Published files are in ./publish directory
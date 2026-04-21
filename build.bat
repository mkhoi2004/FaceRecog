@echo off
chcp 65001 >nul
dotnet build "%~dp0FaceIDApp\FaceIDApp.csproj" --nologo 2>&1
echo EXIT_CODE=%ERRORLEVEL%

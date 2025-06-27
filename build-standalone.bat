@echo off
title Création Exécutable Standalone - QuickAlias
color 0B

echo.
echo ========================================
echo   CRÉATION EXÉCUTABLE STANDALONE
echo ========================================
echo.

echo [1/3] Nettoyage...
if exist "publish" rmdir /s /q "publish"
if exist "release" rmdir /s /q "release"

echo [2/3] Compilation standalone (inclut .NET)...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish

if %ERRORLEVEL% NEQ 0 (
    echo ❌ Erreur lors de la compilation
    pause
    exit /b 1
)

echo [3/3] Création du package...
mkdir release
copy publish\PowerShellShortcutCreator.exe release\
copy README.md release\ 2>nul

echo.
echo ✅ Exécutable standalone créé !
echo 📁 Fichier : release\PowerShellShortcutCreator.exe
echo 📦 Taille : ~70MB (inclut .NET)
echo.
echo 🚀 Prêt pour upload sur GitHub Releases !
pause

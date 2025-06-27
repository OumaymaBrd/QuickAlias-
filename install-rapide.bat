@echo off
title Installation QuickAlias - Gestionnaire de Raccourcis
color 0A

echo.
echo ========================================
echo   QUICKALIAS - INSTALLATION RAPIDE
echo ========================================
echo.
echo Installation automatique en cours...
echo Repository: https://github.com/oumaymabrd/quickalias-
echo.

REM Vérifier si .NET 6.0 est installé
echo [1/5] Vérification de .NET 6.0...
dotnet --version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ❌ .NET 6.0 n'est pas installé !
    echo.
    echo 📥 Téléchargement automatique...
    start https://dotnet.microsoft.com/download/dotnet/6.0
    echo.
    echo ⏳ Veuillez installer .NET 6.0 puis relancer ce script.
    pause
    exit /b 1
)
echo ✅ .NET 6.0 détecté

REM Vérifier si Git est installé
echo.
echo [2/5] Vérification de Git...
git --version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Git n'est pas installé !
    echo.
    echo 📥 Téléchargement automatique...
    start https://git-scm.com/download/win
    echo.
    echo ⏳ Veuillez installer Git puis relancer ce script.
    pause
    exit /b 1
)
echo ✅ Git détecté

REM Nettoyer les anciens builds
echo.
echo [3/5] Nettoyage des anciens fichiers...
if exist "bin" rmdir /s /q "bin" >nul 2>&1
if exist "obj" rmdir /s /q "obj" >nul 2>&1
echo ✅ Nettoyage terminé

REM Restaurer les dépendances
echo.
echo [4/5] Restauration des dépendances...
dotnet restore >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Erreur lors de la restauration
    echo.
    echo 🔍 Tentative avec verbose...
    dotnet restore
    pause
    exit /b 1
)
echo ✅ Dépendances restaurées

REM Compiler en mode Release
echo.
echo [5/5] Compilation en cours...
dotnet build -c Release >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Erreur lors de la compilation
    echo.
    echo 🔍 Détails de l'erreur :
    dotnet build -c Release
    pause
    exit /b 1
)
echo ✅ Compilation réussie

REM Créer un raccourci sur le Bureau
echo.
echo [BONUS] Création d'un raccourci sur le Bureau...
set "exePath=%CD%\bin\Release\net6.0-windows\PowerShellShortcutCreator.exe"
set "desktopPath=%USERPROFILE%\Desktop"
set "shortcutPath=%desktopPath%\QuickAlias.lnk"

powershell -Command "& {$WshShell = New-Object -ComObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('%shortcutPath%'); $Shortcut.TargetPath = '%exePath%'; $Shortcut.WorkingDirectory = '%CD%\bin\Release\net6.0-windows'; $Shortcut.Description = 'QuickAlias - Gestionnaire de Raccourcis Globaux'; $Shortcut.Save()}" >nul 2>&1

echo.
echo ========================================
echo   🎉 INSTALLATION TERMINÉE !
echo ========================================
echo.
echo 📁 Exécutable : bin\Release\net6.0-windows\
echo 🖥️ Raccourci Bureau : QuickAlias.lnk
echo 📖 Documentation : https://github.com/oumaymabrd/quickalias-
echo.
echo 🚀 Voulez-vous lancer l'application maintenant ? (O/N)
set /p choice=
if /i "%choice%"=="O" (
    echo.
    echo 🎯 Lancement de QuickAlias...
    start "" "%exePath%"
    echo.
    echo ✅ Application lancée !
    echo 💡 Conseil : Laissez l'application ouverte pour que les raccourcis globaux fonctionnent.
)

echo.
echo 📚 Guide complet : https://github.com/oumaymabrd/quickalias-#readme
echo 🐛 Support : https://github.com/oumaymabrd/quickalias-/issues
echo.
echo Merci d'utiliser QuickAlias ! 🙏
pause

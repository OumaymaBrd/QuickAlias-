@echo off
echo ========================================
echo   Gestionnaire de Raccourcis Globaux
echo ========================================
echo.

echo Nettoyage des fichiers précédents...
if exist "bin" rmdir /s /q "bin"
if exist "obj" rmdir /s /q "obj"

echo.
echo Compilation en cours...
dotnet build --configuration Release

if %ERRORLEVEL% EQU 0 (
    echo.
    echo  Compilation réussie !
    echo.
    echo  L'exécutable se trouve dans : bin\Release\net6.0-windows\
    echo  Fichier : PowerShellShortcutCreator.exe
    echo.
    echo Voulez-vous lancer l'application ? (O/N)
    set /p choice=
    if /i "%choice%"=="O" (
        start "" "bin\Release\net6.0-windows\PowerShellShortcutCreator.exe"
    )
) else (
    echo.
    echo  Erreur lors de la compilation !
    echo Vérifiez les messages d'erreur ci-dessus.
)

echo.
pause

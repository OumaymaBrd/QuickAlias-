# ========================================
#   QUICKALIAS - INSTALLATION DIRECTE
# ========================================

Write-Host "🚀 QuickAlias - Installation Automatique" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Cyan

# Configuration
$installDir = "$env:USERPROFILE\AppData\Local\QuickAlias"
$exePath = "$installDir\PowerShellShortcutCreator.exe"

try {
    # Créer le dossier d'installation
    if (-not (Test-Path $installDir)) {
        New-Item -ItemType Directory -Path $installDir -Force | Out-Null
    }

    # Vérifier si déjà installé
    if (Test-Path $exePath) {
        Write-Host "✅ QuickAlias déjà installé. Lancement..." -ForegroundColor Green
        Start-Process $exePath
        exit 0
    }

    Write-Host "📥 Téléchargement depuis GitHub Releases..." -ForegroundColor Yellow
    
    # URL directe vers votre release
    $downloadUrl = "https://github.com/OumaymaBrd/QuickAlias-/releases/download/v2.0.0/PowerShellShortcutCreator.exe"
    
    # Télécharger l'exécutable
    Write-Host "⬇️ Téléchargement de PowerShellShortcutCreator.exe..." -ForegroundColor Yellow
    Invoke-WebRequest -Uri $downloadUrl -OutFile $exePath -UseBasicParsing

    # Vérifier le téléchargement
    if (-not (Test-Path $exePath)) {
        throw "Échec du téléchargement"
    }

    # Créer un raccourci sur le Bureau
    Write-Host "🖥️ Création du raccourci Bureau..." -ForegroundColor Yellow
    $desktopPath = [Environment]::GetFolderPath("Desktop")
    $shortcutPath = "$desktopPath\QuickAlias.lnk"
    
    $WshShell = New-Object -ComObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut($shortcutPath)
    $Shortcut.TargetPath = $exePath
    $Shortcut.WorkingDirectory = $installDir
    $Shortcut.Description = "QuickAlias - Gestionnaire de Raccourcis Globaux"
    $Shortcut.Save()

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "🎉 INSTALLATION TERMINÉE AVEC SUCCÈS !" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "📁 Installé dans: $installDir" -ForegroundColor White
    Write-Host "🖥️ Raccourci Bureau: QuickAlias.lnk" -ForegroundColor White
    Write-Host ""
    Write-Host "🚀 Lancement de QuickAlias..." -ForegroundColor Green
    
    # Lancer l'application
    Start-Process $exePath
    
    Write-Host ""
    Write-Host "💡 Conseil: Laissez l'application ouverte pour que les raccourcis globaux fonctionnent." -ForegroundColor Cyan
    Write-Host "📚 Repository: https://github.com/OumaymaBrd/QuickAlias-" -ForegroundColor Blue

} catch {
    Write-Host ""
    Write-Host "❌ ERREUR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "🔧 Solutions:" -ForegroundColor Yellow
    Write-Host "1. Vérifiez votre connexion internet" -ForegroundColor White
    Write-Host "2. Téléchargez manuellement: https://github.com/OumaymaBrd/QuickAlias-/releases" -ForegroundColor White
    Write-Host "3. Exécutez PowerShell en tant qu'administrateur" -ForegroundColor White
    Read-Host "Appuyez sur Entrée pour fermer"
}

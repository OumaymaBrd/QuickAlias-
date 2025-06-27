# ========================================
#   QUICKALIAS - LANCEMENT DIRECT
# ========================================

Write-Host "🚀 QuickAlias - Lancement Direct" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Cyan

# Configuration
$releaseUrl = "https://api.github.com/repos/OumaymaBrd/QuickAlias-/releases/latest"
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

    Write-Host "📥 Téléchargement de la dernière version..." -ForegroundColor Yellow
    
    # Obtenir l'URL de téléchargement de la dernière release
    $releaseInfo = Invoke-RestMethod -Uri $releaseUrl -UseBasicParsing
    $downloadUrl = $releaseInfo.assets | Where-Object { $_.name -like "*.exe" } | Select-Object -First 1 -ExpandProperty browser_download_url
    
    if (-not $downloadUrl) {
        throw "Aucun exécutable trouvé dans les releases"
    }

    # Télécharger l'exécutable
    Write-Host "⬇️ Téléchargement depuis GitHub..." -ForegroundColor Yellow
    Invoke-WebRequest -Uri $downloadUrl -OutFile $exePath -UseBasicParsing

    # Créer un raccourci sur le Bureau
    $desktopPath = [Environment]::GetFolderPath("Desktop")
    $shortcutPath = "$desktopPath\QuickAlias.lnk"
    
    $WshShell = New-Object -ComObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut($shortcutPath)
    $Shortcut.TargetPath = $exePath
    $Shortcut.WorkingDirectory = $installDir
    $Shortcut.Description = "QuickAlias - Gestionnaire de Raccourcis Globaux"
    $Shortcut.Save()

    Write-Host "✅ Installation terminée !" -ForegroundColor Green
    Write-Host "🖥️ Raccourci créé sur le Bureau" -ForegroundColor Green
    Write-Host "🚀 Lancement de QuickAlias..." -ForegroundColor Green
    
    # Lancer l'application
    Start-Process $exePath
    
    Write-Host ""
    Write-Host "💡 Conseil: Laissez l'application ouverte pour que les raccourcis globaux fonctionnent." -ForegroundColor Cyan
    Write-Host "📚 Repository: https://github.com/OumaymaBrd/QuickAlias-" -ForegroundColor Blue

} catch {
    Write-Host "❌ Erreur: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "🔧 Solutions:" -ForegroundColor Yellow
    Write-Host "1. Vérifiez votre connexion internet" -ForegroundColor White
    Write-Host "2. Téléchargez manuellement: https://github.com/OumaymaBrd/QuickAlias-/releases" -ForegroundColor White
    Read-Host "Appuyez sur Entrée pour fermer"
}

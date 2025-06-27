# ========================================
#   INSTALLATION UNIVERSELLE - QuickAlias
# ========================================

# Cette commande peut être exécutée directement dans PowerShell
# sans avoir besoin de fichier sur GitHub

Write-Host "🚀 Installation de QuickAlias - Gestionnaire de Raccourcis" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$repoUrl = "https://github.com/OumaymaBrd/QuickAlias-.git"
$installDir = "$env:USERPROFILE\AppData\Local\QuickAlias"
$tempDir = "$env:TEMP\QuickAlias-Install-$(Get-Random)"

# Fonction pour vérifier si une commande existe
function Test-Command($cmdname) {
    return [bool](Get-Command -Name $cmdname -ErrorAction SilentlyContinue)
}

# Fonction pour installer Git si nécessaire
function Install-Git {
    Write-Host "📥 Git non trouvé. Ouverture de la page de téléchargement..." -ForegroundColor Yellow
    Start-Process "https://git-scm.com/download/win"
    Write-Host "⏳ Veuillez installer Git puis relancer cette commande." -ForegroundColor Yellow
    Read-Host "Appuyez sur Entrée après avoir installé Git"
    
    # Vérifier à nouveau
    if (-not (Test-Command "git")) {
        throw "Git n'est toujours pas installé. Veuillez l'installer manuellement."
    }
}

# Fonction pour installer .NET 6.0 si nécessaire
function Install-DotNet {
    Write-Host "📥 .NET 6.0 non trouvé. Ouverture de la page de téléchargement..." -ForegroundColor Yellow
    Start-Process "https://dotnet.microsoft.com/download/dotnet/6.0"
    Write-Host "⏳ Veuillez installer .NET 6.0 Runtime puis relancer cette commande." -ForegroundColor Yellow
    Read-Host "Appuyez sur Entrée après avoir installé .NET 6.0"
    
    # Vérifier à nouveau
    if (-not (Test-Command "dotnet")) {
        throw ".NET 6.0 n'est toujours pas installé. Veuillez l'installer manuellement."
    }
}

try {
    # Étape 1: Vérifier Git
    Write-Host "[1/6] Vérification de Git..." -ForegroundColor Cyan
    if (-not (Test-Command "git")) {
        Install-Git
    }
    Write-Host "✅ Git disponible" -ForegroundColor Green

    # Étape 2: Vérifier .NET 6.0
    Write-Host ""
    Write-Host "[2/6] Vérification de .NET 6.0..." -ForegroundColor Cyan
    if (-not (Test-Command "dotnet")) {
        Install-DotNet
    }
    Write-Host "✅ .NET 6.0 disponible" -ForegroundColor Green

    # Étape 3: Nettoyer et préparer les dossiers
    Write-Host ""
    Write-Host "[3/6] Préparation des dossiers..." -ForegroundColor Cyan
    if (Test-Path $tempDir) {
        Remove-Item $tempDir -Recurse -Force
    }
    if (Test-Path $installDir) {
        Remove-Item $installDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
    New-Item -ItemType Directory -Path $installDir -Force | Out-Null
    Write-Host "✅ Dossiers préparés" -ForegroundColor Green

    # Étape 4: Cloner le repository
    Write-Host ""
    Write-Host "[4/6] Téléchargement du code source..." -ForegroundColor Cyan
    Set-Location $tempDir
    git clone $repoUrl . 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "Erreur lors du clonage du repository. Vérifiez votre connexion internet."
    }
    Write-Host "✅ Code source téléchargé" -ForegroundColor Green

    # Étape 5: Compilation
    Write-Host ""
    Write-Host "[5/6] Compilation du projet..." -ForegroundColor Cyan
    dotnet restore 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "Erreur lors de la restauration des packages NuGet"
    }
    
    dotnet build -c Release 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "Erreur lors de la compilation du projet"
    }
    Write-Host "✅ Compilation réussie" -ForegroundColor Green

    # Étape 6: Installation
    Write-Host ""
    Write-Host "[6/6] Installation finale..." -ForegroundColor Cyan
    
    # Copier les fichiers compilés
    $sourceDir = "bin\Release\net6.0-windows"
    if (Test-Path $sourceDir) {
        Copy-Item "$sourceDir\*" $installDir -Recurse -Force
        Write-Host "✅ Fichiers copiés vers $installDir" -ForegroundColor Green
    } else {
        throw "Dossier de compilation non trouvé: $sourceDir"
    }

    # Créer un raccourci sur le Bureau
    $desktopPath = [Environment]::GetFolderPath("Desktop")
    $shortcutPath = "$desktopPath\QuickAlias.lnk"
    $exePath = "$installDir\PowerShellShortcutCreator.exe"
    
    $WshShell = New-Object -ComObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut($shortcutPath)
    $Shortcut.TargetPath = $exePath
    $Shortcut.WorkingDirectory = $installDir
    $Shortcut.Description = "QuickAlias - Gestionnaire de Raccourcis Globaux"
    $Shortcut.Save()
    Write-Host "✅ Raccourci Bureau créé" -ForegroundColor Green

    # Ajouter au PATH utilisateur
    $userPath = [Environment]::GetEnvironmentVariable("PATH", "User")
    if ($userPath -notlike "*$installDir*") {
        [Environment]::SetEnvironmentVariable("PATH", "$userPath;$installDir", "User")
        Write-Host "✅ Ajouté au PATH utilisateur" -ForegroundColor Green
    }

    # Nettoyer
    Set-Location $env:USERPROFILE
    Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue

    # Succès !
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "🎉 INSTALLATION TERMINÉE AVEC SUCCÈS !" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "📁 Installé dans: $installDir" -ForegroundColor White
    Write-Host "🖥️ Raccourci Bureau: QuickAlias.lnk" -ForegroundColor White
    Write-Host "⚡ Commande globale: PowerShellShortcutCreator" -ForegroundColor White
    Write-Host ""
    Write-Host "🚀 Voulez-vous lancer l'application maintenant ? (O/N)" -ForegroundColor Yellow
    $choice = Read-Host
    if ($choice -eq "O" -or $choice -eq "o" -or $choice -eq "Y" -or $choice -eq "y") {
        Write-Host "🎯 Lancement de QuickAlias..." -ForegroundColor Green
        Start-Process $exePath
        Write-Host ""
        Write-Host "✅ Application lancée !" -ForegroundColor Green
        Write-Host "💡 Conseil: Laissez l'application ouverte pour que les raccourcis globaux fonctionnent." -ForegroundColor Cyan
    }

    Write-Host ""
    Write-Host "📚 Repository: https://github.com/OumaymaBrd/QuickAlias-" -ForegroundColor Blue
    Write-Host "🐛 Support: https://github.com/OumaymaBrd/QuickAlias-/issues" -ForegroundColor Blue
    Write-Host ""
    Write-Host "Merci d'utiliser QuickAlias ! 🙏" -ForegroundColor Magenta

} catch {
    Write-Host ""
    Write-Host "❌ ERREUR LORS DE L'INSTALLATION" -ForegroundColor Red
    Write-Host "Détails: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "🔧 Solutions possibles:" -ForegroundColor Yellow
    Write-Host "1. Exécutez PowerShell en tant qu'administrateur" -ForegroundColor White
    Write-Host "2. Autorisez l'exécution de scripts: Set-ExecutionPolicy RemoteSigned -Scope CurrentUser" -ForegroundColor White
    Write-Host "3. Vérifiez votre connexion internet" -ForegroundColor White
    Write-Host "4. Installez manuellement Git et .NET 6.0" -ForegroundColor White
    Write-Host ""
    Write-Host "📧 Besoin d'aide ? Ouvrez une issue: https://github.com/OumaymaBrd/QuickAlias-/issues" -ForegroundColor Blue
    
    # Nettoyer en cas d'erreur
    Set-Location $env:USERPROFILE -ErrorAction SilentlyContinue
    Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
    
    Read-Host "Appuyez sur Entrée pour fermer"
    exit 1
}

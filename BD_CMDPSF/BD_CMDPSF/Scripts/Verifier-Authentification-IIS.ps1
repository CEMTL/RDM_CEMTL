param(
    [Parameter(Mandatory=$true)]
    [string]$Location
)

Import-Module WebAdministration -ErrorAction Stop

function Get-IisAuthValue([string]$Filter) {
    try {
        return (Get-WebConfigurationProperty -PSPath 'MACHINE/WEBROOT/APPHOST' -Location $Location -Filter $Filter -Name enabled -ErrorAction Stop).Value
    }
    catch {
        return "ERREUR: $($_.Exception.Message)"
    }
}

$windows = Get-IisAuthValue 'system.webServer/security/authentication/windowsAuthentication'
$anonymous = Get-IisAuthValue 'system.webServer/security/authentication/anonymousAuthentication'

Write-Host "Application IIS : $Location"
Write-Host "Windows Authentication : $windows"
Write-Host "Anonymous Authentication: $anonymous"
Write-Host ""

if ($windows -eq $false -and $anonymous -eq $true) {
    Write-Host "OK : configuration recommandee pour le formulaire RDM compte Windows + mot de passe."
}
else {
    Write-Warning "Pour le formulaire RDM : Anonymous Authentication doit etre Enabled et Windows Authentication doit etre Disabled. Les identifiants saisis sont valides par l'application aupres d'Active Directory. Utiliser HTTPS obligatoirement."
}

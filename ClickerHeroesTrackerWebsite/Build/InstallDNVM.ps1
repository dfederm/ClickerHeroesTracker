Param(
    [string]$SourceDirectory = $Env:BUILD_SOURCESDIRECTORY
)

try
{
    # Log params
    Write-Host "SourceDirectory=$SourceDirectory"

    # Validate params
    if (-not $SourceDirectory)
    {
        throw "SourceDirectory was not provided"
    }

    if (!(Test-Path -Path $SourceDirectory))
    {
        throw "Could not find SourceDirectory: $SourceDirectory"
    }

    $dnvm = Get-Command "dnvm" -ErrorAction SilentlyContinue

    if ($dnvm -ne $null)
    {
        Write-Output "DNVM found:"
        Write-Output "    $($dnvm.Path)"
        $dnvmPath = $dnvm.Path
    }
    else
    {
        Write-Output "DNVM not found, installing..."

        $dnvmPs1Path = "$PSScriptRoot\Tools"
        if (-not (Test-Path -PathType Container $dnvmPs1Path))
        {
            New-Item -ItemType Directory -Path $dnvmPs1Path
        }

        $dnvmPs1Path = "$dnvmPs1Path\dnvm.ps1"

        $webClient = New-Object System.Net.WebClient
        $webClient.Proxy = [System.Net.WebRequest]::DefaultWebProxy
        $webClient.Proxy.Credentials = [System.Net.CredentialCache]::DefaultNetworkCredentials
        Write-Output "Downloading dnvm.ps1 to $dnvmPs1Path"
        $webClient.DownloadFile("https://raw.githubusercontent.com/aspnet/Home/dev/dnvm.ps1", $dnvmPs1Path)

        $dnvmPath = $dnvmPs1Path
    }

    # Load up the global.json so we can find the DNX version
    $globalJson = Get-Content -Path $SourceDirectory\global.json -Raw -ErrorAction Ignore | ConvertFrom-Json -ErrorAction Ignore
    if($globalJson)
    {
        $dnxVersion = $globalJson.sdk.version
    }
    else
    {
        Write-Warning "Unable to locate global.json to determine using 'latest'"
        $dnxVersion = "latest"
    }

    # Install DNX
    Write-Output "Calling: $dnvmPath install $dnxVersion -Persistent"
    & $dnvmPath install "$dnxVersion" -Persistent

    # Run DNU restore on all project.json files in the src folder including 2>1 to redirect stderr to stdout for badly behaved tools
    Get-ChildItem -Path $SourceDirectory -Filter project.json -Recurse | ForEach-Object { & dnu restore $_.FullName 2>1 }

    Write-Host "Done!"
}
catch {
    Write-Error $_
    exit 1
}

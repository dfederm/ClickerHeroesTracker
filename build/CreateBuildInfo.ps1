Param(
    [string]$SourceVersion,
    [string]$BuildUrl,
    [string]$WebClientDirectory,
    [string]$OutputDirectory
)

try
{
    # Log params
    Write-Host "SourceVersion=$SourceVersion"
    Write-Host "BuildUrl=$BuildUrl"
    Write-Host "WebClientDirectory=$WebClientDirectory"
    Write-Host "OutputDirectory=$OutputDirectory"

    # Validate params
    if (-not $BuildUrl)
    {
        throw "BuildUrl was not provided"
    }

    if (-not $SourceVersion)
    {
        throw "SourceVersion was not provided"
    }

    if (-not $WebClientDirectory)
    {
        throw "WebClientDirectory was not provided"
    }

    if (!(Test-Path -Path $WebClientDirectory))
    {
        throw "Could not find WebClientDirectory: $WebClientDirectory"
    }

    if (-not $OutputDirectory)
    {
        throw "OutputDirectory was not provided"
    }

    if (!(Test-Path -Path $OutputDirectory))
    {
        throw "Could not find OutputDirectory: $OutputDirectory"
    }

    # Build the object
    $buildInfo = @{
         changelist = $SourceVersion
         buildUrl = $BuildUrl
    }

    $webclient = @{}
    $WebClientDirectory = Resolve-Path $WebClientDirectory
    foreach ($file in (Get-ChildItem -Path $WebClientDirectory -Recurse -File))
    {
        $relativePath = $file.FullName.Substring($WebClientDirectory.Length + 1);
        $hashInfo = Get-FileHash -Path $file.FullName -Algorithm SHA256
        $webclient[$relativePath] = $hashInfo.Hash
    }
    
    $buildInfo["webclient"] = $webclient

    # Serialize
    $content = ConvertTo-Json -InputObject $buildInfo
    Write-Host "New build info content: $content"

    # Write the file
    $buildInfoFilePath = Join-Path $OutputDirectory "BuildInfo.json"
    Write-Host "Writing build info to: $buildInfoFilePath"
    Set-Content -Path $buildInfoFilePath -Value $content

    Write-Host "Done!"
}
catch {
    Write-Error $_
    exit 1
}

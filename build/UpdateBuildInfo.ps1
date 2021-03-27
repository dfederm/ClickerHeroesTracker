Param(
	[string]$SourceDirectory,
	[string]$SourceVersion,
	[string]$BuildUrl
)

try
{
	# Log params
	Write-Host "SourceDirectory=$SourceDirectory"
	Write-Host "SourceVersion=$SourceVersion"
	Write-Host "BuildUrl=$BuildUrl"

	# Validate params
	if (-not $SourceDirectory)
	{
		throw "SourceDirectory was not provided"
	}

	if (!(Test-Path -Path $SourceDirectory))
	{
		throw "Could not find SourceDirectory: $SourceDirectory"
	}

	if (-not $BuildUrl)
	{
		throw "BuildUrl was not provided"
	}

	if (-not $SourceVersion)
	{
		throw "SourceVersion was not provided"
	}

	# Find the build info
	$buildInfoItem = Get-ChildItem -Path $SourceDirectory -Filter "BuildInfo.json" -Recurse
	if (-not $buildInfoItem)
	{
		throw "Did not find any BuildInfo.json files under the SourceDirectory: $SourceDirectory"
	}

	$buildInfoFile = $buildInfoItem.FullName;
	Write-Host "Found build info file: $buildInfoFile"

	$content = Get-Content -Raw -Path $buildInfoFile
	Write-Host "Original build info content: $content"

	# Deserialize
	$buildInfo = $content | ConvertFrom-Json

	# Replace the fields
	$buildInfo.changelist = $SourceVersion
	$buildInfo.buildUrl = $BuildUrl

	# Serialize
	$content = ConvertTo-Json -InputObject $buildInfo
	Write-Host "New build info content: $content"

	# Re-write the file
	Set-Content -Path $buildInfoFile -Value $content

	Write-Host "Done!"
}
catch {
	Write-Error $_
	exit 1
}

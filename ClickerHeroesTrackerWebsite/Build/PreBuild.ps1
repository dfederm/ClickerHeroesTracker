Param(
	[string]$SourceDirectory = $Env:BUILD_SOURCESDIRECTORY,
	[string]$SourceVersion = $Env:BUILD_SOURCEVERSION,
	[string]$BuildNumber = $Env:BUILD_BUILDNUMBER
)

try
{
	# Log params
	Write-Host "SourceDirectory=$SourceDirectory"
	Write-Host "SourceVersion=$SourceVersion"
	Write-Host "BuildNumber=$BuildNumber"

	# Validate params
	if (-not $SourceDirectory)
	{
		throw "SourceDirectory was not provided"
	}

	if (!(Test-Path -Path $SourceDirectory))
	{
		throw "Could not find SourceDirectory: $SourceDirectory"
	}

	if (-not $BuildNumber)
	{
		throw "BuildNumber was not provided"
	}

	if (-not $SourceVersion)
	{
		throw "SourceVersion was not provided"
	}

	# The string is of the form C<changelist>, so cut off the "C" part
	$versionPattern = "(\\d+)\\.(\\d+)\\.(\\d+)\\.(\\d+)"
	if ($SourceVersion[0] -ine 'C')
	{
		throw "SourceVersion did not start with 'C': $SourceVersion"
	}

	$changelist = [int]$SourceVersion.Substring(1)
	Write-Host "Parsed changelist $changelist"

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
	$buildInfo.changelist = $changelist
	$buildInfo.buildId = $BuildNumber

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

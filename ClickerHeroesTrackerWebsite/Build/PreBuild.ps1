Param(
	[string]$SourceDirectory = $Env:BUILD_SOURCESDIRECTORY,
	[string]$SourceVersion = $Env:BUILD_SOURCEVERSION,
	[string]$BuildNumber = $Env:BUILD_BUILDNUMBER
)

function ValidateParameters()
{
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
}

function InstallDnx()
{
	# bootstrap DNVM into this session.
	&{$Branch='dev';iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}

	# load up the global.json so we can find the DNX version
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

	# install DNX
	# only installs the default (x86, clr) runtime of the framework.
	# If you need additional architectures or runtimes you should add additional calls
	# ex: & $env:USERPROFILE\.dnx\bin\dnvm install $dnxVersion -r coreclr
	& $env:USERPROFILE\.dnx\bin\dnvm install $dnxVersion -Persistent

	 # run DNU restore on all project.json files in the src folder including 2>1 to redirect stderr to stdout for badly behaved tools
	Get-ChildItem -Path $SourceDirectory -Filter project.json -Recurse | ForEach-Object { & dnu restore $_.FullName 2>1 }
}

function UpdateBuildInfo()
{
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
	$buildInfo.buildId = $BuildNumber

	# Serialize
	$content = ConvertTo-Json -InputObject $buildInfo
	Write-Host "New build info content: $content"
 
	# Re-write the file
	Set-Content -Path $buildInfoFile -Value $content
}

try
{
	# Log params
	Write-Host "SourceDirectory=$SourceDirectory"
	Write-Host "SourceVersion=$SourceVersion"
	Write-Host "BuildNumber=$BuildNumber"

	ValidateParameters

	UpdateBuildInfo

	InstallDnx
 
	Write-Host "Done!"
}
catch {
	Write-Error $_
	exit 1
}

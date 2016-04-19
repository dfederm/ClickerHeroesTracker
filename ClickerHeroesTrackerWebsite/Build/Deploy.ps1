Param(
	[string] $WebsiteName,
	[string] $WebsiteSlot,
	[string] $PackOutput
)

# Log params
Write-Host "WebsiteName=$WebsiteName"
Write-Host "WebsiteSlot=$WebsiteSlot"
Write-Host "PackOutput=$PackOutput"

$website = Get-AzureWebsite -Name $WebsiteName -Slot $WebsiteSlot

# get the scm url to use with MSDeploy. By default this will be the second in the array
$msDeployUrl = $website.EnabledHostNames[1]

$publishProperties = @{'WebPublishMethod'='MSDeploy';
                        'MSDeployServiceUrl'=$msDeployUrl;
                        'DeployIisAppPath'=$website.Name;
                        'Username'=$website.PublishingUsername;
                        'Password'=$website.PublishingPassword}

$publishScript = "${env:ProgramFiles(x86)}\Microsoft Visual Studio 14.0\Common7\IDE\Extensions\Microsoft\Web Tools\Publish\Scripts\default-publish.ps1"
. $publishScript -publishProperties $publishProperties -packOutput $PackOutput
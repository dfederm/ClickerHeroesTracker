Param(
	[string]$WebsiteName,
	[string]$Slot
)

try
{
	# Log params
	Write-Host "WebsiteName=$WebsiteName"
	Write-Host "StagingSlot=$Slot"

	$siteUrl = "http://" + $WebsiteName

	if (![string]::IsNullOrEmpty($Slot))
	{
		$siteUrl += "-" + $Slot
	}

	$siteUrl += ".azurewebsites.net/"
	Write-Host "Querying the web site: $siteUrl"

	$time = [System.Diagnostics.Stopwatch]::StartNew()
	$response = Invoke-WebRequest -Method Get -Uri $siteUrl -TimeoutSec 120 -UseBasicParsing

	$elapsedTime = $time.Elapsed.TotalMilliseconds
	$statusCode = $response.StatusCode
	$contentLength = $response.RawContentLength
	Write-Host "Received status code $statusCode with $contentLength bytes in $elapsedTime ms"
}
catch
{
	Write-Host $_
	exit 1
}

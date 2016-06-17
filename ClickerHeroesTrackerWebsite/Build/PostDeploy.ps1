Param(
    [string]$WebsiteName,
    [string]$Slot
)

try
{
    # Log params
    Write-Host "WebsiteName=$WebsiteName"
    Write-Host "Slot=$Slot"

    $siteUrl = "http://" + $WebsiteName

    if (![string]::IsNullOrEmpty($Slot))
    {
        $siteUrl += "-" + $Slot
    }

    $siteUrl += ".azurewebsites.net/"
    Write-Host "Querying the web site: $siteUrl"

    $done = $false
    $attempt = 0
    $successes = 0
    $timeout = 60
    $retryDelay = 5
    $maxRetries = 10
    $numSuccesses = 10
 
    do
    {
        try
        {
            $time = [System.Diagnostics.Stopwatch]::StartNew()
            $response = Invoke-WebRequest -Method Get -Uri $siteUrl -TimeoutSec $timeout -UseBasicParsing

            $elapsedTime = $time.Elapsed.TotalMilliseconds
            $statusCode = $response.StatusCode
            $contentLength = $response.RawContentLength
            Write-Host "Received status code $statusCode with $contentLength bytes in $elapsedTime ms"

            $successes++
            if ($successes -gt $numSuccesses)
            {
                $done = $true
            }
        }
        catch
        {
            Write-Host $_
            if ($attempt -gt $maxRetries)
            {
                Write-Host "Failed after $maxRetries retries."
                $done = $true
                throw
            }
            else
            {
                Write-Host "Failed. Retrying in $retryDelay seconds..."
                Start-Sleep -Seconds $retryDelay
                $attempt++
            }
        }
    }
    while (-not $done)
}
catch
{
    Write-Host $_
    exit 1
}

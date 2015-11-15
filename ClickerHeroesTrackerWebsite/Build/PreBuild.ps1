Param(
	[string]$SourceDirectory = $env:BUILD_SOURCESDIRECTORY,
	[string]$SourceVersion = $env:BUILD_SOURCEVERSION
)

try
{
	# Log params
	Write-Host "SourceDirectory=$SourceDirectory"
	Write-Host "SourceVersion=$SourceVersion"

	$versionPattern = "(\\d+)\\.(\\d+)\\.(\\d+)\\.(\\d+)"

	# The string is of the form C<changelist>, so cut off the "C" part
	$SourceVersion = $SourceVersion.Substring(1)

	Write-Host "Using version $SourceVersion"
 
	$assemblyInfoFiles = Get-ChildItem -Path $SourceDirectory -Filter "AssemblyInfo.cs" -Recurse | %{
		$assemblyInfoFile = $_.FullName
		Write-Host "Changing $assemblyInfoFile"
         
		# Remove the read-only bit on the file
		Set-ItemProperty -Path $assemblyInfoFile -Name IsReadOnly -Value $false
 
		# Replace the version number
		$content = Get-Content -Path $assemblyInfoFile
		$content = $content -Replace $versionPattern,"`$1.`$2.$SourceVersion.`$4"

		# Re-write the file
		Set-Content -Path $assemblyInfoFile -Value $content
    }
 
	Write-Host "Done!"
}
catch {
	Write-Host $_
	exit 1
}

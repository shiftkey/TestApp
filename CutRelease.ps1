C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe TestApp.sln /t:Rebuild /p:Configuration=Release

$outputPackage = (gci .\bin\Release -filter TestApp*.nupkg)[0].FullName

write-host "Creating a release package using $outputPackage"

# TODO: invoke Create-Release with required parameters?

.\packages\Shimmer.0.6.1.0-beta\tools\CreateReleasePackage.exe --output-directory=".\artifacts" $outputPackage
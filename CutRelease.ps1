$rootFolder = split-path -parent $MyInvocation.MyCommand.Definition
$configuration = "Release"

if (Test-Path "bin\$configuration")  { Remove-Item "bin\$configuration" -Recurse -Force }

. "$rootFolder\.nuget\nuget.exe" install "$rootFolder\packages.config" -OutputDirectory "$rootFolder\packages"

# . "$rootFolder\.nuget\nuget.exe" update "$rootFolder\packages.config" -RepositoryPath "$rootFolder\packages" -NonInteractive

. $env:SystemRoot\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe TestApp.sln /t:Rebuild /p:Configuration=$configuration

$scripts = (Get-ChildItem "$rootFolder\packages\" -Filter "Create-Release.ps1" -Recurse)

. $scripts[0].FullName -ProjectNameToBuild "TestApp" -SolutionDir . -BuildDirectory "bin\Release"

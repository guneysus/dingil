dotnet pack





 Get-ChildItem .\nupkg\*.nupkg | % {
    nuget push $_.FullName $env:MYGET_SECRET -Source https://www.myget.org/F/guneysu/api/v2/package
 }
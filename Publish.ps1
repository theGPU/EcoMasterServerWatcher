Remove-Item -Path ./Publish/Windows -Recurse -ErrorAction Ignore
Remove-Item -Path ./Publish/Android -Recurse -ErrorAction Ignore

dotnet publish ./EcoMasterServerWatcher.Desktop/EcoMasterServerWatcher.Desktop.csproj /p:PublishProfile=Windows-x64.pubxml
dotnet publish ./EcoMasterServerWatcher.Desktop/EcoMasterServerWatcher.Desktop.csproj /p:PublishProfile=Windows-x86.pubxml
dotnet publish ./EcoMasterServerWatcher.Desktop/EcoMasterServerWatcher.Desktop.csproj /p:PublishProfile=Windows-arm64.pubxml

dotnet publish ./EcoMasterServerWatcher.Android/EcoMasterServerWatcher.Android.csproj /p:PublishProfile=Android-x64.pubxml
dotnet publish ./EcoMasterServerWatcher.Android/EcoMasterServerWatcher.Android.csproj /p:PublishProfile=Android-x86.pubxml
dotnet publish ./EcoMasterServerWatcher.Android/EcoMasterServerWatcher.Android.csproj /p:PublishProfile=Android-arm.pubxml
dotnet publish ./EcoMasterServerWatcher.Android/EcoMasterServerWatcher.Android.csproj /p:PublishProfile=Android-arm64.pubxml

./Pack.ps1
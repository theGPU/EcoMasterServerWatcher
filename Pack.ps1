Remove-Item -Path ./Publish/Windows-x86.zip -Recurse -ErrorAction Ignore
Remove-Item -Path ./Publish/Windows-x64.zip -Recurse -ErrorAction Ignore
Remove-Item -Path ./Publish/Windows-arm64.zip -Recurse -ErrorAction Ignore

Remove-Item -Path ./Publish/Android-x86.zip -Recurse -ErrorAction Ignore
Remove-Item -Path ./Publish/Android-x64.zip -Recurse -ErrorAction Ignore
Remove-Item -Path ./Publish/Android-arm.zip -Recurse -ErrorAction Ignore
Remove-Item -Path ./Publish/Android-arm64.zip -Recurse -ErrorAction Ignore



Compress-Archive -Path ./Publish/Windows/x86 -DestinationPath ./Publish/Windows-x86.zip
Compress-Archive -Path ./Publish/Windows/x64 -DestinationPath ./Publish/Windows-x64.zip
Compress-Archive -Path ./Publish/Windows/arm64 -DestinationPath ./Publish/Windows-arm64.zip

Compress-Archive -Path ./Publish/Android/x86/com.CompanyName.EcoMasterServerWatcher-signed.apk -DestinationPath ./Publish/Android-x86.zip
Compress-Archive -Path ./Publish/Android/x64/com.CompanyName.EcoMasterServerWatcher-signed.apk -DestinationPath ./Publish/Android-x64.zip
Compress-Archive -Path ./Publish/Android/arm/com.CompanyName.EcoMasterServerWatcher-signed.apk -DestinationPath ./Publish/Android-arm.zip
Compress-Archive -Path ./Publish/Android/arm64/com.CompanyName.EcoMasterServerWatcher-signed.apk -DestinationPath ./Publish/Android-arm64.zip
SET publishFolderPB="..\publish\PB"
if exist %publishFolder% rmdir %publishFolderPB% /s /q 
dotnet publish ..\SCME.ProfileBuilder\SCME.ProfileBuilder.csproj -c Debug -o %publishFolderPB% --self-contained false  -r win-x64
GetVersion.bat %publishFolderPB%\SCME.ProfileBuilder.exe %publishFolderPB%\Version.txt
dotnet build-server shutdown
pause
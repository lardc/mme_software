SET publishFolderPL="..\publish\PL"
if exist %publishFolder% rmdir %publishFolderPB% /s /q 
dotnet publish ..\SCME.ProfileLoader\SCME.ProfileLoader.csproj -c Debug -o %publishFolderPL% --self-contained false  -r win-x64
dotnet build-server shutdown
pause
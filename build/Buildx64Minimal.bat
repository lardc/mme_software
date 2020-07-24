"C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\msbuild.exe" ..\SCME.Service\SCME.Service.csproj /restore /t:Rebuild /p:OutputPath="..\publish\UIService" /p:Configuration=Debug /p:WarningLevel=0 
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\msbuild.exe" ..\SCME.DatabaseServer\SCME.DatabaseServer.csproj /restore /t:Rebuild /p:OutputPath="..\publish\Server" /p:Configuration=Debug /p:WarningLevel=0 
dotnet publish ..\SCME.UI\SCME.UI.csproj -c Debug -o ..\publish\UIService\UI --self-contained false -r win-x64
GetVersion.bat ..\publish\UIService\UI\SCME.UI.exe ..\publish\UIService\UI\Version.txt
dotnet build-server shutdown
pause
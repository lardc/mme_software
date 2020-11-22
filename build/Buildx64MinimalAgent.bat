dotnet publish ..\SCME.Agent\SCME.Agent.csproj -c Debug -o ..\publish\Agent --self-contained false -r win-x64
dotnet build-server shutdown
pause
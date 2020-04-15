dotnet publish ..\SCME.ProfileBuilder\SCME.ProfileBuilder.csproj -c Debug -o ..\publish\PB --self-contained true -r win-x86
dotnet build-server shutdown
pause
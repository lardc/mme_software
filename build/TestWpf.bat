dotnet publish ..\TestWpf\TestWpf.csproj -c Debug -o ..\publish\TestWpf --self-contained false -r win-x64
dotnet build-server shutdown
pause
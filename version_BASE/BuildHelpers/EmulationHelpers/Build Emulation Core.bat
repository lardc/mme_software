set VSVersion=/p:VisualStudioVersion=12.0

c:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe "%~dp0..\..\SCME.Service\SCME.Service.csproj" %VSVersion% ^
/p:Configuration=Emulation /p:DeployOnBuild=True /p:OutputPath="%~dp0..\EmulationBuild\Core" /t:rebuild /verbosity:minimal %*

set VSVersion=/p:VisualStudioVersion=12.0

c:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe "%~dp0..\..\SCME.DatabaseServer\SCME.DatabaseServer.csproj" %VSVersion% ^
/p:Configuration=Emulation /p:DeployOnBuild=True /p:Platform=x86 /p:OutputPath="%~dp0..\EmulationBuild\DatabaseServer" /t:rebuild /verbosity:minimal %*

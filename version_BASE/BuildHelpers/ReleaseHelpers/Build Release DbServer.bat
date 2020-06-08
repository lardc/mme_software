set VSVersion=/p:VisualStudioVersion=12.0

c:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe "%~dp0..\..\SCME.DatabaseServer\SCME.DatabaseServer.csproj" %VSVersion% ^
/p:Configuration=Release /p:DeployOnBuild=True /p:Platform=x86 /p:OutputPath="%~dp0..\ReleaseBuild\DatabaseServer" /t:rebuild /verbosity:minimal %*

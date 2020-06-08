set VSVersion=/p:VisualStudioVersion=12.0

c:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe "%~dp0..\..\SCME.UI\SCME.UI.csproj" %VSVersion% ^
/p:Configuration=Release /p:DeployOnBuild=True /p:OutputPath="%~dp0..\ReleaseBuild\UI" /t:rebuild /verbosity:minimal %*

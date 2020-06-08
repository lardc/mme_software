set VSVersion=/p:VisualStudioVersion=12.0

c:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe "%~dp0..\SCME.Service\SCME.Service.csproj" %VSVersion% ^
/p:Configuration=Sctu /p:DeployOnBuild=True /p:OutputPath="%~dp0SCTU\Core" /t:rebuild /verbosity:minimal %*

call "%~dp0ReleaseHelpers\Build Release UI Sctu.bat"

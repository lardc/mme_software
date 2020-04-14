@if (@this==@isBatch) @then
    setlocal enableextensions

    set "file=%~f1"
	set "outFile=%~f2"
    if not exist "%file%" goto :eof

    cscript //nologo //e:jscript "%~f0" /file:"%file%" /outFile:"%outFile%"

    endlocal

    exit /b
@end
	var fso = new ActiveXObject("Scripting.FileSystemObject");
    var file = WScript.Arguments.Named.Item('file').replace(/\\/g,'\\\\');
	var outFile = WScript.Arguments.Named.Item('outFile').replace(/\\/g,'\\\\');
    var wmi = GetObject('winmgmts:{impersonationLevel=impersonate}!\\\\.\\root\\cimv2')
    var files = new Enumerator(wmi.ExecQuery('Select Version from CIM_datafile where name=\''+file+'\'')) 

    while (!files.atEnd()){
		var f = fso.CreateTextFile(outFile, true);
		f.Write(files.item().Version);
		f.Close();
		break;
    };
	


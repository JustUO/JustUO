@SET CURPATH=%~dp0
@SET CSCPATH=%windir%\Microsoft.NET\Framework\v4.0.30319\

@SET SRVPATH=%CURPATH%Server\
@SET SCRPATH=%CURPATH%Scripts\

@TITLE: JustUO - http://www.PlayUO.org

::##########

@ECHO:
@ECHO: Step 2 - Compile JustUO (MONO)
@ECHO:

@PAUSE

::#Append .MONO for people who test on WIN and host on MONO
@DEL "%CURPATH%JustUO.MONO.exe"

@ECHO ON

%CSCPATH%csc.exe /win32icon:"%SRVPATH%justuo.ico" /r:"%CURPATH%OpenUO.Core.dll" /r:"%CURPATH%OpenUO.Ultima.dll" /r:"%CURPATH%OpenUO.Ultima.Windows.Forms.dll" /r:"%CURPATH%SevenZipSharp.dll" /target:exe /out:"%CURPATH%JustUO.MONO.exe" /recurse:"%SRVPATH%*.cs" /d:MONO /d:Framework_4_0 /d:JustUO /nowarn:0618 /debug /nologo /optimize /unsafe

@ECHO OFF

@ECHO:
@ECHO: Done!
@ECHO:

@PAUSE

@CLS


::##########

@ECHO:
@ECHO: Step 3 - Compile Scripts (MONO)
@ECHO:

@PAUSE

@DEL "%SCRPATH%Output\Scripts.CS.dll"

@ECHO ON

%CSCPATH%csc.exe /r:"%CURPATH%JustUO.MONO.exe" /r:"%CURPATH%OpenUO.Core.dll" /r:"%CURPATH%OpenUO.Ultima.dll" /r:"%CURPATH%OpenUO.Ultima.Windows.Forms.dll" /r:"%CURPATH%SevenZipSharp.dll" /target:library /out:"%SCRPATH%Output\Scripts.CS.dll" /recurse:"%SCRPATH%*.cs" /d:MONO /d:Framework_4_0 /d:JustUO /nowarn:0618 /debug /nologo /optimize /unsafe

@ECHO OFF

@ECHO:
@ECHO: Done!
@ECHO:

@PAUSE

@CLS


::##########

@ECHO:
@ECHO: Step 4 - Launch JustUO (MONO)
@ECHO:

@PAUSE

@CLS

@ECHO OFF

%CURPATH%JustUO.MONO.exe

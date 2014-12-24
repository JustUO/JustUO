@SET CURPATH=%~dp0
@SET CSCPATH=%windir%\Microsoft.NET\Framework\v4.0.30319\

@SET SRVPATH=%CURPATH%Server\
@SET SCRPATH=%CURPATH%Scripts\

@TITLE: JustUO - http://www.PlayUO.org

::##########

@ECHO:
@ECHO: Ready to Compile JustUO
@ECHO:

@PAUSE

@DEL "%CURPATH%JustUO.exe"

@ECHO ON

%CSCPATH%csc.exe /win32icon:"%SRVPATH%justuo.ico" /r:"%CURPATH%OpenUO.Core.dll" /r:"%CURPATH%OpenUO.Ultima.dll" /r:"%CURPATH%OpenUO.Ultima.Windows.Forms.dll" /r:"%CURPATH%SevenZipSharp.dll" /target:exe /out:"%CURPATH%JustUO.exe" /recurse:"%SRVPATH%*.cs" /d:Framework_4_0 /d:JustUO /nowarn:0618 /debug /nologo /optimize /unsafe

@ECHO OFF

@ECHO:
@ECHO: Done!
@ECHO:

@PAUSE

@CLS


@ECHO:
@ECHO: Ready to start JustUO Configuration Utility
@ECHO:

@PAUSE

@CLS

@ECHO OFF

start %CURPATH%JustUOStartup.exe

@ECHO OFF
EXIT /B

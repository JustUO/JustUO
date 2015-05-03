@SET CURPATH=%~dp0
@SET CSCPATH=%windir%\Microsoft.NET\Framework\v4.0.30319\

@SET SRVPATH=%CURPATH%Server\
@SET SCRPATH=%CURPATH%Scripts\

@TITLE: JustUO - http://www.PlayUO.org

::##########

@ECHO:
@ECHO: Ready to Compile JustUO (WIN)
@ECHO:

@PAUSE

@DEL "%CURPATH%JustUO.exe"

@ECHO ON

%CSCPATH%csc.exe /win32icon:"%SRVPATH%justuo.ico" /r:"%CURPATH%OpenUO.Core.dll" /r:"%CURPATH%OpenUO.Ultima.dll" /r:"%CURPATH%OpenUO.Ultima.Windows.Forms.dll" /r:"%CURPATH%SevenZipSharp.dll" /target:exe /out:"%CURPATH%JustUO.exe" /recurse:"%SRVPATH%*.cs" /d:JustUO /d:Framework_4_0 /d:NEWTIMERS /d:NEWPARENT /nowarn:618 /debug /nologo /optimize /unsafe

@ECHO OFF

@ECHO:
@ECHO: Done!
@ECHO:

@PAUSE

@CLS


@ECHO:
@ECHO: The server core has been compiled. 
@ECHO: If you have not run JustUOStartup.exe, you should run that now. 
@ECHO: If you have already done this, you can just 
@ECHO: run the JustUO.exe to start your server.
@ECHO: 
@ECHO: Thanks, and Enjoy!
@ECHO:

@PAUSE

EXIT /B

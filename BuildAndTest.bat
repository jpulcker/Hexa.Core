echo Setting VS 2010 Environment...
if exist "%programfiles(x86)%\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" call "%programfiles(x86)%\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86
if exist "%programfiles%\Microsoft Visual Studio 10.0\VC\vcvarsall.bat"	call "%programfiles%\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86
if exist "%programfiles(x86)%\Microsoft Visual Studio 11.0\VC\vcvarsall.bat" call "%programfiles(x86)%\Microsoft Visual Studio 11.0\VC\vcvarsall.bat" x86
if exist "%programfiles%\Microsoft Visual Studio 11.0\VC\vcvarsall.bat"	call "%programfiles%\Microsoft Visual Studio 11	.0\VC\vcvarsall.bat" x86
echo Building solution...
call msbuild.exe BuildAndTest.msbuild /p:"EnableTests=true"
pause
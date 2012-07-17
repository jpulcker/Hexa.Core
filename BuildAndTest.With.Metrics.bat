echo Setting VS 2010 Environment...
if exist "%programfiles(x86)%\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" call "%programfiles(x86)%\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86
if exist "%programfiles%\Microsoft Visual Studio 10.0\VC\vcvarsall.bat"	call "%programfiles%\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86
if exist "%programfiles(x86)%\Microsoft Visual Studio 11.0\VC\vcvarsall.bat" call "%programfiles(x86)%\Microsoft Visual Studio 11.0\VC\vcvarsall.bat" x86
if exist "%programfiles%\Microsoft Visual Studio 11.0\VC\vcvarsall.bat"	call "%programfiles%\Microsoft Visual Studio 11	.0\VC\vcvarsall.bat" x86
echo Building solution...
call msbuild.exe BuildAndTest.msbuild /fl /p:"EnableMetrics=true" /p:"EnableFxCop=true" /p:"EnableSimian=true" /p:"EnableTests=true"
pause
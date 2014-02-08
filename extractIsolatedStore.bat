@echo off
@setlocal enableextensions enabledelayedexpansion

set wp7AppId=e45afa26-ada4-4435-a63d-97390c52f337
set wp8AppId=3715797b-f831-4e64-98a4-39446456c0b5

echo Running script to grab Isolated Store data from target emulator or device.

echo Make sure that the target emulator or device is running and has the application installed. It's not necessary that it's attached to Visual Studio.

"C:\Program Files (x86)\Microsoft SDKs\Windows Phone\v8.0\Tools\IsolatedStorageExplorerTool\"ISETool.exe EnumerateDevices

set /P TARGETDEVICE=Please input target emulator or device from the list : %=%
echo Target is %TARGETDEVICE%

:versionSelection
set /P TARGETVERSION=Is this for the WP7 or WP8 version? (7/8) : %=%

if "%TARGETVERSION%" == "7" (
    set TARGETID=%wp7AppId%
    echo Target version is %TARGETVERSION%
    echo App ID: !TARGETID!
    goto beginDataGrab
)
if "%TARGETVERSION%" == "8" (
    set TARGETID=%wp8AppId%
    echo Target version is %TARGETVERSION%
    echo App ID: !TARGETID!
    goto beginDataGrab
)

echo Incorrect input.
goto versionSelection

:beginDataGrab
echo Beginning Isolated Store data extraction and saving to the testdata directory.

"C:\Program Files (x86)\Microsoft SDKs\Windows Phone\v8.0\Tools\IsolatedStorageExplorerTool\"ISETool.exe ts deviceindex:%TARGETDEVICE% !TARGETID! "testdata"
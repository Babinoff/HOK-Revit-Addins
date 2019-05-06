@echo off
echo.
echo Signing Dlls...
setlocal ENABLEEXTENSIONS

set DLL_PATH=%1
set PFX_PATH="C:\Users\ksobon\source\repos\HOK-Revit-Addins\_cert\archilabCertificate.pfx"
set KEY_NAME="HKEY_LOCAL_MACHINE\SOFTWARE\HOK"
set VALUE_NAME="certificatePassword"

for /f "tokens=3" %%a in ('%windir%\Sysnative\reg query %KEY_NAME% /V %VALUE_NAME% ^|findstr /ri "REG_SZ"') do set PFX_PASS=%%a

"C:\Program Files (x86)\Windows Kits\10\bin\10.0.15063.0\x86\signtool.exe" sign /f "%PFX_PATH%" /p %PFX_PASS% /t http://timestamp.comodoca.com/authenticode %DLL_PATH%

endlocal
REM start "" WinAppDriver.exe
call start "" /b "C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe" & call dotnet test --logger:trx
REM start driver
REM start "" /b "C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe"
REM timeout /t 5 > NUL

REM start test
REM start "" dotnet  test --logger:trx
REM PowerShell [-noexit] -executionpolicy bypass -File 1.ps1
REM PowerShell Start-Job dotnet "test --logger:trx"
REM cmd /C 'start /B "C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe" & start /B ping dotnet  test --logger:trx'


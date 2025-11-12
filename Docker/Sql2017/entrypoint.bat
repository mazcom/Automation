@echo off
REM Запуск SQL Server
start /wait C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\Binn\sqlservr.exe &

REM Ожидание запуска сервера
timeout /t 15 /nobreak

REM Изменение пароля для пользователя sa на пустой
sqlcmd -S localhost -U sa -P "YourInitialPassword" -Q "ALTER LOGIN sa WITH PASSWORD=''"
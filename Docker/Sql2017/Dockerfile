# Используем базовый образ Windows Server Core с поддержкой SQL Server 2017
FROM mcr.microsoft.com/mssql/server:2017-latest-windows

# Устанавливаем переменные среды для конфигурации SQL Server
ENV MSSQL_PID=Developer \
    ACCEPT_EULA=Y \
    SA_PASSWORD=""

# Открываем порты для SQL Server
EXPOSE 1433

# Копируем скрипт и файл инициализации в контейнер
COPY ./entrypoint.bat C:/entrypoint.bat

# Команда для запуска SQL Server
CMD ["cmd", "/C", "C:/entrypoint.bat"]
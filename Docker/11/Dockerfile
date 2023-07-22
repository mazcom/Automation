FROM mcr.microsoft.com/mssql/server:2022-latest
COPY ./script1.sql .
ENV ACCEPT_EULA Y
ENV SA_PASSWORD ##$12345Ab!
RUN /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P ##$12345Ab! -i script1.sql
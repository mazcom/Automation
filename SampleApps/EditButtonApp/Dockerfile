# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
COPY EditButtonApp/*.csproj ./EditButtonApp/
COPY EditButtonAppTest/*.csproj ./EditButtonAppTest/
RUN dotnet restore

# copy full solution over
COPY . .
RUN dotnet build

# create a new layer from the build later
FROM build AS publish

# set the working directory to be the web api project
WORKDIR /app/EditButtonAppTest

# publish the web api project to a directory called out
RUN dotnet publish -c Release -o out



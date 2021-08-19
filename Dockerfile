#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY EventsApi.sln ./
COPY EventsApi/*.csproj ./EventsApi/
COPY EventsApi.CommandHandlers/*.csproj ./EventsApi.CommandHandlers/
COPY EventsApi.Commands/*.csproj ./EventsApi.Commands/
COPY EventsApi.Events/*.csproj ./EventsApi.Events/
COPY EventsApi.Commands/*.csproj ./EventsApi.Commands/
COPY EventsApi.Messages/*.csproj ./EventsApi.Messages/
COPY EventsApi.Services/*.csproj ./EventsApi.Services/
COPY EventsApi.AcceptanceTests/*.csproj ./EventsApi.AcceptanceTests/
COPY EventsApi.UnitTests/*.csproj ./EventsApi.UnitTests/

RUN dotnet restore
COPY . .
WORKDIR "/src/EventsApi"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventsApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventsApi.dll"]
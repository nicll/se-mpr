# Unter https://aka.ms/customizecontainer erfahren Sie, wie Sie Ihren Debugcontainer anpassen und wie Visual Studio dieses Dockerfile verwendet, um Ihre Images f�r ein schnelleres Debuggen zu erstellen.

# Diese Stufe wird verwendet, wenn sie von VS im Schnellmodus ausgef�hrt wird (Standardeinstellung f�r Debugkonfiguration).
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Diese Stufe wird zum Erstellen des Dienstprojekts verwendet.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OrleansPrototype/OrleansPrototype.ClientService/OrleansPrototype.ClientService.csproj", "OrleansPrototype/OrleansPrototype.ClientService/"]
RUN dotnet restore "./OrleansPrototype/OrleansPrototype.ClientService/OrleansPrototype.ClientService.csproj"
COPY . .
WORKDIR "/src/OrleansPrototype/OrleansPrototype.ClientService"
RUN dotnet build "./OrleansPrototype.ClientService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Diese Stufe wird verwendet, um das Dienstprojekt zu ver�ffentlichen, das in die letzte Phase kopiert werden soll.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OrleansPrototype.ClientService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Diese Stufe wird in der Produktion oder bei Ausf�hrung von VS im regul�ren Modus verwendet (Standard, wenn die Debugkonfiguration nicht verwendet wird).
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrleansPrototype.ClientService.dll"]
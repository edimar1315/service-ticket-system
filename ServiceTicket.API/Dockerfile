FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ServiceTicket.API/ServiceTicket.API.csproj ServiceTicket.API/
COPY ServiceTicket.Core/ServiceTicket.Core.csproj ServiceTicket.Core/
COPY ServiceTicket.Infrastructure/ServiceTicket.Infrastructure.csproj ServiceTicket.Infrastructure/
RUN dotnet restore ServiceTicket.API/ServiceTicket.API.csproj

COPY . .
RUN dotnet publish ServiceTicket.API/ServiceTicket.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ServiceTicket.API.dll"]

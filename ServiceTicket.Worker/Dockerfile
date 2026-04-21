FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ServiceTicket.Worker/ServiceTicket.Worker.csproj ServiceTicket.Worker/
COPY ServiceTicket.Core/ServiceTicket.Core.csproj ServiceTicket.Core/
COPY ServiceTicket.Infrastructure/ServiceTicket.Infrastructure.csproj ServiceTicket.Infrastructure/
RUN dotnet restore ServiceTicket.Worker/ServiceTicket.Worker.csproj

COPY . .
RUN dotnet publish ServiceTicket.Worker/ServiceTicket.Worker.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ServiceTicket.Worker.dll"]

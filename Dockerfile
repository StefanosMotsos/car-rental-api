FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY CarRentalApp/*.csproj CarRentalApp/
RUN dotnet restore CarRentalApp/CarRentalApp.csproj

COPY CarRentalApp/ CarRentalApp/

WORKDIR /src/CarRentalApp
RUN dotnet publich -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app .

#EXPOSE 8081

ENTRYPOINT ["dotnet", "CarRentalApp.dll"]
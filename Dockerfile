FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["scoutify-features-api.csproj", "."]
RUN dotnet restore "./scoutify-features-api.csproj"
COPY . .
RUN dotnet publish "./scoutify-features-api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "scoutify-features-api.dll"]

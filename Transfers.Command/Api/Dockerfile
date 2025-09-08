# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia toda a solução
COPY . .

# Restaura e publica o projeto da API
RUN dotnet restore "Transfers/Api/Api.csproj"
RUN dotnet publish "Transfers/Api/Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8083
EXPOSE 8083
ENTRYPOINT ["dotnet", "Api.dll"]
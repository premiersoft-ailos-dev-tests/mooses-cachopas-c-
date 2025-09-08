# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia toda a solução (melhor para projetos referenciados)
COPY . .

# Restaura e publica o projeto da API de Command
RUN dotnet restore "Api/Api.csproj"
RUN dotnet publish "Api/Api.csproj" -c Release -o /app/publish

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Api.dll"]
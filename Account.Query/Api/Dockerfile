# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "Bankmore.Accounts.Query.Api/Bankmore.Accounts.Query.Api.csproj"
RUN dotnet publish "Bankmore.Accounts.Query.Api/Bankmore.Accounts.Query.Api.csproj" -c Release -o /app/publish

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Bankmore.Accounts.Query.Api.dll"]
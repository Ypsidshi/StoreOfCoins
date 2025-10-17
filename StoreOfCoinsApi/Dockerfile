FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5174
ENV ASPNETCORE_URLS=http://+:5174

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем csproj и восстанавливаем зависимости
COPY StoreOfCoinsApi.csproj ./
RUN dotnet restore "StoreOfCoinsApi.csproj"

# Копируем остальной код и публикуем
COPY . .
RUN dotnet publish "StoreOfCoinsApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StoreOfCoinsApi.dll"]



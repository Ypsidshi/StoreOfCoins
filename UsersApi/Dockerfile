FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5180
ENV ASPNETCORE_URLS=http://+:5180

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY UsersApi.csproj ./
RUN dotnet restore UsersApi.csproj
COPY . .
RUN dotnet publish UsersApi.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UsersApi.dll"]



FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Faker.Api/Faker.Api.csproj", "Faker.Api/"]
RUN dotnet restore  -s "https://api.nuget.org/v3/index.json" "Faker.Api/Faker.Api.csproj"
COPY . .
WORKDIR "/src/Faker.Api"
RUN dotnet build "Faker.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Faker.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN ln -sf /usr/share/zoneinfo/Europe/Moscow /etc/localtime
ENTRYPOINT dotnet Faker.Api.dll --environment=$dotnet

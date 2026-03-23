FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ZuriFluxAPI/ZuriFluxAPI.csproj", "ZuriFluxAPI/"]
RUN dotnet restore "ZuriFluxAPI/ZuriFluxAPI.csproj"
COPY . .
WORKDIR "/src/ZuriFluxAPI"
RUN dotnet build "ZuriFluxAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ZuriFluxAPI.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ZuriFluxAPI.dll"]

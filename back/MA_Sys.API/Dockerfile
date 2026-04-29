FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY MA_Sys.API.csproj ./
RUN dotnet restore MA_Sys.API.csproj

COPY . ./
RUN dotnet publish MA_Sys.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV PORT=8080

COPY --from=build /app/publish ./

RUN mkdir -p /data/uploads/academias

EXPOSE 8080

ENTRYPOINT ["dotnet", "MA_Sys.API.dll"]

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY InvestEasy.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish InvestEasy.csproj -c Release -o /app --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app ./

# The SQLite database and the data protection keys are stored in /app/Data.
# Mount a volume there so users and their data survive a redeploy.
RUN mkdir -p /app/Data && chown -R $APP_UID /app/Data
VOLUME /app/Data

USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "InvestEasy.dll"]

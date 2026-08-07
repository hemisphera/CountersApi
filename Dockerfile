# --- build stage ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first to leverage Docker layer caching
COPY CounterAPI/CounterAPI.csproj CounterAPI/
COPY CounterAPI.Common/CounterAPI.Common.csproj CounterAPI.Common/
COPY CounterAPI.LocalFile/CounterAPI.LocalFile.csproj CounterAPI.LocalFile/
COPY CounterAPI.SqlStorage/CounterAPI.SqlStorage.csproj CounterAPI.SqlStorage/
COPY CounterAPI.AzureTable/CounterAPI.AzureTable.csproj CounterAPI.AzureTable/
COPY CounterAPI.sln ./

RUN dotnet restore "CounterAPI.sln"

# Copy the rest of the source and publish
COPY . .
RUN dotnet publish CounterAPI/CounterAPI.csproj -c Release -o /app/publish /p:UseAppHost=false

# --- runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "CounterAPI.dll"]
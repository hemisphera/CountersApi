# --- build stage ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first to leverage Docker layer caching
COPY CountersApi/CountersApi.csproj CountersApi/
COPY CountersApi.Common/CountersApi.Common.csproj CountersApi.Common/
COPY CountersApi.LocalFile/CountersApi.LocalFile.csproj CountersApi.LocalFile/
COPY CountersApi.DynamoDb/CountersApi.DynamoDb.csproj CountersApi.DynamoDb/
COPY CountersApi.sln ./

RUN dotnet restore "CountersApi.sln"

# Copy the rest of the source and publish
COPY . .
RUN dotnet publish CountersApi/CountersApi.csproj -c Release -o /app/publish /p:UseAppHost=false

# --- runtime stage ---
# The app self-bootstraps the Lambda runtime via Amazon.Lambda.AspNetCoreServer.Hosting
# (AddAWSLambdaHosting), so it needs no Lambda Web Adapter or AWS Lambda base image.
# The same image runs on Lambda (Lambda runtime client) and standalone (Kestrel on 8080).
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENV PORT=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CountersApi.dll"]
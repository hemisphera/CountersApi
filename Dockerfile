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
# Lambda Web Adapter bridges the Lambda Runtime API to the container's HTTP
# port, so the same image can run on Lambda, Fargate, or EC2 unchanged.
# It ships its own Runtime Interface Client, so no AWS Lambda base image is needed.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
COPY --from=public.ecr.aws/awsguru/aws-lambda-adapter:1.0.1 /lambda-adapter /opt/extensions/lambda-adapter
ENV PORT=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CountersApi.dll"]
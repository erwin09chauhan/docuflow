FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/DocuFlow.Api/DocuFlow.Api.csproj", "src/DocuFlow.Api/"]
COPY ["src/DocuFlow.Application/DocuFlow.Application.csproj", "src/DocuFlow.Application/"]
COPY ["src/DocuFlow.Domain/DocuFlow.Domain.csproj", "src/DocuFlow.Domain/"]
COPY ["src/DocuFlow.Infrastructure/DocuFlow.Infrastructure.csproj", "src/DocuFlow.Infrastructure/"]

RUN dotnet restore "src/DocuFlow.Api/DocuFlow.Api.csproj"

COPY . .

RUN dotnet publish "src/DocuFlow.Api/DocuFlow.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DocuFlow.Api.dll"]
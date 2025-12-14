FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base

RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/* && \
    useradd --no-create-home --uid 1001 appuser

WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["OnlineSurvey.sln", "./"]
COPY ["src/OnlineSurvey.Domain/OnlineSurvey.Domain.csproj", "src/OnlineSurvey.Domain/"]
COPY ["src/OnlineSurvey.Application/OnlineSurvey.Application.csproj", "src/OnlineSurvey.Application/"]
COPY ["src/OnlineSurvey.Infrastructure/OnlineSurvey.Infrastructure.csproj", "src/OnlineSurvey.Infrastructure/"]
COPY ["src/OnlineSurvey.Api/OnlineSurvey.Api.csproj", "src/OnlineSurvey.Api/"]

RUN dotnet restore "src/OnlineSurvey.Api/OnlineSurvey.Api.csproj"

COPY src/ src/

WORKDIR "/src/src/OnlineSurvey.Api"
RUN dotnet build "OnlineSurvey.Api.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OnlineSurvey.Api.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER appuser

ENTRYPOINT ["dotnet", "OnlineSurvey.Api.dll"]

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY NewJira.slnx .
COPY NewJira/NewJira.csproj NewJira/
COPY NewJira.Application/NewJira.Application.csproj NewJira.Application/
COPY NewJira.Domain/NewJira.Domain.csproj NewJira.Domain/
COPY NewJira.Infrastructure/NewJira.Infrastructure.csproj NewJira.Infrastructure/
RUN dotnet restore NewJira/NewJira.csproj

COPY NewJira/ NewJira/
COPY NewJira.Application/ NewJira.Application/
COPY NewJira.Domain/ NewJira.Domain/
COPY NewJira.Infrastructure/ NewJira.Infrastructure/
RUN dotnet publish NewJira/NewJira.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NewJira.dll"]

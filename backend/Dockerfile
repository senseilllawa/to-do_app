FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TodoApp.sln ./
COPY TodoApp.Api/*.csproj ./TodoApp.Api/
COPY TodoApp.Core/*.csproj ./TodoApp.Core/
COPY TodoApp.Services/*.csproj ./TodoApp.Services/
COPY TodoApp.DataAccess/*.csproj ./TodoApp.DataAccess/
COPY TodoApp.Tests/*.csproj ./TodoApp.Tests/
RUN dotnet restore TodoApp.Api/TodoApp.Api.csproj

COPY . .
RUN dotnet publish TodoApp.Api/TodoApp.Api.csproj -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TodoApp.Api.dll"]

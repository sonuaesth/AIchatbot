FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY AiChat.Backend.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5184
EXPOSE 5184

ENTRYPOINT ["dotnet", "AiChat.Backend.dll"]

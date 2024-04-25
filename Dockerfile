# .NET SDK 6.0 kullan
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app


COPY src/*.csproj ./

RUN dotnet clean

RUN dotnet restore

COPY src/ ./

RUN dotnet dev-certs https --trust

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0

WORKDIR /app

COPY --from=build-env /app/out .

EXPOSE 5000

#RUN dotnet dev-certs https --trust

ENTRYPOINT ["dotnet", "src.dll", "--urls", "http://0.0.0.0:5000", "dev-certs", "https", "--trust"]

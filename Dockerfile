# 1. Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Proje dosyalarını kopyala ve restore et
COPY *.csproj ./
RUN dotnet restore

# Tüm dosyaları kopyala ve publish et
COPY . ./
RUN dotnet publish -c Release -o out

# 2. Runtime aşaması
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

# Projenin çalışacağı portu aç
EXPOSE 5258

# Projeyi çalıştır
ENTRYPOINT ["dotnet", "CRMSystem.dll"]

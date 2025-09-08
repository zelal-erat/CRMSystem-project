# 1️⃣ Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Solution ve proje dosyalarını kopyala
COPY *.sln ./
COPY *.csproj ./
RUN dotnet restore

# Tüm dosyaları kopyala ve publish et
COPY . .
RUN dotnet publish CRMSystem.csproj -c Release -o /app/out

# 2️⃣ Runtime aşaması
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out ./

# Render ve genel PaaS platformları portu 80 bekler
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

# Uygulamayı çalıştır
ENTRYPOINT ["dotnet", "CRMSystem.dll"]

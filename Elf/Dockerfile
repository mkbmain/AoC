FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["Elf/Elf.csproj", "Elf/"]
RUN dotnet restore "Elf/Elf.csproj"
COPY . .
WORKDIR "/src/Elf"
RUN dotnet build "Elf.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Elf.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Elf.dll"]

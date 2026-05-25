FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY . ./
RUN dotnet restore "MicroServiceProduct.csproj"
RUN dotnet publish "MicroServiceProduct.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS="http://+:80"
EXPOSE 80
ENTRYPOINT ["dotnet", "MicroServiceProduct.dll"]

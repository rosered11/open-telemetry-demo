# Stage 1: Build
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS builder
ARG TARGETARCH
ARG CACHE_BUST=1

WORKDIR /app/
COPY ./src/product/ ./

RUN dotnet restore ./Product.Api/Product.Api.csproj -r linux-musl-$TARGETARCH
RUN dotnet publish ./Product.Api/Product.Api.csproj -r linux-musl-$TARGETARCH --no-restore -c Release -o /out

# Stage 2: Runtime (includes .NET runtime and ASP.NET)
FROM --platform=linux/arm64 mcr.microsoft.com/dotnet/aspnet:8.0-alpine3.20

WORKDIR /app/
COPY --from=builder /out ./

ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false

EXPOSE 80

RUN ls -l

ENTRYPOINT ["./Product.Api"]

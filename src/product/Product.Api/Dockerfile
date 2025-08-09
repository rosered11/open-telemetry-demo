# Stage 1: Build
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS builder
ARG TARGETARCH

WORKDIR /app/
COPY ./src/product/ ./

RUN dotnet restore ./Product.Api/Product.Api.csproj -r linux-musl-$TARGETARCH
RUN dotnet publish ./Product.Api/Product.Api.csproj -r linux-musl-$TARGETARCH --no-restore -c Release -o /out

# Stage 2: Runtime (includes .NET runtime and ASP.NET)
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine3.20

WORKDIR /app/
COPY --from=builder /out ./

ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false

EXPOSE 80

ENTRYPOINT ["./Product.Api"]

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS builder
ARG TARGETARCH

WORKDIR /app/
COPY ./src/order/ ./
COPY ./src/protobuf/ ../protobuf/

RUN dotnet restore ./Order.Api/Order.Api.csproj
RUN dotnet publish ./Order.Api/Order.Api.csproj --no-restore -c Release -o /out

FROM --platform=linux/${TARGETARCH} mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

RUN apt-get update && apt-get install -y librdkafka1

WORKDIR /app/
COPY --from=builder /out ./

ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false

EXPOSE 80

ENTRYPOINT ["dotnet", "./Order.Api.dll"]

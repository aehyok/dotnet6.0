# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md
# 多阶段构建
# 第一阶段只关注编译
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG TARGETARCH

# 先拷贝源码
WORKDIR /source

COPY  net8/.  .

# 设置当前工作目录
WORKDIR Services/SystemService/aehyok.SystemService
# 编译当前项目
RUN dotnet publish "aehyok.SystemService.csproj" -o /app -f net8.0


# Enable globalization and time zones:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/enable-globalization.md
# final stage/image
# 第二阶段只关注运行（产物是第一阶段编译的）
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
EXPOSE 8080

WORKDIR /app
COPY net8/etc/.  .
COPY --from=build /app .

ENTRYPOINT ["./aehyok.SystemService"]
FROM mcr.microsoft.com/dotnet/sdk as build

WORKDIR /app

RUN apt-get update && \
    apt-get install git

COPY . .

RUN ls -la

RUN dotnet build -c Release


FROM mcr.microsoft.com/dotnet/runtime as runtime

WORKDIR /app

COPY --from=build /app/bin/Release/* /app

RUN rm yamlmerge.pdb
RUN rm yamlmerge.deps.json

RUN ls -la

ENTRYPOINT ["/app/yamlmerge"]

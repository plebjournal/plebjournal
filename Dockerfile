FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /source

COPY PlebJournal.sln .
COPY PlebJournal.Domain/. ./PlebJournal.Domain/
COPY PlebJournal.Identity/. ./PlebJournal.Identity/
COPY PlebJournal.Domain.Tests/. ./PlebJournal.Domain.Tests/
COPY PlebJournal.Db/. ./PlebJournal.Db/
COPY PlebJournal.Web/. ./PlebJournal.Web/

RUN dotnet build
RUN dotnet test
RUN dotnet publish PlebJournal.Web/PlebJournal.Web.fsproj -c Release -o /app --self-contained false

FROM mcr.microsoft.com/dotnet/aspnet:7.0

EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080


WORKDIR /app
COPY --from=build /app .
CMD ["dotnet", "PlebJournal.Web.dll"]

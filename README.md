## Запуск

1. Создать базу `railway_db` и выполнить `Railway/railway_db.sql`.
2. Проверить строку подключения в `Railway/appsettings.json`.
3. Запустить:

```bash
dotnet run --project Railway/Railway.csproj
```

## Тесты

```bash
dotnet test Railway.Tests/Railway.Tests.csproj
```

Алгоритмы находятся в `Railway/Services/TrainAlgorithms.cs`, тесты — в `Railway.Tests/TrainAlgorithmsTests.cs`.

# VideoTube

Приложение для загрузки и просмотра видео. **Авторизация отключена для демонстрации.**

## Текущий статус

**⚠️ ВНИМАНИЕ: Авторизация отключена в демо-режиме**

Все функции авторизации закомментированы для упрощения работы с приложением. Приложение работает в демо-режиме без необходимости настройки Google OAuth.

## Настройка Google OAuth (ЗАКОММЕНТИРОВАНО)

### 1. Создание проекта в Google Cloud Console

1. Перейдите в [Google Cloud Console](https://console.cloud.google.com/)
2. Создайте новый проект или выберите существующий
3. Включите Google+ API

### 2. Настройка OAuth 2.0

1. В меню слева выберите "APIs & Services" > "Credentials"
2. Нажмите "Create Credentials" > "OAuth 2.0 Client IDs"
3. Выберите "Web application"
4. Добавьте авторизованные URI перенаправления:
   - `https://localhost:7185/api/auth/google-callback`
   - `http://localhost:7185/api/auth/google-callback`
5. Сохраните Client ID и Client Secret

### 3. Обновление конфигурации

Обновите файл `VideoTube.Server/appsettings.json`:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    }
  },
  "Jwt": {
    "Key": "your-super-secret-key-with-at-least-32-characters-change-this-in-production",
    "Issuer": "VideoTube",
    "Audience": "VideoTubeClient"
  }
}
```

## Запуск приложения

### Предварительные требования

- .NET 9.0 SDK

### Запуск

1. **Серверная часть:**
   ```bash
   cd VideoTube.Server
   dotnet run
   ```

2. **Клиентская часть:**
   ```bash
   cd VideoTube.Client
   dotnet run
   ```

3. Откройте браузер и перейдите по адресу: `https://localhost:7185`

## Функциональность

- ✅ Демо-режим без авторизации
- ✅ Загрузка видео (до 500MB)
- ✅ Просмотр списка видео
- ✅ Скачивание видео
- ✅ Удаление видео
- ❌ ~~Аутентификация через Google~~ (отключено)
- ❌ ~~JWT токены для сессий~~ (отключено)
- ❌ ~~Защищенные маршруты~~ (отключено)

## Структура проекта

```
VideoTube/
├── VideoTube.Server/          # ASP.NET Core Web API
│   ├── Controllers/           # API контроллеры
│   │   ├── AuthController.cs  # Контроллер аутентификации (закомментирован)
│   │   └── VideoController.cs # Контроллер для видео
│   ├── Program.cs             # Конфигурация приложения
│   └── appsettings.json       # Конфигурация
└── VideoTube.Client/          # Blazor WebAssembly
    ├── Pages/                 # Страницы приложения
    │   ├── Home.razor         # Главная страница
    │   ├── Upload.razor       # Загрузка видео
    │   ├── AuthCallback.razor # Обработка callback (закомментирована)
    │   └── LoginError.razor   # Страница ошибок (закомментирована)
    ├── Services/              # Сервисы
    │   └── AuthService.cs     # Сервис аутентификации (модифицирован)
    └── Layout/                # Макеты
        └── NavMenu.razor      # Навигационное меню
```

## Восстановление авторизации

Для восстановления авторизации необходимо:

1. Раскомментировать код в `VideoTube.Server/Program.cs`
2. Раскомментировать `VideoTube.Server/Controllers/AuthController.cs`
3. Восстановить оригинальную логику в `VideoTube.Client/Services/AuthService.cs`
4. Раскомментировать страницы `AuthCallback.razor` и `LoginError.razor`
5. Восстановить проверки авторизации в `NavMenu.razor`, `Home.razor` и `Pages.razor`
6. Настроить Google OAuth согласно инструкции выше

## Безопасность

В демо-режиме:
- ❌ Нет проверки авторизации
- ❌ Нет защиты маршрутов
- ❌ Нет валидации токенов

При восстановлении авторизации:
- JWT токены с истечением срока действия (7 дней)
- Валидация токенов на сервере
- Безопасное хранение токенов в localStorage
- Защищенные маршруты для авторизованных пользователей

## Дальнейшее развитие

- Добавление базы данных для хранения информации о пользователях
- Реализация загрузки видео на сервер
- Добавление комментариев и лайков
- Система подписок и рекомендаций 
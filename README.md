# NewMyApp - Соціальна мережа

## Ідентифікаційний розділ

### Призначення системи

NewMyApp - це сучасна соціальна мережа, розроблена для створення та підтримки соціальних зв'язків між користувачами. Система дозволяє користувачам обмінюватися повідомленнями, створювати пости, формувати групи за інтересами та взаємодіяти з іншими учасниками платформи.

### Технічна специфікація

- **Автор**: [Ваше ім'я]
- **Платформа**: ASP.NET Core MVC
- **Мова програмування**: C#
- **База даних**: Microsoft SQL Server
- **Фреймворк**: .NET 8.0
- **Frontend**: Bootstrap 5, JavaScript

### Системні вимоги

- .NET 7.0 SDK или выше
- SQL Server (LocalDB или полная версия)
- Node.js и npm (для фронтенд-зависимостей)
- Visual Studio 2022 или Visual Studio Code

## Установка и запуск

1. Клонируйте репозиторий:

```bash
git clone https://github.com/yourusername/NewMyApp.git
cd NewMyApp
```

2. Восстановите NuGet-пакеты:

```bash
dotnet restore
```

3. Установите фронтенд-зависимости:

```bash
cd src/NewMyApp.Web
npm install
```

4. Настройте строку подключения к базе данных в `src/NewMyApp.Web/appsettings.json`

5. Примените миграции базы данных:

```bash
cd src/NewMyApp.Infrastructure
dotnet ef database update
```

6. Запустите приложение:

```bash
cd src/NewMyApp.Web
dotnet run
```

## Структура проекта

- `NewMyApp.Core` - Бизнес-логика и модели
- `NewMyApp.Infrastructure` - Доступ к данным и миграции
- `NewMyApp.Web` - Веб-приложение ASP.NET Core MVC

## Основные функции

1. Аутентификация и авторизация

- Регистрация пользователей
- Вход/выход
- Управление профилем

2. Система постов

- Создание/редактирование/удаление постов
- Комментарии
- Лайки
- Теги

3. Группы

- Создание групп
- Управление участниками
- Роли (админ, модератор, участник)
- Групповые посты

4. Рейтинг и сертификаты

- Система рейтинга пользователей
- Выдача сертификатов за достижения
- Топ пользователей

## API Документация

Подробная документация по API доступна по адресу `/swagger` после запуска приложения.

## Опис інтерфейсу

### Головна сторінка

- **Навігаційна панель**:
  - Логотип NewMyApp (перехід на головну)
  - Меню навігації:
    - Головна
    - Мої пости
    - Мої групи
    - Друзі
    - Пошук користувачів
    - Рейтинг
  - Поле пошуку
  - Профіль користувача

### Профіль користувача

- **Елементи керування**:
  - Аватар користувача
  - Інформація профілю
  - Налаштування профілю
  - Список друзів
  - Особисті пости

### Сторінка постів

- **Функціональність**:
  - Створення нового поста
  - Редагування існуючих постів
  - Додавання тегів
  - Завантаження медіа-контенту
  - Система лайків та коментарів

### Групи

- **Можливості**:
  - Створення нової групи
  - Керування учасниками
  - Публікація групового контенту
  - Налаштування прав доступу

### Система повідомлень

- **Функції**:
  - Особисті повідомлення
  - Групові чати
  - Сповіщення
  - Статуси повідомлень

## Структура бази даних

### Основні таблиці

#### Users

- Id (PK)
- UserName
- Email
- PasswordHash
- RegisterDate
- LastLoginDate

#### Posts

- Id (PK)
- UserId (FK)
- Content
- CreatedAt
- UpdatedAt
- IsPublic

#### Groups

- Id (PK)
- Name
- Description
- CreatedBy (FK)
- CreatedAt
- IsPublic

#### Comments

- Id (PK)
- PostId (FK)
- UserId (FK)
- Content
- CreatedAt

#### Friendships

- Id (PK)
- RequesterId (FK)
- AddresseeId (FK)
- Status
- CreatedAt

### Зв'язки між таблицями

- Users -> Posts (1:N)
- Users -> Groups (M:N через GroupMembers)
- Posts -> Comments (1:N)
- Users -> Users (M:N через Friendships)

## SQL Запити

### Пошук друзів користувача

```sql
SELECT u.*
FROM Users u
INNER JOIN Friendships f ON (u.Id = f.RequesterId OR u.Id = f.AddresseeId)
WHERE (f.RequesterId = @userId OR f.AddresseeId = @userId)
AND f.Status = 'Accepted';
```

### Отримання популярних постів

```sql
SELECT p.*, COUNT(l.Id) as LikesCount
FROM Posts p
LEFT JOIN Likes l ON p.Id = l.PostId
GROUP BY p.Id
HAVING COUNT(l.Id) > 10
ORDER BY LikesCount DESC;
```

### Активні користувачі в групі

```sql
SELECT u.*, COUNT(p.Id) as PostsCount
FROM Users u
INNER JOIN GroupMembers gm ON u.Id = gm.UserId
LEFT JOIN Posts p ON u.Id = p.UserId AND p.GroupId = gm.GroupId
WHERE gm.GroupId = @groupId
GROUP BY u.Id
ORDER BY PostsCount DESC;
```

## Вимоги до користувача

### Кваліфікація

- Базові навички роботи з комп'ютером
- Досвід роботи з Windows
- Розуміння принципів роботи веб-браузера
- Знання української мови (інтерфейс системи)

### Рекомендовані навички

- Базове розуміння соціальних мереж
- Вміння працювати з формами введення даних
- Навички роботи з файловою системою (для завантаження медіа)

### Безпека

- Дотримання правил безпечного використання паролів
- Розуміння принципів конфіденційності
- Обережність при обміні особистою інформацією

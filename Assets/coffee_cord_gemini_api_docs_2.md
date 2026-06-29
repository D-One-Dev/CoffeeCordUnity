
# 📚 Messenger API Documentation

**Базовый URL (Gateway):** `http://<host>:8080`
**WebSocket URL:** `ws://<host>:8080`

## 🔐 Аутентификация
Большинство REST-запросов (кроме логина и регистрации) требуют наличия JWT токена в заголовке `Authorization`.

**Формат заголовка:**
`Authorization: Bearer <твой_jwt_токен>`

---

## 🌐 1. REST API (HTTP)

### 👤 Auth & Users (Маршруты пользователей)

#### 1.1. Регистрация пользователя
*   **Метод:** `POST /register`
*   **Описание:** Создает нового пользователя в системе.
*   **Request Body:**
    ```json
    {
      "name": "Иван",
      "lastName": "Иванов",
      "email": "ivan@example.com",
      "password": "secure_password"
    }
    ```
*   **Response (201 Created):**
    ```json
    {
      "id": "uuid-пользователя",
      "name": "Иван",
      "lastname": "Иванов",
      "email": "ivan@example.com"
    }
    ```

#### 1.2. Авторизация (Логин)
*   **Метод:** `POST /login`
*   **Описание:** Проверяет учетные данные и выдает JWT токен.
*   **Request Body:**
    ```json
    {
      "email": "ivan@example.com",
      "password": "secure_password"
    }
    ```
*   **Response (200 OK):**
    ```json
    {
      "token": "eyJhbGciOiJIUzI1NiIsIn..."
    }
    ```
*   **Response (401 Unauthorized):** `{"error": "Invalid credentials"}`

#### 1.3. Получить свой профиль
*   **Метод:** `POST /me` *(согласно коду используется POST)*
*   **Требует Auth:** Да
*   **Response (200 OK):**
    ```json
    {
      "user": {
        "id": "uuid",
        "name": "Иван",
        "lastname": "Иванов",
        "email": "ivan@example.com"
      }
    }
    ```

#### 1.4. Найти пользователя по ID
*   **Метод:** `GET /users/{userId}`
*   **Требует Auth:** Да
*   **Response (200 OK):** Аналогично `/me`.

#### 1.5. Найти пользователя по Email
*   **Метод:** `GET /users?email={email}`
*   **Требует Auth:** Да
*   **Response (200 OK):** Аналогично `/me`.

---

### 🏢 Servers (Управление серверами)

#### 1.6. Создать сервер
*   **Метод:** `POST /servers`
*   **Требует Auth:** Да
*   **Request Body:**
    ```json
    {
      "name": "My Cool Server"
    }
    ```
*   **Response (201 Created):**
    ```json
    {
      "serverId": "uuid-сервера"
    }
    ```

#### 1.7. Получить участников сервера
*   **Метод:** `GET /servers/{serverId}/members`
*   **Требует Auth:** Да
*   **Response (200 OK):**
    ```json
    [
      "uuid-пользователя-1",
      "uuid-пользователя-2"
    ]
    ```

#### 1.8. Добавить участника на сервер
*   **Метод:** `POST /servers/{serverId}/members`
*   **Требует Auth:** Да
*   **Описание:** Можно передать либо `userId`, либо `email`.
*   **Request Body:**
    ```json
    {
      "email": "friend@example.com" 
    }
    // ИЛИ
    {
      "userId": "uuid-пользователя"
    }
    ```
*   **Response (200 OK):**
    ```json
    {
      "status": "success"
    }
    ```

#### 1.9. Создать текстовый канал на сервере
*   **Метод:** `POST /servers/{serverId}/channels`
*   **Требует Auth:** Да
*   **Request Body:**
    ```json
    {
      "name": "general"
    }
    ```
*   **Response (200 OK):**
    ```json
    {
      "serverId": "uuid-сервера",
      "channelId": "uuid-канала",
      "channelName": "general"
    }
    ```

---

## 🔌 2. WebSocket API (Real-time взаимодействие)

**Подключение:** `ws://<host>:8080/chat?token=<твой_jwt_токен>`
*При успешном подключении сервер пришлет текстовое сообщение: `connected`*

Все сообщения внутри сокета передаются в формате **JSON**. Обязательное поле для отправки — `"type"`.

### ➡️ Входящие события (Client -> Server)

Отправляются клиентом (Frontend/Mobile) на сервер.

| Тип (`type`) | Описание | Формат JSON |
| :--- | :--- | :--- |
| `send_message` | Отправка сообщения в канал или ЛС | `{"type": "send_message", "channelId": "uuid", "text": "Привет!"}` |
| `get_history` | Запрос истории сообщений | `{"type": "get_history", "channelId": "uuid", "limit": 50, "beforeTimestamp": 1700000000000}` *(beforeTimestamp опционален)* |
| `get_servers` | Получить список серверов юзера | `{"type": "get_servers"}` |
| `get_channels` | Запросить каналы сервера | `{"type": "get_channels", "serverId": "uuid"}` |
| `get_direct_chats` | Запросить список личных чатов | `{"type": "get_direct_chats"}` |
| `create_channel` | Создать канал через сокет | `{"type": "create_channel", "serverId": "uuid", "name": "news"}` |
| `add_member` | Добавить юзера через сокет | `{"type": "add_member", "serverId": "uuid", "userId": "uuid"}` |
| `ping` | Поддержание активности (Keep-alive) | `{"type": "ping"}` |

---

### ⬅️ Исходящие события (Server -> Client)

Асинхронные ответы и push-уведомления от сервера к клиенту. Клиент должен слушать поле `"type"`.

#### Уведомление о новом сообщении (Push)
Приходит всем участникам сервера (или личного чата), когда кто-то пишет сообщение.
```json
{
  "type": "new_message",
  "serverId": "uuid-сервера (или пустая строка для ЛС)",
  "channelId": "uuid-канала",
  "message": {
    "messageId": "uuid-сообщения",
    "senderId": "uuid-отправителя",
    "text": "Привет!",
    "createdAt": 1700000000000
  }
}
```

#### Подтверждение отправки своего сообщения
Приходит автору в ответ на `send_message`.
```json
{
  "type": "sent",
  "messageId": "uuid-сообщения",
  "createdAt": 1700000000000
}
```

#### История сообщений
Ответ на запрос `get_history`.
```json
{
  "type": "history",
  "channelId": "uuid-канала",
  "messages": [
    {
      "messageId": "uuid",
      "senderId": "uuid",
      "text": "Текст",
      "createdAt": 1700000000000
    }
  ],
  "hasMore": true
}
```

#### Список серверов пользователя
Ответ на `get_servers`.
```json
{
  "type": "servers_list",
  "servers": [
    {
      "serverId": "uuid",
      "serverName": "My Cool Server"
    }
  ]
}
```

#### Уведомление о новом сервере (Push)
Приходит клиенту, когда его добавили на сервер, или когда он сам его создал.
```json
{
  "type": "server_created",
  "serverId": "uuid",
  "serverName": "My Cool Server"
}
```

#### Список каналов сервера
Ответ на `get_channels`.
```json
{
  "type": "channels_list",
  "serverId": "uuid",
  "channels": [
    {
      "channelId": "uuid",
      "channelName": "general"
    }
  ]
}
```

#### Список личных чатов
Ответ на `get_direct_chats`.
```json
{
  "type": "direct_chats_list",
  "chats": [
    {
      "channelId": "uuid",
      "title": "Chat with Ivan"
    }
  ]
}
```

#### Глобальная ошибка сокета
Если клиент прислал неверный JSON или произошел таймаут БД.
```json
{
  "type": "error",
  "message": "Описание ошибки (например: timeout, invalid json, channel not found)"
}
```

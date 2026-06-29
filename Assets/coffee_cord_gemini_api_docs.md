Документация API для распределенного сервера CoffeeCord описывает все доступные REST API эндпоинты (HTTP) и структуру сообщений для WebSocket-соединения.

---

# Спецификация API CoffeeCord

## Общая информация

* **Базовый URL шлюза REST API:** `http://176.123.169.111:8080/`
* **URL WebSocket-соединения:** `ws://176.123.169.111:8080//chat?token=<URL_ENCODED_TOKEN>`
* **Формат данных:** Все запросы и ответы REST API, а также сообщения WebSocket (за исключением системного сообщения приветствия) используют формат `application/json`.
* **Авторизация:** Для большинства REST-запросов требуется передача JWT-токена в заголовке:  
  `Authorization: Bearer <token>`

---

## Раздел 1. REST API (HTTP)

### 1. Аутентификация и пользователи

#### 1.1. Регистрация нового пользователя
Создает учетную запись пользователя в базе данных PostgreSQL.

* **Метод:** `POST`
* **Путь:** `/register`
* **Заголовки:** `Content-Type: application/json`
* **Тело запроса (`CreateUserRequest`):**
```json
{
  "name": "Egor",
  "lastName": "Egorov",
  "email": "egor@coffee.cord",
  "password": "super_secure_pass"
}
```
* **Ответы:**
  * **`201 Created`** (Успешная регистрация):
    ```json
    {
      "user": {
        "id": "e5fc4637-d79f-4f53-a224-c4fd690e446c",
        "name": "Egor",
        "lastname": "Egorov",
        "email": "egor@coffee.cord"
      }
    }
    ```
  * **`400 Bad Request` / `500 Internal Error`** (Ошибка валидации или дубликат почты):
    ```json
    {
      "error": "Reason of failure"
    }
    ```

---

#### 1.2. Авторизация (Логин)
Генерирует JWT-токен сессии на основе предоставленных данных.

* **Метод:** `POST`
* **Путь:** `/login`
* **Заголовки:** `Content-Type: application/json`
* **Тело запроса (`LoginRequest`):**
```json
{
  "email": "egor@coffee.cord",
  "password": "super_secure_pass"
}
```
* **Ответы:**
  * **`200 OK`** (Успешный вход):
    ```json
    {
      "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "expireIn": 86400
    }
    ```
  * **`401 Unauthorized`** (Неверные учетные данные):
    ```json
    {
      "error": "Invalid credentials"
    }
    ```

---

#### 1.3. Получение профиля текущего пользователя
Возвращает профиль пользователя, которому принадлежит JWT-токен.

* **Метод:** `POST`
* **Путь:** `/me`
* **Заголовки:** 
  * `Authorization: Bearer <token>`
* **Тело запроса:** Отсутствует (BodyPublishers.noBody())
* **Ответы:**
  * **`200 OK`**:
    ```json
    {
      "user": {
        "id": "e5fc4637-d79f-4f53-a224-c4fd690e446c",
        "name": "Egor",
        "lastname": "Egorov",
        "email": "egor@coffee.cord"
      }
    }
    ```
  * **`401 Unauthorized`** (Токен отсутствует или невалиден):
    ```json
    {
      "error": "Invalid Token"
    }
    ```

---

#### 1.4. Поиск пользователя по Email
Ищет зарегистрированного пользователя по адресу электронной почты.

* **Метод:** `GET`
* **Путь:** `/users?email={email}`
* **Заголовки:**
  * `Authorization: Bearer <token>`
* **Ответы:**
  * **`200 OK`** (Пользователь найден):
    ```json
    {
      "user": {
        "id": "6b0e82c7-43ea-4f84-a9bb-0173d68401a6",
        "name": "Bob",
        "lastname": "Bobov",
        "email": "bob@coffee.cord"
      }
    }
    ```
  * **`404 Not Found`** (Пользователь не зарегистрирован):
    ```json
    {
      "error": "User not found"
    }
    ```
  * **`400 Bad Request`** (Пропущен параметр email):
    ```json
    {
      "error": "email parameter required"
    }
    ```

---

#### 1.5. Получение информации о пользователе по UUID
Возвращает информацию о пользователе по его уникальному физическому идентификатору.

* **Метод:** `GET`
* **Путь:** `/users/{userId}`
* **Заголовки:**
  * `Authorization: Bearer <token>`
* **Ответы:**
  * **`200 OK`**:
    ```json
    {
      "user": {
        "id": "6b0e82c7-43ea-4f84-a9bb-0173d68401a6",
        "name": "Bob",
        "lastname": "Bobov",
        "email": "bob@coffee.cord"
      }
    }
    ```
  * **`404 Not Found`**:
    ```json
    {
      "error": "User not found"
    }
    ```

---

### 2. Управление серверами и каналами

#### 2.1. Создание нового сервера
Создает распределенный сервер (шард) в кластере. Вызывающий пользователь автоматически становится владельцем (owner).

* **Метод:** `POST`
* **Путь:** `/servers`
* **Заголовки:**
  * `Authorization: Bearer <token>`
  * `Content-Type: application/json`
* **Тело запроса:**
```json
{
  "name": "Coffee Lovers"
}
```
* **Ответы:**
  * **`201 Created`**:
    ```json
    {
      "serverId": "ebfd8480-12b1-445b-a440-aed7cce58f8e"
    }
    ```

---

#### 2.2. Получение списка участников сервера
Возвращает список UUID пользователей, состоящих в сервере.

* **Метод:** `GET`
* **Путь:** `/servers/{serverId}/members`
* **Заголовки:**
  * `Authorization: Bearer <token>`
* **Ответы:**
  * **`200 OK`**:
    ```json
    [
      "984b9d59-c40c-4b32-88f4-1dfbd0b54e59",
      "15344ea3-9504-4e2f-809e-09be6131cfb9"
    ]
    ```

---

#### 2.3. Добавление участника на сервер
Позволяет добавить нового пользователя на сервер по его UUID или по адресу электронной почты.

* **Метод:** `POST`
* **Путь:** `/servers/{serverId}/members`
* **Заголовки:**
  * `Authorization: Bearer <token>`
  * `Content-Type: application/json`
* **Тело запроса** (Должен быть передан либо `userId`, либо `email`):
```json
{
  "userId": "15344ea3-9504-4e2f-809e-09be6131cfb9",
  "email": ""
}
```
* **Ответы:**
  * **`200 OK`**:
    ```json
    {
      "status": "success"
    }
    ```
  * **`400 Bad Request`** (Пользователь уже является участником сервера или неверный запрос):
    ```json
    {
      "error": "User is already a member"
    }
    ```
  * **`404 Not Found`** (Если поиск по email не увенчался успехом):
    ```json
    {
      "error": "User with this email not found"
    }
    ```

---

#### 2.4. Создание канала на сервере
Создает текстовый канал внутри указанного сервера.

* **Метод:** `POST`
* **Путь:** `/servers/{serverId}/channels`
* **Заголовки:**
  * `Authorization: Bearer <token>`
  * `Content-Type: application/json`
* **Тело запроса:**
```json
{
  "name": "General Lounge"
}
```
* **Ответы:**
  * **`200 OK`**:
    ```json
    {
      "serverId": "ebfd8480-12b1-445b-a440-aed7cce58f8e",
      "channelId": "af69ade7-ad3f-4abc-b047-4b0ae77f2cfb",
      "channelName": "General Lounge"
    }
    ```

---

## Раздел 2. WebSocket API

Все сообщения в WebSocket-сессии передаются в виде текстовых фреймов.

### 1. Приветствие (Handshake)
Сразу после успешной установки соединения шлюз отправляет клиенту простое текстовое приветствие:

* **Направление:** Server $\rightarrow$ Client
* **Формат:** Текст (String)
```text
connected
```

---

### 2. Запросы клиента к серверу (Client $\rightarrow$ Server)

Все запросы содержат обязательное поле `type`.

#### 2.1. Ping (Keep-alive)
Поддерживает сессию активной и предотвращает закрытие по таймауту.

```json
{
  "type": "ping"
}
```

#### 2.2. Запрос списка серверов пользователя
Запрашивает список распределенных серверов, на которые добавлен текущий пользователь.

```json
{
  "type": "get_servers"
}
```

#### 2.3. Запрос списка каналов сервера
Запрашивает список текстовых каналов конкретного сервера.

```json
{
  "type": "get_channels",
  "serverId": "ebfd8480-12b1-445b-a440-aed7cce58f8e"
}
```

#### 2.4. Запрос списка личных (прямых) чатов
Запрашивает личные переписки пользователя.

```json
{
  "type": "get_direct_chats"
}
```

#### 2.5. Создание текстового канала
Отправляет запрос на создание канала.

```json
{
  "type": "create_channel",
  "serverId": "ebfd8480-12b1-445b-a440-aed7cce58f8e",
  "name": "Music Room"
}
```

#### 2.6. Отправка сообщения в канал
Отправляет новое текстовое сообщение в указанный канал.

```json
{
  "type": "send_message",
  "channelId": "af69ade7-ad3f-4abc-b047-4b0ae77f2cfb",
  "text": "Check out this link: https://media1.tenor.com/...gif"
}
```

#### 2.7. Запрос истории сообщений (С пагинацией по времени)
Запрашивает историю сообщений для канала.

* `beforeTimestamp` — (Опционально) Передается при прокрутке вверх. Будут возвращены сообщения, отправленные строго до этого таймстампа (Unix Epoch, мс).

```json
{
  "type": "get_history",
  "channelId": "af69ade7-ad3f-4abc-b047-4b0ae77f2cfb",
  "limit": 30,
  "beforeTimestamp": 1782670149657
}
```

#### 2.8. Добавление участника на сервер
```json
{
  "type": "add_member",
  "serverId": "ebfd8480-12b1-445b-a440-aed7cce58f8e",
  "userId": "15344ea3-9504-4e2f-809e-09be6131cfb9"
}
```

---

### 3. Ответы и уведомления сервера клиенту (Server $\rightarrow$ Client)

#### 3.1. Ответ: Список серверов пользователя
```json
{
  "type": "servers_list",
  "servers": [
    {
      "serverId": "ebfd8480-12b1-445b-a440-aed7cce58f8e",
      "serverName": "Test"
    }
  ]
}
```

#### 3.2. Ответ: Список каналов сервера
```json
{
  "type": "channels_list",
  "serverId": "ebfd8480-12b1-445b-a440-aed7cce58f8e",
  "channels": [
    {
      "channelId": "af69ade7-ad3f-4abc-b047-4b0ae77f2cfb",
      "channelName": "channel"
    },
    {
      "channelId": "2f5aaba3-afc5-4339-ab84-de1a06f7a3c1",
      "channelName": "channel 2"
    }
  ]
}
```

#### 3.3. Ответ: Список личных чатов пользователя
```json
{
  "type": "direct_chats_list",
  "chats": [
    {
      "channelId": "695cb32f-375b-4695-9def-21a1fa7db4f5",
      "title": "Bel Gray"
    }
  ]
}
```

#### 3.4. Ответ: Канал создан
Подтверждает создание нового канала на сервере.

```json
{
  "type": "channel_created",
  "channelId": "af69ade7-ad3f-4abc-b047-4b0ae77f2cfb",
  "serverId": "ebfd8480-12b1-445b-a440-aed7cce58f8e",
  "channelName": "General"
}
```

#### 3.5. Ответ: Сообщение доставлено на бэкенд (для отправителя)
Информирует отправителя, что его сообщение успешно записано в лог событий бэкенда.

```json
{
  "type": "sent",
  "messageId": "07d0c038-98a2-4d42-8e92-37b6298fdb02",
  "createdAt": 1782670149657
}
```

#### 3.6. Уведомление: Новое сообщение (для всех участников канала)
Рассылается всем участникам сервера/канала в реальном времени при публикации нового сообщения.

```json
{
  "type": "new_message",
  "serverId": "ebfd8480-12b1-445b-a440-aed7cce58f8e",
  "channelId": "af69ade7-ad3f-4abc-b047-4b0ae77f2cfb",
  "message": {
    "messageId": "07d0c038-98a2-4d42-8e92-37b6298fdb02",
    "senderId": "4cb4614c-0842-41af-95a9-ce4e3f5b08d6",
    "text": "Test!",
    "createdAt": 1782670149657
  }
}
```

#### 3.7. Уведомление: Новый сервер создан (или пользователя добавили на сервер)
Отправляется пользователю, когда он создает сервер или когда его добавляет другой участник.

```json
{
  "type": "server_created",
  "serverId": "ebfd8480-12b1-445b-a440-aed7cce58f8e",
  "serverName": "Coffee Lovers"
}
```

#### 3.8. Ответ: История сообщений
Возвращает список сообщений канала от новых к старым.

```json
{
  "type": "history",
  "channelId": "af69ade7-ad3f-4abc-b047-4b0ae77f2cfb",
  "messages": [
    {
      "messageId": "07d0c038-98a2-4d42-8e92-37b6298fdb02",
      "senderId": "4cb4614c-0842-41af-95a9-ce4e3f5b08d6",
      "text": "Test!",
      "createdAt": 1782670149657
    },
    {
      "messageId": "e613ca93-48f2-4c7d-9ecf-82aba7906364",
      "senderId": "6b0e82c7-43ea-4f84-a9bb-0173d68401a6",
      "text": "Message",
      "createdAt": 1782670116388
    }
  ],
  "hasMore": false
}
```

#### 3.9. Уведомление: Участник добавлен на сервер
```json
{
  "type": "member_added",
  "serverId": "ebfd8480-12b1-445b-a440-aed7cce58f8e",
  "userId": "15344ea3-9504-4e2f-809e-09be6131cfb9"
}
```

#### 3.10. Уведомление об ошибке
Отправляется сервером в случае провала асинхронных операций (например, таймаута, нехватки прав или неверных UUID).

```json
{
  "type": "error",
  "message": "Reason of the failure"
}
```

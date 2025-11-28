# Sea-RTS

Морская онлайн RTS игра для игры с друзьями! (Online Sea-themed RTS game to play with friends!)

## 🎮 О проекте / About

Sea-RTS - это многопользовательская стратегия в реальном времени с морской тематикой, созданная на Unity с использованием Netcode for GameObjects для сетевого взаимодействия.

Sea-RTS is a multiplayer real-time strategy game with a naval theme, built with Unity using Netcode for GameObjects for networking.

## ✨ Возможности / Features

- **Мультиплеер до 4 игроков / Multiplayer up to 4 players** - Играйте с друзьями по сети
- **Морские юниты / Naval units** - Различные типы кораблей (разведчик, истребитель, эсминец, авианосец, линкор)
- **RTS управление / RTS Controls** - Выделение юнитов, команды движения и атаки
- **Лобби система / Lobby system** - Создание и присоединение к играм

## 🛠️ Технологии / Technologies

- Unity 2022.3+
- Unity Netcode for GameObjects 1.7.1
- Unity Transport 2.0.2
- TextMeshPro

## 🚀 Как начать / Getting Started

### Требования / Requirements

- Unity 2022.3 или новее / Unity 2022.3 or newer
- Стабильное интернет-соединение для мультиплеера / Stable internet connection for multiplayer

### Установка / Installation

1. Клонируйте репозиторий / Clone the repository:
   ```bash
   git clone https://github.com/Blizzek/Sea-RTS.git
   ```

2. Откройте проект в Unity Hub / Open the project in Unity Hub

3. Дождитесь импорта пакетов / Wait for packages to import

### Запуск игры / Running the Game

1. Откройте сцену `MainMenu` / Open the `MainMenu` scene
2. Нажмите Play в редакторе / Press Play in the editor
3. Выберите "Host Game" для создания сервера или "Join Game" для подключения / Choose "Host Game" to create a server or "Join Game" to connect

## 🎯 Управление / Controls

| Действие / Action | Клавиша / Key |
|-------------------|---------------|
| Движение камеры / Camera movement | WASD или стрелки / WASD or Arrow keys |
| Поворот камеры / Camera rotation | Q / E |
| Приближение / Zoom | Колесо мыши / Mouse wheel |
| Выбор юнита / Select unit | Левый клик / Left click |
| Групповой выбор / Box select | Левый клик + тянуть / Left click + drag |
| Команда движения / Move command | Правый клик / Right click |
| Команда атаки / Attack command | Правый клик по врагу / Right click on enemy |

## 📁 Структура проекта / Project Structure

```
Assets/
├── Scripts/
│   ├── Core/           # Основные компоненты (GameManager, SelectionManager, Camera)
│   ├── Network/        # Сетевой код (GameNetworkManager, PlayerNetwork)
│   ├── Units/          # Классы юнитов (UnitBase, Ship)
│   └── UI/             # Интерфейс (MainMenuUI, GameHUD)
├── Scenes/             # Игровые сцены
├── Prefabs/            # Префабы юнитов и UI
└── Packages/           # Зависимости Unity
```

## 🔧 Настройка сервера / Server Setup

Для игры по интернету:
1. Хост должен открыть порт 7777 (UDP) на роутере
2. Хост сообщает свой внешний IP адрес друзьям
3. Друзья вводят IP адрес хоста в поле "IP Address" и нажимают "Join"

For playing over the internet:
1. Host needs to forward port 7777 (UDP) on router
2. Host shares their external IP address with friends
3. Friends enter host's IP address in "IP Address" field and click "Join"

## 📝 Лицензия / License

MIT License

## 🤝 Вклад / Contributing

Вклад приветствуется! / Contributions are welcome!

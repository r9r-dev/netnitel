# NetNitel - Serveur WebSocket .NET Core 9

Ce projet permet de créer un serveur WebSocket pour communiquer avec un Minitel disposant d'un dongle compatible.

C'est à la base un portage de la partie websocket du projet [pynitel](https://github.com/cquest/pynitel/tree/websockets) par cquest, qui a donné naissance à un portage en javascript [js-poc](https://github.com/Ucodia/pynitel/tree/js-poc) par Ucodia.

Il fait désormais son petit bout de chemin tout seul, ce pourquoi j'ai préféré en faire un projet à part entière.

## Description

NetNitel est un serveur WebSocket permettant de communiquer avec un Minitel via un dongle USB ou un émulateur Minitel comme celui de [MiniPavi](https://www.minipavi.fr/emulminitel).

Dans sa version initiale, j'ai implémenté le portage de l'annuaire et du jeu Snake.

## Prérequis

- .NET 9 SDK
- Un minitel avec dongle ESP32 (comme celui de [iodeo](https://www.tindie.com/stores/iodeo/) que j'utilise).

Vous pouvez également utiliser un émulateur Minitel comme celui de [MiniPavi](https://www.minipavi.fr/emulminitel).

## Installation

1. Clonez ce dépôt
2. Naviguez dans /src
3. Lancez l'application avec la commande:

```bash
dotnet run
```

## Utilisation (avec émulateur)

Ouvrez un navigateur web et accédez à l'adresse de l'[émulateur MiniPavi](https://www.minipavi.fr/emulminitel/indexws.php?url=ws%3A%2F%2Flocalhost%3A3615%2Findex)

Pour le reste, il va falloir coder :)

## Architecture

Le projet est développé en C# avec .NET Core 9. Injection de dépendances sont donc implémentés dans Program.cs. Le Controleur `IndexController` ouvre une connexion WebSocket sur la route `/index`.

Le serveur WebSocket est ensuite géré par le service `Minitel` qui dispose des méthodes permettant de communiquer avec le Minitel.

## Licence

Ce projet est sous licence GNU GPL v3.

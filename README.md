# NetNitel - Serveur WebSocket .NET Core 9

Ce projet permet de créer un serveur WebSocket pour communiquer avec un Minitel disposant d'un dongle compatible.

C'est à la base un portage de la partie websocket du projet [pynitel](https://github.com/cquest/pynitel/tree/websockets) par cquest, qui a donné naissance à un portage en javascript [js-poc](https://github.com/Ucodia/pynitel/tree/js-poc) par Ucodia.

Il fait désormais son petit bout de chemin tout seul, ce pourquoi j'ai préféré en faire un projet à part entière.

## Description

NetNitel est un serveur WebSocket permettant de communiquer avec un Minitel via un dongle USB ou un émulateur Minitel comme celui de [MiniPavi](https://www.minipavi.fr/emulminitel).

Dans sa version initiale, j'ai implémenté le portage de l'annuaire et du jeu Snake (qui n'existent plus depuis).

## Fonctionnalités

- Communication avec un Minitel via WebSocket
- Gestion des entrées utilisateur
- Affichage de texte et de graphiques
- Gestion des accents et caractères spéciaux
- Gestion des couleurs d'avant-plan et d'arrière-plan
- Affichage d'images converties au format Minitel (80x72 pixels, 8 couleurs)

### Affichage d'images

Le projet inclut un service de traitement d'images (`ImageService`) qui permet :

- De redimensionner des images à la résolution du Minitel (80x72 pixels)
- De convertir les images en 8 couleurs selon la palette du Minitel (noir, rouge, vert, jaune, bleu, magenta, cyan, blanc)
- D'afficher ces images sur l'écran du Minitel en utilisant le mode graphique (blocs 2x3)

Pour afficher une image, utilisez la méthode `DrawImage` :

```csharp
// Exemple d'utilisation
await netNitel.DrawImage("chemin/vers/image.png");
```

L'image sera automatiquement :
1. Redimensionnée à 80x72 pixels
2. Convertie en 8 couleurs
3. Affichée sur l'écran du Minitel

## Prérequis

- .NET 9 SDK
- Un minitel avec dongle ESP32 (comme celui de [iodeo](https://www.tindie.com/stores/iodeo/) que j'utilise).
- La bibliothèque System.Drawing.Common pour le traitement d'images

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

### Exemples d'utilisation

```csharp
// Afficher du texte
await netNitel.Print("Bonjour Minitel !");

// Changer les couleurs
await netNitel.ForeColor(MiniColor.Rouge);
await netNitel.BackColor(MiniColor.Noir);

// Déplacer le curseur
await netNitel.Move(10, 5);

// Afficher une image
await netNitel.DrawImage("images/logo.png");

// Demander une saisie utilisateur
var input = await netNitel.Input(20, 1, 30, "Votre nom: ");
```

## Architecture

Le projet est développé en C# avec .NET Core 9. L'injection de dépendances est implémentée dans Program.cs. Le Controleur `IndexController` ouvre une connexion WebSocket sur la route `/index`.

Le serveur WebSocket est ensuite géré par les services :
- `NetNitel` : Interface principale pour manipuler le Minitel
- `MiniControl` : Gestion des commandes de contrôle du Minitel
- `ImageService` : Traitement des images pour affichage sur le Minitel

## Licence

Ce projet est sous licence GNU GPL v3.

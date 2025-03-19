using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using netnitel.Services.MinitelEngine;

namespace netnitel.Services;

public class MinitelService
{
    private readonly ILogger<MinitelService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public MinitelService(ILogger<MinitelService> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Service d'annuaire téléphonique
    /// </summary>
    public async Task Annuaire(NetNitel netNitel)
    {
        // Variables pour stocker les entrées utilisateur
        var quoi = "";
        var ou = "";
        
        // Fonction pour afficher la page de recherche
        async Task PageRecherche()
        {
            await netNitel.Home();
            await netNitel.ClearScreen();
            
            // Titre
            await netNitel.MoveTo(2, 10);
            await netNitel.Color(MiniColor.Blanc);
            await netNitel.BackColor(MiniColor.Bleu);
            await netNitel.Print(" ANNUAIRE ELECTRONIQUE ");
            await netNitel.Normal();
            
            // Formulaire
            await netNitel.MoveTo(6, 5);
            await netNitel.Print("Qui recherchez-vous ?");
            
            await netNitel.MoveTo(9, 5);
            await netNitel.Print("Dans quelle ville ?");
            
            // Instructions
            await netNitel.MoveTo(14, 5);
            await netNitel.Print("Appuyez sur ENVOI pour rechercher");
            
            await netNitel.MoveTo(16, 5);
            await netNitel.Print("ou SOMMAIRE pour revenir au menu");
        }
        
        // Boucle principale
        var running = true;
        while (running)
        {
            await PageRecherche();
            
            // Saisie des critères de recherche
            await netNitel.MoveTo(7, 5);
            var reponseQuoi = await netNitel.Input(7, 5, 30, quoi);
            
            if (reponseQuoi.SpecialKey == MiniKey.Sommaire)
            {
                running = false;
                continue;
            }
            
            await netNitel.MoveTo(10, 5);
            var reponseOu = await netNitel.Input(10, 5, 30, ou);
            
            if (reponseOu.SpecialKey == MiniKey.Sommaire)
            {
                running = false;
                continue;
            }
            else if (reponseOu.SpecialKey == MiniKey.Envoi || reponseOu.SpecialKey == MiniKey.Suite)
            {
                if (!string.IsNullOrWhiteSpace(quoi) && !string.IsNullOrWhiteSpace(ou))
                {
                    // Afficher message de recherche
                    await netNitel.Message(0, 1, 2, "Recherche en cours...", true);
                    
                    // Effectuer la recherche (simulation)
                    var resultats = await RechercheAnnuaire(quoi, ou);
                    
                    // Afficher les résultats
                    await AfficherResultats(netNitel, quoi, ou, resultats);
                }
                else
                {
                    await netNitel.Message(0, 1, 3, "Veuillez remplir tous les champs", true);
                }
            }
        }
    }
    
    /// <summary>
    /// Effectue une recherche d'annuaire (simulation)
    /// </summary>
    private async Task<List<ResultatAnnuaire>> RechercheAnnuaire(string quoi, string ou)
    {
        // Cette fonction simule une recherche dans un annuaire
        // Dans la vraie vie, ce serait une API ou une base de données
        await Task.Delay(1000); // Simulation de la latence réseau
        
        var resultats = new List<ResultatAnnuaire>();
        
        // Générer quelques résultats fictifs
        if (!string.IsNullOrWhiteSpace(quoi) && !string.IsNullOrWhiteSpace(ou))
        {
            resultats.Add(new ResultatAnnuaire 
            { 
                Nom = quoi + " SA", 
                Adresse = "10 rue des Entreprises", 
                CodePostal = "75001", 
                Ville = ou, 
                Telephone = "01 23 45 67 89" 
            });
            
            resultats.Add(new ResultatAnnuaire 
            { 
                Nom = quoi + " SARL", 
                Adresse = "25 avenue du Commerce", 
                CodePostal = "75002", 
                Ville = ou, 
                Telephone = "01 98 76 54 32" 
            });
            
            resultats.Add(new ResultatAnnuaire 
            { 
                Nom = quoi + " et Fils", 
                Adresse = "3 boulevard de l'Industrie", 
                CodePostal = "75003", 
                Ville = ou, 
                Telephone = "01 45 67 89 10" 
            });
        }
        
        return resultats;
    }
    
    /// <summary>
    /// Affiche les résultats de la recherche d'annuaire
    /// </summary>
    private async Task AfficherResultats(NetNitel netNitel, string quoi, string ou, List<ResultatAnnuaire> resultats)
    {
        var page = 0;
        var resultsPerPage = 1;
        var viewing = true;
        
        while (viewing)
        {
            await netNitel.Home();
            await netNitel.ClearScreen();
            
            // Titre
            await netNitel.MoveTo(2, 10);
            await netNitel.Color(MiniColor.Blanc);
            await netNitel.BackColor(MiniColor.Bleu);
            await netNitel.Print(" RESULTATS DE RECHERCHE ");
            await netNitel.Normal();
            
            // Critères
            await netNitel.MoveTo(4, 5);
            await netNitel.Print($"Recherche : {quoi} à {ou}");
            
            // Nombre total
            await netNitel.MoveTo(5, 5);
            await netNitel.Print($"Trouvé : {resultats.Count} résultat(s)");
            
            if (resultats.Count > 0)
            {
                var startIdx = page * resultsPerPage;
                var endIdx = Math.Min(startIdx + resultsPerPage, resultats.Count);
                
                for (var i = startIdx; i < endIdx; i++)
                {
                    var result = resultats[i];
                    var baseRow = 7 + (i - startIdx) * 5;
                    
                    // Afficher un résultat
                    await netNitel.MoveTo(baseRow, 5);
                    await netNitel.Color(MiniColor.Jaune);
                    await netNitel.Print(result.Nom);
                    await netNitel.Color(MiniColor.Blanc);
                    
                    await netNitel.MoveTo(baseRow + 1, 5);
                    await netNitel.Print(result.Adresse);
                    
                    await netNitel.MoveTo(baseRow + 2, 5);
                    await netNitel.Print($"{result.CodePostal} {result.Ville}");
                    
                    await netNitel.MoveTo(baseRow + 3, 5);
                    await netNitel.Color(MiniColor.Vert);
                    await netNitel.Print($"Tél: {result.Telephone}");
                    await netNitel.Color(MiniColor.Blanc);
                }
                
                // Navigation
                await netNitel.MoveTo(18, 5);
                
                if (page > 0)
                {
                    await netNitel.Print("RETOUR: résultat précédent  ");
                }
                
                if (page < (resultats.Count - 1) / resultsPerPage)
                {
                    await netNitel.Print("SUITE: résultat suivant");
                }
            }
            else
            {
                await netNitel.MoveTo(10, 5);
                await netNitel.Print("Aucun résultat trouvé !");
            }
            
            // Instructions
            await netNitel.MoveTo(20, 5);
            await netNitel.Print("SOMMAIRE: retour à la recherche");
            
            // Attendre l'entrée utilisateur
            var input = await netNitel.Input(22, 5, 1, "", " ", false);
            
            if (input.SpecialKey == MiniKey.Sommaire)
            {
                viewing = false;
            }
            else if (input.SpecialKey == MiniKey.Suite && page < (resultats.Count - 1) / resultsPerPage)
            {
                page++;
            }
            else if (input.SpecialKey == MiniKey.Retour && page > 0)
            {
                page--;
            }
        }
    }
    
    /// <summary>
    /// Jeu de snake
    /// </summary>
    public async Task Snake(NetNitel netNitel)
    {
        // Constantes du jeu
        const int GameWidth = 20;
        const int GameHeight = 20;
        const int InitialSnakeLength = 3;
        const int InitialDelay = 300; // millisecondes entre les mouvements
        const int MinDelay = 100; // vitesse maximale
        
        // Éléments du jeu
        const string Empty = " ";
        const string SnakeHead = "O";
        const string SnakeBody = "o";
        const string Food = "*";
        const string Wall = "█"; // Caractère plein
        
        // Variables du jeu
        var running = true;
        var score = 0;
        var lastScore = 0;
        var delay = InitialDelay;
        var direction = "right";
        var nextDirection = "right";
        var snake = new List<(int x, int y)>();
        (int x, int y)? lastTail = null;
        var food = (x: 0, y: 0);
        var foodEaten = false;
        
        // Initialiser le jeu
        void InitGame()
        {
            // Créer le serpent initial au milieu de l'écran
            snake.Clear();
            var midY = GameHeight / 2;
            var midX = GameWidth / 4;
            
            for (var i = 0; i < InitialSnakeLength; i++)
            {
                snake.Add((midX - i, midY));
            }
            
            // Placer la nourriture initiale
            PlaceFood();
            
            // Réinitialiser l'état du jeu
            score = 0;
            lastScore = 0;
            delay = InitialDelay;
            direction = "right";
            nextDirection = "right";
            running = true;
            lastTail = null;
            foodEaten = false;
        }
        
        // Placer la nourriture à une position aléatoire
        void PlaceFood()
        {
            var validPosition = false;
            var random = new Random();
            
            while (!validPosition)
            {
                food.x = random.Next(1, GameWidth - 1);
                food.y = random.Next(1, GameHeight - 1);
                
                // Vérifier que la nourriture n'est pas sur le serpent
                validPosition = true;
                foreach (var segment in snake)
                {
                    if (segment.x == food.x && segment.y == food.y)
                    {
                        validPosition = false;
                        break;
                    }
                }
            }
        }
        
        // Dessiner le plateau de jeu initial (une seule fois)
        async Task DrawInitialBoard()
        {
            await netNitel.Home();
            await netNitel.ClearScreen();
            
            // Plateau commence à la position (3,3)
            
            // Titre
            await netNitel.MoveTo(1, 10);
            await netNitel.Color(MiniColor.Vert);
            await netNitel.Print("SNAKE MINITEL");
            await netNitel.Color(MiniColor.Blanc);
            
            // Dessiner les murs
            await netNitel.Color(MiniColor.Magenta);
            
            // Mur horizontal supérieur
            await netNitel.MoveTo(2, 3);
            await netNitel.Print(new string(Wall[0], GameWidth + 2));
            
            // Murs latéraux et inférieur
            for (var y = 1; y <= GameHeight; y++)
            {
                await netNitel.MoveTo(2 + y, 3);
                await netNitel.Print(Wall);
                
                await netNitel.MoveTo(2 + y, 3 + GameWidth + 1);
                await netNitel.Print(Wall);
            }
            
            // Mur horizontal inférieur
            await netNitel.MoveTo(2 + GameHeight + 1, 3);
            await netNitel.Print(new string(Wall[0], GameWidth + 2));
            
            await netNitel.Color(MiniColor.Blanc);
            
            // Instructions
            await netNitel.MoveTo(2 + GameHeight + 3, 3);
            await netNitel.Print("Utilisez:");
            await netNitel.MoveTo(2 + GameHeight + 4, 3);
            await netNitel.Print("8:Haut 2:Bas 4:Gauche 6:Droite");
            await netNitel.MoveTo(2 + GameHeight + 5, 3);
            await netNitel.Print("ENVOI: Quitter");
        }
        
        // Mettre à jour le score
        async Task UpdateScore()
        {
            // Si le score a changé, le mettre à jour
            if (score != lastScore)
            {
                await netNitel.MoveTo(1, 25);
                await netNitel.Color(MiniColor.Jaune);
                await netNitel.Print($"Score: {score}");
                await netNitel.Color(MiniColor.Blanc);
                lastScore = score;
            }
        }
        
        // Dessiner la nourriture
        async Task DrawFood()
        {
            await netNitel.MoveTo(2 + food.y, 3 + food.x + 1);
            await netNitel.Color(MiniColor.Jaune);
            await netNitel.Print(Food);
            await netNitel.Color(MiniColor.Blanc);
        }
        
        // Mettre à jour l'affichage du serpent
        async Task UpdateSnake()
        {
            // Dessiner la tête du serpent
            await netNitel.MoveTo(2 + snake[0].y, 3 + snake[0].x + 1);
            await netNitel.Color(MiniColor.Vert);
            await netNitel.Print(SnakeHead);
            
            // Si nous avons un second segment, l'ancien segment de tête devient un segment de corps
            if (snake.Count > 1)
            {
                await netNitel.MoveTo(2 + snake[1].y, 3 + snake[1].x + 1);
                await netNitel.Print(SnakeBody);
            }
            
            // Effacer l'ancienne queue uniquement si aucune nourriture n'a été mangée
            if (lastTail.HasValue && !foodEaten)
            {
                await netNitel.MoveTo(2 + lastTail.Value.y, 3 + lastTail.Value.x + 1);
                await netNitel.Print(Empty);
            }
            
            // Réinitialiser le drapeau de nourriture mangée
            foodEaten = false;
            
            await netNitel.Color(MiniColor.Blanc);
        }
        
        // Déplacer le serpent
        void MoveSnake()
        {
            // Sauvegarder l'ancienne queue
            if (snake.Count > 0)
            {
                lastTail = snake[snake.Count - 1];
            }
            
            // Mettre à jour la direction
            direction = nextDirection;
            
            // Calculer la nouvelle position de la tête
            var head = snake[0];
            var newHead = head;
            
            switch (direction)
            {
                case "up":
                    newHead = (head.x, head.y - 1);
                    break;
                case "down":
                    newHead = (head.x, head.y + 1);
                    break;
                case "left":
                    newHead = (head.x - 1, head.y);
                    break;
                case "right":
                    newHead = (head.x + 1, head.y);
                    break;
            }
            
            // Vérifier les collisions avec les murs
            if (newHead.Item1 < 1 || newHead.Item1 >= GameWidth ||
                newHead.Item2 < 1 || newHead.Item2 >= GameHeight)
            {
                running = false;
                return;
            }
            
            // Vérifier les collisions avec le serpent
            foreach (var segment in snake)
            {
                if (newHead.Item1 == segment.x && newHead.Item2 == segment.y)
                {
                    running = false;
                    return;
                }
            }
            
            // Déplacer le serpent (ajouter la nouvelle tête)
            snake.Insert(0, newHead);
            
            // Vérifier si la nourriture a été mangée
            if (newHead.Item1 == food.x && newHead.Item2 == food.y)
            {
                // Augmenter le score
                score += 10;
                
                // Augmenter la vitesse
                delay = Math.Max(MinDelay, delay - 10);
                
                // Marquer la nourriture comme mangée (ne pas supprimer la queue)
                foodEaten = true;
                
                // Placer une nouvelle nourriture
                PlaceFood();
            }
            else
            {
                // Si la nourriture n'a pas été mangée, supprimer la queue
                snake.RemoveAt(snake.Count - 1);
            }
        }
        
        // Afficher l'écran de fin de jeu
        async Task GameOver()
        {
            await netNitel.MoveTo(10, 8);
            await netNitel.Color(MiniColor.Rouge);
            await netNitel.Print("GAME OVER");
            
            await netNitel.MoveTo(12, 8);
            await netNitel.Color(MiniColor.Jaune);
            await netNitel.Print($"Score final: {score}");
            
            await netNitel.MoveTo(14, 8);
            await netNitel.Color(MiniColor.Blanc);
            await netNitel.Print("Appuyez sur ENVOI");
            
            // Attendre que l'utilisateur appuie sur ENVOI
            while (true)
            {
                var isEnvoi = await netNitel.Input(0, 0, 1, "", " ", false);
                if (isEnvoi.SpecialKey == MiniKey.Envoi || isEnvoi.SpecialKey == MiniKey.Sommaire)
                {
                    break;
                }
            }
        }
        
        // Traiter l'entrée clavier
        void ProcessKey(MiniInput key)
        {
            // Les touches numériques sont utilisées pour la direction
            if (key.Type == MiniInputType.Text)
            {
                var ch = key.Message[0];
                if (ch >= '0' && ch <= '9')
                {
                    switch (ch)
                    {
                        case '8': // Haut
                            if (direction != "down") nextDirection = "up";
                            break;
                        case '2': // Bas
                            if (direction != "up") nextDirection = "down";
                            break;
                        case '4': // Gauche
                            if (direction != "right") nextDirection = "left";
                            break;
                        case '6': // Droite
                            if (direction != "left") nextDirection = "right";
                            break;
                    }
                }
            }
            
            else if (key.SpecialKey == MiniKey.Envoi || key.SpecialKey == MiniKey.Sommaire)
            {
                running = false;
            }
        }
        
        // Boucle principale du jeu
        async Task GameLoop()
        {
            // Initialiser le jeu
            InitGame();
            
            // Dessiner le plateau
            await DrawInitialBoard();
            
            // Créer une tâche pour l'entrée utilisateur
            var inputTask = Task.Run(async () =>
            {
                var buffer = new byte[1024];
                
                try
                {
                    while (running)
                    {
                        // Utilisation de la méthode Input de Pynitel pour gérer les entrées
                        var input = await netNitel.Input(0, 0, 1, "", " ", false);
                        var ch = ' ';
                        
                        // Simuler une entrée clavier pour les directions
                        if (input.SpecialKey == MiniKey.Envoi || input.SpecialKey == MiniKey.Sommaire)
                        {
                            ProcessKey(input);
                        }
                    }
                }
                catch
                {
                    running = false;
                }
            });
            
            // Boucle principale du jeu
            while (running)
            {
                // Mettre à jour le score
                await UpdateScore();
                
                // Dessiner la nourriture
                await DrawFood();
                
                // Déplacer le serpent
                MoveSnake();
                
                // Si le jeu est toujours en cours, mettre à jour l'affichage
                if (running)
                {
                    await UpdateSnake();
                    await Task.Delay(delay);
                }
            }
            
            // Attendre la fin de la tâche d'entrée
            // Ajoutons un délai court pour permettre la fin de la tâche
            await Task.Delay(100);
            
            // Afficher l'écran de fin de jeu
            await GameOver();
        }
        
        // Commencer le jeu
        await GameLoop();
    }
}

/// <summary>
/// Classe représentant un résultat d'annuaire
/// </summary>
public class ResultatAnnuaire
{
    public string Nom { get; set; } = "";
    public string Adresse { get; set; } = "";
    public string CodePostal { get; set; } = "";
    public string Ville { get; set; } = "";
    public string Telephone { get; set; } = "";
} 
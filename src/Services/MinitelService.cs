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
    public async Task Annuaire(Minitel minitel)
    {
        // Variables pour stocker les entrées utilisateur
        var quoi = "";
        var ou = "";
        
        // Fonction pour afficher la page de recherche
        async Task PageRecherche()
        {
            await minitel.Home();
            await minitel.ClearScreen();
            
            // Titre
            await minitel.MoveTo(2, 10);
            await minitel.Color(Minitel.Blanc);
            await minitel.BackColor(Minitel.Bleu);
            await minitel.Print(" ANNUAIRE ELECTRONIQUE ");
            await minitel.Normal();
            
            // Formulaire
            await minitel.MoveTo(6, 5);
            await minitel.Print("Qui recherchez-vous ?");
            
            await minitel.MoveTo(9, 5);
            await minitel.Print("Dans quelle ville ?");
            
            // Instructions
            await minitel.MoveTo(14, 5);
            await minitel.Print("Appuyez sur ENVOI pour rechercher");
            
            await minitel.MoveTo(16, 5);
            await minitel.Print("ou SOMMAIRE pour revenir au menu");
        }
        
        // Boucle principale
        var running = true;
        while (running)
        {
            await PageRecherche();
            
            // Saisie des critères de recherche
            await minitel.MoveTo(7, 5);
            var reponseQuoi = await minitel.Input(7, 5, 30, quoi);
            
            if (reponseQuoi.SpecialKey == MiniKey.Sommaire)
            {
                running = false;
                continue;
            }
            
            await minitel.MoveTo(10, 5);
            var reponseOu = await minitel.Input(10, 5, 30, ou);
            
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
                    await minitel.Message(0, 1, 2, "Recherche en cours...", true);
                    
                    // Effectuer la recherche (simulation)
                    var resultats = await RechercheAnnuaire(quoi, ou);
                    
                    // Afficher les résultats
                    await AfficherResultats(minitel, quoi, ou, resultats);
                }
                else
                {
                    await minitel.Message(0, 1, 3, "Veuillez remplir tous les champs", true);
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
    private async Task AfficherResultats(Minitel minitel, string quoi, string ou, List<ResultatAnnuaire> resultats)
    {
        var page = 0;
        var resultsPerPage = 1;
        var viewing = true;
        
        while (viewing)
        {
            await minitel.Home();
            await minitel.ClearScreen();
            
            // Titre
            await minitel.MoveTo(2, 10);
            await minitel.Color(Minitel.Blanc);
            await minitel.BackColor(Minitel.Bleu);
            await minitel.Print(" RESULTATS DE RECHERCHE ");
            await minitel.Normal();
            
            // Critères
            await minitel.MoveTo(4, 5);
            await minitel.Print($"Recherche : {quoi} à {ou}");
            
            // Nombre total
            await minitel.MoveTo(5, 5);
            await minitel.Print($"Trouvé : {resultats.Count} résultat(s)");
            
            if (resultats.Count > 0)
            {
                var startIdx = page * resultsPerPage;
                var endIdx = Math.Min(startIdx + resultsPerPage, resultats.Count);
                
                for (var i = startIdx; i < endIdx; i++)
                {
                    var result = resultats[i];
                    var baseRow = 7 + (i - startIdx) * 5;
                    
                    // Afficher un résultat
                    await minitel.MoveTo(baseRow, 5);
                    await minitel.Color(Minitel.Jaune);
                    await minitel.Print(result.Nom);
                    await minitel.Color(Minitel.Blanc);
                    
                    await minitel.MoveTo(baseRow + 1, 5);
                    await minitel.Print(result.Adresse);
                    
                    await minitel.MoveTo(baseRow + 2, 5);
                    await minitel.Print($"{result.CodePostal} {result.Ville}");
                    
                    await minitel.MoveTo(baseRow + 3, 5);
                    await minitel.Color(Minitel.Vert);
                    await minitel.Print($"Tél: {result.Telephone}");
                    await minitel.Color(Minitel.Blanc);
                }
                
                // Navigation
                await minitel.MoveTo(18, 5);
                
                if (page > 0)
                {
                    await minitel.Print("RETOUR: résultat précédent  ");
                }
                
                if (page < (resultats.Count - 1) / resultsPerPage)
                {
                    await minitel.Print("SUITE: résultat suivant");
                }
            }
            else
            {
                await minitel.MoveTo(10, 5);
                await minitel.Print("Aucun résultat trouvé !");
            }
            
            // Instructions
            await minitel.MoveTo(20, 5);
            await minitel.Print("SOMMAIRE: retour à la recherche");
            
            // Attendre l'entrée utilisateur
            var input = await minitel.Input(22, 5, 1, "", " ", false);
            
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
    public async Task Snake(Minitel minitel)
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
            await minitel.Home();
            await minitel.ClearScreen();
            
            // Plateau commence à la position (3,3)
            
            // Titre
            await minitel.MoveTo(1, 10);
            await minitel.Color(Minitel.Vert);
            await minitel.Print("SNAKE MINITEL");
            await minitel.Color(Minitel.Blanc);
            
            // Dessiner les murs
            await minitel.Color(Minitel.Magenta);
            
            // Mur horizontal supérieur
            await minitel.MoveTo(2, 3);
            await minitel.Print(new string(Wall[0], GameWidth + 2));
            
            // Murs latéraux et inférieur
            for (var y = 1; y <= GameHeight; y++)
            {
                await minitel.MoveTo(2 + y, 3);
                await minitel.Print(Wall);
                
                await minitel.MoveTo(2 + y, 3 + GameWidth + 1);
                await minitel.Print(Wall);
            }
            
            // Mur horizontal inférieur
            await minitel.MoveTo(2 + GameHeight + 1, 3);
            await minitel.Print(new string(Wall[0], GameWidth + 2));
            
            await minitel.Color(Minitel.Blanc);
            
            // Instructions
            await minitel.MoveTo(2 + GameHeight + 3, 3);
            await minitel.Print("Utilisez:");
            await minitel.MoveTo(2 + GameHeight + 4, 3);
            await minitel.Print("8:Haut 2:Bas 4:Gauche 6:Droite");
            await minitel.MoveTo(2 + GameHeight + 5, 3);
            await minitel.Print("ENVOI: Quitter");
        }
        
        // Mettre à jour le score
        async Task UpdateScore()
        {
            // Si le score a changé, le mettre à jour
            if (score != lastScore)
            {
                await minitel.MoveTo(1, 25);
                await minitel.Color(Minitel.Jaune);
                await minitel.Print($"Score: {score}");
                await minitel.Color(Minitel.Blanc);
                lastScore = score;
            }
        }
        
        // Dessiner la nourriture
        async Task DrawFood()
        {
            await minitel.MoveTo(2 + food.y, 3 + food.x + 1);
            await minitel.Color(Minitel.Jaune);
            await minitel.Print(Food);
            await minitel.Color(Minitel.Blanc);
        }
        
        // Mettre à jour l'affichage du serpent
        async Task UpdateSnake()
        {
            // Dessiner la tête du serpent
            await minitel.MoveTo(2 + snake[0].y, 3 + snake[0].x + 1);
            await minitel.Color(Minitel.Vert);
            await minitel.Print(SnakeHead);
            
            // Si nous avons un second segment, l'ancien segment de tête devient un segment de corps
            if (snake.Count > 1)
            {
                await minitel.MoveTo(2 + snake[1].y, 3 + snake[1].x + 1);
                await minitel.Print(SnakeBody);
            }
            
            // Effacer l'ancienne queue uniquement si aucune nourriture n'a été mangée
            if (lastTail.HasValue && !foodEaten)
            {
                await minitel.MoveTo(2 + lastTail.Value.y, 3 + lastTail.Value.x + 1);
                await minitel.Print(Empty);
            }
            
            // Réinitialiser le drapeau de nourriture mangée
            foodEaten = false;
            
            await minitel.Color(Minitel.Blanc);
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
            await minitel.MoveTo(10, 8);
            await minitel.Color(Minitel.Rouge);
            await minitel.Print("GAME OVER");
            
            await minitel.MoveTo(12, 8);
            await minitel.Color(Minitel.Jaune);
            await minitel.Print($"Score final: {score}");
            
            await minitel.MoveTo(14, 8);
            await minitel.Color(Minitel.Blanc);
            await minitel.Print("Appuyez sur ENVOI");
            
            // Attendre que l'utilisateur appuie sur ENVOI
            while (true)
            {
                var isEnvoi = await minitel.Input(0, 0, 1, "", " ", false);
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
                        var input = await minitel.Input(0, 0, 1, "", " ", false);
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
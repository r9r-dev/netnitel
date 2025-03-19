import { Pynitel, PynitelWS } from "./pynitel.js";
import { startServer } from "./server.js";

// Import service functions
import { annuaire } from "./annuaire.js";
import { snakeGame } from "./snake.js";

// Welcome page handler
async function welcomePage(websocket) {
  const m = new Pynitel(new PynitelWS(websocket));

  // Function to display the welcome page
  async function displayWelcome() {
    await m.home();
    await m.cls();

    // Display title
    await m.pos(2, 15);
    await m.color(m.blanc);
    await m.backcolor(m.bleu);
    await m.print(" 3615 SYMPA ");
    await m.normal();

    // Display menu
    await m.pos(6, 10);
    await m.print("1 - Annuaire");

    await m.pos(8, 10);
    await m.print("2 - Snake");

    // Instructions
    await m.pos(12, 5);
    await m.print("Choisissez un service (1-2)");

    await m.pos(14, 5);
    await m.print("ou appuyez sur ENVOI");
  }

  // Display the welcome page
  await displayWelcome();

  // Handle user input
  while (true) {
    const [input, key] = await m.input(20, 20, 1, "", " ", false);

    if (key === m.envoi && !input) {
      // Show message if ENVOI is pressed without a selection
      await m.message(0, 1, 3, "Veuillez choisir un service (1-2)");
    } else if (input === "1") {
      // Redirect to annuaire service
      await m.cls();
      await annuaire(websocket);
      await displayWelcome();
    } else if (input === "2") {
      // Redirect to snake game
      await m.cls();
      await snakeGame(websocket);
      await displayWelcome();
    }
  }
}

// Start the welcome page server
(async function () {
  startServer(welcomePage, 3615, "Welcome Page");
})().catch((err) => {
  console.error("Server error:", err);
  process.exit(1);
});

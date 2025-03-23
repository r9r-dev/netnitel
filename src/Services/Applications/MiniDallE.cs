using NetNitel.Services.Engine;
using NetNitel.Services.Engine.Enums;

namespace NetNitel.Services.Applications;

public class MiniDallE(Minitel tel) : MiniApplication(tel)
{
    public override async Task StartAsync()
    {
        await tel.Reset();
        await tel.Control.ForeColor(MiniColor.Blue);
        await tel.Control.WriteGraphic("110000");
        await tel.Control.Repeat(39);
        
        await tel.Control.Move(9);
        await tel.Control.ForeColor(MiniColor.Blue);
        await tel.Control.WriteGraphic("000011");
        await tel.Control.Repeat(39);
        
        await tel.Draw.Text("Mini", ligne: 2, colonne: 7, foreColor: MiniColor.Yellow);
        await tel.Draw.Text(" Dall·E", ligne: 7, colonne: 12, foreColor: MiniColor.Yellow, disjoint: true);

        await tel.Draw.HorizontalLine(12, MiniPosition.Down);
        await tel.Control.ForeColor(MiniColor.White);
        await tel.Control.BackColor(MiniColor.Blue);
        await tel.Write.Text("          Dall-E sur Minitel ?          ");
        await tel.Control.ForeColor(MiniColor.White);
        await tel.Control.BackColor(MiniColor.Blue);
        await tel.Write.Text("            C'est possible !            ");
        await tel.Draw.HorizontalLine(15, MiniPosition.Up);

        await tel.Draw.HorizontalLine(20, MiniPosition.Middle);
        await tel.Write.Text("Valider description");
        await tel.Control.Move(21, 33);
        await tel.Control.BackColor(MiniColor.Green);
        await tel.Control.ForeColor(MiniColor.Black);
        await tel.Write.Text(" Envoi ");
        await tel.Control.Move(22);
        await tel.Write.Text("Annuaire des services");
        await tel.Control.Move(22, 33);
        await tel.Control.BackColor(MiniColor.Green);
        await tel.Control.ForeColor(MiniColor.Black);
        await tel.Write.Text(" Guide ");

        
        await tel.Draw.HorizontalLine(23, MiniPosition.Middle, MiniColor.Black, MiniColor.Red);

        var result = await tel.Read.TextBox(16, 1, 160, "Description: ");
        
        
        

    }
}
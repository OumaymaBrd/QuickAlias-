using System;
using System.Windows.Forms;

namespace PowerShellShortcutCreator
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Vérifier si l'application tourne déjà
            bool createdNew;
            using (var mutex = new System.Threading.Mutex(true, "GlobalShortcutManager", out createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show("Le Gestionnaire de Raccourcis est déjà en cours d'exécution!", 
                        "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                Application.Run(new MainForm());
            }
        }
    }
}

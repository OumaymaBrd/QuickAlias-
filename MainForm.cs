using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Text.Json;
using System.Diagnostics;

namespace PowerShellShortcutCreator
{
    public partial class MainForm : Form
    {
        private Label nameLabel;
        private TextBox shortcutNameTextBox;
        private Label hotkeyLabel;
        private ComboBox modifier1ComboBox;
        private ComboBox modifier2ComboBox;
        private ComboBox keyComboBox;
        private Button createButton;
        private Label statusLabel;
        private GroupBox shortcutGroupBox; // Ajouter cette ligne

        private TabControl mainTabControl;
        private TabPage createTabPage;
        private TabPage manageTabPage;
        
        // Create Tab Controls
        private GroupBox actionGroupBox;
        private ComboBox actionTypeComboBox;
        private Label actionLabel;
        private TextBox actionPathTextBox;
        private Button browseActionButton;
        
        // Manage Tab Controls
        private ListView shortcutsListView;
        private Button deleteButton;
        private Button editButton;
        private Button testButton;
        
        private List<ShortcutItem> shortcuts = new List<ShortcutItem>();
        private readonly string configFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ShortcutManager", "shortcuts.json");
        
        // Raccourcis Windows réservés
        private readonly HashSet<string> reservedShortcuts = new HashSet<string>
        {
            "Win+L", "Win+R", "Win+D", "Win+E", "Win+I", "Win+X", "Win+A", "Win+S",
            "Win+Tab", "Win+1", "Win+2", "Win+3", "Win+4", "Win+5", "Win+6", "Win+7", "Win+8", "Win+9", "Win+0",
            "Ctrl+C", "Ctrl+V", "Ctrl+X", "Ctrl+Z", "Ctrl+Y", "Ctrl+A", "Ctrl+S", "Ctrl+O", "Ctrl+N", "Ctrl+P",
            "Alt+Tab", "Alt+F4", "Ctrl+Alt+Del", "Ctrl+Shift+Esc"
        };

       
        private GlobalKeyboardHook keyboardHook;

        
        public MainForm()
        {
            InitializeComponent();
            LoadShortcuts();
            RefreshShortcutsList();
            
           
            keyboardHook = new GlobalKeyboardHook();
            RegisterAllShortcuts();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

           
            this.Text = "Gestionnaire de Raccourcis Globaux";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(700, 600);
            this.Icon = SystemIcons.Application;

            
            mainTabControl = new TabControl
            {
                Location = new Point(10, 10),
                Size = new Size(660, 540),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // Create Tab
            createTabPage = new TabPage("Créer Raccourci");
            InitializeCreateTab();
            
            // Manage Tab
            manageTabPage = new TabPage("Gérer Raccourcis");
            InitializeManageTab();

            mainTabControl.TabPages.Add(createTabPage);
            mainTabControl.TabPages.Add(manageTabPage);

            this.Controls.Add(mainTabControl);
            this.ResumeLayout(false);
        }

        private void InitializeCreateTab()
        {
            // Action GroupBox
            actionGroupBox = new GroupBox
            {
                Text = "Action à exécuter",
                Location = new Point(20, 20),
                Size = new Size(600, 120),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            actionLabel = new Label
            {
                Text = "Type d'action :",
                Location = new Point(15, 25),
                Size = new Size(100, 20)
            };

            actionTypeComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(120, 23),
                Size = new Size(150, 23)
            };
            actionTypeComboBox.Items.AddRange(new[] { 
                "Ouvrir Dossier", 
                "Ouvrir Terminal", 
                "Lancer Application",
                "Ouvrir Site Web",
                "Commande Personnalisée"
            });
            actionTypeComboBox.SelectedIndex = 0;
            actionTypeComboBox.SelectedIndexChanged += ActionTypeComboBox_SelectedIndexChanged;

            var actionPathLabel = new Label
            {
                Text = "Chemin/Commande :",
                Location = new Point(15, 55),
                Size = new Size(120, 20)
            };

            actionPathTextBox = new TextBox
            {
                Location = new Point(140, 53),
                Size = new Size(350, 23),
                Text = @"C:\wamp64\www\oumayma"
            };

            browseActionButton = new Button
            {
                Text = "Parcourir...",
                Location = new Point(500, 52),
                Size = new Size(80, 25)
            };
            browseActionButton.Click += BrowseActionButton_Click;

            actionGroupBox.Controls.AddRange(new Control[] { 
                actionLabel, actionTypeComboBox, actionPathLabel, actionPathTextBox, browseActionButton 
            });

            // Shortcut GroupBox
            shortcutGroupBox = new GroupBox
            {
                Text = "Configuration du raccourci",
                Location = new Point(20, 150),
                Size = new Size(600, 120),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            nameLabel = new Label
            {
                Text = "Nom du raccourci :",
                Location = new Point(15, 25),
                Size = new Size(120, 20)
            };

            shortcutNameTextBox = new TextBox
            {
                Text = "Mon Raccourci",
                Location = new Point(140, 23),
                Size = new Size(200, 23)
            };

            hotkeyLabel = new Label
            {
                Text = "Combinaison :",
                Location = new Point(15, 55),
                Size = new Size(120, 20)
            };

            modifier1ComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(140, 53),
                Size = new Size(80, 23)
            };
            modifier1ComboBox.Items.AddRange(new[] { "Win", "Ctrl", "Alt", "Shift" });
            modifier1ComboBox.SelectedIndex = 0;
            modifier1ComboBox.SelectedIndexChanged += ValidateShortcut;

            var plusLabel1 = new Label
            {
                Text = "+",
                Location = new Point(225, 55),
                Size = new Size(15, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            modifier2ComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(245, 53),
                Size = new Size(80, 23)
            };
            modifier2ComboBox.Items.AddRange(new[] { "(Aucun)", "Ctrl", "Alt", "Shift" });
            modifier2ComboBox.SelectedIndex = 0;
            modifier2ComboBox.SelectedIndexChanged += ValidateShortcut;

            var plusLabel2 = new Label
            {
                Text = "+",
                Location = new Point(330, 55),
                Size = new Size(15, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            keyComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(350, 53),
                Size = new Size(70, 23)
            };
            
            // Ajouter les lettres A-Z et chiffres 0-9
            for (char c = 'A'; c <= 'Z'; c++)
                keyComboBox.Items.Add(c.ToString());
            for (char c = '0'; c <= '9'; c++)
                keyComboBox.Items.Add(c.ToString());
            keyComboBox.Items.AddRange(new[] { "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12" });
            keyComboBox.SelectedIndex = 7; // H par défaut
            keyComboBox.SelectedIndexChanged += ValidateShortcut;

            shortcutGroupBox.Controls.AddRange(new Control[] { 
                nameLabel, shortcutNameTextBox, hotkeyLabel, 
                modifier1ComboBox, plusLabel1, modifier2ComboBox, plusLabel2, keyComboBox 
            });

            // Status Label
            statusLabel = new Label
            {
                Location = new Point(20, 280),
                Size = new Size(600, 60),
                ForeColor = Color.Green,
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.TopLeft
            };

            // Create Button
            createButton = new Button
            {
                Text = "Créer le Raccourci",
                Location = new Point(520, 350),
                Size = new Size(120, 35),
                BackColor = Color.LightGreen,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                UseVisualStyleBackColor = false
            };
            createButton.Click += CreateButton_Click;

            createTabPage.Controls.AddRange(new Control[] {
                actionGroupBox, shortcutGroupBox, statusLabel, createButton
            });
        }

        private void InitializeManageTab()
        {
            // ListView for shortcuts
            shortcutsListView = new ListView
            {
                Location = new Point(20, 20),
                Size = new Size(600, 350),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false
            };
            
            shortcutsListView.Columns.Add("Nom", 150);
            shortcutsListView.Columns.Add("Raccourci", 120);
            shortcutsListView.Columns.Add("Type", 100);
            shortcutsListView.Columns.Add("Action", 200);

            // Buttons
            testButton = new Button
            {
                Text = "Tester",
                Location = new Point(20, 380),
                Size = new Size(100, 30),
                BackColor = Color.LightBlue
            };
            testButton.Click += TestButton_Click;

            editButton = new Button
            {
                Text = "Modifier",
                Location = new Point(130, 380),
                Size = new Size(100, 30),
                BackColor = Color.LightYellow
            };
            editButton.Click += EditButton_Click;

            deleteButton = new Button
            {
                Text = "Supprimer",
                Location = new Point(240, 380),
                Size = new Size(100, 30),
                BackColor = Color.LightCoral
            };
            deleteButton.Click += DeleteButton_Click;

            manageTabPage.Controls.AddRange(new Control[] {
                shortcutsListView, testButton, editButton, deleteButton
            });
        }

        private void ActionTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedType = actionTypeComboBox.SelectedItem.ToString();
            
            switch (selectedType)
            {
                case "Ouvrir Dossier":
                    actionPathTextBox.Text = @"C:\wamp64\www\oumayma";
                    browseActionButton.Enabled = true;
                    break;
                case "Ouvrir Terminal":
                    actionPathTextBox.Text = "cmd.exe";
                    browseActionButton.Enabled = true;
                    break;
                case "Lancer Application":
                    actionPathTextBox.Text = "notepad.exe";
                    browseActionButton.Enabled = true;
                    break;
                case "Ouvrir Site Web":
                    actionPathTextBox.Text = "https://www.google.com";
                    browseActionButton.Enabled = false;
                    break;
                case "Commande Personnalisée":
                    actionPathTextBox.Text = "echo Hello World";
                    browseActionButton.Enabled = false;
                    break;
            }
        }

        private void BrowseActionButton_Click(object sender, EventArgs e)
        {
            string selectedType = actionTypeComboBox.SelectedItem.ToString();
            
            if (selectedType == "Ouvrir Dossier")
            {
                using (var folderDialog = new FolderBrowserDialog())
                {
                    folderDialog.SelectedPath = actionPathTextBox.Text;
                    if (folderDialog.ShowDialog() == DialogResult.OK)
                    {
                        actionPathTextBox.Text = folderDialog.SelectedPath;
                    }
                }
            }
            else
            {
                using (var fileDialog = new OpenFileDialog())
                {
                    fileDialog.Filter = "Exécutables (*.exe)|*.exe|Tous les fichiers (*.*)|*.*";
                    if (fileDialog.ShowDialog() == DialogResult.OK)
                    {
                        actionPathTextBox.Text = fileDialog.FileName;
                    }
                }
            }
        }

        private void ValidateShortcut(object sender, EventArgs e)
        {
            string shortcut = GetCurrentShortcutString();
            
            if (reservedShortcuts.Contains(shortcut))
            {
                statusLabel.ForeColor = Color.Red;
                statusLabel.Text = $"⚠️ ATTENTION: {shortcut} est un raccourci Windows réservé!\nChoisissez une autre combinaison.";
                createButton.Enabled = false;
            }
            else if (shortcuts.Any(s => s.Shortcut == shortcut))
            {
                statusLabel.ForeColor = Color.Orange;
                statusLabel.Text = $"⚠️ Le raccourci {shortcut} existe déjà dans vos raccourcis personnalisés.";
                createButton.Enabled = false;
            }
            else
            {
                statusLabel.ForeColor = Color.Green;
                statusLabel.Text = $"✅ Raccourci {shortcut} disponible!";
                createButton.Enabled = true;
            }
        }

        private string GetCurrentShortcutString()
        {
            string shortcut = modifier1ComboBox.SelectedItem.ToString();
            if (modifier2ComboBox.SelectedItem.ToString() != "(Aucun)")
            {
                shortcut += "+" + modifier2ComboBox.SelectedItem.ToString();
            }
            shortcut += "+" + keyComboBox.SelectedItem.ToString();
            return shortcut;
        }

        private void RegisterAllShortcuts()
        {
            keyboardHook.ClearShortcuts();
            foreach (var shortcut in shortcuts)
            {
                keyboardHook.RegisterShortcut(shortcut);
            }
        }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            try
            {
                var newShortcut = new ShortcutItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = shortcutNameTextBox.Text,
                    Shortcut = GetCurrentShortcutString(),
                    ActionType = actionTypeComboBox.SelectedItem.ToString(),
                    ActionPath = actionPathTextBox.Text,
                    CreatedDate = DateTime.Now
                };

                shortcuts.Add(newShortcut);
                SaveShortcuts();
                RefreshShortcutsList();
                
                // Enregistrer le nouveau raccourci dans le hook
                keyboardHook.RegisterShortcut(newShortcut);

                statusLabel.ForeColor = Color.Green;
                statusLabel.Text = $"✅ Raccourci '{newShortcut.Name}' créé et activé!\nUtilisez {newShortcut.Shortcut} pour l'activer.";

                // Switch to manage tab
                mainTabControl.SelectedTab = manageTabPage;
            }
            catch (Exception ex)
            {
                statusLabel.ForeColor = Color.Red;
                statusLabel.Text = $"❌ Erreur: {ex.Message}";
            }
        }

        private void CreateGlobalShortcutScript(ShortcutItem shortcut)
        {
            string scriptDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ShortcutManager", "Scripts");
            Directory.CreateDirectory(scriptDir);
            
            string scriptPath = Path.Combine(scriptDir, $"{shortcut.Id}.ps1");
            
            string command = GetCommandForAction(shortcut);
            
            string scriptContent = $@"
# Raccourci: {shortcut.Name} ({shortcut.Shortcut})
# Créé le: {shortcut.CreatedDate}

Add-Type -AssemblyName System.Windows.Forms

# Fonction pour exécuter l'action
function Execute-Action {{
    try {{
        {command}
        Write-Host ""Raccourci '{shortcut.Name}' exécuté avec succès!""
    }}
    catch {{
        Write-Host ""Erreur lors de l'exécution: $($_.Exception.Message)""
    }}
}}

# Enregistrer le raccourci global (nécessite un service ou AutoHotkey)
Write-Host ""Script créé pour {shortcut.Name} - {shortcut.Shortcut}""
Write-Host ""Pour activer les raccourcis globaux, utilisez AutoHotkey ou un service Windows.""
";

            File.WriteAllText(scriptPath, scriptContent);
        }

        private string GetCommandForAction(ShortcutItem shortcut)
        {
            switch (shortcut.ActionType)
            {
                case "Ouvrir Dossier":
                    return $"Start-Process explorer.exe -ArgumentList '\"{shortcut.ActionPath}\"'";
                case "Ouvrir Terminal":
                    return $"Start-Process cmd.exe -WorkingDirectory '{Path.GetDirectoryName(shortcut.ActionPath) ?? "C:\\"}'";
                case "Lancer Application":
                    return $"Start-Process '{shortcut.ActionPath}'";
                case "Ouvrir Site Web":
                    return $"Start-Process '{shortcut.ActionPath}'";
                case "Commande Personnalisée":
                    return $"Invoke-Expression '{shortcut.ActionPath}'";
                default:
                    return $"Write-Host 'Action non reconnue: {shortcut.ActionType}'";
            }
        }

        private void TestButton_Click(object sender, EventArgs e)
        {
            if (shortcutsListView.SelectedItems.Count > 0)
            {
                var selectedItem = shortcutsListView.SelectedItems[0];
                var shortcut = shortcuts.FirstOrDefault(s => s.Id == selectedItem.Tag.ToString());
                
                if (shortcut != null)
                {
                    ExecuteShortcut(shortcut);
                }
            }
        }

        private void ExecuteShortcut(ShortcutItem shortcut)
        {
            try
            {
                switch (shortcut.ActionType)
                {
                    case "Ouvrir Dossier":
                        Process.Start("explorer.exe", shortcut.ActionPath);
                        break;
                    case "Ouvrir Terminal":
                        Process.Start("cmd.exe");
                        break;
                    case "Lancer Application":
                        Process.Start(shortcut.ActionPath);
                        break;
                    case "Ouvrir Site Web":
                        Process.Start(new ProcessStartInfo(shortcut.ActionPath) { UseShellExecute = true });
                        break;
                    case "Commande Personnalisée":
                        var psi = new ProcessStartInfo("cmd.exe", $"/c {shortcut.ActionPath}")
                        {
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        Process.Start(psi);
                        break;
                }
                
                MessageBox.Show($"Raccourci '{shortcut.Name}' exécuté avec succès!", "Test Réussi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'exécution: {ex.Message}", "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            // TODO: Implémenter l'édition
            MessageBox.Show("Fonctionnalité d'édition à implémenter", "Info", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Modifier la méthode DeleteButton_Click pour désenregistrer le raccourci
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (shortcutsListView.SelectedItems.Count > 0)
            {
                var selectedItem = shortcutsListView.SelectedItems[0];
                var shortcut = shortcuts.FirstOrDefault(s => s.Id == selectedItem.Tag.ToString());
                
                if (shortcut != null)
                {
                    var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer le raccourci '{shortcut.Name}' ?", 
                        "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    
                    if (result == DialogResult.Yes)
                    {
                        // Désenregistrer du hook
                        keyboardHook.UnregisterShortcut(shortcut.Shortcut);
                        
                        shortcuts.Remove(shortcut);
                        SaveShortcuts();
                        RefreshShortcutsList();
                    }
                }
            }
        }

        private void RefreshShortcutsList()
        {
            shortcutsListView.Items.Clear();
            
            foreach (var shortcut in shortcuts)
            {
                var item = new ListViewItem(shortcut.Name);
                item.SubItems.Add(shortcut.Shortcut);
                item.SubItems.Add(shortcut.ActionType);
                item.SubItems.Add(shortcut.ActionPath);
                item.Tag = shortcut.Id;
                
                shortcutsListView.Items.Add(item);
            }
        }

        private void LoadShortcuts()
        {
            try
            {
                if (File.Exists(configFile))
                {
                    string json = File.ReadAllText(configFile);
                    shortcuts = JsonSerializer.Deserialize<List<ShortcutItem>>(json) ?? new List<ShortcutItem>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement: {ex.Message}", "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SaveShortcuts()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(configFile));
                string json = JsonSerializer.Serialize(shortcuts, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configFile, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde: {ex.Message}", "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Ajouter cette méthode pour nettoyer les ressources
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            keyboardHook?.Dispose();
            base.OnFormClosed(e);
        }
    }

    public class ShortcutItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Shortcut { get; set; }
        public string ActionType { get; set; }
        public string ActionPath { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

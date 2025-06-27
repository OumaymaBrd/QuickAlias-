using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PowerShellShortcutCreator
{
    public class GlobalKeyboardHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        
        private LowLevelKeyboardProc _proc = HookCallback;
        private IntPtr _hookID = IntPtr.Zero;
        
        private static Dictionary<string, ShortcutItem> _shortcuts = new Dictionary<string, ShortcutItem>();
        private static HashSet<Keys> _pressedKeys = new HashSet<Keys>();
        private static IntPtr _staticHookID = IntPtr.Zero; 

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public GlobalKeyboardHook()
        {
            _hookID = SetHook(_proc);
            _staticHookID = _hookID;
        }

        public void RegisterShortcut(ShortcutItem shortcut)
        {
            _shortcuts[shortcut.Shortcut] = shortcut;
        }

        public void UnregisterShortcut(string shortcutKey)
        {
            _shortcuts.Remove(shortcutKey);
        }

        public void ClearShortcuts()
        {
            _shortcuts.Clear();
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;
                
                _pressedKeys.Add(key);
                
                // Vérifier si une combinaison correspond
                CheckShortcutCombination();
            }
            else if (nCode >= 0)
            {
                // Relâchement de touche - nettoyer les touches pressées
                _pressedKeys.Clear();
            }

            return CallNextHookEx(_staticHookID, nCode, wParam, lParam); // Utiliser _staticHookID
        }

        private static void CheckShortcutCombination()
        {
            foreach (var shortcut in _shortcuts)
            {
                if (IsShortcutPressed(shortcut.Key))
                {
                    ExecuteShortcut(shortcut.Value);
                    _pressedKeys.Clear(); 
                    break;
                }
            }
        }

        private static bool IsShortcutPressed(string shortcutString)
        {
            var parts = shortcutString.Split('+');
            var requiredKeys = new HashSet<Keys>();
            
            foreach (var part in parts)
            {
                switch (part.Trim())
                {
                    case "Win":
                        requiredKeys.Add(Keys.LWin);
                        requiredKeys.Add(Keys.RWin);
                        break;
                    case "Ctrl":
                        requiredKeys.Add(Keys.LControlKey);
                        requiredKeys.Add(Keys.RControlKey);
                        break;
                    case "Alt":
                        requiredKeys.Add(Keys.LMenu);
                        requiredKeys.Add(Keys.RMenu);
                        break;
                    case "Shift":
                        requiredKeys.Add(Keys.LShiftKey);
                        requiredKeys.Add(Keys.RShiftKey);
                        break;
                    default:
                        if (Enum.TryParse<Keys>(part.Trim(), out Keys key))
                        {
                            requiredKeys.Add(key);
                        }
                        break;
                }
            }

            // Vérifier si toutes les touches requises sont pressées
            foreach (var requiredKey in requiredKeys)
            {
                bool keyPressed = false;
                foreach (var pressedKey in _pressedKeys)
                {
                    if (pressedKey == requiredKey || 
                        (requiredKey == Keys.LWin && pressedKey == Keys.RWin) ||
                        (requiredKey == Keys.RWin && pressedKey == Keys.LWin) ||
                        (requiredKey == Keys.LControlKey && pressedKey == Keys.RControlKey) ||
                        (requiredKey == Keys.RControlKey && pressedKey == Keys.LControlKey))
                    {
                        keyPressed = true;
                        break;
                    }
                }
                if (!keyPressed) return false;
            }
            
            return requiredKeys.Count > 0;
        }

        private static void ExecuteShortcut(ShortcutItem shortcut)
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'exécution du raccourci '{shortcut.Name}': {ex.Message}", 
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Dispose()
        {
            UnhookWindowsHookEx(_hookID);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}

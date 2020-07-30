using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace QueryWorkbenchUI {
    public class KeyboardShortcutsProvider {
        public Dictionary<Tuple<Keys, Keys, Keys>, ShortcutDef> Shortcuts { get; protected set; }

        public KeyboardShortcutsProvider() {
            Shortcuts = new Dictionary<Tuple<Keys, Keys, Keys>, ShortcutDef>();
        }

        public void Add(Tuple<Keys, Keys, Keys> keys, Action action, string description) {
            Shortcuts.Add(keys, new ShortcutDef(action, description));
        }

        public Dictionary<Keys, Action> GetKeyboardActionsMap() {
            return Shortcuts.ToDictionary(x => x.Key.Item1 | x.Key.Item2 | x.Key.Item3, x => x.Value.Action);
        }

        public Dictionary<string, ShortcutDescriptionDef> GetKeyboardDescriptionMap() {
            return Shortcuts.ToDictionary(x => GetKeyName(x.Key), x => GetShortcutDescriptionRef(x));
        }

        private string GetKeyName(Tuple<Keys, Keys, Keys> keys) {
            var key1 = keys.Item1.ToString();
            var key2 = keys.Item2.ToString();
            var keyCombinationName = $"{key1} + {key2}";
            if (keys.Item3 != Keys.None) {
                keyCombinationName += " + " + keys.Item3.ToString();
            }
            return keyCombinationName.Replace("Control", "Ctrl");
        }
        
        private ShortcutDescriptionDef GetShortcutDescriptionRef(KeyValuePair<Tuple<Keys, Keys, Keys>, ShortcutDef> keys) {
            int keyCount = (keys.Key.Item1 != Keys.None ? 1 : 0) +
                            (keys.Key.Item2 != Keys.None ? 1 : 0) +
                            (keys.Key.Item3 != Keys.None ? 1 : 0);
            return new ShortcutDescriptionDef(keyCount, keys.Value.Description);
        }
    }

    public class ShortcutDef {
        public readonly Action Action;
        public readonly string Description;

        public ShortcutDef(Action action, string description) {
            Action = action;
            Description = description;
        }
    }

    public class ShortcutDescriptionDef {
        public readonly int KeyCount;
        public readonly string Description;
        public ShortcutDescriptionDef(int keyCount, string description) {
            KeyCount = keyCount;
            Description = description;
        }
    }
}

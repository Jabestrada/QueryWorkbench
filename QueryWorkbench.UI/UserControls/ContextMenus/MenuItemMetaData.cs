namespace QueryWorkbenchUI.UserControls.ContextMenus {
    public class MenuItemMetadata {
        public MenuItemActionType ActionType { get; set; }
        public MenuItemMetadata(MenuItemActionType actionType) {
            ActionType = actionType;
        }
    }

    public enum MenuItemActionType { 
        ColumnHeaderVisibility
    }
}

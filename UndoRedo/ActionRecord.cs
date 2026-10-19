using System;

namespace UndoRedo
{
    /// <summary>
    /// Encapsulates the details a single action that was performed by the user.
    /// </summary>
    public class ActionRecord
    {
        #region Constructors
        public ActionRecord()
        {
        }

        public ActionRecord(DateTime time, string name, string description, ActionType type)
        {
            _actionTime = time;
            _actionName = name;
            _actionDescription = description;
            _actionType = type;
        }
        #endregion

        #region Public Properties
        public DateTime ActionTime
        {
            get { return _actionTime; }
            set { _actionTime = value; }
        }

        public string ActionName
        {
            get { return _actionName; }
            set { _actionName = value; }
        }

        public string ActionDescription
        {
            get { return _actionDescription; }
            set { _actionDescription = value; }
        }

        public ActionType ActionType
        {
            get { return _actionType; }
            set { _actionType = value; }
        }
        #endregion

        #region Private Variables
        private DateTime _actionTime;
        private string _actionName;
        private string _actionDescription;
        private ActionType _actionType = ActionType.None;
        #endregion
    }

    /// <summary>
    /// The type of the action that was performed by the user
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// Illegal value
        /// </summary>
        None = 0,

        /// <summary>
        /// Normal action
        /// </summary>
        Do = 1,

        /// <summary>
        /// Undo action
        /// </summary>
        Undo = 2,

        /// <summary>
        /// Redo action
        /// </summary>
        Redo = 3,
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Windows.Input;

using Monopoly.Game;

namespace Monopoly
{
    /// <summary>
    /// 
    /// </summary>
    public class BoundCanExecuteCommand : ICommand
    {

        #region Parameters
        private Action<object> _Action;
        private Func<bool> _CanExecute;
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor; the parameter will determine what function is run when the ICommand is called by the View.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="canExecute">This parameter is optional, but can include a Predicate to allow for the CanExecute implementation.</param>
        public BoundCanExecuteCommand(Action<object> action, Func<bool> canExecute = null)
        {
            _Action = action;
            _CanExecute = canExecute;
        }
        #endregion

        #region ICommand implementation
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public void RaiseCanExecuteChanged()
        {
            // Raise the event!
            CanExecuteChanged += (sender, e) => { };
        }
        /// <summary>
        /// Determines whether or not the RelayCommand given can be executed (or not).
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            if(_CanExecute == null) { return true; }
            return _CanExecute.Invoke();
        }
            
        /// <summary>
        /// Used to execute the defined command.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        public void Execute(object parameter)
        {
            Console.WriteLine("BoundCanExecuteCommand request, parameter: " + parameter);
            _Action(parameter);
        }
        #endregion

    }
}

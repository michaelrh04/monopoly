using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Windows.Input;

namespace Monopoly
{
    /// <summary>
    /// RelayCommands are used in the MVVM framework to pass instructions from the view to action (subroutines) in the models.
    /// This is a basic implementation of a relay command. It is used for most (if not all) commands by invoking the action passed when necessary.
    /// </summary>
    public class RelayCommand : ICommand
    {

        #region Parameters
        private Action<object> _Action;
        Predicate<object[]> _CanExecute;
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor; the parameter will determine what function is run when the ICommand is called by the View.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="canExecute">This parameter is optional, but can include a Predicate to allow for the CanExecute implementation.</param>
        public RelayCommand(Action<object> action, Predicate<object[]> canExecute = null)
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
            // If the variable is not given, the result should always be true.
            // This also means that there is no need for any CanExecuteChanged event handling.
            if (_CanExecute == null)
            {
                return true;
            }
            // If the parameter is null, but the _CanExecute is not null, CanExecute() has been called before a parameter has loaded in the View. Return false.
            if(parameter == null)
            {
                return false;
            }
            // If the parameter itself is not a list of objects (e.g. it is a single object), we must first convert it so that the parameter is the first index in a list of objects.
            if (parameter is object[]) { }
            else
            {
                // If the object is an ObservableCollection, it is the Players ObservableCollection being passed. It first needs to be converted to a list!
                if(parameter is ObservableCollection<Player> oc)
                {
                    parameter = oc.ToList();
                }
                // Create an object[] with the parameter as its first index.
                object[] param = new object[] { parameter };
                parameter = param;
            }
            // Preparation of the parameter is complete, we can continue with execution.
            try
            {
                // If it is given, calling it will determine the output.
                return _CanExecute((object[])parameter);
            }
            catch (InvalidCastException e)
            {
                // This error is expected: the ParameterCompilerConverter can occasionally fall 
                // behind the ViewModel when conditions in the view update.
                // Handle it to prevent the program crashing, printing a message just in case.
                Console.WriteLine("The CanExecute RelayCommand subroutine has thrown an InvalidCastException. This is not usually anything to worry about: in most cases, it is because the ViewModel has attempted to assess the CanExecute before the View has had enough time to change the applicable parameters.");
                Console.WriteLine("Error readout: " + e.Message);
            }
            catch (IndexOutOfRangeException e)
            {
                // This error is expected: the ParameterCompilerConverter can occasionally fall 
                // behind the ViewModel when conditions in the view update.
                // Handle it to prevent the program crashing, printing a message just in case.
                Console.WriteLine("The CanExecute RelayCommand subroutine has thrown an IndexOutOfRangeException. This is not usually anything to worry about: in most cases, it is because the ViewModel has attempted to assess the CanExecute before the View has had enough time to change the applicable parameters.");
                Console.WriteLine("Error readout: " + e.Message);
            }
            // If we reach this point, return false: not being able to select an option will prevent the user from inadvertently crashing the program.
            return false;
        }
            
        /// <summary>
        /// Used to execute the defined command.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        public void Execute(object parameter)
        {
            try
            {
                _Action(parameter);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Experienced an ArgumentException upon calling (RelayCommand).Execute(). Readout is as follows: " + e.Message);
            }
        }
        #endregion

    }
}

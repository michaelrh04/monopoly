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
    public class RelayCommand : ICommand
    {

        #region Parameters
        private Action<object> _Action;
        private object _CanExecute;
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor; the parameter will determine what function is run when the ICommand is called by the View.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="canExecute">This parameter is optional, but can include a Predicate to allow for the CanExecute implementation.</param>
        public RelayCommand(Action<object> action, object canExecute = null, Func<bool> canExecuteFunction = null)
        {
            _Action = action;
            if(canExecute == null && canExecuteFunction != null)
            {
                _CanExecute = canExecuteFunction;
            } 
            else
            {
                _CanExecute = canExecute;
            }
            
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
            if (_CanExecute == null)
            {
                return true;
            }
            #region Bool function handling
            else if (_CanExecute is Func<bool> function)
            {
                return function.Invoke();
            }
            #endregion
            #region Predicate handling
            else if (_CanExecute is Predicate<object[]> predicate)
            {
                if (parameter == null)
                {
                    // If the parameter is null, but the _CanExecute is not null, CanExecute() has been called before a parameter has loaded in the View. Return false.
                    return false;
                }
                else
                {
                    // We must then handle the specifics of the parameter:
                    // If the parameter itself is not a list of objects (e.g. it is a single object), we must first convert it so that the parameter is the first index in a list of objects.
                    // This allows the same predicate format to handle all requests and avoids code replication.
                    if (!(parameter is object[])) 
                    {
                        // If the object is an ObservableCollection, furthermore, it must be the Players ObservableCollection being passed. 
                        // It first needs to be converted to a list before continuing!
                        if (parameter is ObservableCollection<Player> oc)
                        {
                            parameter = oc.ToList();
                        }
                        // Create an object[] with the parameter as its first index.
                        object[] param = new object[] { parameter };
                        parameter = param;
                    }
                    // Preparation of the parameter is complete, we can continue with execution.
                    // This area also must be accompanied by significant exception handling, considering the V-VM relationship can suffer from View components loading slowly.
                    try
                    {
                        // If it is given, calling it will determine the output.
                        return predicate((object[])parameter);
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
            }
            #endregion
            else
            {
                throw new NotImplementedException();
            }
            
        }
            
        /// <summary>
        /// Used to execute the defined command.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        public void Execute(object parameter)
        {
            _Action(parameter);
        }
        #endregion

    }

    /// <summary></summary>
    public class CopyOfRelayCommand : ICommand
    {

        #region Parameters
        private Action<object> _Action;
        private object _CanExecute;
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor; the parameter will determine what function is run when the ICommand is called by the View.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="canExecute">This parameter is optional, but can include a Predicate to allow for the CanExecute implementation.</param>
        public CopyOfRelayCommand(Action<object> action, object canExecute = null, Func<bool> canExecuteFunction = null)
        {
            _Action = action;
            if (canExecute == null && canExecuteFunction != null)
            {
                _CanExecute = canExecuteFunction;
            }
            else
            {
                _CanExecute = canExecute;
            }

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
        /// Determines whether or not the CopyOfRelayCommand given can be executed (or not).
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            if (_CanExecute == null)
            {
                return true;
            }
            #region Bool function handling
            else if (_CanExecute is Func<bool> function)
            {
                return function.Invoke();
            }
            #endregion
            #region Predicate handling
            else if (_CanExecute is Predicate<object[]> predicate)
            {
                if (parameter == null)
                {
                    // If the parameter is null, but the _CanExecute is not null, CanExecute() has been called before a parameter has loaded in the View. Return false.
                    return false;
                }
                else
                {
                    // We must then handle the specifics of the parameter:
                    // If the parameter itself is not a list of objects (e.g. it is a single object), we must first convert it so that the parameter is the first index in a list of objects.
                    // This allows the same predicate format to handle all requests and avoids code replication.
                    if (!(parameter is object[]))
                    {
                        // If the object is an ObservableCollection, furthermore, it must be the Players ObservableCollection being passed. 
                        // It first needs to be converted to a list before continuing!
                        if (parameter is ObservableCollection<Player> oc)
                        {
                            parameter = oc.ToList();
                        }
                        // Create an object[] with the parameter as its first index.
                        object[] param = new object[] { parameter };
                        parameter = param;
                    }
                    // Preparation of the parameter is complete, we can continue with execution.
                    // This area also must be accompanied by significant exception handling, considering the V-VM relationship can suffer from View components loading slowly.
                    try
                    {
                        // If it is given, calling it will determine the output.
                        return predicate((object[])parameter);
                    }
                    catch (InvalidCastException e)
                    {
                        // This error is expected: the ParameterCompilerConverter can occasionally fall 
                        // behind the ViewModel when conditions in the view update.
                        // Handle it to prevent the program crashing, printing a message just in case.
                        Console.WriteLine("The CanExecute CopyOfRelayCommand subroutine has thrown an InvalidCastException. This is not usually anything to worry about: in most cases, it is because the ViewModel has attempted to assess the CanExecute before the View has had enough time to change the applicable parameters.");
                        Console.WriteLine("Error readout: " + e.Message);
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        // This error is expected: the ParameterCompilerConverter can occasionally fall 
                        // behind the ViewModel when conditions in the view update.
                        // Handle it to prevent the program crashing, printing a message just in case.
                        Console.WriteLine("The CanExecute CopyOfRelayCommand subroutine has thrown an IndexOutOfRangeException. This is not usually anything to worry about: in most cases, it is because the ViewModel has attempted to assess the CanExecute before the View has had enough time to change the applicable parameters.");
                        Console.WriteLine("Error readout: " + e.Message);
                    }
                    // If we reach this point, return false: not being able to select an option will prevent the user from inadvertently crashing the program.
                    return false;
                }
            }
            #endregion
            else
            {
                throw new NotImplementedException();
            }

        }

        /// <summary>
        /// Used to execute the defined command.
        /// </summary>
        /// <param name="parameter">The parameter for the command.</param>
        public void Execute(object parameter)
        {
            _Action(parameter);
        }
        #endregion

    }
}

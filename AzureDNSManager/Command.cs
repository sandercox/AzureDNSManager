using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDNSManager
{
    class Command : System.Windows.Input.ICommand
    {
        private bool _canExecute = true;
        private Action action = null;
        private Action<Object> parameterizedAction = null;

        public event EventHandler CanExecuteChanged;

        public Command(Action action, bool canExecute = true)
        {
            this.action = action;
            this._canExecute = canExecute;
        }

        public Command(Action<Object> action, bool canExecute = true)
        {
            this.parameterizedAction = action;
            this._canExecute = canExecute;
        }

        public void Execute(Object parameter)
        {
            if (action != null)
            {
                action();
            }
            else if (parameterizedAction != null)
            {
                parameterizedAction(parameter);
            }
        }

        public bool CanExecute(Object parameter)
        {
            return _canExecute;
        }

    }
}

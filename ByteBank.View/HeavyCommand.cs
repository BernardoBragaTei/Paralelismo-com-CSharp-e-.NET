using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ByteBank
{
    public interface IHeavyCommand<T> : ICommand
    {
        IProgress<T> Progress { get; }
        void Cancel();
    }

    public abstract class HeavyCommand<T> : IHeavyCommand<T>
    {
        public IProgress<T> Progress { get; set; }
        public CancellationTokenSource Cancellation { get; set; }


                    

        //Icommand
        public event EventHandler CanExecuteChanged;
        public abstract void Execute(object parameter);
        public abstract bool CanExecute(object parameter);

        //New Methods
        public abstract void ProgressCommand(T progress);
        public void Cancel()
        {
            throw new NotImplementedException();
        }

    }
}

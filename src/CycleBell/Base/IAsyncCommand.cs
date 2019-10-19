
using System.Threading.Tasks;
using System.Windows.Input;

namespace CycleBell.Base
{
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync ( object obj );
    }
}

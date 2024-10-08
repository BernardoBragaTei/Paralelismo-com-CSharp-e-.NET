using ByteBank.Core.Model;
using ByteBank.Core.Repository;
using ByteBank.Core.Service;
using ByteBank.View.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ByteBank.View
{
    public partial class MainWindow : Window
    {
        private readonly ContaClienteRepository r_Repositorio;
        private readonly ContaClienteService r_Servico;
        private CancellationTokenSource _cts;

        public MainWindow()
        {
            InitializeComponent();

            r_Repositorio = new ContaClienteRepository();
            r_Servico = new ContaClienteService();
        }

        private async void BtnProcessar_Click(object sender, RoutedEventArgs e)
        {
            _cts = new CancellationTokenSource();
            BtnProcessar.IsEnabled = false;
            LimparView();

            var contas = r_Repositorio.GetContaClientes();

            PgsProgresso.Maximum = contas.Count();

            var inicio = DateTime.Now;
            BtnCancelar.IsEnabled = true;

            var progresso = new Progress<string>(s => Progresso(s));

            try
            {
                var resultado = await ConsolidarContas(contas, progresso, _cts.Token);
                var fim = DateTime.Now;
                AtualizarView(resultado, fim - inicio);
            }
            catch(OperationCanceledException) 
            {
                TxtTempo.Text = "Operação cancelada pelo usuário";
            }
            finally
            {
                BtnProcessar.IsEnabled = true;
                BtnCancelar.IsEnabled = false;
            }            
            
        }
        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            BtnCancelar.IsEnabled = false;
            PgsProgresso.Foreground = new SolidColorBrush(Colors.Red);
            _cts.Cancel();
        }

        private async Task<string[]> ConsolidarContas(IEnumerable<ContaCliente> contas, 
            IProgress<string> progress, CancellationToken ct)
        {
            var tasks = contas.Select(conta =>
                Task.Factory.StartNew(() =>
                {
                    ct.ThrowIfCancellationRequested();

                    var retorno = r_Servico.ConsolidarMovimentacao(conta, ct);
                    progress.Report(retorno);

                    ct.ThrowIfCancellationRequested();

                    return retorno;
                }, ct));
            
            return await Task.WhenAll(tasks);
        }
        private void LimparView()
        {
            LstResultados.ItemsSource = null;
            LstResultados.Items.Clear();
            TxtTempo.Text = string.Empty;
            PgsProgresso.Value = 0;
            PgsProgresso.Foreground = new SolidColorBrush(Colors.Green);
        }

        private void Progresso(string consolidado)
        {
            LstResultados.Items.Add(consolidado);
            PgsProgresso.Value++;
        }

        private void AtualizarView(IEnumerable<String> result, TimeSpan elapsedTime)
        {
            var tempoDecorrido = $"{elapsedTime.Seconds}.{elapsedTime.Milliseconds} segundos!";
            var mensagem = $"Processamento de {result.Count()} clientes em {tempoDecorrido}";
            TxtTempo.Text = mensagem;
        }

        public class CmdProcessar : IHeavyCommand<string>
        {
            public IProgress<string> Progress { get; set; }
            public CancellationTokenSource Cancellation { get; set; }

            public event EventHandler CanExecuteChanged;

            public void Cancel()
            {
                
            }

            public bool CanExecute(object parameter)
            {
                throw new NotImplementedException();
            }

            public void Execute(object parameter)
            {
                throw new NotImplementedException();
            }
        }

    }
}

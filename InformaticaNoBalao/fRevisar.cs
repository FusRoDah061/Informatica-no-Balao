using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

namespace InformaticaNoBalao
{
    public partial class fRevisar : Form
    {
        Thread ChecaConexaoThread;//Thread que deve verificar a conexão com o banco
        GerenciaBanco bancoDeDados = new GerenciaBanco();//Referência à classe que gerencia o banco de dados
        GeraPdf Nota;//Referência à classe que gera a nota
        private bool estaConectado;

        //Informações da compra
        private string pCliente;
        private string pCpf;
        private string pCodigoProduto = "";
        private double pValorFinal = 0;
        private List<ushort> pCodigosIndividuais = new List<ushort>();
        private List<string> produtosNomes = new List<string>();//Contém os nomes dos produtos vindos das outras classes
        private List<double> produtosPrecos = new List<double>();//Contém os precos dos produtos vindos das outras classes
        private string operador;

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
                         int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        //FUNÇÕES
        private List<ushort> isolaCodigos(string codigo)
        {
            string texto = codigo;
            List<ushort> lista = new List<ushort>();
            int index;
            string textoAux;

            if(texto.Contains(',')){
                while (texto.Contains(','))
                {
                    index = texto.IndexOf(',') + 2;
                    textoAux = texto.Substring(0, index - 2);
                    texto = texto.Remove(0, index);
                    lista.Add(UInt16.Parse(textoAux));
                    if (!texto.Contains(','))
                    {
                        lista.Add(UInt16.Parse(texto));
                    }
                }
            }else{
                lista.Add(UInt16.Parse(texto));
                return lista;
            }

            return lista;
        }
        //Limpa as variáveis
        private void reset()
        {
            produtosNomes.Clear();
            produtosPrecos.Clear();

            pCliente = "";
            pCpf = "";
            pCodigoProduto = "";
            pValorFinal = 0;
            operador = "";
        }

        //Pega os dados vindos do form fMain
        public fRevisar(string cliente, string cpf, string codigos, double total, string operador,List<string> produtos, List<double> precos)
        {
            InitializeComponent();

            produtosNomes.AddRange(produtos);
            produtosPrecos.AddRange(precos);

            pCliente = cliente;
            pCpf = cpf;
            pCodigoProduto = codigos;
            pValorFinal = total;
            this.operador = operador;
        }

        //Sombra do Form
        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x20000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        //Fecha esse form e abre o fMain
        private void btEditar_Click(object sender, EventArgs e)
        {
            reset();
            this.Close();
        }

        //Verifica a conexão com o banco
        private void fRevisar_Load(object sender, EventArgs e)
        {
            //carrega o banco
            if (!bancoDeDados.estabeleceConexao())
            {
                estaConectado = false;
                label30.Visible = true;
                label30.Text = "Sem conexão";
                label30.ForeColor = Color.Red;
            }
            else
            {
                estaConectado = true;
                label30.Visible = true;
                label30.Text = "Conectado";
                label30.ForeColor = Color.Lime;

                pCodigosIndividuais.AddRange(isolaCodigos(pCodigoProduto));

                for (int i = 0; i < pCodigosIndividuais.Count; i++)
                {
                    lbProdutos.Items.Add(bancoDeDados.pegaProdutos(pCodigosIndividuais[i]));
                }

                lbProdutos.Items.Add("TOTAL - R$ " + pValorFinal.ToString("0.00"));
                lblClienteNome.Text = pCliente;
                lblClienteCpf.Text = pCpf;
                lblOperador.Text = operador;
            }

            ChecaConexaoThread = new Thread(new ThreadStart(checaConexaoThread));
            ChecaConexaoThread.Start();
        }

        //Mover o form
        private void fRevisar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void btConfirmar_Click(object sender, EventArgs e)
        {
            if (estaConectado)
            {
                bancoDeDados.setPedido(pCliente, pCpf, pCodigoProduto, pValorFinal, operador);
                bool dadosSalvos = bancoDeDados.gravaDadosNoBanco(2);
                if (dadosSalvos)
                {

                        if (MessageBox.Show("Compra finalizada com sucesso! Deseja gerar a nota agora?", "Concluir compra", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            //Gerar nota
                            Nota = new GeraPdf(pCliente, pCpf, pCodigoProduto, pValorFinal, operador, produtosNomes, produtosPrecos);
                            if (Nota.escreveNota())
                            {
                                MessageBox.Show("A nota foi gerada no desktop da máquina.", "Concluir compra", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("A nota não pode ser gerada. Você pode não estar conectado no sistema.", "Concluir compra", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            reset();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Compra finalizada com sucesso!", "Concluir compra", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            reset();
                            this.Close();
                        }
                    
                }
                else
                {
                    MessageBox.Show("Não foi possível finalizar a compra!", "Concluir compra", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    reset();
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Você não está conectado no sistema.", "Sem conexão", MessageBoxButtons.OK, MessageBoxIcon.Error);
                reset();
                this.Close();
            }
        }

        //Medtodo que roda em outro thread e checa o estado da conexão com o banco
        private void checaConexaoThread()
        {
            while (true)
            {
                atualizaInterface(checaConexao());
                Thread.Sleep(500);
            }
        }

        //Pega o estado da conexão com o banco
        private bool checaConexao()
        {
            if (!bancoDeDados.estabeleceConexao())
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //Chamado no thread para atualizar a interface com o estado da conexão com o banco
        private void atualizaInterface(bool estado)
        {
            Func<int> del = delegate()
            {
                if (!estado)
                {
                    estaConectado = false;
                    label30.Visible = true;
                    label30.Text = "Sem conexão";
                    label30.ForeColor = Color.Red;
                    return 0;
                }
                else
                {
                    estaConectado = true;
                    label30.Visible = true;
                    label30.Text = "Conectado";
                    label30.ForeColor = Color.Lime;
                    return 0;
                }
            };
            Invoke(del);
        }

        private void fRevisar_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Termina o thread. Sem isso o thread continua rodando mesmo com o app fechado
            ChecaConexaoThread.Abort();
        }
    }
}

/*
 * ISOLAR O CODIGO DOS PRODUTOS - exemplo
 * 
            string texto = "10359435846, 05438547656, 035848576";
            List<string> lista = new List<string>();
            int index;
            string textoAux;
            Console.WriteLine(texto + "\n");

            while (texto.Contains(','))
            {
                index = texto.IndexOf(',') + 2;
                textoAux = texto.Substring(0, index - 2);
                texto = texto.Remove(0,index);
                lista.Add(textoAux);
                if (!texto.Contains(','))
                {
                    lista.Add(texto);
                }
            }

            for (int i = 0; i < lista.Count; i++)
            {
                Console.WriteLine(lista[i]);
            }
            Console.ReadKey();
*/
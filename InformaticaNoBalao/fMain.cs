using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using System.Threading;

namespace InformaticaNoBalao
{
    public partial class fMain : Form
    {
        Thread ChecaConexaoThread;//Thread que deve verificar a conexão com o banco
        //Informações do cliente
        private string cNome, cCPF, cRG, cEmail, cCelular, cTelefone, cBairro, cEndereco, cCidade, cEstado, cCEP;
        //Informações da compra
        private string pCliente;
        private string pCpf;
        private string pCodigoProduto = "";//Contém os codigos dos produtos todos juntos (a, b, c, d ...)
        private double pValorFinal = 0;
        List<string> produtos = new List<string>();//Contém os nomes dos produtos depois de extraidos (pegaProdutosId())
        List<double> precos = new List<double>();//Contém os preços depois de extraidos (pegaProdutosPreco())

        private bool primeiraVez = true;//Impede que as combobox atualiza toda vez
        private string operador;

        //Estado da conexão
        private bool estaConectado;

        GerenciaBanco bancoDeDados = new GerenciaBanco();//Referência à classe que gerencia o banco de dados
        GeraPdf Nota;//Referência à classe que gera a nota

        //Receberá os produtos no FormLoad
        ComboBox[] campos;
    

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
                         int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        //FUNÇÕES
        //Pega as informações do cliente
        private void pegaValoresDoCadastro()
        {
            cNome = tbNome.Text.ToUpper();
            cCEP = tbCEP.Text;
            cRG = tbRG.Text;
            if (tbEmail.Text == "")
            {
                cEmail = "ND";
            }
            else
            {
                cEmail = tbEmail.Text;
            }

            if (tbCelular.Text == "")
            {
                cCelular = "ND";
            }
            else
            {
                cCelular = tbCelular.Text;
            }

            cTelefone = tbTelefone.Text;
            cCPF = tbCPF.Text;
            cCidade = tbCidade.Text.ToUpper();
            cBairro = tbBairro.Text.ToUpper();
            cEndereco = tbEndereço.Text.ToUpper();
            cEstado = tbEstado.Text.ToUpper();
        }
        //Pega os nomes do produtos
        private void pegaProdutosId()
        {
            for (int i = 0; i < campos.Length; i++)
            {
                if (campos[i].Text != "")
                {
                    produtos.Add(bancoDeDados.extraiProdutoId(campos[i].Text));
                }
            }
        }
        //Pega os preços dos produtos
        private void pegaProdutosPreco()
        {
            for (int i = 0; i < campos.Length; i++)
            {
                if (campos[i].Text != "")
                {
                    precos.Add(bancoDeDados.extraiProdutoPrecos(campos[i].Text));
                }
            }
        }
        //Limpra todas as variáveis
        private void reset()
        {
            cNome = "";
            cCPF = "";
            cRG = "";
            cEmail = "";
            cCelular = "";
            cTelefone = "";
            cBairro = "";
            cEndereco = "";
            cCidade = "";
            cEstado = "";
            cCEP = "";
            pCliente = "";
            pCpf = "";
            pCodigoProduto = "";
            pValorFinal = 0;
            produtos.Clear();
            precos.Clear();
        }


        public fMain(string operadorCod)
        {
            InitializeComponent();
            operador = operadorCod;
            label17.Text = "Op: " + operador.ToString();
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

        //Verifica a conexão com o banco
        private void fMain_Load(object sender, EventArgs e)
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
            }

            ChecaConexaoThread = new Thread(new ThreadStart(checaConexaoThread));
            ChecaConexaoThread.Start();

            campos = new ComboBox[] {cbFonte,
                                     cbGabinete,
                                     cbHD,
                                     cbMemoriaRam,
                                     cbMonitor,
                                     cbMouse,
                                     cbPlacaMae,
                                     cbPlacaVideo,
                                     cbProcessador,
                                     cbTeclado};
        }

        //Sair do programa
        private void btFechar_Click(object sender, EventArgs e)
        {
            fSair sair = new fSair();
            sair.Show();
        }

        //Mover o Form
        private void fMain_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        //Muda a tela ativa
        private void btTabInativo_Click(object sender, EventArgs e)
        {
            string tabAnterior = btTabAtivo.Text;
            btTabAtivo.Text = btTabInativo.Text;
            btTabInativo.Text = tabAnterior;

            if (btTabAtivo.Text == "Cadastro")
            {//Tela de cadastro
                panelPedido.Visible = false;
                this.Text = "Infomática no Balão - Cadastro";
                this.AcceptButton = btConfirmar;
            }
            else if (btTabAtivo.Text == "Pedido")
            {//tela de pedido
                this.Text = "Infomática no Balão - Pedido";
                panelPedido.Visible = true;
                this.AcceptButton = btContinuar;
                if (estaConectado)
                {
                    if (primeiraVez)
                    {
                        //Atualiza as combobox na primeira vez que a tela de pedidos é aberta
                        bancoDeDados.pegaProdutos("processador", cbProcessador);
                        bancoDeDados.pegaProdutos("placamae", cbPlacaMae);
                        bancoDeDados.pegaProdutos("placavideo", cbPlacaVideo);
                        bancoDeDados.pegaProdutos("memoria", cbMemoriaRam);
                        bancoDeDados.pegaProdutos("fonte", cbFonte);
                        bancoDeDados.pegaProdutos("hd", cbHD);
                        bancoDeDados.pegaProdutos("monitor", cbMonitor);
                        bancoDeDados.pegaProdutos("teclado", cbTeclado);
                        bancoDeDados.pegaProdutos("mouse", cbMouse);
                        bancoDeDados.pegaProdutos("gabinete", cbGabinete);
                        primeiraVez = false;
                    }
                }
                else
                {
                    MessageBox.Show("Você não está conectado no sistema para ver os produtos.", "Sem conexão", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        //Confirmar o cadastro
        private void btConfirmar_Click(object sender, EventArgs e)
        {
            if (estaConectado)
            {
                if (tbNome.Text == "" || tbCPF.Text == "" || tbRG.Text == "" || tbTelefone.Text == "" || tbEstado.Text == "" || tbEndereço.Text == "" || tbBairro.Text == "" || tbCidade.Text == "" || tbCEP.Text == "")
                {
                    MessageBox.Show("Preencha todos os campos obrigatórios para continuar", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    //Retira os valores dos campos de texto
                    pegaValoresDoCadastro();
                    //Manda os valores para serem gravados
                    bancoDeDados.setInfoBasica(cNome, cCPF, cRG, operador);
                    bancoDeDados.setContato(cEmail, cCelular, cTelefone);
                    bancoDeDados.setLocal(cBairro, cEndereco, cCidade, cCEP, cEstado);

                    //Grava no banco
                    bool dadosSalvos = bancoDeDados.gravaDadosNoBanco(1);
                    if (dadosSalvos)
                    {
                        MessageBox.Show("Cadastro realizado com sucesso!", "Cliente cadastrado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Não foi possível salvar os dados!", "Falha no cadastro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Você não está conectado no sistema.", "Sem conexão", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        //Limpar os campos
        private void btLimpar_Click(object sender, EventArgs e)
        {
            reset();

            tbNome.Text = "";
            tbEmail.Text = "";
            tbCidade.Text = "";
            tbCEP.Text = "";
            tbBairro.Text = "";
            tbCelular.Text = "";
            tbCPF.Text = "";
            tbEndereço.Text = "";
            tbEstado.Text = "";
            tbRG.Text = "";
            tbTelefone.Text = "";

        }

        //Pesquisar um cliente no banco
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (estaConectado)
            {
                if (tbCPF.Text == "")
                {
                    MessageBox.Show("Digite o CPF do cliente", "Pesquisar cliente", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    string cliente = bancoDeDados.pesquisarCliente(tbCPF.Text);

                    if (cliente != "0")
                    {
                        MessageBox.Show("Cliente já é cadastrado.", "Pesquisar cliente", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Cliente não é cadastrado.", "Pesquisar cliente", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            else
            {
                MessageBox.Show("Você não está conectado no sistema.", "Sem conexão", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        //Conclusão da compra
        private void btContinuar_Click(object sender, EventArgs e)
        {
            if (estaConectado)
            {
                    if (tbPedidoClienteNome.Text != "" && tbPedidoClienteCpf.Text != "")
                    {
                        //Pesquisa o cpf digitado
                        string resultado = bancoDeDados.pesquisarCliente(tbPedidoClienteCpf.Text);
                        if (resultado != "0")//se o cliente existir
                        {
                            //Segue a compra
                            pegaProdutosId();
                            pegaProdutosPreco();

                            int tamanho = produtos.Count - 1;

                            for (int i = 0; i <= tamanho; i++)
                            {
                                try
                                {
                                    if (i == tamanho)
                                    {
                                        pCodigoProduto += bancoDeDados.pegaCodigoProduto(produtos[i]);
                                    }
                                    else
                                    {
                                        pCodigoProduto += bancoDeDados.pegaCodigoProduto(produtos[i]) + ", ";
                                    }
                                }catch { }

                                pValorFinal += precos[i];

                            }

                            tamanho = 0;

                            if (pCodigoProduto != "")
                            {

                                pCliente = tbPedidoClienteNome.Text.ToUpper();
                                pCpf = tbPedidoClienteCpf.Text;


                                    if (MessageBox.Show("Compra finalizada com sucesso! Deseja rever o pedido?", "Concluir compra", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                                    {
                                        //Abre form de revisão
                                        fRevisar revisa = new fRevisar(pCliente, pCpf, pCodigoProduto, pValorFinal, operador, produtos, precos);
                                        reset();
                                        revisa.Show();
                                    }
                                    else
                                    {
                                        bancoDeDados.setPedido(pCliente, pCpf, pCodigoProduto, pValorFinal, operador);
                                        bool dadosSalvos = bancoDeDados.gravaDadosNoBanco(2);
                                        if (dadosSalvos)
                                        {
                                            if (MessageBox.Show("Deseja gerar a nota agora?", "Concluir compra", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                            {

                                                //Gerar nota
                                                Nota = new GeraPdf(pCliente, pCpf, pCodigoProduto, pValorFinal, operador, produtos, precos);
                                                reset();
                                                if (Nota.escreveNota())
                                                {
                                                    MessageBox.Show("A nota foi gerada no desktop da máquina.", "Concluir compra", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                                }
                                                else
                                                {
                                                    MessageBox.Show("A nota não pode ser gerada. Você pode não estar conectado no sistema.", "Concluir compra", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            MessageBox.Show("Não foi possível salvar a compra.", "Concluir compra", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                    }
                            }//if (pCodigoProduto != "")
                            else
                            {
                                MessageBox.Show("Não há produtos selecionados.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }

                        }//if (resultado != "0")
                        else
                        {
                            if (MessageBox.Show("Esse cliente ainda não é cadastrado. Cadastrar agora?", "Concluir compra", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                panelPedido.Visible = false;
                                btTabAtivo.Text = "Cadastro";
                                btTabInativo.Text = "Pedido";
                            }
                        }

                    }//if (tbPedidoClienteNome.Text != "" && tbPedidoClienteCpf.Text != "")
                    else
                    {
                        MessageBox.Show("Informe o nome e cpf do cliente.", "Concluir compra", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
            }//if (estaConectado)   
            else
            {
                MessageBox.Show("Você não está conectado no sistema.", "Sem conexão", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        //Formata o CPF
        private void tbCPF_TextChanged(object sender, EventArgs e)
        {
            /*       012 345 678 910
             * CPF = XXX.XXX.XXX-XX
             */ 
            if(tbCPF.TextLength == 3){
                tbCPF.AppendText(".");
            }
            if (tbCPF.TextLength == 7)
            {
                tbCPF.AppendText(".");
            }
            if (tbCPF.TextLength == 11)
            {
                tbCPF.AppendText("-");
            }
    
        }

        //Formata o RG
        private void tbRG_TextChanged(object sender, EventArgs e)
        {
            /*     
            * RG = XX.XXX.XXX-X
            */
            if (tbRG.TextLength == 2)
            {
                tbRG.AppendText(".");
            }
            if (tbRG.TextLength == 6)
            {
                tbRG.AppendText(".");
            }
            if (tbRG.TextLength == 10)
            {
                tbRG.AppendText("-");
            }
        }

        //Formata Celular
        private void tbCelular_TextChanged(object sender, EventArgs e)
        {
            /*     
            * Cel = XX XXXX-XXXX
            */
            if (tbCelular.TextLength == 2)
            {
                tbCelular.AppendText(" ");
            }
            if (tbCelular.TextLength == 7)
            {
                tbCelular.AppendText("-");
            }

        }

        //Formata Telefone
        private void tbTelefone_TextChanged(object sender, EventArgs e)
        {
            /*
            * Tel = XX XXXX-XXXX
            */
            if (tbTelefone.TextLength == 2)
            {
                tbTelefone.AppendText(" ");
            }
            if (tbTelefone.TextLength == 7)
            {
                tbTelefone.AppendText("-");
            }
        }

        //Formata CEP
        private void tbCEP_TextChanged(object sender, EventArgs e)
        {
            /*
             * CEP = XXXXX-XXX
             */
            if (tbCEP.TextLength == 5)
            {
                tbCEP.AppendText("-");
            }
        }

        //Formata o CPF na tela do pedido
        private void tbPedidoClienteCpf_TextChanged(object sender, EventArgs e)
        {
            /*       012 345 678 910
             * CPF = XXX.XXX.XXX-XX
             */ 
            if (tbPedidoClienteCpf.TextLength == 3)
            {
                tbPedidoClienteCpf.AppendText(".");
            }
            if (tbPedidoClienteCpf.TextLength == 7)
            {
                tbPedidoClienteCpf.AppendText(".");
            }
            if (tbPedidoClienteCpf.TextLength == 11)
            {
                tbPedidoClienteCpf.AppendText("-");
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

        private void fMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Termina o thread. Sem isso o thread continua rodando mesmo com o app fechado
            ChecaConexaoThread.Abort();
        }
        

    }//class


}//namespace InformaticaNoBalao



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
    public partial class fLogin : Form
    {
        GerenciaBanco bancoDeDados = new GerenciaBanco();
        GeraPdf pdf = new GeraPdf();
        Thread ChecaConexaoThread;//Thread que deve verificar a conexão com o banco
        private bool estaConectado = false;

        private string cNome, cCPF, cRG, cEmail, cCelular, cTelefone, cBairro, cEndereco, cCidade, cEstado, cCEP;


        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
                         int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        public fLogin()
        {
            InitializeComponent();
            this.Size = new Size(501, 217);
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

        //Sair do programa
        private void btFechar_Click(object sender, EventArgs e)
        {
            fSair sair = new fSair();
            sair.Show();
        }

        //Mover o form
        private void fLogin_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void btEntrar_Click(object sender, EventArgs e)
        {
            //Tamanhos
            //501; 521 com cadastro
            //501; 217 sem cadastro
            if (estaConectado)
            {
                if (tbLoginCpf.Text != "" || tbOperadorCod.Text != "")
                {
                    string cpfDigitado = tbLoginCpf.Text;
                    string codigoDigitado = tbOperadorCod.Text;

                    string codGravado = bancoDeDados.pegaLogin(cpfDigitado);
                    if (codGravado != null)
                    {

                        if (codigoDigitado == codGravado)
                        {
                            label16.Visible = false;
                            fMain main = new fMain(codigoDigitado);
                            main.Show();
                            this.Dispose();
                        }
                        else
                        {
                            label16.Visible = true;
                        }
                    }
                    else
                    {
                        if (MessageBox.Show("Esse funcionário não existe. Cadastrá-lo agora?", "Funcionário não encontrado", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                        {
                            this.Size = new Size(501, 521);
                            panel2.Visible = true;
                            panel2.Enabled = true;
                            panel1.Enabled = false;
                            //centraliza o form
                            this.CenterToScreen();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Preencha todos os campos.", "Campos em branco", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            else
            {
                MessageBox.Show("Você não está conectado no sistema.", "Sem conexão", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btCadastrar_Click(object sender, EventArgs e)
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
                    bancoDeDados.setInfoBasica(cNome, cCPF, cRG);
                    bancoDeDados.setContato(cEmail, cCelular, cTelefone);
                    bancoDeDados.setLocal(cBairro, cEndereco, cCidade, cCEP, cEstado);

                    //Grava no banco
                    bool dadosSalvos = bancoDeDados.gravaDadosNoBanco(3);
                    if (dadosSalvos)
                    {
                        string cod = bancoDeDados.getCodOp();
                        MessageBox.Show("Cadastro realizado com sucesso! Esse é seu código para os próximos logins: " + cod, "Funcionário cadastrado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        fMain main = new fMain(cod);
                        main.Show();
                        this.Dispose();
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

        //Cadastrar funcionario
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Size = new Size(501, 521);
            panel2.Visible = true;
            panel2.Enabled = true;
            panel1.Enabled = false;
            this.CenterToScreen();
        }

        private void label17_Click(object sender, EventArgs e)
        {
            this.Size = new Size(501, 217);
            panel2.Visible = false;
            panel2.Enabled = false;
            panel1.Enabled = true;
            this.CenterToScreen();
        }  

        private void fLogin_Load(object sender, EventArgs e)
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
        }

        //Formata o cpf de login
        private void tbLoginCpf_TextChanged(object sender, EventArgs e)
        {
            /*       012 345 678 910
             * CPF = XXX.XXX.XXX-XX
             */
            if (tbLoginCpf.TextLength == 3)
            {
                tbLoginCpf.AppendText(".");
            }
            if (tbLoginCpf.TextLength == 7)
            {
                tbLoginCpf.AppendText(".");
            }
            if (tbLoginCpf.TextLength == 11)
            {
                tbLoginCpf.AppendText("-");
            }
        }

        //Formata o cpf de cadastro
        private void tbCpf_TextChanged(object sender, EventArgs e)
        {
            /*       012 345 678 910
             * CPF = XXX.XXX.XXX-XX
             */
            if (tbCPF.TextLength == 3)
            {
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

        //Formata o telefone
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

        //formata o celular
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

        //formata o cep
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

        //Gerar relatório
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (pdf.escreveRelatório())
            {
                MessageBox.Show("O relatório foi gerado no desktop da máquina.", "Gerar relatório", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Não foi possível gerar o relatório.", "Gerar relatório", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            try
            {
                Invoke(del);
            }catch(Exception ex){}
        }

        private void fLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Termina o thread. Sem isso o thread continua rodando mesmo com o app fechado
            ChecaConexaoThread.Abort();
        }


    }
}

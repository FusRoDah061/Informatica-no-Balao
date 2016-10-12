using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace InformaticaNoBalao
{
    public partial class fSair : Form
    {
        public fSair()
        {
            InitializeComponent();
        }

        //Sai
        private void btSair_Click(object sender, EventArgs e)
        {
            GerenciaBanco banco = new GerenciaBanco();
            banco.terminaConexao();
            Application.Exit();
        }
        //Não sai
        private void btVoltar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}

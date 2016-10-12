using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using System.ComponentModel;

namespace InformaticaNoBalao
{
    class GerenciaBanco
    {
        private const string STRING_DE_CONEXAO = "server=localhost;database=balao_da_informatica;uid=root;pwd=''";

        private MySqlConnection Conection; // Conection é o nome da variavel de conexao, poderia ser qualquer nome
        private MySqlDataAdapter Adapter; // Adapter é a nossa variável que será responsavel por acessar os dados na tabela
        private DataSet DSet;

        //Informações do cliente
        private string ClienteNome;
        private string ClienteCpf;
        private string ClienteRg;
        private string ClienteEmail;
        private string ClienteCelular;
        private string ClienteTelefone;
        //Endereços dos clientes
        private string ClienteOperador;
        private string ClienteBairro;
        private string ClienteEstado;
        private string ClienteCidade;
        private string ClienteCep;
        private string ClienteEndereco;
        //Informações da compra
        private string CompraCliente;
        private string CompraCpf;
        private string CompraCodigosProduto;
        private double CompraValorFinal;
        private string CompraOperador;
        //Codigo do funcionario
        string opCod = "00000";

        //Conecta ao banco de dados
        public bool estabeleceConexao()
        {
            DSet = new DataSet();

            Conection = new MySqlConnection(STRING_DE_CONEXAO);
            try
            {
                Conection.Open();
                return true;//conectado
            }
            catch
            {
                //Há um atraso no retorno do falso devido ao tempo de espera pela conexão antes de jogar uma excessão
                return false;//não conectado
            }
        }

        //encerra a conexão com o banco
        public void terminaConexao()
        {
            try
            {
                if (Conection.State == ConnectionState.Open)
                {
                    Conection.Close();
                }
            }
            catch { }
        }

        /*
         * Pegam as informações digitadas:
         * setInfoBasica
         * setContato
         * setLocal
         * setPedido
         */
        public void setInfoBasica(string nome, string cpf, string rg, string op)
        {
            ClienteNome = nome;
            ClienteCpf = cpf;
            ClienteRg = rg;
            ClienteOperador = op;
        }
        public void setInfoBasica(string nome, string cpf, string rg)
        {
            ClienteNome = nome;
            ClienteCpf = cpf;
            ClienteRg = rg;
        }
        public void setContato(string email, string celular, string telefone)
        {
            ClienteEmail = email;
            ClienteCelular = celular;
            ClienteTelefone = telefone;
        }
        public void setLocal(string bairro, string endereco, string cidade, string cep, string estado)
        {
            ClienteBairro = bairro;
            ClienteEndereco = endereco;
            ClienteCidade = cidade;
            ClienteCep = cep;
            ClienteEstado = estado;
        }
        public void setPedido(string cliente, string cpf, string codigos, double total,string op)
        {
            CompraCliente = cliente;
            CompraCpf = cpf;
            CompraCodigosProduto = codigos;
            CompraValorFinal = total;
            CompraOperador = op;
        }

        //Grava os dados no banco(Clientes e Compras)
        public bool gravaDadosNoBanco(int tipo)
        {

            string SqlCmd = "";
            //Cadastro
            if (tipo == 1)
            {
                SqlCmd = "INSERT INTO clientes VALUES ('" + ClienteNome + "','" + ClienteRg + "','" + ClienteCpf + "','" + ClienteEmail + "','" + ClienteTelefone + "','" + ClienteCelular + "','" + ClienteOperador + "'); INSERT INTO localizacoes VALUES ('" + ClienteCpf + "','" + ClienteBairro + "','" + ClienteEndereco + "','" + ClienteCidade + "','" + ClienteCep + "','" + ClienteEstado + "');";
            }

            //Compra
            if (tipo == 2)
            {
                SqlCmd = "INSERT INTO compras (operador, cliente_nome, cliente_cpf, codigo_produto, valor_final) VALUES ('"+ CompraOperador + "','" + CompraCliente.ToUpper() + "','" + CompraCpf + "','" + CompraCodigosProduto + "'," + CompraValorFinal.ToString().Replace(',','.') + ");";
            }

            //Novo funcionário
            if (tipo == 3)
            {
                Random rnd = new Random();
                opCod = rnd.Next(1, 65000).ToString();//Retornado para o form de login pelo getCodOp()
                SqlCmd = "INSERT INTO funcionarios VALUES ('" + opCod + "','" + ClienteNome + "','" + ClienteRg + "','" + ClienteCpf + "','" + ClienteEmail + "','" + ClienteTelefone + "','" + ClienteCelular + "'); INSERT INTO localizacoes VALUES ('" + ClienteCpf + "','" + ClienteBairro + "','" + ClienteEndereco + "','" + ClienteCidade + "','" + ClienteCep + "','" + ClienteEstado + "');";
            }

            if (Conection.State == ConnectionState.Open)
            {
                //Se estiver aberta insere os dados no BD
                MySqlCommand commS = new MySqlCommand(SqlCmd, Conection);
                commS.BeginExecuteNonQuery();
                return true;
            }
            return false;
            
        }

        //Retorna o codigo do operador
        public string getCodOp()
        {
            return opCod;
        }

        //Verifica se o cliente já está cadastrado
        public string pesquisarCliente(string cpf)
        {
            //verifica se a conexão esta aberta
            if (Conection.State == ConnectionState.Open)
            {
                MySqlCommand commS = new MySqlCommand("SELECT EXISTS (SELECT cpf FROM clientes WHERE cpf = '" + cpf + "');", Conection);
                string cliente = commS.ExecuteScalar().ToString();
                return cliente;
            }
            return null;
        }

        //Mostra os produtos nos ComboBox da tela de Pedidos
        public void pegaProdutos(string categoria, System.Windows.Forms.ComboBox alvo)
        {
            //verifica se a conexão esta aberta
            if (Conection.State == ConnectionState.Open)
            {
                //cria um adapter usando a instrução SQL para acessar a tabela 
                Adapter = new MySqlDataAdapter("SELECT CONCAT('R$ ',preco_unit,' - ', nome) FROM produtos WHERE tipo = '" + categoria + "';", Conection);
                //preenche o dataset via adapter
                DataTable dt = new DataTable();
                Adapter.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    string material = string.Format("{0}", row.ItemArray[0]);
                    //adiciona a string no final da combobox
                    alvo.Items.Add(material);
                }

            }
        }

        //Mostra os produtos nos ListBox da tela de revisão
        public void pegaProdutos(ushort codigo, System.Windows.Forms.ListBox alvo)
        {
            //verifica se a conexão esta aberta
            if (Conection.State == ConnectionState.Open)
            {
                //cria um adapter usando a instrução SQL para acessar a tabela 
                Adapter = new MySqlDataAdapter("SELECT CONCAT('R$ ',preco_unit,' - ', nome) FROM produtos WHERE tipo = " + codigo + ";", Conection);
                //preenche o dataset via adapter
                DataTable dt = new DataTable();
                Adapter.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    string material = string.Format("{0}", row.ItemArray[0]);
                    //adiciona a string no final da listbox
                    alvo.Items.Add(material);
                }

            }
        }
    
        //Retorna as informações do produto
        public string pegaProdutos(ushort codigo)
        {
            //verifica se a conexão esta aberta
            if (Conection.State == ConnectionState.Open)
            {
                //pesquisa as informações do produto pelo codigo
                MySqlCommand commS = new MySqlCommand("SELECT CONCAT('R$ ',preco_unit,' - ', nome) FROM produtos WHERE codigo = " + codigo + ";", Conection);
                string texto = commS.ExecuteScalar().ToString();

                return texto;
            }

            return null;
        }
        
        //Extrai o nome do produto do ComboBox da tela de pedidos
        public string extraiProdutoId(string texto)
        {
            //R$ 168.00 - texto ----------> texto
            //R$ 1165.00 - texto ---------> texto

            byte startIndex = (byte)(texto.IndexOf('-') + 2);
            string nomeProduto = texto.Substring(startIndex);

            return nomeProduto;
        }

        //Extrai o preço do ComboBox da tela de pedidos
        public double extraiProdutoPrecos(string texto)
        {

            //R$ 168.00 - texto ----------> 168.00
            //R$ 1165.00 - texto ---------> 1165.00
            byte startIndex = (byte)3;
            byte tamanho = (byte)(texto.IndexOf('-') - (startIndex + 1));
            string precoProduto = texto.Substring(startIndex,tamanho);

            return double.Parse(precoProduto.Replace('.', ','));
        }

        //Pesquisa os produtos para recuperar o codigo
        public string pegaCodigoProduto(string produto)
        {
            //verifica se a conexão esta aberta
            if (Conection.State == ConnectionState.Open)
            {
                string codigo = "";
                //pesquisa o codigo do produto a partir do nome
                MySqlCommand commS = new MySqlCommand("SELECT codigo FROM produtos WHERE nome = '" + produto + "';", Conection);
                codigo = commS.ExecuteScalar().ToString();

                return codigo;
            }
            return null;
        }
        
        //Pega o codigo da última compra
        public string pegaCodigoCompra()
        {
            //verifica se a conexão esta aberta
            if (Conection.State == ConnectionState.Open)
            {
                //pesquisa a última compra registrada
                MySqlCommand commS = new MySqlCommand("SELECT codigo FROM compras ORDER BY codigo DESC LIMIT 1;", Conection);
                string codigo = commS.ExecuteScalar().ToString();

                return codigo;
            }
            return null;
        }
        
        //Verifica se o usuario digitado no login existe e retorna ele pra validar
        public string pegaLogin(string cpf)
        {
            //verifica se a conexão esta aberta
            if (Conection.State == ConnectionState.Open)
            {
                try
                {
                    //Retira o codigo
                    MySqlCommand commS = new MySqlCommand("SELECT id FROM funcionarios WHERE cpf = '" + cpf + "';", Conection);
                    string codigo = commS.ExecuteScalar().ToString();

                    return codigo;
              }
                catch
                {
                    return null;
                }
            }
            return null;
        }
    
    }
}

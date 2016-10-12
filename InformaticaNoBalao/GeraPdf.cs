using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//Para gerar o pdf
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

using System.Data;
using MySql.Data.MySqlClient;

namespace InformaticaNoBalao
{
    class GeraPdf
    {
        GerenciaBanco banco = new GerenciaBanco();

        //codigo do operador
        private string operador;
        //diretório onde o pdf será salvo
        string path;

        //Informações usadas na nota
        private string codigoCompra;
        private string clienteNome;//
        private string clienteCpf;//
        private string compraTotal;//
        private string codigoBarraDigitos = "";//Digitos do código de barras
        List<string> codigosSeparados = new List<string>();//Contém os codigos dos produtos já separados
        List<string> produtosNomes = new List<string>();//Contém os nomes dos produtos
        List<double> produtosPrecos = new List<double>();//Contém os preços dos produtos

        //Nota e Relatório
        private string data;//dd/mm/aaaa atual
        private string hora;//hh:mm:ss atual

        private MySqlConnection Conection;
        private string STRING_DE_CONEXAO = "server=localhost;database=balao_da_informatica;uid=root;pwd=''";

        private bool estaConectado;

        
        public GeraPdf(string cliente, string cpf, string codigos, double total, string operador, List<string> produtos, List<double> precos)
        {
            estaConectado = banco.estabeleceConexao();
            if (estaConectado)
            {

                clienteNome = cliente;
                clienteCpf = cpf;
                //codigos dos produtos separados
                codigosSeparados.AddRange(isolaCodigos(codigos));
                produtosNomes.AddRange(produtos);
                produtosPrecos.AddRange(precos);
                compraTotal = total.ToString();
                codigoCompra = banco.pegaCodigoCompra();
                this.operador = operador;
                data = DateTime.Now.ToString("dd/MM/yyyy");
                hora = DateTime.Now.ToString("HH:mm:ss");
            }
        }

        public GeraPdf()
        {
            Conection = new MySqlConnection(STRING_DE_CONEXAO);
            try
            {
                Conection.Open();
                estaConectado = true;//conectado
            }
            catch
            {
                estaConectado = false;//não conectado
            }
        }
        
        //Separa os codigos
        private List<string> isolaCodigos(string codigo)
        {
            string texto = codigo;
            List<string> lista = new List<string>();
            int index;
            string textoAux;

            //enquanto existir ',' no texto o loop continua
            while (texto.Contains(','))
            {
                index = texto.IndexOf(',') + 2;
                textoAux = texto.Substring(0, index - 2);
                texto = texto.Remove(0, index);
                lista.Add(textoAux);
                if (!texto.Contains(','))
                {
                    lista.Add(texto);
                }
            }

            return lista;
        }

        //Gera um codigo com 20 caracteres
        public void geraCodigoBarra()
        {
            Random rnd = new Random();
            int index;

            for(int i = 1; i<= 15;i++){
                index = rnd.Next(1,10);
                switch(index){
                
                    case 1:
                        codigoBarraDigitos = codigoBarraDigitos + "1";
                        break;
                    case 2:
                        codigoBarraDigitos = codigoBarraDigitos + "2";
                        break;
                    case 3:
                        codigoBarraDigitos = codigoBarraDigitos + "3";
                        break;
                    case 4:
                        codigoBarraDigitos = codigoBarraDigitos + "4";
                        break;
                    case 5:
                        codigoBarraDigitos = codigoBarraDigitos + "5";
                        break;
                    case 6:
                        codigoBarraDigitos = codigoBarraDigitos + "6";
                        break;
                    case 7:
                        codigoBarraDigitos = codigoBarraDigitos + "7";
                        break;
                    case 8:
                        codigoBarraDigitos = codigoBarraDigitos + "8";
                        break;
                    case 9:
                        codigoBarraDigitos = codigoBarraDigitos + "9";
                        break;
                    case 10:
                        codigoBarraDigitos = codigoBarraDigitos + "0";
                        break;
                }
            }

        }

        //Limpa todas as variáveis
        private void limpaTudo()
        {
            clienteNome = "";
            clienteCpf = "";
            codigosSeparados.Clear();
            produtosNomes.Clear();
            produtosPrecos.Clear();
            compraTotal = "";
            codigoCompra = "";
            operador = "";
            data = "";
            hora = "";
        }

        public bool escreveNota()
        {
            if (estaConectado)
            {
                //diretorio
                path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "/Nota";
                //fontes
                BaseFont fonte1 = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);
                BaseFont fonte2 = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
                iTextSharp.text.Font title = new iTextSharp.text.Font(fonte1, 14);
                iTextSharp.text.Font normal = new iTextSharp.text.Font(fonte2, 11);
                //Cria o documento
                Document nota = new Document(iTextSharp.text.PageSize.A4, 60, 50, 65, 50);
                //Cria o pdf
                PdfWriter writer = PdfWriter.GetInstance(nota, new FileStream(path + codigoCompra + ".pdf", FileMode.CreateNew));

                //Abre o documento para edição
                nota.Open();
                //Escreve o conteúdo
                Chunk head1 = new Chunk("INFORMÁTICA NO BALÃO\n", title);
                Phrase line1 = new Phrase(14.0f, "Avenida Barão de Jaguara, 1027. Centro - Campinas - SP\nCNPJ: 35.856.758/0001-00\nIE: 773.586.010.451\n\n", normal);
                Phrase line2 = new Phrase(14.0f, data + "   " + hora + "    COO: " + codigoCompra + "   Operador: " + operador + "\nCliente: " + clienteNome.ToUpper() + "    CPF: " + clienteCpf, normal);

                //Cria uma tabela para os produtos
                PdfPTable table = new PdfPTable(4);
                table.TotalWidth = 450f;
                float[] widths = new float[] { 40f, 40f, 300f , 70f };
                table.SetWidths(widths);
                table.LockedWidth = true; 

                //Gera o conteúdo da tabela
                PdfPCell cell = new PdfPCell(new Phrase("CUPOM FISCAL", title));
                cell.Colspan = 4;
                cell.Border = 0;
                cell.HorizontalAlignment = 1;//0=Left, 1=Centre, 2=Right
                table.AddCell(cell);
                table.AddCell("ITEM");
                table.AddCell("COD");
                table.AddCell("DESCRIÇÃO");
                table.AddCell("VALOR(R$)");

                int item = 1;
                for (int i = 0; i < codigosSeparados.Count; i++)
                {

                    table.AddCell(item.ToString());
                    table.AddCell(codigosSeparados[i]);
                    table.AddCell(produtosNomes[i]);
                    table.AddCell(produtosPrecos[i].ToString());
                    item++;
                }

                PdfPCell total = new PdfPCell(new Phrase("TOTAL R$", title));
                total.Colspan = 3;
                table.AddCell(total);
                table.AddCell(compraTotal);

                geraCodigoBarra();
                Phrase line3 = new Phrase(14.0f, "\n\n\n" + "Código: " + codigoBarraDigitos, normal);

                //Adiciona o conteúdo à tabela
                nota.Add(head1);
                nota.Add(line1);
                nota.Add(line2);
                nota.Add(table);
                nota.Add(line3);

                nota.Close();
                limpaTudo();
                return true;
            }
            else
            {
                limpaTudo();
                return false;
            }
        }

        public bool escreveRelatório()
        {
            if (estaConectado)
            {
                data = DateTime.Now.ToString("dd/MM/yyyy");
                hora = DateTime.Now.ToString("HH:mm:ss");

                path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "/Relatório-" + data.Replace('/', '_') + "-" + hora.Replace(':', '_');

                BaseFont fonte1 = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);
                BaseFont fonte2 = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
                iTextSharp.text.Font title = new iTextSharp.text.Font(fonte1, 12);
                iTextSharp.text.Font normal = new iTextSharp.text.Font(fonte2, 10);
                iTextSharp.text.Font item = new iTextSharp.text.Font(fonte2, 9);
                //Cria o documento
                Document relatorio = new Document(iTextSharp.text.PageSize.A4, 60, 50, 65, 50);
                //Cria o pdf
                PdfWriter writer = PdfWriter.GetInstance(relatorio, new FileStream(path + ".pdf", FileMode.CreateNew));

                //Abre o documento para edição
                relatorio.Open();

                Chunk head1 = new Chunk("RELATÓRIO DE VENDAS\n", title);
                Phrase line1 = new Phrase(14.0f, "Informática no Balão\n", normal);
                Phrase line2 = new Phrase(14.0f, "Avenida Barão de Jaguara, 1027. Centro - Campinas - SP\nCNPJ: 35.856.758/0001-00\nIE: 773.586.010.451\n", normal);
                Phrase line3 = new Phrase(14.0f, "Gerado em: " + data + "   " + hora , normal);

                /*  _________________________________________
                 * |       |       |           |             |
                 * |NOME   |CPF    |N COMPRAS  |VALOR VENDIDO|
                 * |       |       |           |             |
                 */

                PdfPTable table = new PdfPTable(4);
                table.TotalWidth = 500f;
                float[] widths = new float[] { 280f, 80f, 70f, 80f };
                table.SetWidths(widths);
                table.LockedWidth = true;

                table.AddCell(new Phrase("NOME", normal));
                table.AddCell(new Phrase("CPF", normal));
                table.AddCell(new Phrase("COMPRAS", normal));
                table.AddCell(new Phrase("VENDIDO", normal));

                List<string> dados = new List<string>();

                MySqlCommand cmd = new MySqlCommand("SELECT funcionarios.nome, funcionarios.cpf, COUNT(compras.codigo), SUM(compras.valor_final)FROM funcionarios, compras WHERE funcionarios.id = compras.operador GROUP BY funcionarios.id ORDER BY COUNT(compras.codigo)DESC;", Conection);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    table.AddCell(new Phrase(row.Field<string>(0), item));
                    table.AddCell(new Phrase(row.Field<string>(1), item));
                    table.AddCell(new Phrase(row.Field<long>(2).ToString(), item));
                    table.AddCell(new Phrase("R$ " + row.Field<double>(3).ToString("0.00"), item));
                }

                relatorio.Add(head1);
                relatorio.Add(line1);
                relatorio.Add(line2);
                relatorio.Add(line3);
                relatorio.Add(table);

                relatorio.Close();
                return true;
            }
            else
            {
                return false;
            }
            
        }

    }
}

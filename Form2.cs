using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;

namespace Projeto_av3
{
    public partial class Form2 : Form
    {
        string conexaoString = "server=localhost;uid=root;database=sistemagestao;pwd=;";
        private int idFornecedorSelecionado = 0;
        private int idProdutoSelecionado = 0;

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            ListarFornecedores();
            //ListarProdutos();  
            textBox3.Enabled = false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string texto = "Deseja sair do Sistema de Gestão?";
            string titulo = "FINALIZANDO";
            if (MessageBox.Show(texto, titulo, MessageBoxButtons.YesNo,
                MessageBoxIcon.Asterisk,
                MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                Application.Exit(); //para sair
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) ||
        string.IsNullOrWhiteSpace(textBox2.Text) ||
        string.IsNullOrWhiteSpace(textBox4.Text) ||
        string.IsNullOrWhiteSpace(textBox5.Text) ||
        !maskedTextBox1.MaskCompleted)
            {
                MessageBox.Show("Preencha todos os campos!");
                return;
            }
            if (textBox2.Text.Length < 14)
            {
                MessageBox.Show("CNPJ inválido!");
                return;
            }
            if (!textBox4.Text.Contains("@") || !textBox4.Text.Contains("."))
            {
                MessageBox.Show("Digite um e-mail válido!");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(conexaoString))
                {
                    conn.Open();

                    // VERIFICA CNPJ DUPLICADO
                    string verifica = "SELECT COUNT(*) FROM fornecedores WHERE CNPJ=@cnpj";

                    MySqlCommand cmdVerifica = new MySqlCommand(verifica, conn);
                    cmdVerifica.Parameters.AddWithValue("@cnpj", textBox2.Text);

                    int existe = Convert.ToInt32(cmdVerifica.ExecuteScalar());

                    if (existe > 0)
                    {
                        MessageBox.Show("CNPJ já cadastrado!");
                        return;
                    }

                    // INSERIR
                    string sql = @"INSERT INTO fornecedores
                          (Nome, CNPJ, Telefone, Email, Endereco)
                          VALUES
                          (@nome, @cnpj, @tel, @email, @endereco)";

                    MySqlCommand cmd = new MySqlCommand(sql, conn);

                    cmd.Parameters.AddWithValue("@nome", textBox1.Text);
                    cmd.Parameters.AddWithValue("@cnpj", textBox2.Text);
                    cmd.Parameters.AddWithValue("@tel", maskedTextBox1.Text);
                    cmd.Parameters.AddWithValue("@email", textBox4.Text);
                    cmd.Parameters.AddWithValue("@endereco", textBox5.Text);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Fornecedor salvo!");

                    ListarFornecedores();

                    // LIMPA
                    textBox1.Clear();
                    textBox2.Clear();
                    textBox3.Clear();
                    textBox4.Clear();
                    textBox5.Clear();
                    maskedTextBox1.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            textBox5.Clear();
            maskedTextBox1.Clear();

            idFornecedorSelecionado = 0;

            dataGridView1.DataSource = null;
            dataGridView2.DataSource = null;

            //ListarFornecedores();
        }
        private void ListarFornecedores()
        {
            using (MySqlConnection conn = new MySqlConnection(conexaoString))
            {
                string sql = "SELECT * FROM fornecedores";

                MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);

                DataTable dt = new DataTable();

                da.Fill(dt);

                dataGridView1.DataSource = dt;
            }
        }


        private void ListarProdutos()
        {
            using (MySqlConnection conn = new MySqlConnection(conexaoString))
            {
                // Busca todos os produtos e marca em verde os q já são vinculados
                string sql = @"SELECT p.IdProduto, p.Nome, p.Preco, p.QuantidadeEstoque,
                       CASE WHEN fp.IdFornecedor IS NOT NULL 
                            THEN 'Sim' ELSE 'Não' END AS Vinculado
                       FROM produtos p
                       LEFT JOIN fornecedorproduto fp 
                            ON p.IdProduto = fp.IdProduto 
                            AND fp.IdFornecedor = @id";

                MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@id", idFornecedorSelecionado);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dataGridView2.DataSource = dt;

                // Pinta de verde os já vinculados
                foreach (DataGridViewRow row in dataGridView2.Rows)
                {
                    if (row.Cells["Vinculado"].Value?.ToString() == "Sim")
                        row.DefaultCellStyle.BackColor = Color.LightGreen;
                    else
                        row.DefaultCellStyle.BackColor = Color.White;
                }
            }
        }
        private void ListarProdutosDoFornecedor()
        {
            using (MySqlConnection conn = new MySqlConnection(conexaoString))
            {
                string sql = @"SELECT p.IdProduto, p.Nome, p.Preco, p.QuantidadeEstoque
                       FROM produtos p
                       INNER JOIN fornecedorproduto fp ON p.IdProduto = fp.IdProduto
                       WHERE fp.IdFornecedor = @id";

                MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@id", idFornecedorSelecionado);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dataGridView2.DataSource = dt;
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                // PEGA O VALOR DA CÉLULA
                var valorId = row.Cells["IdFornecedor"].Value;

                // VERIFICA SE NÃO ESTÁ VAZIO (Evita o erro de InvalidCastException)
                if (valorId != null && valorId != DBNull.Value)
                {
                    idFornecedorSelecionado = Convert.ToInt32(valorId);

                    // Preenche os campos de texto para edição
                    textBox3.Text = row.Cells["IdFornecedor"].Value.ToString();
                    textBox1.Text = row.Cells["Nome"].Value.ToString();
                    textBox2.Text = row.Cells["CNPJ"].Value.ToString();
                    maskedTextBox1.Text = row.Cells["Telefone"].Value.ToString();
                    textBox4.Text = row.Cells["Email"].Value.ToString();
                    textBox5.Text = row.Cells["Endereco"].Value.ToString();

                    // CHAMA A FUNÇÃO PARA FILTRAR OS PRODUTOS
                    ListarProdutos();
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (idFornecedorSelecionado == 0 ||
                 idProdutoSelecionado == 0)
            {
                MessageBox.Show("Selecione fornecedor e produto!");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(conexaoString))
                {
                    conn.Open();

                    // VERIFICA DUPLICAÇÃO
                    string verifica = @"SELECT COUNT(*)
                    FROM fornecedorproduto
                    WHERE IdFornecedor=@f AND IdProduto=@p";

                    MySqlCommand cmdVerifica =
                        new MySqlCommand(verifica, conn);

                    cmdVerifica.Parameters.AddWithValue("@f", idFornecedorSelecionado);
                    cmdVerifica.Parameters.AddWithValue("@p", idProdutoSelecionado);

                    int existe =
                        Convert.ToInt32(cmdVerifica.ExecuteScalar());

                    if (existe > 0)
                    {
                        MessageBox.Show("Esse vínculo já existe!");
                        return;
                    }

                    // INSERT VÍNCULO
                    string sql = @"INSERT INTO fornecedorproduto
                    (IdFornecedor, IdProduto)
                    VALUES
                    (@f, @p)";

                    MySqlCommand cmd =
                        new MySqlCommand(sql, conn);

                    cmd.Parameters.AddWithValue("@f", idFornecedorSelecionado);
                    cmd.Parameters.AddWithValue("@p", idProdutoSelecionado);

                    cmd.ExecuteNonQuery(); //Executa comando SQL sem retornar linhas.

                    MessageBox.Show("Produto vinculado!");

                    ListarProdutosDoFornecedor();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {

            if (idFornecedorSelecionado == 0)
            {
                MessageBox.Show("Selecione um fornecedor!");
                return;
            }

            if (MessageBox.Show("Deseja excluir?", "Confirmação",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(conexaoString))
                    {
                        conn.Open();

                        // REMOVE VÍNCULOS
                        string sqlVinculo =
                            "DELETE FROM fornecedorproduto WHERE IdFornecedor=@id";

                        MySqlCommand cmdVinculo =
                            new MySqlCommand(sqlVinculo, conn);

                        cmdVinculo.Parameters.AddWithValue("@id", idFornecedorSelecionado);

                        cmdVinculo.ExecuteNonQuery();

                        // REMOVE FORNECEDOR
                        string sql =
                            "DELETE FROM fornecedores WHERE IdFornecedor=@id";

                        MySqlCommand cmd =
                            new MySqlCommand(sql, conn);

                        cmd.Parameters.AddWithValue("@id", idFornecedorSelecionado);

                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Fornecedor removido!");

                        ListarFornecedores();

                        dataGridView2.DataSource = null;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro: " + ex.Message);
                }
            }
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            Form1 telaPrincipal = new Form1();

            // Mostra o Form1
            telaPrincipal.Show();

            // Fecha o formulário atual (Form2)
            this.Close();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) ||
                string.IsNullOrWhiteSpace(textBox2.Text) ||
                string.IsNullOrWhiteSpace(textBox4.Text) ||
                string.IsNullOrWhiteSpace(textBox5.Text) ||
                !maskedTextBox1.MaskCompleted) // Verifica se o telefone foi todo preenchido
            {
                MessageBox.Show("Por favor, preencha todos os campos obrigatórios!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Interrompe o código aqui e não salva no banco
            }
            if (idFornecedorSelecionado == 0) 
            { 
                MessageBox.Show("Selecione um fornecedor na tabela primeiro!"); 
                return; 
            }

            if (textBox2.Text.Length < 14)
            {
                MessageBox.Show("CNPJ inválido!");
                return;
            }
            if (!textBox4.Text.Contains("@") || !textBox4.Text.Contains("."))
            {
                MessageBox.Show("Digite um e-mail válido!");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(conexaoString))
                {
                    conn.Open();
                    string sql = "UPDATE fornecedores SET Nome=@nome, CNPJ=@cnpj, Telefone=@tel, Email=@email, Endereco=@end WHERE IdFornecedor=@id";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);

                    cmd.Parameters.AddWithValue("@nome", textBox1.Text);
                    cmd.Parameters.AddWithValue("@cnpj", textBox2.Text);
                    cmd.Parameters.AddWithValue("@tel", maskedTextBox1.Text);
                    cmd.Parameters.AddWithValue("@email", textBox4.Text);
                    cmd.Parameters.AddWithValue("@end", textBox5.Text);
                    cmd.Parameters.AddWithValue("@id", idFornecedorSelecionado);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Dados atualizados!");
                    ListarFornecedores();
                }
            }
            catch (Exception ex) { MessageBox.Show("Erro: " + ex.Message); }
        }

        private void dataGridView2_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var valorId = dataGridView2.Rows[e.RowIndex].Cells["IdProduto"].Value;

                // Verifica se não é nulo ou DBNull antes de converter
                if (valorId != null && valorId != DBNull.Value)
                {
                    idProdutoSelecionado = Convert.ToInt32(valorId);
                }
            }
        }
        private void BuscarFornecedor()
        {
            using (MySqlConnection conn = new MySqlConnection(conexaoString))
            {
                string sql = "SELECT * FROM fornecedores WHERE Nome LIKE @nome";

                MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@nome", "%" + textBoxBuscaFornecedor.Text + "%");

                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridView1.DataSource = dt;
            }
        }

        private void textBoxBuscaFornecedor_TextChanged(object sender, EventArgs e)
        {
            BuscarFornecedor();
        }
    }
}


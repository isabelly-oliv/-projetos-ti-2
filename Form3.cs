using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Projeto_av3
{
    public partial class Form3 : Form
    {
        string conexaoString = "server=localhost;uid=root;database=sistemagestao;pwd=;";
        int idProdutoSelecionado = 0;
        public Form3()
        {
            InitializeComponent();
        }
        private void Form3_Load(object sender, EventArgs e)
        {
            textBox5.Enabled = false;
            ListarProdutos();
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

        private void button6_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            textBox5.Clear(); 

            idProdutoSelecionado = 0;

            dataGridView1.DataSource = null;
            dataGridView2.DataSource = null;

            //ListarProdutos();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form1 telaPrincipal = new Form1();

            // Mostra o Form1
            telaPrincipal.Show();

            // Fecha o formulário atual (Form2)
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) ||
                string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("Nome e preço são obrigatórios!");
                return;
            }

            decimal preco;
            int estoque;

            if (!decimal.TryParse(textBox3.Text, out preco))
            {
                MessageBox.Show("Preço inválido!");
                return;
            }

            if (!int.TryParse(textBox4.Text, out estoque))
            {
                MessageBox.Show("Estoque inválido!");
                return;
            }
            if (estoque < 5)
            {
                MessageBox.Show("Estoque baixo!");
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(conexaoString))
                {
                    conn.Open();

                    string sql = @"INSERT INTO produtos
                          (Nome, Descricao, Preco, QuantidadeEstoque)
                          VALUES
                          (@nome, @desc, @preco, @estoque)";

                    MySqlCommand cmd = new MySqlCommand(sql, conn);

                    cmd.Parameters.AddWithValue("@nome", textBox1.Text);
                    cmd.Parameters.AddWithValue("@desc", textBox2.Text);
                    cmd.Parameters.AddWithValue("@preco", preco);
                    cmd.Parameters.AddWithValue("@estoque", estoque);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Produto salvo!");

                    ListarProdutos();

                    textBox1.Clear();
                    textBox2.Clear();
                    textBox3.Clear();
                    textBox4.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (idProdutoSelecionado == 0)
            {
                MessageBox.Show("Selecione um produto na tabela primeiro!");
                return;
            }

            try
            {
                // Tira o texto do TextBox e transforma em número decimal (aceita vírgula)
                decimal precoConvertido;
                if (!decimal.TryParse(textBox3.Text, out precoConvertido))
                {
                    MessageBox.Show("Por favor, insira um preço válido!");
                    return;
                }

                using (MySqlConnection conn = new MySqlConnection(conexaoString))
                {
                    conn.Open();
                    string sql = "UPDATE produtos SET Nome=@nome, Descricao=@desc, Preco=@preco, QuantidadeEstoque=@estoque WHERE IdProduto=@id";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@nome", textBox1.Text);
                        cmd.Parameters.AddWithValue("@desc", textBox2.Text);

                        cmd.Parameters.AddWithValue("@preco", precoConvertido);

                        cmd.Parameters.AddWithValue("@estoque", int.Parse(textBox4.Text));
                        cmd.Parameters.AddWithValue("@id", idProdutoSelecionado);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Produto atualizado com sucesso!");
                        ListarProdutos();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao atualizar: " + ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (idProdutoSelecionado == 0)
            {
                MessageBox.Show("Selecione um produto!");
                return;
            }

            if (MessageBox.Show("Excluir produto?",
                "Confirmação",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(conexaoString))
                    {
                        conn.Open();

                        // REMOVE VÍNCULOS
                        string sqlVinculo =
                            "DELETE FROM fornecedorproduto WHERE IdProduto=@id";

                        MySqlCommand cmdVinculo =
                            new MySqlCommand(sqlVinculo, conn);

                        cmdVinculo.Parameters.AddWithValue("@id", idProdutoSelecionado);

                        cmdVinculo.ExecuteNonQuery();

                        // REMOVE PRODUTO
                        string sql =
                            "DELETE FROM produtos WHERE IdProduto=@id";

                        MySqlCommand cmd =
                            new MySqlCommand(sql, conn);

                        cmd.Parameters.AddWithValue("@id", idProdutoSelecionado);

                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Produto removido!");

                        ListarProdutos();

                        dataGridView2.DataSource = null;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro: " + ex.Message);
                }
            }
        }
        private void ListarProdutos()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(conexaoString))
                {
                    // Busca todos os dados da tabela produtos
                    string sql = "SELECT IdProduto, Nome, Descricao, Preco, QuantidadeEstoque FROM produtos";

                    MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    
                    dataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao listar produtos: " + ex.Message);
            }
        }


        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var valorId = dataGridView1.Rows[e.RowIndex].Cells["IdProduto"].Value;
                if (valorId != DBNull.Value && valorId != null)
                {
                    idProdutoSelecionado = Convert.ToInt32(valorId);
                    ListarFornecedoresDoProduto(idProdutoSelecionado);

                    textBox5.Text = valorId.ToString();
                }

                textBox1.Text = dataGridView1.Rows[e.RowIndex].Cells["Nome"].Value?.ToString() ?? "";
                textBox2.Text = dataGridView1.Rows[e.RowIndex].Cells["Descricao"].Value?.ToString() ?? "";
                textBox3.Text = dataGridView1.Rows[e.RowIndex].Cells["Preco"].Value?.ToString() ?? "0";
                textBox4.Text = dataGridView1.Rows[e.RowIndex].Cells["QuantidadeEstoque"].Value?.ToString() ?? "0";
            }

        }
        private void ListarFornecedoresDoProduto(int idProd)
        {
            using (MySqlConnection conn = new MySqlConnection(conexaoString))
            {
                // Query para buscar quem fornece esse produto específico
                string sql = @"SELECT f.Nome, f.CNPJ, f.Telefone, f.Email, f.`Endereco` 
               FROM fornecedores f 
               INNER JOIN fornecedorproduto fp ON f.IdFornecedor = fp.IdFornecedor 
               WHERE fp.IdProduto = @id";

                MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@id", idProd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                MessageBox.Show("Linhas encontradas: " + dt.Rows.Count);

                dataGridView2.DataSource = dt; // Exibe na tabela cinza 2
            }
        }
        private void BuscarProduto()
        {
            using (MySqlConnection conn = new MySqlConnection(conexaoString))
            {
                string sql = "SELECT * FROM produtos WHERE Nome LIKE @nome";

                MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@nome", "%" + textBoxBuscaProduto.Text + "%");

                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridView1.DataSource = dt;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBoxBuscaProduto_TextChanged(object sender, EventArgs e)
        {
            BuscarProduto();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            using (MySqlConnection conn = new MySqlConnection(conexaoString))
            {
                //estoque menor ou = a 10 é estoque baixo
                string sql = @"SELECT * FROM produtos 
                       WHERE QuantidadeEstoque < 10";

                MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);

                DataTable dt = new DataTable();
                da.Fill(dt); //método que executa a consulta SQL e preenche o DataTable

                dataGridView1.DataSource = dt;

            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            using (MySqlConnection conn = new MySqlConnection(conexaoString))
            {
                string sql = @"SELECT * FROM produtos
                       ORDER BY Preco ASC";

                MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);

                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridView1.DataSource = dt;
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            ListarProdutos();
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

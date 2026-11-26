using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Projeto_av3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2 formFornecedor = new Form2();
            // Abre o formulário
            formFornecedor.Show();
            this.Hide();      
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form3 formProduto = new Form3();
            formProduto.Show();
            this.Hide();      // Esconde o formulário atual (Menu)
        }

        private void button3_Click(object sender, EventArgs e)
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

        private void buttonInfo_Click(object sender, EventArgs e)
        {
            string mensagem =
            "O sistema possui funcionalidades adicionais como:\n\n" +
            "• Busca de fornecedores e produtos\n" +
            "• Filtros por preço e estoque\n" +
            "• Ordenação de listas\n" +
            "• Validação de CNPJ e e-mail\n" +
            "• Mensagens de confirmação e erro\n" +
            "• Controle básico de estoque\n";

            MessageBox.Show(
                mensagem,
                "Funcionalidades do Sistema",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
    }
}

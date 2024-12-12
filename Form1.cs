using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Finance_Tracker
{
    public partial class Form1 : Form
    {
       
        private List<Transaction> transactions = new List<Transaction>();
        private  SQLiteHelper dbHelper;
        private OpenAIHelper aiHelper;


        public Form1()
        {
            InitializeComponent();
            aiHelper = new OpenAIHelper("API_KEY");
            dbHelper = new SQLiteHelper();
            dbHelper.CreateDatabase();
            LoadTransactions();
            UpdateSummary();
            btnGetSuggestions.Click += async (sender, e) =>
            {
                try
                {
                    textBoxAISuggestions.Text = "Fetching suggestions...";

                    // Fetch transaction summary from the database
                    string transactionSummary = dbHelper.GetTransactionSummaryForAI();
                    string prompt = $"Based on these financial details, provide personalized financial advice:\n\n{transactionSummary}";

                    // Get AI suggestions
                    string aiResponse = await aiHelper.GetAISuggestions(prompt);

                    textBoxAISuggestions.Text = aiResponse.Trim();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "AI Suggestions", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

        }
        private void LoadTransactions()
        {
            transactions = dbHelper.LoadTransactions(); 
            Console.WriteLine(transactions);
            dataGridView1.DataSource = null; 
            dataGridView1.DataSource = transactions; 
        }

        // Handle adding a new transaction
        private void AddTransaction_Click(object sender, EventArgs e)
        {
            string date = dateTimePicker1.Value.ToString("yyyy-MM-dd");
            string description = textBox1.Text;
            decimal amount = numericUpDown1.Value;
            string category = comboBox1.SelectedItem.ToString();

            dbHelper.InsertTransaction(date, description, amount, category); 
            dataGridView1.DataSource = dbHelper.LoadTransactions();
            UpdateSummary();    
        }
        

        // Handle deleting the selected transaction
        private void deleteSelected_Click(object sender, EventArgs e)
        {

            if (dataGridView1.SelectedRows.Count > 0)
            {
                int transactionId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value);
                dbHelper.DeleteTransaction(transactionId); 
            }
            UpdateSummary();
        }

        // Update income, expenses, and balance summary
        private void UpdateSummary()
        {
            decimal income = transactions.Where(t => t.Category == "Income").Sum(t => t.Amount);
            decimal expenses = transactions.Where(t => t.Category != "Income").Sum(t => t.Amount);
            
            lblIncome.Text = $"Income: {income:C}";
            lblExpenses.Text = $"Expenses: {expenses:C}";
            lblBalance.Text = $"Balance: {(income - expenses):C}";
            LoadTransactions();

            UpdateChart();
        }

        // Update the pie chart with income and expenses data
        private void UpdateChart()
        {
            chartSummary.Series.Clear();
            var series = chartSummary.Series.Add("Income vs Expenses");
            series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;

            decimal income = transactions.Where(t => t.Category == "Income").Sum(t => t.Amount);
            decimal expenses = transactions.Where(t => t.Category != "Income").Sum(t => t.Amount);

            series.Points.AddXY("Income", income);
            series.Points.AddXY("Expenses", expenses);
        }




    }


}
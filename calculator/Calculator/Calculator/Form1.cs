using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Calculator
{
    public partial class Form1 : Form
    {
        // 定义历史记录类
        public class CalculationRecord
        {
            public double FirstNumber { get; set; }
            public string Operator { get; set; }
            public double SecondNumber { get; set; }
            public double Result { get; set; }
            public DateTime CalculateTime { get; set; }
        }

        // 存储历史记录的列表
        private List<CalculationRecord> historyList = new List<CalculationRecord>();

        // SQL连接字符串
        private string connectionString = "Data Source=LAPTOP-PMDRRHMT;Initial Catalog=CalculatorDB;Integrated Security=True;";

        public Form1()
        {
            InitializeComponent();
            textBox1.Focus();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string checkTableQuery = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Table_1' AND xtype='U')
                    BEGIN
                        CREATE TABLE dbo.Table_1 (
                            id INT IDENTITY(1,1) PRIMARY KEY,
                            Num1 FLOAT NOT NULL,
                            Num2 FLOAT NOT NULL,
                            Operator NVARCHAR(10) NOT NULL,
                            ResultNum FLOAT NOT NULL,
                            UpdateTime DATETIME DEFAULT GETDATE()
                        )
                    END";

                    SqlCommand command = new SqlCommand(checkTableQuery, connection);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"数据库初始化失败: {ex.Message}");
                }
            }
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox2.Text = "";
            comboBox1.SelectedIndex = 0;
            labelResult.Text = "";
            textBox1.Focus();
        }




        private void calculateButton_Click(object sender, EventArgs e)
        {
            if (!double.TryParse(textBox1.Text, out double num1))
            {
                MessageBox.Show("请输入有效的第一个数字");
                textBox1.Focus();
                return;
            }

            if (!double.TryParse(textBox2.Text, out double num2))
            {
                MessageBox.Show("请输入有效的第二个数字");
                textBox2.Focus();
                return;
            }

            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("请选择运算符", "操作错误",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboBox1.Focus();
                return;
            }

            string op = comboBox1.SelectedItem.ToString();
            double result = 0;

            switch (op)
            {
                case "+":
                    result = num1 + num2;
                    break;
                case "-":
                    result = num1 - num2;
                    break;
                case "*":
                    result = num1 * num2;
                    break;
                case "/":
                    if (num2 == 0)
                    {
                        MessageBox.Show("除数不能为零", "计算错误",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                        labelResult.Text = "∞";
                        return;
                    }
                    result = num1 / num2;
                    break;
                default:
                    MessageBox.Show("不支持的运算符", "操作错误",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
            }

            labelResult.Text = result.ToString("G15");

            CalculationRecord record = new CalculationRecord
            {
                FirstNumber = num1,
                Operator = op,
                SecondNumber = num2,
                Result = result,
                CalculateTime = DateTime.Now
            };

            historyList.Add(record);
            dataGridView1.Rows.Add(
                record.FirstNumber,
                record.Operator,
                record.SecondNumber,
                record.Result,
                record.CalculateTime.ToString("yyyy-MM-dd HH:mm:ss")
            );

            // 保存到数据库
            SaveToDatabase(num1, comboBox1.Text, num2, result, DateTime.Now);
        }

        private void SaveToDatabase(double num1, string op, double num2, double result, DateTime time)
        {
            string connectionString = "Server=LAPTOP-PMDRRHMT;Database=CalculatorDB;User Id=sa ;Password=sql123456;";

            string query = "INSERT INTO calculate_log (Num1, Operator, Num2, ResultNum, UpdateTime) " +
                           "VALUES (@Num1, @Operator, @Num2, @ResultNum, @UpdateTime)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Num1", num1);
                cmd.Parameters.AddWithValue("@Operator", op);
                cmd.Parameters.AddWithValue("@Num2", num2);
                cmd.Parameters.AddWithValue("@ResultNum", result);
                cmd.Parameters.AddWithValue("@UpdateTime", time);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("保存到数据库时出错：" + ex.Message);
                }
            }
        }



    }


}


        

        

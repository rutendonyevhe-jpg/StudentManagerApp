using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace StudentManagerApp
{
    public partial class Form1 : Form
    {
        // ✅ GLOBAL CONNECTION
        SqlConnection conn = new SqlConnection(
            "Data Source=.;Initial Catalog=StudentDB;Integrated Security=True");

        public Form1()
        {
            InitializeComponent();
        }

        // ✅ SAFE CLOSE CONNECTION
        private void CloseConnection()
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }

        // ✅ FORM LOAD
        private void Form1_Load(object sender, EventArgs e)
        {
            cmbGender.Items.Clear();
            cmbGender.Items.AddRange(new string[] { "Male", "Female" });

            cmbCourse.Items.Clear();
            cmbCourse.Items.AddRange(new string[] { "IT", "Business", "Engineering" });
        }

        // ✅ FIX DESIGNER ERRORS (EMPTY EVENTS)
        private void cmbGender_SelectedIndexChanged(object sender, EventArgs e) { }
        private void cmbCourse_SelectedIndexChanged(object sender, EventArgs e) { }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }

        // ✅ LOAD SELECTED ROW INTO FIELDS (IMPORTANT FEATURE)
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                txtName.Text = dataGridView1.CurrentRow.Cells[1].Value?.ToString();
                txtAge.Text = dataGridView1.CurrentRow.Cells[2].Value?.ToString();
                cmbGender.Text = dataGridView1.CurrentRow.Cells[3].Value?.ToString();
                cmbCourse.Text = dataGridView1.CurrentRow.Cells[4].Value?.ToString();
            }
        }

        // ✅ CLEAR INPUTS
        private void ClearFields()
        {
            txtName.Clear();
            txtAge.Clear();
            cmbGender.SelectedIndex = -1;
            cmbCourse.SelectedIndex = -1;
            dtpDOB.Value = DateTime.Now;
        }

        // ✅ ADD
        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Name required!");
                    return;
                }

                if (!int.TryParse(txtAge.Text, out int age))
                {
                    MessageBox.Show("Enter a valid age!");
                    return;
                }

                if (age < 0)
                {
                    throw new InvalidAgeException("Age cannot be negative");
                }

                if (string.IsNullOrWhiteSpace(cmbGender.Text) || string.IsNullOrWhiteSpace(cmbCourse.Text))
                {
                    MessageBox.Show("Select Gender and Course!");
                    return;
                }

                conn.Open();

                SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Students (FullName, Age, Gender, Course, DateOfBirth) VALUES (@name,@age,@gender,@course,@dob)", conn);

                cmd.Parameters.AddWithValue("@name", txtName.Text);
                cmd.Parameters.AddWithValue("@age", age);
                cmd.Parameters.AddWithValue("@gender", cmbGender.Text);
                cmd.Parameters.AddWithValue("@course", cmbCourse.Text);
                cmd.Parameters.AddWithValue("@dob", dtpDOB.Value);

                cmd.ExecuteNonQuery();

                MessageBox.Show("Student Added!");
                ClearFields();
            }
            catch (InvalidAgeException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }
        }

        // ✅ VIEW
        private void btnView_Click(object sender, EventArgs e)
        {
            try
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Students", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dataGridView1.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // ✅ UPDATE
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.CurrentRow == null)
                {
                    MessageBox.Show("Select a record first!");
                    return;
                }

                if (!int.TryParse(txtAge.Text, out int age))
                {
                    MessageBox.Show("Invalid age!");
                    return;
                }

                conn.Open();

                SqlCommand cmd = new SqlCommand(
                    "UPDATE Students SET FullName=@name, Age=@age, Gender=@gender, Course=@course WHERE StudentID=@id", conn);

                cmd.Parameters.AddWithValue("@id", dataGridView1.CurrentRow.Cells[0].Value);
                cmd.Parameters.AddWithValue("@name", txtName.Text);
                cmd.Parameters.AddWithValue("@age", age);
                cmd.Parameters.AddWithValue("@gender", cmbGender.Text);
                cmd.Parameters.AddWithValue("@course", cmbCourse.Text);

                cmd.ExecuteNonQuery();

                MessageBox.Show("Updated!");
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                CloseConnection();
            }
        }

        // ✅ DELETE
        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.CurrentRow == null)
                {
                    MessageBox.Show("Select a record first!");
                    return;
                }

                conn.Open();

                SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Students WHERE StudentID=@id", conn);

                cmd.Parameters.AddWithValue("@id", dataGridView1.CurrentRow.Cells[0].Value);

                cmd.ExecuteNonQuery();

                MessageBox.Show("Deleted!");
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                CloseConnection();
            }
        }

        // ✅ EXPORT
        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter("students.txt"))
                {
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (!row.IsNewRow && row.Cells[1].Value != null)
                        {
                            sw.WriteLine($"{row.Cells[1].Value},{row.Cells[2].Value}");
                        }
                    }
                }

                MessageBox.Show("Exported!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // ✅ IMPORT
        private void btnImport_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists("students.txt"))
                {
                    MessageBox.Show("File not found!");
                    return;
                }

                listBoxLog.Items.Clear();

                foreach (string line in File.ReadAllLines("students.txt"))
                {
                    listBoxLog.Items.Add(line);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }

    // ✅ CUSTOM EXCEPTION
    public class InvalidAgeException : Exception
    {
        public InvalidAgeException(string message) : base(message) { }
    }
}
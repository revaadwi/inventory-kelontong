using System.Data;
using MySql.Data.MySqlClient;

namespace kelontongku
{
    public partial class Form1 : Form
    {
        MySqlConnection koneksi = Koneksi.getConnection();
        DataTable dataTable = new DataTable();
        //Tampilan MySql
        public DataTable getDataTable()
        {
            dataTable.Reset();
            dataTable = new DataTable();
            using (MySqlCommand command = new MySqlCommand("SELECT * FROM kelontong", koneksi))
            {
                koneksi.Open();

                MySqlDataReader reader = command.ExecuteReader();
                dataTable.Load(reader);
            }
            return dataTable;
        }

        // mengambil data tabe
        public void filldataTable()
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.RowTemplate.Height = 170;
            dataGridView1.DataSource = getDataTable();
            Column1.DataPropertyName = "id";
            Column2.DataPropertyName = "nama_produk";
            Column3.DataPropertyName = "brand";
            Column4.DataPropertyName = "harga";
            Column5.DataPropertyName = "stock";
            Column6.DataPropertyName = "gambar";

        }

        // fungsi menghapus isi field
        public void clear()
        {
            id_produk.Clear();
            produk.Clear();
            brand.Clear();
            harga.Clear();
            stock.Clear();
            gambar.Image = null;
        }

        // mengurutkan id
        public void resetIncrement()
        {
            MySqlScript script = new MySqlScript(koneksi, "SET @id :=0; Update kelontong SET id = @id := (@id+1); " + "ALTER TABLE kelontong AUTO_INCREMENT = 1;");
            script.Execute();
        }

        // fungsi mencari data
        public void searchData(string ValueToFind)
        {
            string searchQuery = "SELECT * FROM kelontong WHERE CONCAT (id, nama_produk, brand, harga, stock) LIKE '%" + ValueToFind + "%'";
            MySqlDataAdapter adapter = new MySqlDataAdapter(searchQuery, koneksi);
            DataTable table = new DataTable();
            adapter.Fill(table);
            dataGridView1.DataSource = table;
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void txt_search_TextChanged(object sender, EventArgs e)
        {
            searchData(txt_search.Text);
        }

        //menambah data
        private void btn_tambah_Click(object sender, EventArgs e)
        {
            MySqlCommand cmd;
            // error handling jika data tidak masuk
            try
            {
                // Convert image to byte array
                byte[] imageData;
                using (MemoryStream ms = new MemoryStream())
                {
                    gambar.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    imageData = ms.ToArray();
                }
                resetIncrement();
                cmd = koneksi.CreateCommand();
                cmd.CommandText = "INSERT INTO kelontong(nama_produk, brand, harga, stock, gambar) VALUE(@nama_produk, @brand, @harga, @stock, @gambar)";
                cmd.Parameters.AddWithValue("@nama_produk", produk.Text);
                cmd.Parameters.AddWithValue("@brand", brand.Text);
                cmd.Parameters.AddWithValue("@harga", harga.Text);
                cmd.Parameters.AddWithValue("@stock", stock.Text);
                cmd.Parameters.AddWithValue("@gambar", imageData);
                cmd.ExecuteNonQuery();

                koneksi.Close();
                filldataTable();
                clear();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Terjadi kesalahan saat memasukkan data: " + ex.Message);
            }
        }

        private void btn_update_Click(object sender, EventArgs e)
        {
            MySqlCommand cmd;
            // error handling jika data tidak masuk
            try
            {
                // Convert image to byte array
                byte[] imageData;
                using (MemoryStream ms = new MemoryStream())
                {
                    gambar.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    imageData = ms.ToArray();
                }
                cmd = koneksi.CreateCommand();
                cmd.CommandText = "UPDATE kelontong SET nama_produk = @nama_produk, brand = @brand, harga = @harga, stock = @stock ,gambar = @gambar WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id_produk.Text);
                cmd.Parameters.AddWithValue("@nama_produk", produk.Text);
                cmd.Parameters.AddWithValue("@brand", brand.Text);
                cmd.Parameters.AddWithValue("@harga", harga.Text);
                cmd.Parameters.AddWithValue("@stock", stock.Text);
                cmd.Parameters.AddWithValue("@gambar", imageData);
                cmd.ExecuteNonQuery();

                clear();
                koneksi.Close();
                filldataTable();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Terjadi kesalahan saat memasukkan data: " + ex.Message);
            }
        }

        private void btn_delete_Click(object sender, EventArgs e)
        {
            MySqlCommand cmd;

            try
            {
                cmd = koneksi.CreateCommand();
                cmd.CommandText = "DELETE from kelontong WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id_produk.Text);
                cmd.ExecuteNonQuery();

                resetIncrement();
                clear();
                koneksi.Close();
                dataTable.Clear();
                filldataTable();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Data Gagal Dihapus = " + ex);
            }
        }

        private void gambar_Click(object sender, EventArgs e)
        {
            OpenFileDialog openfd = new OpenFileDialog();
            openfd.Filter = "Image Files (*.bmp, *.jpg, *.jpeg, *.png, *.gif)|*.bmp;*.jpg;*.jpeg;*.png;*.gif";
            if (openfd.ShowDialog() == DialogResult.OK)
            {
                // Load the selected image file into a new Bitmap object
                Bitmap selectedImage = new Bitmap(openfd.FileName);

                // Check the size of the image file in bytes
                long imageSize = new System.IO.FileInfo(openfd.FileName).Length;

                // If the image file size is larger than 100 KB, display an error message
                if (imageSize > 100000)
                {
                    MessageBox.Show("Ukuran gambar terlalu besar!");
                }
                else
                {
                    // Set the Image property of the PictureBox control to the selected image
                    gambar.Image = selectedImage;
                }
            }

        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int id = Convert.ToInt32(dataGridView1.CurrentCell.RowIndex.ToString());
            id_produk.Text = dataGridView1.Rows[id].Cells[0].Value.ToString();
            produk.Text = dataGridView1.Rows[id].Cells[1].Value.ToString();
            brand.Text = dataGridView1.Rows[id].Cells[2].Value.ToString();
            harga.Text = dataGridView1.Rows[id].Cells[3].Value.ToString();
            stock.Text = dataGridView1.Rows[id].Cells[4].Value.ToString();
            // retrieve the BLOB image data for the clicked row and create an Image object
            Byte[] img = (Byte[])dataGridView1.CurrentRow.Cells[5].Value;
            MemoryStream ms = new MemoryStream(img);
            gambar.Image = Image.FromStream(ms);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            filldataTable();
        }

        private void btn_clear_Click(object sender, EventArgs e)
        {
            clear();
        }
    }
}
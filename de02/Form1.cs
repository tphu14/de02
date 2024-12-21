using System;
using System.Linq;
using System.Windows.Forms;
using de02.Models; // Namespace chứa SPDbContext, LoaiSP, và Sanpham

namespace de02
{
    public partial class frmSanpham : Form
    {
        private SPDbContext dbContext; // Khai báo DbContext

        public frmSanpham()
        {
            InitializeComponent();
            dbContext = new SPDbContext(); // Khởi tạo DbContext
        }

        private void frmSanpham_Load(object sender, EventArgs e)
        {
            // Gọi hàm LoadData và LoadLoaiSP khi form tải
            LoadData();
            LoadLoaiSP();
            dataGridView1.CellClick += dataGridView1_CellClick;

        }

        private void LoadData()
        {
            try
            {
                // Lấy dữ liệu từ bảng Sanpham và hiển thị trên DataGridView
                var sanphamList = dbContext.Sanphams
                    .Select(sp => new
                    {
                        sp.MaSP,
                        sp.TenSP,
                        sp.Ngaynhap,
                        sp.MaLoai,
                        TenLoai = sp.LoaiSP.TenLoai // Hiển thị tên loại sản phẩm
                    })
                    .ToList();

                // Gán dữ liệu vào DataGridView
                dataGridView1.DataSource = sanphamList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadLoaiSP()
        {
            try
            {
                // Lấy danh sách các loại sản phẩm
                var loaiSPList = dbContext.LoaiSPs
                    .Select(ls => new
                    {
                        ls.MaLoai,
                        ls.TenLoai
                    })
                    .ToList();

                // Gán dữ liệu vào ComboBox
                cbbLoai.DataSource = loaiSPList;
                cbbLoai.DisplayMember = "TenLoai"; // Hiển thị tên loại
                cbbLoai.ValueMember = "MaLoai";   // Giá trị là mã loại
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách loại sản phẩm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frmSanpham_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Giải phóng DbContext khi form đóng
            dbContext.Dispose();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Xử lý sự kiện click trên DataGridView nếu cần
        }
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Kiểm tra xem người dùng có nhấp vào hàng hợp lệ không
                if (e.RowIndex >= 0)
                {
                    // Lấy hàng hiện tại
                    DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                    // Lấy giá trị từ các cột của hàng
                    string maSP = row.Cells["MaSP"].Value?.ToString();
                    string tenSP = row.Cells["TenSP"].Value?.ToString();
                    string ngayNhap = row.Cells["Ngaynhap"].Value?.ToString();
                    string maLoai = row.Cells["MaLoai"].Value?.ToString();

                    // Gán giá trị vào các điều khiển khác
                    txtID.Text = maSP;    // TextBox hiển thị mã sản phẩm
                    txtName.Text = tenSP;  // TextBox hiển thị tên sản phẩm
                    dateTimePicker1.Value = DateTime.Parse(ngayNhap); // DateTimePicker
                    cbbLoai.SelectedValue = maLoai; // ComboBox loại sản phẩm
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xử lý Cell Click: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                // Lấy dữ liệu từ các điều khiển
                string maSP = txtID.Text.Trim();
                string tenSP = txtName.Text.Trim();
                DateTime ngayNhap = dateTimePicker1.Value;
                string maLoai = cbbLoai.SelectedValue?.ToString();

                // Kiểm tra dữ liệu hợp lệ
                if (string.IsNullOrEmpty(maSP) || string.IsNullOrEmpty(tenSP) || string.IsNullOrEmpty(maLoai))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Kiểm tra trùng mã sản phẩm
                if (dbContext.Sanphams.Any(sp => sp.MaSP == maSP))
                {
                    MessageBox.Show("Mã sản phẩm đã tồn tại!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Tạo đối tượng Sanpham mới
                var sanpham = new Sanpham
                {
                    MaSP = maSP,
                    TenSP = tenSP,
                    Ngaynhap = ngayNhap,
                    MaLoai = maLoai
                };

                // Thêm sản phẩm mới vào DbContext
                dbContext.Sanphams.Add(sanpham);
                dbContext.SaveChanges();

                // Làm mới DataGridView
                LoadData();

                // Thông báo thành công
                MessageBox.Show("Thêm sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Xóa trắng các điều khiển
                ClearControls();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm sản phẩm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Hàm xóa trắng các điều khiển sau khi thêm sản phẩm
        private void ClearControls()
        {
            txtID.Clear();
            txtName.Clear();
            dateTimePicker1.Value = DateTime.Now;
            cbbLoai.SelectedIndex = -1; // Bỏ chọn ComboBox
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                // Lấy mã sản phẩm từ TextBox
                string maSP = txtID.Text.Trim();

                if (string.IsNullOrEmpty(maSP))
                {
                    MessageBox.Show("Vui lòng chọn sản phẩm cần sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Tìm sản phẩm trong cơ sở dữ liệu
                var sanpham = dbContext.Sanphams.FirstOrDefault(sp => sp.MaSP == maSP);

                if (sanpham == null)
                {
                    MessageBox.Show("Không tìm thấy sản phẩm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Cập nhật thông tin sản phẩm
                sanpham.TenSP = txtName.Text.Trim();
                sanpham.Ngaynhap = dateTimePicker1.Value;
                sanpham.MaLoai = cbbLoai.SelectedValue?.ToString();

                // Lưu thay đổi vào cơ sở dữ liệu
                dbContext.SaveChanges();

                // Làm mới DataGridView
                LoadData();

                // Thông báo thành công
                MessageBox.Show("Cập nhật sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Xóa trắng các điều khiển
                ClearControls();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi sửa sản phẩm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                // Lấy mã sản phẩm từ TextBox
                string maSP = txtID.Text.Trim();

                if (string.IsNullOrEmpty(maSP))
                {
                    MessageBox.Show("Vui lòng chọn sản phẩm cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Tìm sản phẩm trong cơ sở dữ liệu
                var sanpham = dbContext.Sanphams.FirstOrDefault(sp => sp.MaSP == maSP);

                if (sanpham == null)
                {
                    MessageBox.Show("Không tìm thấy sản phẩm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Xác nhận xóa
                var confirmResult = MessageBox.Show("Bạn có chắc chắn muốn xóa sản phẩm này?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirmResult == DialogResult.Yes)
                {
                    // Xóa sản phẩm
                    dbContext.Sanphams.Remove(sanpham);
                    dbContext.SaveChanges();

                    // Làm mới DataGridView
                    LoadData();

                    // Thông báo thành công
                    MessageBox.Show("Xóa sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Xóa trắng các điều khiển
                    ClearControls();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa sản phẩm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            // Hiển thị hộp thoại xác nhận
            var confirmResult = MessageBox.Show(
                "Bạn có chắc chắn muốn đóng form này?", // Nội dung cảnh báo
                "Xác nhận đóng",                       // Tiêu đề hộp thoại
                MessageBoxButtons.YesNo,               // Các nút Yes/No
                MessageBoxIcon.Question);              // Biểu tượng cảnh báo

            // Kiểm tra lựa chọn của người dùng
            if (confirmResult == DialogResult.Yes)
            {
                // Người dùng chọn Yes, đóng form
                this.Close();
            }
            else
            {
                // Người dùng chọn No, không làm gì cả
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                // Lấy giá trị từ TextBox tìm kiếm
                string searchKeyword = txtSearch.Text.Trim();

                // Kiểm tra nếu không có từ khóa nào được nhập
                if (string.IsNullOrEmpty(searchKeyword))
                {
                    // Nếu không nhập gì, hiển thị toàn bộ dữ liệu
                    LoadData();
                    return;
                }

                // Lấy dữ liệu từ cơ sở dữ liệu theo từ khóa tìm kiếm
                var searchResult = dbContext.Sanphams
                    .Where(sp => sp.TenSP.Contains(searchKeyword)) // Tìm kiếm theo tên sản phẩm
                    .Select(sp => new
                    {
                        sp.MaSP,
                        sp.TenSP,
                        sp.Ngaynhap,
                        sp.MaLoai,
                        TenLoai = sp.LoaiSP.TenLoai // Hiển thị tên loại sản phẩm
                    })
                    .ToList();

                // Gán kết quả tìm kiếm vào DataGridView
                dataGridView1.DataSource = searchResult;

                // Thông báo nếu không tìm thấy kết quả nào
                if (searchResult.Count == 0)
                {
                    MessageBox.Show("Không tìm thấy sản phẩm nào phù hợp!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}

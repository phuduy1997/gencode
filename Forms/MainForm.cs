using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SqlGeneratorApp.Models;
using SqlGeneratorApp.Utils;
using System.Threading;

namespace SqlGeneratorApp.Forms
{
    public partial class MainForm : Form
    {
        private ModelInfo _model;
        private BindingList<FieldInfo> _fields;

        public MainForm()
        {
            InitializeComponent();
            _model = new ModelInfo();
            _fields = new BindingList<FieldInfo>();
            
            // Thiết lập các ComboBox
            cboDataType.Items.AddRange(new string[] {
                "int", "bigint", "smallint", "tinyint", "bit", 
                "decimal", "numeric", "money", "float", "real",
                "char", "nchar", "varchar", "nvarchar", "text", "ntext",
                "date", "datetime", "datetime2", "time",
                "uniqueidentifier"
            });
            cboDataType.SelectedIndex = 0;
            
            // Thiết lập DataGridView
            SetupDataGridView();
            
            this.Load += MainForm_Load;
        }

        private void SetupDataGridView()
        {
            // Clear existing columns
            dgvFields.Columns.Clear();
            
            // Add columns
            DataGridViewTextBoxColumn nameColumn = new DataGridViewTextBoxColumn();
            nameColumn.Name = "colName";
            nameColumn.HeaderText = "Tên trường";
            nameColumn.DataPropertyName = "Name";
            nameColumn.Width = 150;
            dgvFields.Columns.Add(nameColumn);
            
            DataGridViewTextBoxColumn dataTypeColumn = new DataGridViewTextBoxColumn();
            dataTypeColumn.Name = "colDataType";
            dataTypeColumn.HeaderText = "Kiểu dữ liệu";
            dataTypeColumn.DataPropertyName = "DataType";
            dataTypeColumn.Width = 100;
            dgvFields.Columns.Add(dataTypeColumn);
            
            DataGridViewTextBoxColumn lengthColumn = new DataGridViewTextBoxColumn();
            lengthColumn.Name = "colMaxLength";
            lengthColumn.HeaderText = "Độ dài";
            lengthColumn.DataPropertyName = "MaxLength";
            lengthColumn.Width = 80;
            dgvFields.Columns.Add(lengthColumn);
            
            DataGridViewCheckBoxColumn pkColumn = new DataGridViewCheckBoxColumn();
            pkColumn.Name = "colPK";
            pkColumn.HeaderText = "Khóa chính";
            pkColumn.DataPropertyName = "IsPrimaryKey";
            pkColumn.Width = 80;
            dgvFields.Columns.Add(pkColumn);
            
            DataGridViewCheckBoxColumn identityColumn = new DataGridViewCheckBoxColumn();
            identityColumn.Name = "colIdentity";
            identityColumn.HeaderText = "Identity";
            identityColumn.DataPropertyName = "IsIdentity";
            identityColumn.Width = 80;
            dgvFields.Columns.Add(identityColumn);
            
            DataGridViewCheckBoxColumn nullableColumn = new DataGridViewCheckBoxColumn();
            nullableColumn.Name = "colNullable";
            nullableColumn.HeaderText = "Nullable";
            nullableColumn.DataPropertyName = "IsNullable";
            nullableColumn.Width = 80;
            dgvFields.Columns.Add(nullableColumn);
            
            DataGridViewTextBoxColumn defaultColumn = new DataGridViewTextBoxColumn();
            defaultColumn.Name = "colDefault";
            defaultColumn.HeaderText = "Giá trị mặc định";
            defaultColumn.DataPropertyName = "DefaultValue";
            defaultColumn.Width = 120;
            dgvFields.Columns.Add(defaultColumn);
            
            // Set the datasource
            dgvFields.DataSource = _fields;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Set default value for txtModelName
            txtModelName.Text = "Model1";
            
            // Clear fields
            _fields.Clear();
            
            // Update UI
            UpdateGenerateButtonState();
        }

        private void btnAddField_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFieldName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên trường.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (cboDataType.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn kiểu dữ liệu.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check if field name already exists
            if (_fields.Any(f => f.Name.Equals(txtFieldName.Text, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Tên trường đã tồn tại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Create new field
            FieldInfo field = new FieldInfo
            {
                Name = txtFieldName.Text,
                DataType = cboDataType.SelectedItem.ToString(),
                IsPrimaryKey = chkPrimaryKey.Checked,
                IsNullable = chkNullable.Checked,
                IsIdentity = chkIdentity.Checked
            };

            // Set MaxLength for string types
            string dataType = cboDataType.SelectedItem.ToString().ToLower();
            if (dataType == "varchar" || dataType == "nvarchar" || dataType == "char" || dataType == "nchar")
            {
                if (!string.IsNullOrWhiteSpace(txtMaxLength.Text) && int.TryParse(txtMaxLength.Text, out int maxLength))
                {
                    field.MaxLength = maxLength;
                }
                else if (txtMaxLength.Text.ToLower() == "max")
                {
                    field.MaxLength = -1; // -1 represents MAX
                }
                else
                {
                    field.MaxLength = 50; // Default
                }
            }

            // Set default value
            if (!string.IsNullOrWhiteSpace(txtDefaultValue.Text))
            {
                field.DefaultValue = txtDefaultValue.Text;
            }

            // Add to list
            _fields.Add(field);

            // Clear input fields
            txtFieldName.Clear();
            txtMaxLength.Clear();
            txtDefaultValue.Clear();
            chkPrimaryKey.Checked = false;
            chkNullable.Checked = true;
            chkIdentity.Checked = false;
            
            // Update UI
            UpdateGenerateButtonState();
        }

        private void btnRemoveField_Click(object sender, EventArgs e)
        {
            if (dgvFields.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dgvFields.SelectedRows)
                {
                    dgvFields.Rows.RemoveAt(row.Index);
                }
            }
            else if (dgvFields.SelectedCells.Count > 0)
            {
                int rowIndex = dgvFields.SelectedCells[0].RowIndex;
                dgvFields.Rows.RemoveAt(rowIndex);
            }
            
            // Update UI
            UpdateGenerateButtonState();
        }

        private void cboDataType_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedType = cboDataType.SelectedItem?.ToString().ToLower() ?? string.Empty;
            
            // Enable/disable MaxLength for string types
            bool isStringType = selectedType == "varchar" || selectedType == "nvarchar" || 
                               selectedType == "char" || selectedType == "nchar";
            
            txtMaxLength.Enabled = isStringType;
            lblMaxLength.Enabled = isStringType;
            
            // Enable/disable Identity for numeric types
            bool isNumericType = selectedType == "int" || selectedType == "bigint" || 
                                selectedType == "smallint" || selectedType == "tinyint";
            
            chkIdentity.Enabled = isNumericType;
            
            // If switching from a numeric type to non-numeric, uncheck identity
            if (!isNumericType)
            {
                chkIdentity.Checked = false;
            }
        }

        private void UpdateGenerateButtonState()
        {
            // Enable generate button if we have a model name and at least one field
            btnGenerate.Enabled = !string.IsNullOrWhiteSpace(txtModelName.Text) && _fields.Count > 0;
        }

        private void txtModelName_TextChanged(object sender, EventArgs e)
        {
            UpdateGenerateButtonState();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtModelName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên model.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_fields.Count == 0)
            {
                MessageBox.Show("Vui lòng thêm ít nhất một trường.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Create model
            _model.ModelName = txtModelName.Text;
            _model.Fields = _fields.ToList();

            // Ask for framework type
            DialogResult result = MessageBox.Show(
                "Bạn muốn tạo mã cho .NET Core không? Chọn 'Có' cho .NET Core, 'Không' cho .NET Framework.",
                "Chọn Framework",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            
            FrameworkType frameworkType = result == DialogResult.Yes 
                ? FrameworkType.DotNetCore 
                : FrameworkType.DotNetFramework;

            // Choose output folder
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Chọn thư mục lưu kết quả";
                folderDialog.ShowNewFolderButton = true;
                
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string outputPath = folderDialog.SelectedPath;
                        
                        // Show waiting form
                        WaitingForm waitingForm = new WaitingForm("Đang tạo file...");
                        Thread generatorThread = new Thread(() => 
                        {
                            try
                            {
                                // Generate code
                                waitingForm.SetStatus("Đang tạo mã nguồn...");
                                CodeGenerator codeGenerator = new CodeGenerator(_model, frameworkType);
                                
                                // Generate SQL first
                                waitingForm.SetStatus("Đang tạo script SQL...");
                                SqlGenerator sqlGenerator = new SqlGenerator(_model);
                                string createTableSql = sqlGenerator.GenerateCreateTableSql();
                                
                                // Save results
                                waitingForm.SetStatus("Đang lưu các file...");
                                codeGenerator.SaveAllFiles(outputPath);
                                
                                // Update UI on main thread
                                this.Invoke((Action)(() =>
                                {
                                    // Show the SQL result
                                    txtSqlResult.Text = createTableSql;
                                    
                                    // Close waiting form
                                    waitingForm.Close();
                                    
                                    // Show success message
                                    MessageBox.Show(
                                        $"Đã tạo thành công các file trong thư mục:\n{Path.Combine(outputPath, _model.ModelName)}",
                                        "Thành công",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                                }));
                            }
                            catch (Exception ex)
                            {
                                // Handle exception on main thread
                                this.Invoke((Action)(() =>
                                {
                                    // Close waiting form
                                    waitingForm.Close();
                                    
                                    // Show error
                                    MessageBox.Show($"Lỗi khi tạo file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }));
                            }
                        });
                        
                        // Start generation in separate thread
                        generatorThread.Start();
                        waitingForm.ShowDialog();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi tạo file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
} 
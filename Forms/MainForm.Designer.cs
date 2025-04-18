using System.Windows.Forms;

namespace SqlGeneratorApp.Forms
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblModelName = new System.Windows.Forms.Label();
            this.txtModelName = new System.Windows.Forms.TextBox();
            this.groupBoxFields = new System.Windows.Forms.GroupBox();
            this.lblFieldName = new System.Windows.Forms.Label();
            this.txtFieldName = new System.Windows.Forms.TextBox();
            this.lblDataType = new System.Windows.Forms.Label();
            this.cboDataType = new System.Windows.Forms.ComboBox();
            this.lblMaxLength = new System.Windows.Forms.Label();
            this.txtMaxLength = new System.Windows.Forms.TextBox();
            this.chkPrimaryKey = new System.Windows.Forms.CheckBox();
            this.chkIdentity = new System.Windows.Forms.CheckBox();
            this.chkNullable = new System.Windows.Forms.CheckBox();
            this.lblDefaultValue = new System.Windows.Forms.Label();
            this.txtDefaultValue = new System.Windows.Forms.TextBox();
            this.btnAddField = new System.Windows.Forms.Button();
            this.btnRemoveField = new System.Windows.Forms.Button();
            this.dgvFields = new System.Windows.Forms.DataGridView();
            this.groupBoxSql = new System.Windows.Forms.GroupBox();
            this.txtSqlResult = new System.Windows.Forms.TextBox();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.groupBoxFields.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFields)).BeginInit();
            this.groupBoxSql.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblModelName
            // 
            this.lblModelName.AutoSize = true;
            this.lblModelName.Location = new System.Drawing.Point(12, 15);
            this.lblModelName.Name = "lblModelName";
            this.lblModelName.Size = new System.Drawing.Size(70, 13);
            this.lblModelName.TabIndex = 0;
            this.lblModelName.Text = "Tên Model:";
            // 
            // txtModelName
            // 
            this.txtModelName.Location = new System.Drawing.Point(88, 12);
            this.txtModelName.Name = "txtModelName";
            this.txtModelName.Size = new System.Drawing.Size(300, 20);
            this.txtModelName.TabIndex = 1;
            this.txtModelName.TextChanged += new System.EventHandler(this.txtModelName_TextChanged);
            // 
            // groupBoxFields
            // 
            this.groupBoxFields.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFields.Controls.Add(this.btnRemoveField);
            this.groupBoxFields.Controls.Add(this.btnAddField);
            this.groupBoxFields.Controls.Add(this.txtDefaultValue);
            this.groupBoxFields.Controls.Add(this.lblDefaultValue);
            this.groupBoxFields.Controls.Add(this.chkNullable);
            this.groupBoxFields.Controls.Add(this.chkIdentity);
            this.groupBoxFields.Controls.Add(this.chkPrimaryKey);
            this.groupBoxFields.Controls.Add(this.txtMaxLength);
            this.groupBoxFields.Controls.Add(this.lblMaxLength);
            this.groupBoxFields.Controls.Add(this.cboDataType);
            this.groupBoxFields.Controls.Add(this.lblDataType);
            this.groupBoxFields.Controls.Add(this.txtFieldName);
            this.groupBoxFields.Controls.Add(this.lblFieldName);
            this.groupBoxFields.Location = new System.Drawing.Point(12, 38);
            this.groupBoxFields.Name = "groupBoxFields";
            this.groupBoxFields.Size = new System.Drawing.Size(760, 100);
            this.groupBoxFields.TabIndex = 2;
            this.groupBoxFields.TabStop = false;
            this.groupBoxFields.Text = "Thông tin trường dữ liệu";
            // 
            // lblFieldName
            // 
            this.lblFieldName.AutoSize = true;
            this.lblFieldName.Location = new System.Drawing.Point(6, 25);
            this.lblFieldName.Name = "lblFieldName";
            this.lblFieldName.Size = new System.Drawing.Size(32, 13);
            this.lblFieldName.TabIndex = 0;
            this.lblFieldName.Text = "Tên:";
            // 
            // txtFieldName
            // 
            this.txtFieldName.Location = new System.Drawing.Point(44, 22);
            this.txtFieldName.Name = "txtFieldName";
            this.txtFieldName.Size = new System.Drawing.Size(150, 20);
            this.txtFieldName.TabIndex = 1;
            // 
            // lblDataType
            // 
            this.lblDataType.AutoSize = true;
            this.lblDataType.Location = new System.Drawing.Point(200, 25);
            this.lblDataType.Name = "lblDataType";
            this.lblDataType.Size = new System.Drawing.Size(33, 13);
            this.lblDataType.TabIndex = 2;
            this.lblDataType.Text = "Kiểu:";
            // 
            // cboDataType
            // 
            this.cboDataType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDataType.FormattingEnabled = true;
            this.cboDataType.Location = new System.Drawing.Point(239, 22);
            this.cboDataType.Name = "cboDataType";
            this.cboDataType.Size = new System.Drawing.Size(121, 21);
            this.cboDataType.TabIndex = 3;
            this.cboDataType.SelectedIndexChanged += new System.EventHandler(this.cboDataType_SelectedIndexChanged);
            // 
            // lblMaxLength
            // 
            this.lblMaxLength.AutoSize = true;
            this.lblMaxLength.Location = new System.Drawing.Point(366, 25);
            this.lblMaxLength.Name = "lblMaxLength";
            this.lblMaxLength.Size = new System.Drawing.Size(45, 13);
            this.lblMaxLength.TabIndex = 4;
            this.lblMaxLength.Text = "Độ dài:";
            // 
            // txtMaxLength
            // 
            this.txtMaxLength.Location = new System.Drawing.Point(417, 22);
            this.txtMaxLength.Name = "txtMaxLength";
            this.txtMaxLength.Size = new System.Drawing.Size(60, 20);
            this.txtMaxLength.TabIndex = 5;
            // 
            // chkPrimaryKey
            // 
            this.chkPrimaryKey.AutoSize = true;
            this.chkPrimaryKey.Location = new System.Drawing.Point(9, 59);
            this.chkPrimaryKey.Name = "chkPrimaryKey";
            this.chkPrimaryKey.Size = new System.Drawing.Size(81, 17);
            this.chkPrimaryKey.TabIndex = 6;
            this.chkPrimaryKey.Text = "Khóa chính";
            this.chkPrimaryKey.UseVisualStyleBackColor = true;
            // 
            // chkIdentity
            // 
            this.chkIdentity.AutoSize = true;
            this.chkIdentity.Location = new System.Drawing.Point(96, 59);
            this.chkIdentity.Name = "chkIdentity";
            this.chkIdentity.Size = new System.Drawing.Size(60, 17);
            this.chkIdentity.TabIndex = 7;
            this.chkIdentity.Text = "Identity";
            this.chkIdentity.UseVisualStyleBackColor = true;
            // 
            // chkNullable
            // 
            this.chkNullable.AutoSize = true;
            this.chkNullable.Checked = true;
            this.chkNullable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNullable.Location = new System.Drawing.Point(162, 59);
            this.chkNullable.Name = "chkNullable";
            this.chkNullable.Size = new System.Drawing.Size(65, 17);
            this.chkNullable.TabIndex = 8;
            this.chkNullable.Text = "Nullable";
            this.chkNullable.UseVisualStyleBackColor = true;
            // 
            // lblDefaultValue
            // 
            this.lblDefaultValue.AutoSize = true;
            this.lblDefaultValue.Location = new System.Drawing.Point(236, 60);
            this.lblDefaultValue.Name = "lblDefaultValue";
            this.lblDefaultValue.Size = new System.Drawing.Size(90, 13);
            this.lblDefaultValue.TabIndex = 9;
            this.lblDefaultValue.Text = "Giá trị mặc định:";
            // 
            // txtDefaultValue
            // 
            this.txtDefaultValue.Location = new System.Drawing.Point(332, 57);
            this.txtDefaultValue.Name = "txtDefaultValue";
            this.txtDefaultValue.Size = new System.Drawing.Size(145, 20);
            this.txtDefaultValue.TabIndex = 10;
            // 
            // btnAddField
            // 
            this.btnAddField.Location = new System.Drawing.Point(483, 56);
            this.btnAddField.Name = "btnAddField";
            this.btnAddField.Size = new System.Drawing.Size(104, 23);
            this.btnAddField.TabIndex = 11;
            this.btnAddField.Text = "Thêm trường";
            this.btnAddField.UseVisualStyleBackColor = true;
            this.btnAddField.Click += new System.EventHandler(this.btnAddField_Click);
            // 
            // btnRemoveField
            // 
            this.btnRemoveField.Location = new System.Drawing.Point(593, 56);
            this.btnRemoveField.Name = "btnRemoveField";
            this.btnRemoveField.Size = new System.Drawing.Size(104, 23);
            this.btnRemoveField.TabIndex = 12;
            this.btnRemoveField.Text = "Xóa trường";
            this.btnRemoveField.UseVisualStyleBackColor = true;
            this.btnRemoveField.Click += new System.EventHandler(this.btnRemoveField_Click);
            // 
            // dgvFields
            // 
            this.dgvFields.AllowUserToAddRows = false;
            this.dgvFields.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvFields.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFields.Location = new System.Drawing.Point(12, 144);
            this.dgvFields.Name = "dgvFields";
            this.dgvFields.ReadOnly = true;
            this.dgvFields.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvFields.Size = new System.Drawing.Size(760, 150);
            this.dgvFields.TabIndex = 3;
            // 
            // groupBoxSql
            // 
            this.groupBoxSql.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSql.Controls.Add(this.txtSqlResult);
            this.groupBoxSql.Location = new System.Drawing.Point(12, 338);
            this.groupBoxSql.Name = "groupBoxSql";
            this.groupBoxSql.Size = new System.Drawing.Size(760, 162);
            this.groupBoxSql.TabIndex = 4;
            this.groupBoxSql.TabStop = false;
            this.groupBoxSql.Text = "SQL";
            // 
            // txtSqlResult
            // 
            this.txtSqlResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSqlResult.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSqlResult.Location = new System.Drawing.Point(6, 19);
            this.txtSqlResult.Multiline = true;
            this.txtSqlResult.Name = "txtSqlResult";
            this.txtSqlResult.ReadOnly = true;
            this.txtSqlResult.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtSqlResult.Size = new System.Drawing.Size(748, 137);
            this.txtSqlResult.TabIndex = 0;
            this.txtSqlResult.WordWrap = false;
            // 
            // btnGenerate
            // 
            this.btnGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGenerate.Enabled = false;
            this.btnGenerate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGenerate.Location = new System.Drawing.Point(627, 300);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(145, 32);
            this.btnGenerate.TabIndex = 5;
            this.btnGenerate.Text = "Tạo mã nguồn";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 512);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.groupBoxSql);
            this.Controls.Add(this.dgvFields);
            this.Controls.Add(this.groupBoxFields);
            this.Controls.Add(this.txtModelName);
            this.Controls.Add(this.lblModelName);
            this.MinimumSize = new System.Drawing.Size(800, 550);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SQL Generator";
            this.groupBoxFields.ResumeLayout(false);
            this.groupBoxFields.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFields)).EndInit();
            this.groupBoxSql.ResumeLayout(false);
            this.groupBoxSql.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblModelName;
        private System.Windows.Forms.TextBox txtModelName;
        private System.Windows.Forms.GroupBox groupBoxFields;
        private System.Windows.Forms.Button btnRemoveField;
        private System.Windows.Forms.Button btnAddField;
        private System.Windows.Forms.TextBox txtDefaultValue;
        private System.Windows.Forms.Label lblDefaultValue;
        private System.Windows.Forms.CheckBox chkNullable;
        private System.Windows.Forms.CheckBox chkIdentity;
        private System.Windows.Forms.CheckBox chkPrimaryKey;
        private System.Windows.Forms.TextBox txtMaxLength;
        private System.Windows.Forms.Label lblMaxLength;
        private System.Windows.Forms.ComboBox cboDataType;
        private System.Windows.Forms.Label lblDataType;
        private System.Windows.Forms.TextBox txtFieldName;
        private System.Windows.Forms.Label lblFieldName;
        private System.Windows.Forms.DataGridView dgvFields;
        private System.Windows.Forms.GroupBox groupBoxSql;
        private System.Windows.Forms.TextBox txtSqlResult;
        private System.Windows.Forms.Button btnGenerate;
    }
} 